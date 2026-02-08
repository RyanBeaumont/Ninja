using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.IntegerTime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public enum State{Idle, Windup, Active, Blocking, BlockSuccess ,Stunned, Launched, FollowThrough, Hanging, Knockdown, HardKnockdown, Dead}

public enum GameplayInput{Jump, Attack, Dash, None, BotBlock}
public enum AttackType{Mid, High, Low}

public static class GameConstants
{
    public const float MoveSpeed = 5f;
    public const float JumpForce = 11f;
    public const float Gravity = -50;
}


public class Character : MonoBehaviour
{
    [HideInInspector] public Vector3 moveVector; //Input
    Vector3 lookVector; //Camera.transform.forward
    [HideInInspector] public int canCancel = -1;
    [HideInInspector] public Vector3 vel = Vector3.zero;
    public float MoveSpeed = 5f;
    float velocityTimeout = 0f;
    float rotateSpeed = 30f; //12
    float smoothMoveX; float smoothMoveY;
    float smoothTime = 0.1f;
    float airActions = 1;
    float autoAim = 0f;
    float maxAirActions = 1;
    [HideInInspector] public float pendingJump = 0f;
    GameplayInput lastInput = GameplayInput.None;
    [HideInInspector] public string attackName = "";
    float inputTimeout = 0f;
    CharacterController controller;
    Animator animator;
    [HideInInspector] public int comboStep = 0;
    public float maxHp = 200f;
    [HideInInspector] public float hp;
    public bool waitForGround = false;
    [HideInInspector] public bool waitForHit = false;
    float freezeY = 0f;
    [HideInInspector] public bool blockInput = false;
    [HideInInspector] public bool crouchInput = false;
    public bool player = false;

    //Create a string-int dictionary of all move names on cooldown
    public Dictionary<string,float> moveCooldowns = new Dictionary<string,float>();

    bool isGrounded;

    public State state;

