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

    public void Initialize(TargetType type, string prompt, GameAction action, bool targetDead = false)
    {
        targetType = type;
        initialized = true;
        this.action = action;
        this.targetDead = targetDead;
    }

    void Start()
    {
        if(targetType == TargetType.SingleEnemy)
        {
            BattleManager.Instance.SetPose(BattleManager.Instance.activeCombatant.transform, "", CameraAngle.behind, "");
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
            BattleManager.Instance.SetPose(spawnPoint, "", CameraAngle.wideBehind, "");
        }
    }

    void Update()
    {
        if (!initialized) return;

        var tag = "Enemy";
        if(targetType == TargetType.SingleEnemy){ tag = "Enemy";}
        else if(targetType == TargetType.SingleAlly) {tag = "PlayerCombatant"; 
        
        }
        
        else if(targetType == TargetType.None){selectedTargets.Add(BattleManager.Instance.activeCombatant); EndSelection(); return;}

        // Passively move the targeter to the closest matching object to the mouse cursor

        var candidates = GameObject.FindGameObjectsWithTag(tag);
        if(candidates != null && candidates.Length > 0){
            var cam = Camera.main;
            if(cam != null){
                var mousePos = Input.mousePosition;
                float bestDistSqr = float.MaxValue;
                Transform best = null;
                foreach(var go in candidates){
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
                    transform.position = best.position;

                    if(Input.GetMouseButtonDown(0)){
                        selectedTargets.Add(best.GetComponent<Combatant>());
                        
                        EndSelection();
                    }
                }else{
                    EndSelection();
                }
            }
        }
    

        if(Input.GetMouseButtonDown(1)){

            EndSelection();
        }

        void EndSelection(){
            BattleManager.Instance.ShowQuickTimeEvent();
            if(targetType == TargetType.SingleEnemy)
            {
                action.caller.SetTargetPosition(selectedTargets[0].transform.position + selectedTargets[0].transform.forward * 2f);
                action.caller.PlayAnimation("FrontFlip");
            }
            BattleManager.Instance.SelectTargets(selectedTargets);
            BattleManager.Instance.actionQueue.Add(action);
            BattleManager.Instance.EndAction();
            if(BattleManager.Instance.currentTargets.Count == 1 && BattleManager.Instance.currentTargets[0].GetComponent<Combatant>() is EnemyJade j){j.PerformCounterAttack();}
            Destroy(gameObject);
        }
    }
}
