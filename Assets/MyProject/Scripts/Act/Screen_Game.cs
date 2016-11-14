using UnityEngine;
using System.Collections;
using MyLibrary.Unity;


/// <summary>
/// View：ゲームシーンのスクリーン.
/// </summary>
public class Screen_Game : ViewBase
{
    [SerializeField]
    private GameObject rootEnemy;    // 敵格納用.
    
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
        // ユニティちゃんを召喚.
        var go = PhotonNetwork.Instantiate("unitychan", Vector3.zero, Quaternion.identity, 0);
        this.gameObject.AddInChild(go);
        
        // ホストなら色々作って提供する.
        this.CreateGameObjects();
    }
    // ゲームに使うオブジェクトの生成.
    private void CreateGameObjects()
    {
        if(!PhotonNetwork.isMasterClient){
            return;
        }
        
        // とりあえず10体ほど敵を出す.
        for(var i = 0 ; i < 5 ; i++){
            var x = Random.Range(1f, 30f);
            var y = Random.Range(1f, 30f);
            var go = PhotonNetwork.Instantiate("Robot", new Vector3(x, y), Quaternion.identity, 0);
            rootEnemy.AddInChild(go);
        }
    }
    
#region debug.
    
    void OnGUI()
    {
        // マスタークライアントなら.
        if(PhotonNetwork.isMasterClient){
            // 敵を追加生成できるようにする.
            if(GUILayout.Button("Create Enemy")){
                var x = Random.Range(1f, 30f);
                var y = Random.Range(1f, 30f);
                var go = PhotonNetwork.Instantiate("Robot", new Vector3(x, y), Quaternion.identity, 0);
                rootEnemy.AddInChild(go);
            }
        }
    }
    
#endregion
}
