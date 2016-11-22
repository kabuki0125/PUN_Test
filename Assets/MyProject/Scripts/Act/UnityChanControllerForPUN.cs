//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;
using MyLibrary.Unity;

namespace UnityChan
{
    // 必要なコンポーネントの列記
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    
    /// <summary>
    /// Photonリアルタイム通信用Unityちゃんコントローラ.
    /// </summary>
    public class UnityChanControllerForPUN : Photon.MonoBehaviour
    {
        public float animSpeed = 1.5f;              // アニメーション再生速度設定
        public float lookSmoother = 3.0f;           // a smoothing setting for camera motion
        public bool useCurves = true;               // Mecanimでカーブ調整を使うか設定する
        // このスイッチが入っていないとカーブは使われない
        public float useCurvesHeight = 0.5f;        // カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）
        
        // 以下キャラクターコントローラ用パラメタ
        // 前進速度
        public float forwardSpeed = 7.0f;
        // 後退速度
        public float backwardSpeed = 2.0f;
        // 旋回速度
        public float rotateSpeed = 2.0f;
        // ジャンプ威力
        public float jumpPower = 3.0f; 
        
        
        // 初期化
        void Start ()
        {   
            if(!photonView.isMine){
                return;
            }
            
            // Animatorコンポーネントを取得する
            anim = GetComponent<Animator> ();
            anim.speed = animSpeed; // Animatorのモーション再生速度に animSpeedを設定する
            
            // CapsuleColliderコンポーネントを取得する（カプセル型コリジョン）
            col = GetComponent<CapsuleCollider> ();
            rb = GetComponent<Rigidbody>();
            
            // CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
            orgColHight = col.height;
            orgVectColCenter = col.center;
            
            //メインカメラを取得する
            flickContoroller = GameObjectEx.LoadAndCreateObject("InputArea").GetComponent<FlickController>();
            flickContoroller.DidTap += JumpProc;                // タップしたらジャンプ.
            flickContoroller.DidDrag += InputFlickReceiver;     // 入力操作受付.
            flickContoroller.DidEndDrag += InputSwipeEnd;
        }
        // フリック入力操作受付..移動する.
        private void InputFlickReceiver(float velocityX, float velocityY)
        {   
            this.StopCoroutine("InertiaBackVelocity");
            var vec2 = new Vector2(velocityX, velocityY);
            var normalX = vec2.normalized.x;
            var normalY = vec2.normalized.y;
            vectorFlick = new Vector2(normalX, normalY);
        }
        // スワイプ入力終了時のコールバック.
        private void InputSwipeEnd()
        {
            this.StartCoroutine("InertiaBackVelocity");
        }
        // 徐々に0に戻す.
        private IEnumerator InertiaBackVelocity()
        {
            while(vectorFlick != Vector2.zero){
                var vec = vectorFlick;
                vec.x -= 0.1f;
                vec.y -= 0.1f;
                if(vec.x <= 0f){
                    vec.x = 0f;
                }
                if(vec.y <= 0f){
                    vec.y = 0f;
                }
                vectorFlick = vec;
                yield return null;
            }
        }
        
        // 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
        // またPhotonの同期通信に載せたい場合はここで値を変更しないと相手がたに届かないので注意.
        void FixedUpdate ()
        {
            // 操作が可能なのは自身がコントロールを持ってる時だけ.
            if(!photonView.isMine){
                return;
            }
            
            float v = Input.GetAxis("Vertical");               // 入力デバイスの垂直軸をvで定義
            float h = Input.GetAxis("Horizontal");             // 入力デバイスの水平軸をhで定義
            
            // TODO : よう修正.動きっぱなしになる.
            if(v==0 && h==0){
                v = vectorFlick.y;
                h = vectorFlick.x;
            }
            
            // axisの値で移動.
            this.LocomotionProc(v, h);
            
            // スペースキーを入力したらJump
            if(Input.GetButtonDown("Jump")){ 
                this.JumpProc();
            }
            
            // --- 以下、Animatorの各ステート中での処理 ---
            rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする
            currentBaseState = anim.GetCurrentAnimatorStateInfo (0);    // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
            // Locomotion中
            if(currentBaseState.fullPathHash == locoState){
                this.LocomotionState();
            }
            // JUMP中の処理
            else if(currentBaseState.fullPathHash == jumpState){
                this.JumpState();
            }
            // IDLE中の処理
            else if(currentBaseState.fullPathHash == idleState){
                this.IdleState();
            }
            // REST中の処理
            else if(currentBaseState.fullPathHash == restState){
                this.RestState();
            }
        }
        
