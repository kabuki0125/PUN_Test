using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyLibrary.Unity;


/// <summary>
/// View：ゲームシーンのスクリーン.
/// </summary>
public class Screen_Game : ViewBase
{
    [SerializeField]
    private GameObject rootEnemy;    // 敵格納用.
    
    [SerializeField]
    private GameObject otherRoot;    // その他オブジェクト格納用.
    
    
    /// <summary>
    /// 初期化.
    /// </summary>
	public void Init()
    {
        this.CreateObjects();
    }
    
    // 必要なオブジェクト類の生成.
    private void CreateObjects()
    {
        // ホストなら色々作って提供する.
        this.CreateMasterObjects();
        
        // ユニティちゃんを召喚.
        var go = PhotonNetwork.Instantiate("unitychan", Vector3.zero, Quaternion.identity, 0);
        this.gameObject.AddInChild(go);
    }
    // ゲームに使うオブジェクトの生成.
    private void CreateMasterObjects()
    {
        if(!PhotonNetwork.isMasterClient){
            return;
        }
        
        // バトルシーンで使う通信用共有インスタンスを生成する.
        var go = PhotonNetwork.Instantiate("PUNConnector_Battle", Vector3.zero, Quaternion.identity, 0);
        this.gameObject.AddInChild(go);
        
        /*
        // とりあえず10体ほど敵を出す.
        for(var i = 0 ; i < 5 ; i++){
            var x = Random.Range(-30f, 30f);
            var z = Random.Range(-30f, 30f);
            go = PhotonNetwork.Instantiate("Robot", new Vector3(x, 0f, z), Quaternion.identity, 0);
            rootEnemy.AddInChild(go);
        }
        PUNConnector_Battle.SharedInstance.UpdateEnemyNum(5);
        */
    }
    
#region debug.
    
    void OnGUI()
    {
        GUILayout.BeginVertical();
        
        // TODO : ホストが抜けた場合なくなるので作る.ちゃんとやるならホストが部屋を抜けた時のコールバックにの指示をRPCかなんかで仕込んでおくのが良さそう.
        if(PUNConnector_Battle.SharedInstance == null){
            var go = PhotonNetwork.Instantiate("PUNConnector_Battle", Vector3.zero, Quaternion.identity, 0);
            this.gameObject.AddInChild(go);
        }
        
        // マスタークライアントなら.
        if(PhotonNetwork.isMasterClient){    
            // 敵を追加生成できるようにする.
            if(GUILayout.Button("Create Enemy")){
                var x = Random.Range(-30f, 30f);
                var z = Random.Range(-30f, 30f);
                var go = PhotonNetwork.Instantiate("Robot", new Vector3(x, 0.2f, z), Quaternion.identity, 0);
                rootEnemy.AddInChild(go);
                PUNConnector_Battle.SharedInstance.UpdateEnemyNum(1);
            }
            // 単調な同期オブジェクトを生成する.
            if(GUILayout.Button("Create Box")){
                var x = Random.Range(-30f, 30f);
                var z = Random.Range(-30f, 30f);
                var go = PhotonNetwork.Instantiate("BoxPrefabForTest", new Vector3(x, 50f, z), Quaternion.identity, 0);
                otherRoot.AddInChild(go);
                PUNConnector_Battle.SharedInstance.UpdateBoxNum(1);
            }
        }
        
        // 敵の数.
        GUILayout.Label("<b><color=red>EnemyCount : "+PUNConnector_Battle.SharedInstance.NumEnemy.ToString()+"</color></b>");
        // 箱の数.
        GUILayout.Label("<b><color=blue>BoxCount : "+PUNConnector_Battle.SharedInstance.NumBox.ToString()+"</color></b>");
        
        GUILayout.EndVertical();
    }
    
#endregion
}
