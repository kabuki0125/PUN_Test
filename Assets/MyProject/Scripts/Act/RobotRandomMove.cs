using UnityEngine;
using System.Collections;

public class RobotRandomMove : MonoBehaviour 
{
    protected Animator animator;
    public float DirectionDampTime = .25f;
    public bool ApplyGravity = true;
    public float SynchronizedMaxSpeed;
    public float TurnSpeedModifier;
    public float SynchronizedTurnSpeed;
    public float SynchronizedSpeedAcceleration;
    
    protected PhotonView m_PhotonView;
    
    PhotonTransformView m_TransformView;
    
    //Vector3 m_LastPosition;
    float m_SpeedModifier;
    
    // Use this for initialization
    void Start () 
    {
        animator = GetComponent<Animator>();
        m_PhotonView = GetComponent<PhotonView>();
        m_TransformView = GetComponent<PhotonTransformView>();
        
        if(animator.layerCount >= 2){
            animator.SetLayerWeight(1, 1);
        }
        
        this.StartCoroutine("DecideMoveVelocity");
    }
    
    // Update is called once per frame
    void Update () 
    {
        if( animator == null )
        {
            return;
        }
        
        
        /*
        // TODO : Jumpとか挨拶モーションをさせるならコメントアウト解除.
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);          
        
        if (stateInfo.IsName("Base Layer.Run"))
        {
            if (m_bJump) animator.SetBool("Jump", true);      
        }
        else
        {
            animator.SetBool("Jump", false);
            m_bJump = false;
        }
        
        if(m_bHi && animator.layerCount >= 2)
        {
            animator.SetBool("Hi", !animator.GetBool("Hi"));
            m_bHi = false;
        }
        */
        
        if( m_vertical < 0 )
        {
            m_vertical = 0;
        }
        
        animator.SetFloat( "Speed", m_horizontal*m_horizontal+m_vertical*m_vertical );
        animator.SetFloat( "Direction", m_horizontal, DirectionDampTime, Time.deltaTime );
        
        float direction = animator.GetFloat( "Direction" );
        
        float targetSpeedModifier = Mathf.Abs( m_vertical );
        
        if( Mathf.Abs( direction ) > 0.2f )
        {
            targetSpeedModifier = TurnSpeedModifier;
        }
        
        m_SpeedModifier = Mathf.MoveTowards( m_SpeedModifier, targetSpeedModifier, Time.deltaTime * 25f );
        
        Vector3 speed = transform.forward * SynchronizedMaxSpeed * m_SpeedModifier;
        float turnSpeed = direction * SynchronizedTurnSpeed;
        
        /*float moveDistance = Vector3.Distance( transform.position, m_LastPosition ) / Time.deltaTime;

        if( moveDistance < 4f && turnSpeed == 0f )
        {
            speed = transform.forward * moveDistance;
        }*/
        
        //Debug.Log( moveDistance );
        //Debug.Log( speed + " - " + speed.magnitude + " - " + speedModifier + " - " + h + " - " + v );
        
        m_TransformView.SetSynchronizedValues( speed, turnSpeed );
        
        //m_LastPosition = transform.position;
    }
    
    // 移動方向決定.定期タイミングで移動・停止を切り替える.
    private IEnumerator DecideMoveVelocity()
    {
        while(true){
            if(Mathf.Approximately(m_vertical, 0f)){
                m_horizontal = Mathf.Approximately(m_horizontal, 0f) ? Random.Range(-1f, 1f) : 0f;
            }
            if(Mathf.Approximately(m_horizontal, 0f)){
                m_vertical = Mathf.Approximately(m_vertical, 0f) ? Random.Range(-1f, 1f) : 0f;
            }
            
            var waitTime = Random.Range(2f, 3f);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private float m_horizontal;
    private float m_vertical;
    
}
