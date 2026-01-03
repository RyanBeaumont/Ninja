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

    public void Initialize(TargetType type, string prompt, GameAction action)
    {
        targetType = type;
        initialized = true;
        print("Targeter initialized for type: " + type.ToString());
        this.action = action;
    }

    void Update()
    {
        if (!initialized) return;

        var tag = "Enemy";
        if(targetType == TargetType.SingleEnemy) tag = "Enemy";
        else if(targetType == TargetType.SingleAlly) tag = "PlayerCombatant";
        
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
            if(targetType == TargetType.SingleEnemy)
            {
                action.caller.SetTargetPosition(selectedTargets[0].transform.position + selectedTargets[0].transform.forward * 2f);
            }
            BattleManager.Instance.SelectTargets(selectedTargets);
            BattleManager.Instance.actionQueue.Add(action);
            BattleManager.Instance.EndAction();
            Destroy(gameObject);
        }
    }
}