        // 移動入力操作.
        private void LocomotionProc(float axisV, float axisH)
        {
            anim.SetFloat("Speed", axisV);         // Animator側で設定している"Speed"パラメタにvを渡す
            anim.SetFloat("Direction", axisH);     // Animator側で設定している"Direction"パラメタにhを渡す
            
            // 以下、キャラクターの移動処理
            velocity = new Vector3 (0, 0, axisV);       // 上下のキー入力からZ軸方向の移動量を取得
            // キャラクターのローカル空間での方向に変換
            velocity = transform.TransformDirection (velocity);
            //以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
            if(axisV > 0.1){
                velocity *= forwardSpeed;       // 移動速度を掛ける
            }else if(axisV < -0.1){
                velocity *= backwardSpeed;  // 移動速度を掛ける
            }
            
            // 上下のキー入力でキャラクターを移動させる
            transform.localPosition += velocity * Time.fixedDeltaTime;
            
            // 左右のキー入力でキャラクタをY軸で旋回させる
            transform.Rotate(0, axisH * rotateSpeed, 0);
        }
        
        // ジャンプ入力操作.
        private void JumpProc()
        {
            //アニメーションのステートがLocomotionの最中のみジャンプできる
            if (currentBaseState.fullPathHash != locoState) {
                return;
            }
            
            //ステート遷移中でなかったらジャンプできる
            if (!anim.IsInTransition (0)) {
                rb.AddForce (Vector3.up * jumpPower, ForceMode.VelocityChange);
                anim.SetBool ("Jump", true);        // Animatorにジャンプに切り替えるフラグを送る
            }
        }
        
#region State Proc
        
        // Locomotion中.現在のベースレイヤーがlocoStateの時.
        private void LocomotionState()
        {
            //カーブでコライダ調整をしている時は、念のためにリセットする
            if (useCurves) {
                resetCollider ();
            }
        }
        
        // JUMP中の処理.現在のベースレイヤーがjumpStateの時.
        private void JumpState()
        {
            // ステートがトランジション中でない場合
            if(anim.IsInTransition(0)){
                return;
            }
                
            // 以下、カーブ調整をする場合の処理
            if(useCurves){
                // 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
                // JumpHeight:JUMP00でのジャンプの高さ（0〜1）
                // GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
                float jumpHeight = anim.GetFloat ("JumpHeight");
                float gravityControl = anim.GetFloat ("GravityControl"); 
                if(gravityControl > 0){
                    rb.useGravity = false;  //ジャンプ中の重力の影響を切る
                }
                
                // レイキャストをキャラクターのセンターから落とす
                Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                RaycastHit hitInfo = new RaycastHit ();
                // 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
                if(Physics.Raycast(ray, out hitInfo)){
                    if(hitInfo.distance > useCurvesHeight){
                        col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
                        float adjCenterY = orgVectColCenter.y + jumpHeight;
                        col.center = new Vector3(0, adjCenterY, 0);    // 調整されたコライダーのセンター
                    }else{
                        // 閾値よりも低い時には初期値に戻す（念のため）                   
                        resetCollider();
                    }
                }
            }
            // Jump bool値をリセットする（ループしないようにする）               
            anim.SetBool ("Jump", false);
        }
        
        // IDLE中の処理.現在のベースレイヤーがidleStateの時.
        private void IdleState()
        {
            //カーブでコライダ調整をしている時は、念のためにリセットする
            if (useCurves) {
                resetCollider ();
            }
            // スペースキーを入力したらRest状態になる
            if (Input.GetButtonDown ("Jump")) {
                anim.SetBool ("Rest", true);
            }
        }
        
        // REST中の処理.現在のベースレイヤーがrestStateの時.
        private void RestState()
        {
            // ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
            if (!anim.IsInTransition (0)) {
                anim.SetBool ("Rest", false);
            }
        }
        
#endregion
        
        // キャラクターのコライダーサイズのリセット関数
        void resetCollider ()
        {
            // コンポーネントのHeight、Centerの初期値を戻す
            col.height = orgColHight;
            col.center = orgVectColCenter;
        }
        
        
#region debug.
        // 同期通信タイミングでコールバックが来る.Photonクラウドに対する負荷検証用.
        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 送信.
            if(stream.isWriting){
//                stream.SendNext((int)GameSystem.GetUnixTimeNow());
                // ゴミを送りまくって負荷を確認してみる.
                for(var i = 0 ; i < m_sendVal ; i++){
                    stream.SendNext("test");
                }
            }
            
