using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class BotInput : MonoBehaviour
{
    Character character;
    Transform player;
    public float preferredRange = 4f;
    float moveCommitmentTimer = 0f;
    float attackCommitmentTimer = 0f;
    
    string queuedAttack = "Jab";
    GameplayInput queuedInput = GameplayInput.None;
    float inputTimer = 0f;
    public TMP_Text debugText;
    public bool active = false;

    //Parameters

    [SerializeField] string[] attackChains = new string[]{};
    [SerializeField] string[] pokes;
    [SerializeField] string[] defenseMoves; //wake up attakcs, block, jump, dodge
    string currentState = "Neutral";
    string[] currentCombo;

    public float clockSpeed = 0.1f;
    public float inputDelay = 0.15f;
    public float attackThreshold = 5f;
    public float distancePickiness = 6f; //1 meter off = 6 point penalty
    public float randomness = 2f;
    public float antiApproach = 3f;
    public float counterPoke = 3f;
    public float jumpIn = 0.2f; //Chance to jump in
    public float whiffPunish = 5f;
    

    void Start()
    {
        character = GetComponent<Character>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
       
        if(currentState == "Neutral") debugText.color = Color.white;
        if(currentState == "Offense") debugText.color = Color.red;
        if(currentState == "Defense") debugText.color = Color.blue;

        Move();

        if(!active) {debugText.color = Color.black; currentState = "Neutral"; return;}

        if(queuedInput == GameplayInput.None)
            TryAttack();
        
        //Wait for input delay and then send input to character
        if(queuedInput != GameplayInput.None)
        {
           inputTimer -= Time.deltaTime;
           if(inputTimer <= 0f) {
                character.SetInput(queuedInput, queuedAttack); 
                if(queuedInput == GameplayInput.BotBlock){
                    character.blockInput = true;
                    attackCommitmentTimer = 0.5f;
                }
                queuedInput = GameplayInput.None;
            }
        }
        
    }

    public void SetActiveBot(bool isActive){
        active = isActive;
    }

    public void OnHitOrBlock()
    {
        attackCommitmentTimer = 0f;
        var currentMovePriority = AttackActions.Instance.GetAttack(queuedAttack).cancelPriority;
        if(currentState == "Neutral")
        {
            //Choose a random attack chain, since your poke connected
            var randomChain = attackChains[Random.Range(0,attackChains.Length)];
            currentCombo = randomChain.Split(',');
            currentState = "Offense";
        }
        if(currentState == "Offense")
        {
            var firstHigherPriorityAttack = "";
            foreach(var nextAttack in currentCombo)
            {
                var nextMovePriority = AttackActions.Instance.GetAttack(nextAttack).cancelPriority;
                if(nextMovePriority > currentMovePriority)
                {
                    firstHigherPriorityAttack = nextAttack;
                    break;
                }
            }

            if(firstHigherPriorityAttack != "")
            {
                queuedInput = GameplayInput.Attack;
                queuedAttack = firstHigherPriorityAttack;
                inputTimer = inputDelay;
                return;
            }
            else
            {
                currentState = "Neutral";
            }
        }
    }
    
    void TryAttack(){

        // IMMEDIATE BLOCK RELEASE
        if (character.blockInput)
        {
            State playerState = player.GetComponent<Character>().state;

            bool playerThreatening =
                playerState == State.Windup ||
                playerState == State.Active;

            if (!playerThreatening)
            {
                character.blockInput = false;
                attackCommitmentTimer = 0f;
                currentState = "Neutral";
            }
        }

        if(attackCommitmentTimer > 0f)
        {
            attackCommitmentTimer -= Time.deltaTime;
            return;
        }else{
             State playerState = player.GetComponent<Character>().state;

        if(character.state == State.FollowThrough || character.state == State.Knockdown || character.state == State.Stunned || character.state == State.HardKnockdown || character.state == State.Launched)
            {
                print("Whiff");
                currentState = "Defense";
                return;
            }

        if(currentState == "Defense")
            {
                var randomMove = defenseMoves[Random.Range(0,defenseMoves.Length)];
                inputTimer = inputDelay;
                if(randomMove == "Block")
                {
                    queuedInput = GameplayInput.BotBlock;
                    attackCommitmentTimer = 1f; //hold block for 1 second
                }else if(randomMove == "Jump")
                {
                    character.SetInput(GameplayInput.Jump);
                }
                else
                {
                    queuedInput = GameplayInput.Attack;
                    queuedAttack = randomMove;
                    inputTimer = inputDelay;
                    return;
                }
            }

        if(currentState == "Neutral"){
            character.blockInput = false;
            attackCommitmentTimer = Random.Range(clockSpeed * 0.5f, clockSpeed * 1.5f);
            float bestScore = -Mathf.Infinity;
            string bestInput = "Jab";
            float dist = (player.position - transform.position).magnitude;
            //Score each input
            foreach(string poke in pokes){
                    float score = 10f;
                    Attack botAttack = AttackActions.Instance.GetAttack(poke);
                    //Distance score from 10(perfect) to 0(worst)
                    float error = Mathf.Abs(dist - botAttack.range);
                    score = Mathf.Clamp(score - error * distancePickiness,0f,10f);

                    //Player state based effects
                    Character playerCharacter = player.GetComponent<Character>();
                    if(playerState == State.Windup) score += counterPoke;
                    if(playerCharacter.moveVector.z > 0f) score += antiApproach;
                    if(playerState == State.Active || playerState == State.FollowThrough) score += whiffPunish;
                    score += Random.Range(-randomness,randomness);

                    //Move risk
                    //score += (moveSafety - GetThreatAtPosition(playerCharacter)) * safetyConcern;

                    //Top score?
                    if(score > bestScore)
                    {
                        bestScore = score;
                        bestInput = botAttack.attackName;
                        debugText.text = $"Chose {bestInput} with score {bestScore:F2}";
                    }
                //}
            }
            //if line of sight to player is blocked, dash sideways with high priority
            RaycastHit hit;
            Vector3 toPlayer = player.position - transform.position;    
            Physics.Raycast(transform.position, toPlayer, out hit);
            if(hit.collider != null && hit.collider.transform != player)
            {
                character.SetMotion(new Vector3(1f,0f,0f), toPlayer.normalized);
                character.SetInput(GameplayInput.Dash);
                return;
            }

            if(bestScore >= attackThreshold)
            {
                queuedInput = GameplayInput.Attack;
                queuedAttack = bestInput;
                inputTimer = inputDelay;
            }
            //Block if player is attacking
            //else if(playerState == State.Windup || playerState == State.Active)
            //{
            //    queuedInput = GameplayInput.BotBlock;
            //    inputTimer = inputDelay;
            //    attackCommitmentTimer = 0.5f;
            //}
        }
        }
    }

    float GetThreatAtPosition(Character character){
        //Simple threat calculation based on distance to player and whether they are attacking
        float threat = 0f;
        Vector3 toBot = transform.position - character.transform.position;
        float dist = toBot.magnitude;
        if(character.state == State.Active || character.state == State.FollowThrough)
        {
            threat += 1f; //High threat if attacking
        }
        threat += Mathf.Clamp01(1f - dist); //Closer = more threat
        return threat;
    }

    void Move(){
        if(moveCommitmentTimer > 0f)
        {
            moveCommitmentTimer -= Time.deltaTime;
            return;
        }else{
            Vector3 toPlayer = player.position - transform.position;
            Vector3 dirToPlayer = toPlayer.normalized;
            float distToPlayer = toPlayer.magnitude;
            Vector3 moveVector = Vector3.zero;

            if(distToPlayer > preferredRange * 1.2f)
            {
                moveVector.z = 1f; //Forward move
            }
            else if(distToPlayer < preferredRange * 0.8f)
            {
            moveVector.z = -1f; //Backward move
            }
            else
            {
            moveVector = Random.insideUnitCircle.normalized; //Strafe
            moveVector.y = 0f;
            }
            moveCommitmentTimer = Random.Range(0.25f,0.5f);

            //Smoothly rotate toward dirToPlayer
            character.SetMotion(moveVector, dirToPlayer);
        }
    }

}

