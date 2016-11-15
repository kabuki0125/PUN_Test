using UnityEngine;
using System.Collections;
using MyLibrary.Unity;


/// <summary>
/// ScreenController : ゲームシーンのコントローラー.
/// </summary>
public class GameSController : MonoBehaviour
{
    
    // とりあえず他のシーンからの連携もないのでここで初期化.
    void Start()
    {
        m_punStarter = GameObjectEx.LoadAndCreateObject("PUNNetwork").GetComponent<PUNConnectStarter>();
        m_punStarter.Init( InitInternal );
    }
    
    // 内部初期化.
    private void InitInternal()
    {
        var go = GameObjectEx.LoadAndCreateObject("Screen_Game", this.gameObject);       
        go.GetComponent<Screen_Game>().Init();
    }
    
    private PUNConnectStarter m_punStarter;
}
