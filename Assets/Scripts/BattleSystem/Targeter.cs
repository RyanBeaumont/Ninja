using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class Targeter : MonoBehaviour
{
    TargetType targetType;
    bool initialized = false;
    bool targetDead = false;
    GameAction action;
    public List<Combatant> selectedTargets = new List<Combatant>();
    bool grapple = false;

    //Animation
    Transform currentTarget;
    float animTime = 0f;

    public float burstDuration = 0.25f;

    public float minScale = 0.8f;
    public float maxScale = 1.8f;

    public float minRotationSpeed = 60f;
    public float maxRotationSpeed = 720f;
    public Transform visual;

    public void Initialize(TargetType type, string prompt, GameAction action, bool targetDead = false)
    {
        targetType = type;
        initialized = true;
        this.action = action;
        this.targetDead = targetDead;
        if(this.action is GrappleDamageAction || this.action is SuplexDamageAction) grapple = true;
    }

    void Start()
    {
        if(targetType == TargetType.SingleEnemy)
        {
            BattleManager.Instance.SetPose(BattleManager.Instance.activeCombatant.transform, "", CameraAngle.lockOn, "");
        }
        else if(targetType == TargetType.AllEnemies)
        {
            Transform spawnPoint = GameObject.Find("BattleSetup/EnemySpawn").transform;
            BattleManager.Instance.SetPose(spawnPoint, "", CameraAngle.wideBehind, "");
        }
        else if(targetType == TargetType.SingleAlly || targetType == TargetType.Any)
        {
            Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
            BattleManager.Instance.SetPose(spawnPoint, "", CameraAngle.wideBehind, "");
        }
        else if(targetType == TargetType.AllAllies)
        {
            Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
            BattleManager.Instance.SetPose(spawnPoint, "", CameraAngle.wideBehind, "");
        }
        else if(targetType == TargetType.None)
        {
            Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
            BattleManager.Instance.SetPose(spawnPoint, "", CameraAngle.lowAngle, "");
        }
    }

    void Update()
    {
        if (!initialized) return;
        List<GameObject> candidates = new List<GameObject>();

    if (targetType == TargetType.None)
    {
        selectedTargets.Add(BattleManager.Instance.activeCombatant);
        EndSelection();
        return;
    }

    // Step 1: Gather base candidates
    if (targetType == TargetType.SingleEnemy)
    {
        candidates = GameObject.FindGameObjectsWithTag("Enemy").ToList();
    }
    else if (targetType == TargetType.SingleAlly)
    {
        candidates = GameObject.FindGameObjectsWithTag("PlayerCombatant").ToList();
    }
    else if (targetType == TargetType.Any)
    {
        candidates = BattleManager.Instance.combatants
            .Select(c => c.gameObject)
            .ToList();
    }

    // Step 2: Convert to Combatants and filter
    var filtered = candidates
        .Select(go => go.GetComponent<Combatant>())
        .Where(c => c != null)
        .Where(c => c.alive != targetDead) // if targetDead=true → alive must be false
        .Where(c => !grapple || c.HasStatusEffect("Off-Balance") != null || c.HasStatusEffect("Prone") != null)
        .Select(c => c.gameObject);
        // Passively move the targeter to the closest matching object to the mouse cursor

        if(filtered != null && filtered.Count() > 0){
            var cam = Camera.main;
            if(cam != null){
                var mousePos = Input.mousePosition;
                float bestDistSqr = float.MaxValue;
                Transform best = null;
                foreach(var go in filtered){
                    if(go == null) continue;
                    // only consider dead targets if targeting dead
                    if(!go.GetComponent<Combatant>().alive && targetDead == false) continue;
                    
                    var screenPos = cam.WorldToScreenPoint(go.transform.position);
                    // skip objects behind the camera
                    if(screenPos.z <= 0) continue;
                    var dx = screenPos.x - mousePos.x;
                    var dy = screenPos.y - mousePos.y;
                    var distSqr = dx*dx + dy*dy;
                    if(distSqr < bestDistSqr){
                        bestDistSqr = distSqr;
                        best = go.transform;
                    }
                }
                if(best != null){
                    // Detect target change
                    if (currentTarget != best)
                    {
                        currentTarget = best;
                        animTime = 0f; // restart animation
                    }

                    // Follow target
                    transform.position = best.position;

                    if(Input.GetMouseButtonDown(0)){
                        selectedTargets.Add(best.GetComponent<Combatant>());
                        
                        EndSelection();
                    }
                }else{
                    EndSelection();
                }

                // Animate reticle
                animTime += Time.deltaTime;
                float t = Mathf.Clamp01(animTime / burstDuration);

                // Ease out (fast → slow, big → small)
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                // Scale animation
                float scale = Mathf.Lerp(maxScale, minScale, eased);
                visual.transform.localScale = Vector3.one * scale;

                // Rotation animation
                float rotSpeed = Mathf.Lerp(maxRotationSpeed, minRotationSpeed, eased);
                visual.transform.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);
            }
        }
    

        //if(Input.GetMouseButtonDown(1)){

        //    EndSelection();
        //}

        void EndSelection(){
            
            if(action != null){
                if(targetType == TargetType.SingleEnemy)
                {
                    action.caller.SetTargetPosition(selectedTargets[0].transform.position + selectedTargets[0].transform.forward * 2f);
                    action.caller.PlayAnimation("FrontFlip");
                    BattleManager.Instance.SetPose(action.caller.transform, "", CameraAngle.behind, "");
                }
                BattleManager.Instance.SelectTargets(selectedTargets);
                BattleManager.Instance.actionQueue.Add(action);
                BattleManager.Instance.EndAction();
                if(BattleManager.Instance.currentTargets.Count == 1 && BattleManager.Instance.currentTargets[0].GetComponent<Combatant>() is EnemyJade j){j.PerformCounterAttack();}
            }
            Destroy(gameObject);
            BattleManager.Instance.ShowQuickTimeEvent();
        }
    }
}
