using UnityEngine;
using UnityEngine.InputSystem;
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
    public Transform ActiveTarget => currentTarget;
    float animTime = 0f;

    public float burstDuration = 0.25f;

    public float minScale = 0.8f;
    public float maxScale = 1.8f;

    public float minRotationSpeed = 60f;
    public float maxRotationSpeed = 720f;
    public Transform visual;
    private int selectedTargetIndex = 0;
    private float stickCooldown = 0f;
    private bool controllerMode = false;
    private InputAction dpadLeftAction;
    private InputAction dpadRightAction;
    private InputAction dpadUpAction;
    private InputAction dpadDownAction;
    private InputAction submitAction;
    private bool actionsInitialized = false;
    private Card card;

    public void Initialize(TargetType type, string prompt, GameAction action, bool targetDead = false, Card card = null)
    {
        targetType = type;
        initialized = true;
        this.action = action;
        this.targetDead = targetDead;
        this.card = card;

        if((this.action is GrappleDamageAction || this.action is SuplexDamageAction) && this.action.wildSwing == false) grapple = true;
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

        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        if (actionsInitialized || BattleManager.Instance?.inputActions == null)
            return;

        var asset = BattleManager.Instance.inputActions;
        dpadLeftAction = asset.FindAction("DpadLeft", false);
        dpadRightAction = asset.FindAction("DpadRight", false);
        dpadUpAction = asset.FindAction("DpadUp", false);
        dpadDownAction = asset.FindAction("DpadDown", false);
        submitAction = asset.FindAction("Submit", false);

        if (dpadLeftAction != null) dpadLeftAction.Enable();
        if (dpadRightAction != null) dpadRightAction.Enable();
        if (dpadUpAction != null) dpadUpAction.Enable();
        if (dpadDownAction != null) dpadDownAction.Enable();
        if (submitAction != null) submitAction.Enable();

        actionsInitialized = true;
    }

    void Update()
{
    if (!initialized) return;

        if (Input.GetButtonDown("Cancel"))
        {
            Cancel();
        }

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

    // Step 2: Filter candidates
    var filtered = candidates
        .Select(go => go.GetComponent<Combatant>())
        .Where(c => c != null)
        .Where(c => c.alive != targetDead)
        .Where(c => !grapple || c.HasStatusEffect("Off-Balance") != null || c.HasStatusEffect("Prone") != null)
        .Select(c => c.gameObject)
        .ToList();

    if (filtered.Count > 0)
    {
        var cam = Camera.main;

        if (cam != null)
        {
            // Sort targets left-to-right for controller navigation
            filtered = filtered
                .OrderBy(go => cam.WorldToScreenPoint(go.transform.position).x)
                .ToList();

            // Detect mouse movement
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
                Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
            {
                controllerMode = false;
            }

            // Controller navigation
            InitializeInputActions();
            float horizontal = 0f;
            if (dpadLeftAction != null) horizontal -= dpadLeftAction.ReadValue<float>();
            if (dpadRightAction != null) horizontal += dpadRightAction.ReadValue<float>();

            stickCooldown -= Time.deltaTime;

            if (stickCooldown <= 0f)
            {
                if (horizontal > 0.5f)
                {
                    selectedTargetIndex++;
                    controllerMode = true;
                    stickCooldown = 0.2f;
                }
                else if (horizontal < -0.5f)
                {
                    selectedTargetIndex--;
                    controllerMode = true;
                    stickCooldown = 0.2f;
                }
            }

            if (selectedTargetIndex < 0)
                selectedTargetIndex = filtered.Count - 1;

            if (selectedTargetIndex >= filtered.Count)
                selectedTargetIndex = 0;

            Transform best = null;

            if (controllerMode)
            {
                best = filtered[selectedTargetIndex].transform;
            }
            else
            {
                Vector3 mousePos = Input.mousePosition;

                float bestDistSqr = float.MaxValue;

                foreach (var go in filtered)
                {
                    if (go == null)
                        continue;

                    var screenPos = cam.WorldToScreenPoint(go.transform.position);

                    if (screenPos.z <= 0)
                        continue;

                    float dx = screenPos.x - mousePos.x;
                    float dy = screenPos.y - mousePos.y;

                    float distSqr = dx * dx + dy * dy;

                    if (distSqr < bestDistSqr)
                    {
                        bestDistSqr = distSqr;
                        best = go.transform;
                    }
                }

                if (best != null)
                {
                    selectedTargetIndex = filtered.IndexOf(best.gameObject);
                }
            }

            if (best != null)
            {
                // Detect target change
                if (currentTarget != best)
                {
                    currentTarget = best;
                    animTime = 0f;
                }

                // Follow target
                transform.position = best.position;

                // Confirm target
                if (Input.GetMouseButtonDown(0) ||
                    (submitAction != null && submitAction.triggered) ||
                    Input.GetButtonDown("Submit"))
                {
                    selectedTargets.Add(best.GetComponent<Combatant>());
                    EndSelection();
                }
            }
            else
            {
                EndSelection();
            }

            // Animate reticle
            animTime += Time.deltaTime;

            float t = Mathf.Clamp01(animTime / burstDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float scale = Mathf.Lerp(maxScale, minScale, eased);
            visual.transform.localScale = Vector3.one * scale;

            float rotSpeed = Mathf.Lerp(maxRotationSpeed, minRotationSpeed, eased);
            visual.transform.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);
        }
    }

    void Cancel()
        {
            
            //Go back to default camera angle
            BattleManager.Instance.SetPose(BattleManager.Instance.activeCombatant.transform, "", CameraAngle.behind, "");
            BattleManager.Instance.attacksRemaining += 1;
            BattleManager.Instance.clock = 0f;
            BattleManager.Instance.EndAction();
            Destroy(gameObject);
        }

    void EndSelection()
    {
        if (action != null)
        {
            if (targetType == TargetType.SingleEnemy)
            {
                action.caller.SetTargetPosition(
                    selectedTargets[0].transform.position +
                    selectedTargets[0].transform.forward * 2f);

                action.caller.PlayAnimation("FrontFlip");

                BattleManager.Instance.SetPose(
                    action.caller.transform,
                    "",
                    CameraAngle.behind,
                    "");
            }

            BattleManager.Instance.SelectTargets(selectedTargets);
            BattleManager.Instance.actionQueue.Add(action);
            BattleManager.Instance.EndAction();

            if (BattleManager.Instance.currentTargets.Count == 1 &&
                BattleManager.Instance.currentTargets[0].GetComponent<Combatant>() is EnemyJade j)
            {
                j.PerformCounterAttack();
            }
        }

        
        BattleManager.Instance.ConsumeCard(card);
        BattleManager.Instance.ShowQuickTimeEvent();
        Destroy(gameObject);
    }
}
}