    void SetState(State newState)
    { 
        if(newState != state)
        {
            state = newState; 
            if(state == State.Idle)
            {
                animator.Play(0,0);
            }
        }
        
    } // Add Hit Stun Logic Here

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        hp = maxHp;
    }

    public IEnumerator ChangeModel(string modelName)
    {
        var modelContainer = transform.Find("Model");
        GameObject modelPrefab = Resources.Load<GameObject>("Characters/" + modelName);
        //var anim = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        if(modelPrefab != null)
        {
            foreach(Transform child in modelContainer) Destroy(child.gameObject);
            GameObject modelInstance = Instantiate(modelPrefab, modelContainer);
            modelInstance.transform.localPosition = new Vector3(0f,0.124f,0f);
            modelInstance.transform.localRotation = Quaternion.identity;
            //modelInstance.transform.localScale = new Vector3(.75f,.75f,.75f);
        }
        //animator.Play(anim);
        yield return new WaitForNextFrameUnit();
        animator = GetComponentInChildren<Animator>();
        
    }

    public void SetMotion(Vector3 m, Vector3 l){moveVector = m; lookVector = l;}

    public void SetInput(GameplayInput input, string newAttackName = ""){
        // Ignore inputs when dead
        if(state == State.Dead) return;
        lastInput = input; attackName = newAttackName; inputTimeout = 0.5f;
    }
    void ResetInput(){lastInput = GameplayInput.None; inputTimeout = 0f;}

    void Update()
    {
        if(animator == null) return;
        animator.speed = 1f;
        //Input buffer
        if(inputTimeout > 0f) inputTimeout -= Time.deltaTime;
        else if(lastInput != GameplayInput.None) lastInput = GameplayInput.None;

        if(GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement || GameManager.Instance.GetGameplayState() == GameplayState.Combat){
            LedgeGrab();
            
            if(state == State.Idle && crouchInput && isGrounded)
            {
                SetState(State.Idle);
                animator.SetBool("Crouching",true);
            }
            else
            {
                animator.SetBool("Crouching",false);
            }

            if(state == State.Idle && blockInput)
            {
                SetState(State.Blocking);
                animator.SetBool("Blocking",true);
            }
            if(state == State.Blocking && !blockInput)
            {
                SetState(State.Idle);
            }
            if(!blockInput)animator.SetBool("Blocking",false);
            
            
            if(state == State.Hanging)
            {
                if(lastInput == GameplayInput.Jump)
                {
                    ResetInput();
                    controller.Move(transform.forward * -1f);
                    print("Jumping from ledge");
                    state = State.Idle;
                    vel.y = GameConstants.JumpForce * 1.5f;
                    animator.SetTrigger("Flip");
                }
            }
            
            if(state != State.Hanging)
            {
                if(GetComponent<CharacterController>().enabled)
                Move(moveVector,lookVector);
            } 
        }
    }
    public void Move(Vector3 moveVector, Vector3 lookVector)
    {
        Vector3 motion = Vector3.zero;
        // --- 1. Smooth animation inputs ---
        if(state == State.Idle){
            smoothMoveX = Mathf.Lerp(smoothMoveX, moveVector.x, Time.deltaTime / smoothTime);
            smoothMoveY = Mathf.Lerp(smoothMoveY, moveVector.z, Time.deltaTime / smoothTime);

            if (animator != null)
            {
                animator.SetFloat("X", smoothMoveX);
                animator.SetFloat("Y", smoothMoveY);
            }

            // Flatten LookVector to horizontal plane
            Vector3 flatForward = lookVector; flatForward.y = 0f; flatForward.Normalize();
            Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward); flatRight.y = 0f;flatRight.Normalize();

            Vector3 moveDirection = flatForward * moveVector.z + flatRight * moveVector.x;
            moveDirection.y = 0f;

            

            // --- 2. Compute movement ---
            motion = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized * MoveSpeed;
        }

        // --- 3. Gravity ---
        if(velocityTimeout > 0f)
        {
            velocityTimeout -= Time.deltaTime;
        }
        else
        {
            // Clear only horizontal override velocity; preserve vel.y which encodes vertical velocity/gravity
            vel.x = 0f;
            vel.z = 0f;
        }

        if(freezeY > 0)
        {
            freezeY -= Time.deltaTime;
        }else{
            //Apply gravity only when no override velocity is specified
            if (isGrounded){
                airActions = maxAirActions;
                
                animator.SetBool("Grounded",true);
            }else{
                vel.y += GameConstants.Gravity * Time.deltaTime;
                animator.SetBool("Grounded",false);
            }

        }

        //Jump
        if(lastInput == GameplayInput.Jump && state == State.Idle)
        {
            ResetInput();
            if(isGrounded || airActions >= 1)
            {
                if(!isGrounded) {airActions -= 1; animator.SetTrigger("Flip");}
                vel.y = GameConstants.JumpForce;
            }
        }
        if(lastInput == GameplayInput.Dash && state == State.Idle)
        {
            ResetInput();
            if(!isGrounded && airActions >= 1) {
                airActions --; 
                freezeY = 0.5f;
                // Preserve current vertical velocity when applying a dash horizontal override
                Vector3 dashVel = motion.normalized * 4f;
                vel.x = dashVel.x;
                vel.z = dashVel.z;
                velocityTimeout = 0.5f;
                animator.SetTrigger("Flip");
            }
        }

        if(pendingJump != 0f){
            vel.y = pendingJump;
            pendingJump = 0f;
        }

        motion.y = vel.y;

        // --- 4. Move the character ---
        // Only add horizontal components from vel here; vertical is handled via vel.y
        motion += new Vector3(vel.x, 0f, vel.z);
        controller.Move(motion * Time.deltaTime);

        //Update grounded
        float snapDistance = 0.5f;
        Vector3 p1 = transform.position + controller.center + Vector3.up * (controller.height / 2f - controller.radius);
        Vector3 p2 = transform.position + controller.center - Vector3.up * (controller.height / 2f - controller.radius);
        RaycastHit hit;
        if (vel.y <=0f && Physics.CapsuleCast(p1,p2,controller.radius * 0.9f,Vector3.down,out hit,snapDistance,LayerMask.GetMask("Ground","Roof","Default"),QueryTriggerInteraction.Ignore))
        {
            // Ensure we stick to ground
            controller.Move(Vector3.down * hit.distance);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // --- 5. Smooth rotation toward look vector ---
        Vector3 flatLook = lookVector;
        if(autoAim > 0f){
           //Set flatLook to point at nearest enemy
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float nearestDist = Mathf.Infinity; 
            GameObject nearestEnemy = null;
            foreach(GameObject e in enemies){
                float dist = Vector3.Distance(transform.position, e.transform.position);
                if(dist < nearestDist){
                    nearestDist = dist;
                    nearestEnemy = e;
                }
            }
            if(nearestEnemy != null){
                flatLook = (nearestEnemy.transform.position - transform.position).normalized;
            }
            if(!GameManager.Instance.IsFrozen())
                autoAim -= Time.deltaTime;
        }
        flatLook.y = 0f;

        if (flatLook.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatLook);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        
    }

    void LedgeGrab()
    {
        if(vel.y < -0.3 && !isGrounded && state != State.Hanging && GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement)
        {
            RaycastHit downHit;
            Vector3 lineDownStart = (transform.position + Vector3.up*1.5f) + transform.forward * 1f;
            Vector3 lineDownEnd = (transform.position + Vector3.up*0.5f) + transform.forward * 1f;
            Physics.Linecast(lineDownStart,lineDownEnd,out downHit, LayerMask.GetMask("Roof"));
            Debug.DrawLine(lineDownStart,lineDownEnd);

            if(downHit.collider != null)
            {
                RaycastHit forwardHit;
                Vector3 lineForwardStart = new Vector3(transform.position.x, downHit.point.y-0.1f, transform.position.z);
                Vector3 lineForwardEnd = new Vector3(transform.position.x, downHit.point.y-0.1f, transform.position.z) + transform.forward;
                Physics.Linecast(lineForwardStart,lineForwardEnd,out forwardHit, LayerMask.GetMask("Roof"));
                Debug.DrawLine(lineForwardStart,lineForwardEnd);
           
                if(forwardHit.collider != null)
                {
                    controller.enabled = false;
                    vel.y = 0;
                    state = State.Hanging;
                    animator.Play("Hanging");
                    Vector3 hangPos = new Vector3(forwardHit.point.x,downHit.point.y,forwardHit.point.z);
                    Vector3 offset = transform.forward * -0.35f + transform.up * -0.1f;//Vector3 offset = transform.forward * -0.35f + transform.up * -0.55f;
                    hangPos += offset;
                    transform.position = hangPos;
                    // Get a horizontal direction pointing away from wall
                    Vector3 wallNormal = forwardHit.normal;
                    wallNormal.y = 0f;
                    wallNormal.Normalize();

                    transform.forward = -wallNormal; // Look toward the ledge
                                        controller.enabled = true;
                                    }
                                }

        }
    }

    
}