using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.IntegerTime;
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
    
    List<AttackAction> attackQueue;
    bool attackQueueFinished = true;
    public float maxHp = 200f;
    [HideInInspector] public float hp;
    public bool waitForGround = false;
    [HideInInspector] public bool waitForHit = false;
    float armor = 1f;
    float freezeY = 0f;
    [HideInInspector] public bool blockInput = false;
    [HideInInspector] public bool crouchInput = false;
    float attackQueueTimer = 0f;
    public bool player = false;

    //Create a string-int dictionary of all move names on cooldown
    public Dictionary<string,float> moveCooldowns = new Dictionary<string,float>();

    bool isGrounded;

    public State state;
    List<string> cooldownKeys = new List<string>();

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
        animator = GetComponent<Animator>();
        attackQueue = new List<AttackAction>();
        hp = maxHp;
    }

    public void TakeDamage(float damage, Transform caller, float knockbackDistance, State hitState, AttackType attackType, bool finalHit)
    {
        // If already dead, ignore further damage
        if(state == State.Dead) return;
        var callerChar = caller.GetComponent<Character>();
        var jump = 0f;
        var success = true;
        print($" Taking damage in state {state}");
        Vector3 dir = (transform.position - caller.transform.position).normalized;
        //if there is a wall between position and position+knockbackDistance
        RaycastHit hit;
        if(Physics.Raycast(transform.position, (transform.position - caller.transform.position).normalized, out hit, knockbackDistance, LayerMask.GetMask("Ground","Wall")))
        {
            if(hit.collider != null && hit.collider.gameObject != gameObject)
            {
                callerChar.vel.x = -dir.x * 5f;
                callerChar.vel.z = -dir.z * 5f;
                callerChar.velocityTimeout = 0.1f;
            }
        }
        
        if((state == State.Blocking || state == State.BlockSuccess) && attackType != AttackType.Low)
        {
            //You blocked it!
            jump = 1f;
            success = false;
            GameManager.Instance.Freeze(0.4f);
            var p = Instantiate(Resources.Load<GameObject>("Particles/Block"), transform.position, Quaternion.identity);
            attackQueue.Clear();
            attackQueueTimer = 0f;
            if(finalHit)
             callerChar.OnHitOrBlock();
            attackQueue.Add(new KnockbackAction{anim = "BlockSuccess", duration = 11 , direction=dir, distance = 1f, state = State.BlockSuccess});
        }else if(state == State.Windup)
        {
            attackQueue.Clear();
            attackQueueTimer = 0f;
            //Critical hit
            hitState = State.Launched;
        }
        else if(state == State.Knockdown || state == State.HardKnockdown || state == State.Dead)
        {
            success = false;
            //Miss, you are already knocked down and can't be double-knocked
        }
        else if(attackType == AttackType.Low && !isGrounded)
        {
            success = false;
            //Miss, you jumped over it!
        }else{
            jump = 1f;
        }
        if(success){
            GameManager.Instance.Freeze(damage/50f);
            //freezeY = 0.5f;
            //callerChar.freezeY = 0.5f;
            callerChar.autoAim = 0.8f;
            autoAim = 0.8f;
            //GameManager.Instance.Freeze(0.5f);
            var p = Instantiate(Resources.Load<GameObject>("Particles/Hit"), transform.position, Quaternion.identity);
            animator.SetBool("Blocking",false);
            var activeHitbox = transform.GetComponentInChildren<Hitbox>();
            if(activeHitbox != null){activeHitbox.gameObject.SetActive(false); Destroy(activeHitbox.gameObject);}
            ResetInput();
            hp -= damage * armor;
            armor *= 0.8f;
            attackQueue.Clear();
            attackQueueTimer = 0f;
            if(finalHit)
            callerChar.OnHitOrBlock();
            transform.LookAt(caller);
            if(hitState == State.Stunned){
                print("Applying Stun");
                attackQueue.Add(new KnockbackAction{anim = "Stunned", duration = 20 , direction=dir, distance = knockbackDistance, state = State.Stunned});
            }if(hitState == State.Knockdown){
                attackQueue.Add(new KnockbackAction{anim = "Knockdown", duration = 15 , direction=dir, distance = knockbackDistance, state = State.Knockdown});
                attackQueue.Add(new WaitAction{duration = 100});
            }if(hitState == State.Launched){
                attackQueue.Add(new KnockbackAction{anim = "Launcher", duration = 45 , direction=dir, distance = knockbackDistance * 0.5f, state = State.Launched});
                attackQueue.Add(new WaitForGroundHitboxAction{duration = 120});
                jump = 2f;
            }if(hitState == State.HardKnockdown){
                attackQueue.Add(new KnockbackAction{anim = "Knockdown", duration = 30 , direction=dir, distance = knockbackDistance, state = State.HardKnockdown});
                attackQueue.Add(new WaitAction{duration = 120});
            }
        }else{
           
        }
                //Send both the player and the hitter into the air slightly (store vertical velocity in vel.y)
        //set position a tiny bit up to avoid ground collision issues
        if(jump > 0f){
            if(!isGrounded || jump > 1f){

                var callerController = caller.GetComponent<CharacterController>();

                controller.Move(new Vector3(0f,0.1f,0f));
                callerController.Move(new Vector3(0f,0.1f,0f));

                callerChar.pendingJump = GameConstants.JumpForce * jump;
                pendingJump = GameConstants.JumpForce * jump;

                // Prevent immediate re-grounding from overwriting upward velocity
                isGrounded = false;
                callerChar.isGrounded = false;
                
            }
        }
        
        
    }

    public void OnHitOrBlock()
    {

        if(GetComponent<BotInput>() != null){
            GetComponent<BotInput>().OnHitOrBlock();
        }
        
        if(AttackActions.Instance.GetAttack(attackName) != null)
            canCancel = AttackActions.Instance.GetAttack(attackName).cancelPriority;
        else
            canCancel = 10;
        if(attackName == "Punch") comboStep = 1;
        if(attackName == "PunchCombo") comboStep = 2;
    }

    public void EndWaitForHit()
    {
        if(waitForHit){
            print("Wait for hit over");
            attackQueueTimer = 0f;
            waitForHit = false;
            var activeHitbox = transform.GetComponentInChildren<Hitbox>();
            if(activeHitbox != null){activeHitbox.timer = 0; activeHitbox.gameObject.SetActive(false); Destroy(activeHitbox.gameObject);}
            velocityTimeout = 0f;
            vel = Vector3.zero;
        }
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
        if(GameManager.Instance.IsFrozen()){animator.speed = 0; return;}

        //Death for NPC characters
        if(hp <= 0f && !player && state != State.Dead)
        {
            animator.Play("Knockdown",0,0f);
            Instantiate(Resources.Load<GameObject>("Particles/Death"), transform.position, Quaternion.identity);
            GameManager.Instance.Freeze(0.6f);
            Destroy(gameObject, 3f);
            state = State.Dead;
            return;
        }

        animator.speed = 1f;
        //Input buffer
        if(inputTimeout > 0f) inputTimeout -= Time.deltaTime;
        else if(lastInput != GameplayInput.None) lastInput = GameplayInput.None;

        if(GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement || GameManager.Instance.GetGameplayState() == GameplayState.Combat){
            LedgeGrab();
            
            TryAttack();

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
                Move(moveVector,lookVector);
            } 
        }
    }

    void FixedUpdate()
    {
            if(GameManager.Instance.IsFrozen()){return;}
            if(GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement || GameManager.Instance.GetGameplayState() == GameplayState.Combat){
                ProcessAttackQueue();
            }
    }

    void ProcessAttackQueue()
    {
        // Do not process attack queue if dead
        if(state == State.Dead) return;
        //Cooldowns
        //for each item in dictionary
        cooldownKeys.Clear();
        cooldownKeys.AddRange(moveCooldowns.Keys);

        foreach (var key in cooldownKeys)
        {
            float time = moveCooldowns[key] - Time.deltaTime;

            if (time <= 0f)
                moveCooldowns.Remove(key);
            else
                moveCooldowns[key] = time;
        }

        if(attackQueueTimer > 0f)
        {
            if (waitForGround)
            {
                if(isGrounded) {
                    waitForGround = false; attackQueueTimer = 0f; vel = Vector3.zero; velocityTimeout = 0f;
                    var activeHitbox = transform.GetComponentInChildren<Hitbox>();
                    if(activeHitbox != null){activeHitbox.timer = 0; activeHitbox.gameObject.SetActive(false); Destroy(activeHitbox.gameObject);}
                }
            }

            attackQueueTimer -= 1;
        }
        else
        {
            if(attackQueue.Count == 0) //attack queue is finished
            {
                if(!attackQueueFinished){
                    attackQueueFinished = true;
                    armor = 1f;
                    comboStep = 0;
                    if(state != State.Hanging)
                        SetState(State.Idle);
                    canCancel = -1;
                    animator.Play("Running");
                }
            }
            else
            {
                attackQueueFinished = false;
                AttackAction thisAction = attackQueue[0];
                attackQueue.RemoveAt(0);
                attackQueueTimer = thisAction.duration;
                if(thisAction.state != State.Idle) SetState(thisAction.state);
                if(thisAction.anim != "")animator.Play(thisAction.anim,0,0f);
                thisAction.Execute(gameObject);
            }
        }
    }

    public void Lunge(Vector3 newVel, float duration)
    {
        vel.x = newVel.x;
        vel.z = newVel.z;
        velocityTimeout = duration;
        vel.y += newVel.y;
    }

    void TryAttack()
    {
        if(lastInput == GameplayInput.Attack)
        {
            var thisAttack = AttackActions.Instance.GetAttack(attackName);
            if(thisAttack == null) return;

            if(state == State.Idle || (state == State.FollowThrough && canCancel > -1 && thisAttack.cancelPriority > canCancel)){
                ResetInput();
                if(!moveCooldowns.ContainsKey(attackName))
                {
                    moveCooldowns[attackName] = thisAttack.cooldown;
                    QueueAttack(thisAttack.attackActions);
                    if(thisAttack.attackName != "Punch" && thisAttack.attackName != "PunchCombo")
                        comboStep = 0; //Only combo for punches
                    canCancel = -1;
                    //On Cooldown
                    return;
                }
            }
        }
    }

    void QueueAttack(List<AttackAction> attackActions)
    {
        attackQueue.Clear();
        attackQueueTimer = 0f;
        foreach(AttackAction a in attackActions)
        {
            attackQueue.Add(a);
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
                attackQueue.Clear();
                attackQueue.Add(new WaitAction{duration = 20 , state = State.FollowThrough});
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
