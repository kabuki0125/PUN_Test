using UnityEngine;
using System.Collections;

/// <summary>
/// クラス：オンライン上で動くキャラクター.
/// </summary>
public class PUNOnlineCharacter : Photon.MonoBehaviour
{
    
    void Awake()
    {
        GetComponent<ThirdPersonCamera>().enabled = photonView.isMine;
        if(!photonView.isMine){
            this.StartCoroutine("UpdateTransform");
        }
    }
    private IEnumerator UpdateTransform()
    {
        while(!photonView.isMine){
            transform.position = Vector3.Lerp(transform.position, m_correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, m_correctPlayerRot, Time.deltaTime * 5);
            yield return null;
        }
    }
    
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting){
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            
            var myC = GetComponent<ThirdPersonController>();
            stream.SendNext((int)myC._characterState);
        }
        else{
            // Network player, receive data
            m_correctPlayerPos = (Vector3)stream.ReceiveNext();
            m_correctPlayerRot = (Quaternion)stream.ReceiveNext();
            
            var myC = GetComponent<ThirdPersonController>();
            myC._characterState = (CharacterState)stream.ReceiveNext();
        }
    }
    
    [PunRPC]
    void Destroy(){
        PhotonNetwork.Destroy(gameObject);
    }
    
    private Vector3 m_correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion m_correctPlayerRot = Quaternion.identity; // We lerp towards this
}