            // 受信.RPCによって書き込みデータ量が書き換えられる可能性がある.書き換え後は前の書き込み数で読み込みを完了するまでスキップする.
            if(stream.isReading && m_bPhotonSyncRead){
                // TODO : ログが負荷になるので.
//                var sendTime = (int)stream.ReceiveNext();
//                var processTime = ((int)GameSystem.GetUnixTimeNow()) - sendTime;
//                Debug.Log("<color=red>from OnPhotonSerializeView process time="+processTime.ToString()+"秒</color>");
                for(var i = 0 ; i < m_sendVal ; i++){
                    stream.ReceiveNext();
                }
            }
        }
        
        // ユニティちゃんPhotonView経由で行うデバッグメニュー.
        private string m_inputStrNum = "";  // TextAreaが文字列入力になるので一旦これで受け取る.
        void OnGUI ()
        {
            if(!this.photonView.isMine){
                return; // モジュールになるのは自分が操作しているユニティちゃんだけ.
            }
            
            // 定期送受信している同期文字列数を変える.必ず検証対象プレイヤー全員が部屋に入っている状態で変更を行うこと!!送受信量不一致でエラーが出る!!
            GUI.Label(new Rect(Screen.width - 260, 10, 300, 20), "Send Message Value : "+m_sendVal.ToString());
            if( GUI.Button(new Rect(Screen.width - 260, 30, 120, 60), "SendVal : ") && !string.IsNullOrEmpty(m_inputStrNum)){
                var val = 0;
                if( int.TryParse( m_inputStrNum, out val) ){
                    this.photonView.RPC("ChangeSendValue", PhotonTargets.AllBuffered, val);
                    m_inputStrNum = "";
                }
            }
            m_inputStrNum = GUI.TextArea(new Rect(Screen.width - 135, 30, 140, 60), m_inputStrNum);
            
            // 大量のRPCを送信する.
            if( GUI.Button(new Rect(Screen.width - 260, 100, 250, 50), "Send Massive RPC.") ){
                for(var i = 0 ; i < 20000 ; i++){
                    this.photonView.RPC("SendMassiveMessage", PhotonTargets.AllBuffered, "test");
                }
            }
        }
        
        /// <summary>デバッグ送信メッセージ量を変更する.</summary>
        [PunRPC]
        public void ChangeSendValue(int val)
        {
            this.StartCoroutine("ChangeOnSerializeSendVal" , val);
        }
        // 書き換えた後のデータ量で送信してくるまでタイムラグがあるので待ちつつ,RPC送信.
        private IEnumerator ChangeOnSerializeSendVal(int val)
        {
            m_bPhotonSyncRead = false;
            
            var time = new WaitForSeconds(3f);  // 前のsendValでの受信終わりを待つ.
            yield return time;
            
            m_sendVal = val;
            Debug.Log("ChangeSendVal for OnPhotonSerializeView : val="+m_sendVal);
            
            time = new WaitForSeconds(3f);  // 各々のRPC受信ラグを待つ.
            yield return time;
            
            m_bPhotonSyncRead = true;
        }
        
        /// <summary>大量のRPC送信デバッグで送受信する際のRPC定義.ログを表示するとその処理だけで負荷がかかり純粋なクラウド負荷が見られなくなるのでデフォルトでは何もしない.</summary>
        [PunRPC]
        public void SendMassiveMessage(string text){}
        
        private static int m_sendVal = 0;   // デバッグ文字列送信量.この値は場にいる全てのユニティちゃんで共有して使用する.
        private static bool m_bPhotonSyncRead = true;   // OnPhotonSerializeViewの読み込み有効無効フラグ.
#endregion
        
        private Vector2 vectorFlick;
        private FlickController flickContoroller;
        
        // キャラクターコントローラ（カプセルコライダ）の参照
        private CapsuleCollider col;
        private Rigidbody rb;
        
        // キャラクターコントローラ（カプセルコライダ）の移動量
        private Vector3 velocity;
        
        // CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
        private float orgColHight;
        private Vector3 orgVectColCenter;
        private Animator anim;                          // キャラにアタッチされるアニメーターへの参照
        private AnimatorStateInfo currentBaseState;         // base layerで使われる、アニメーターの現在の状態の参照
        
        // アニメーター各ステートへの参照
        static int idleState = Animator.StringToHash ("Base Layer.Idle");
        static int locoState = Animator.StringToHash ("Base Layer.Locomotion");
        static int jumpState = Animator.StringToHash ("Base Layer.Jump");
        static int restState = Animator.StringToHash ("Base Layer.Rest");
    }
}