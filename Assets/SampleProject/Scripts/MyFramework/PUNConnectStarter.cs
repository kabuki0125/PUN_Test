using UnityEngine;

/// <summary>
/// クラス：PUNのネットワーク接続を開始するスターター.
/// </summary>
public class PUNConnectStarter : Photon.PunBehaviour 
{
    
    // 初期化
    void Awake()
    {   
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.ConnectUsingSettings("v1.0");     // マスターサーバーへ接続.
    }
    
    // ルーム接続周りはとりあえずGUIで.
    void OnGUI()
    {
        if(!PhotonNetwork.insideLobby){
            return;
        }
        
        // ルーム新規作成.
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("CreateRoom : ") && !string.IsNullOrEmpty(m_roomName)){
            PhotonNetwork.CreateRoom(m_roomName);   // TODO : 必要に応じてRoomOptionクラスを作成しオプション追加.
        }
        m_roomName = GUILayout.TextArea(m_roomName, 10, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        // ランダム接続
        if(GUILayout.Button("RandomConnect")){
            PhotonNetwork.JoinRandomRoom();
        }
        // 既存のルーム
        foreach(var room in PhotonNetwork.GetRoomList()){
            if(GUILayout.Button( room.name+" ... ("+room.playerCount.ToString()+"/"+room.maxPlayers.ToString()+")" )){
                PhotonNetwork.JoinRoom(room.name);
            }
        }
    }
    
#region PUN Behaviour
    
    /// <summary>
    /// PhotonNetwork.autoJoinLobby がtrueならマスターサーバーへ接続後そのままこのメソッドが呼ばれる.
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("[PUNConnectStarter] joined lobby.");
        // TODO : uGUIを使わないパターン(NGUIなど)はここにルーム入室処理を記載するのが流れとしては正しい.
    }
    
    /// <summary>
    /// 部屋に入った際のコールバック.
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("[PUNConnectStarter] joined room.");
        
        // テストでとりあえずユニティちゃんを召喚.
        GameObject unitychan = PhotonNetwork.Instantiate("unitychan", Vector3.zero, Quaternion.identity, 0);
        unitychan.GetComponent<ThirdPersonController>().isControllable = true;
    }
    
#endregion
    
#if false   // ロビー入室前にしたいことがあればこっちのコールバックで処理を追加
    /// <summary>
    /// PhotonNetwork.autoJoinLobby がfalseならマスターサーバーへ接続後そのままこのメソッドが呼ばれる.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        // TODO: ロビー入室前にやりたい処理
        PhotonNetwork.JoinLobby();
    }
#endif
    
    private string m_roomName = "";
//    private PhotonView m_myPhotonView;
}
