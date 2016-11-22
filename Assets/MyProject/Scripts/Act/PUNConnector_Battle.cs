using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MyLibrary.Unity;


/// <summary>
/// PUN通信用コネクタークラス：バトルシーンで全ユーザーで共有する.
/// </summary>
public class PUNConnector_Battle : Photon.MonoBehaviour
{

    /// <summary>全ユーザーで共通して使う共有インスタンス.</summary>
    public static PUNConnector_Battle SharedInstance { get; private set; }    
    
    /// <summary>生成されている敵の数.</summary>
    public int NumEnemy { get; private set; }
    
    /// <summary>生成されている箱の数.</summary>
    public int NumBox { get; private set; }
    
    
    /// <summary>
    /// 敵の数更新.
    /// </summary>
    public void UpdateEnemyNum(int addVal)
    {
        this.photonView.RPC("AddEnemyNum", PhotonTargets.AllBuffered, addVal);
    }
    
    /// <summary>
    /// 箱の数更新.
    /// </summary>
    public void UpdateBoxNum(int addVal)
    {
        this.photonView.RPC("AddBoxNum", PhotonTargets.AllBuffered, addVal);
    }
    
#region RPC.
    
    [PunRPC]
    public void AddEnemyNum(int num)
    {
        this.NumEnemy += num;
    }
    [PunRPC]
    public void AddBoxNum(int num)
    {
        this.NumBox += num;
    }
    
#endregion
    
    void Awake()
    {
        if(SharedInstance != null){
            return;
        }
        
        SharedInstance = this;
    }
}
