using UnityEngine;
using System;

/// <summary>
/// クラス：PUNのネットワーク接続を開始するスターター.
/// </summary>
public class PUNConnectStarter : Photon.PunBehaviour 
{
    /// <summary>チャット用コネクタ.</summary>
    [SerializeField]
    private ChatListener chatListener;
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(Action didInRoom = null)
    {
        m_didInRoom = didInRoom;
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.ConnectUsingSettings("v1.0");     // マスターサーバーへ接続.
    }
    
    // ルーム接続周りはとりあえずGUIで.
    void OnGUI()
    {
        if(!PhotonNetwork.insideLobby){
            return;
        }
        
        // --- ルーム新規作成. ---
        // ユーザー名.チャット用にこれがないと通らないようにする.
        GUILayout.BeginHorizontal();
        GUILayout.TextField("UserName : ");
        m_userName = GUILayout.TextArea(m_userName, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        // ルーム作成
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("CreateRoom : ") && !string.IsNullOrEmpty(m_roomName) && !string.IsNullOrEmpty(m_userName)){
            PhotonNetwork.CreateRoom(m_roomName);   // TODO : 必要に応じてRoomOptionクラスを作成しオプション追加.
        }
        m_roomName = GUILayout.TextArea(m_roomName, 10, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        // ランダム接続
        if(GUILayout.Button("RandomConnect") && !string.IsNullOrEmpty(m_userName)){
            PhotonNetwork.JoinRandomRoom();
        }
        // 既存のルーム
        foreach(var room in PhotonNetwork.GetRoomList()){
            if(GUILayout.Button( room.name+" ... ("+room.playerCount.ToString()+"/"+room.maxPlayers.ToString()+")" ) && !string.IsNullOrEmpty(m_userName) ){
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
        // TODO : GUILayoutを使わないパターン(NGUIなど)はここにルーム入室処理を記載するのが流れとしては正しい.
    }
    
    /// <summary>
    /// 部屋に入った際のコールバック.
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("[PUNConnectStarter] joined room.");
        
        // 併せてチャットウィンドウを作る.こっちは各ユーザーが手元にそれぞれ用意する.
        var go = GameObjectEx.LoadAndCreateObject("View_ChatWindowInAct");
        go.GetComponent<View_ChatWindowInAct>().Init(m_userName, chatListener);
        
        if(m_didInRoom != null){
            m_didInRoom();
        }
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
    
    private string m_userName = "";
    private string m_roomName = "";
    private Action m_didInRoom;
}
