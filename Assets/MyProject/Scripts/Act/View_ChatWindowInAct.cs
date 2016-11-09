using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// View：アクションシーン用のチャットウィンドウ.
/// </summary>
public class View_ChatWindowInAct : ViewBase
{
    public void Init(string userName, ChatListener listener)
    {
        m_listener = listener;
        m_listener.Init( userName, 
                        () => View_ChannelToggle.DidToggleActive += ChannelChanged,
                        DidSubScribe);
        m_listener.DidGetGlobalMessage += DidGetGlobalMessageProc;
        
        m_chatPanel = this.GetScript<Image>("Chat Panel");
        m_logText = this.GetScript<Text>("Selected Channel Text");
        m_inputChatField = this.GetScript<InputField>("Chat InputField");
        
        // ボタン設定.
        this.GetScript<Button>("InputBar Panel/Chat Send Button").onClick.AddListener(DidTapSend);
    }
    
    // チャンネル切り替え時.
    private void ChannelChanged(string channelName)
    {
        foreach(var tgl in m_toggleList){
            // トグルの切り替え.
            tgl.IsOn = tgl.ChannelName == channelName;
            // 文言表示の切り替え.
            if(tgl.IsOn){
                m_currentChannelName = tgl.ChannelName;
                m_listener.ChangeChannel(m_currentChannelName);
            }
        }
    }
    
#region ButtonEvents.
    
    // チャット送信ボタン.
    void DidTapSend()
    {
        if(string.IsNullOrEmpty(m_inputChatField.text)){
            return;
        }
        
        Debug.Log("[View_ChatWindow] DidTapSend : message="+m_inputChatField.text);
        m_listener.SendChatMessage(m_inputChatField.text);
        m_inputChatField.text = "";
        
        // 特定のチャンネル(システム)には発言できない.“システム”で発言しようとしていた場合はチャンネルを”全体"に移動.
        if(m_currentChannelName == "システム"){
            this.ChannelChanged("全体");
        }
    }
    
#endregion
    
    // コールバック：購読開始
    void DidSubScribe(string currentName, string[] channels, bool[] results)
    {
        // 返ってきたチャンネル数分タブを作る.
        for(var i = 0 ; i < channels.Length ; i++){
            if(!results[i]){
                continue;
            }
            var tgl = m_toggleList.Find(item => item.ChannelName == channels[i]);
            if(tgl != null){
                tgl.IsOn = channels[i] == currentName;
                continue;
            }
            var go = GameObjectEx.LoadAndCreateObject("Channel Toggle");
            go.GetComponent<RectTransform>().SetParent(this.GetScript<Image>("ChannelBar Panel").transform);    // RectTransformの親設定は通常と異なる.
            var com = go.GetOrAddComponent<View_ChannelToggle>();
            com.Init(channels[i], channels[i] == currentName);
            m_toggleList.Add(com);
        }
    }
    
    // コールバック：グローバルチャットメッセージ取得.
    void DidGetGlobalMessageProc(string channelName, string newMessage)
    {
        // メッセージの更新.
        m_logText.text = "";
        m_logText.text = newMessage;
    }
    
    
    private ChatListener m_listener;    // TODO : 実際に使用する場合はListenerはアプリ起動直後に初期化、常駐させるのがベター.
    
    private Image m_errorPanel;
    private Image m_pickNamePanel;
    private Image m_chatPanel;
    private Text m_logText;
    
    private InputField m_inputChatField;
    private InputField m_userNameField;
    
    private string m_currentChannelName;
    
    private List<View_ChannelToggle> m_toggleList = new List<View_ChannelToggle>();
}
