using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using System.Collections.Generic;
[System.Serializable] public class CombatBeat
{
    public Transform attacker;
    public Transform defender;
    public string animation = "Punch";
    public string hitPose = "Hit";
    public float delay = 0.3f;
    public string attackSound = "s_whoosh";
    public string sound = "s_punch";
    public bool reversed = false;
}

public class CombatCutscene : ChainedInteractable
{
    public CombatBeat[] combatBeats;
    public Transform cameraSource;
    GameObject cameraRig;
    CinemachineCamera cutsceneCamera;
    Animator cameraAnimator;
    Vector3 originalPosition;
    Quaternion originalRotation;
    float waitClock = 0f;
    bool cutsceneActive = false;
    bool waitingForHit = false;
    int currentIndex = 0;
    CombatBeat thisBeat;

    public override void Interact()
    {
        if (active)
        {
            GameManager.Instance.SetGameplayState(GameplayState.Dialog);
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            var defaultPose = player.GetComponentInChildren<DefaultPose>();
            if(defaultPose){defaultPose.PlayDefault();} else{player.GetComponentInChildren<Animator>().Play("ArmsCrossed");}

            if(combatBeats.Length > 0)
            {
                originalPosition = player.Find("Model").position;
                originalRotation = player.Find("Model").rotation;
                cutsceneActive = true;
                waitClock = 0f;
                currentIndex = 0;
            } 

            if(cameraSource == null)
            {
                cameraSource = GameObject.FindGameObjectWithTag("Player").transform;
                Debug.Log("Defaulting to player camera source");
            }

            if(cameraSource != null)
            {
                cameraRig = GameManager.Instance.GetCamera(out cameraAnimator, out cutsceneCamera);
                cameraRig.transform.SetParent(cameraSource);
                cameraRig.transform.localPosition = Vector3.zero;
                cameraAnimator.Play("Camera_Behind");
            }
        }
    }

    void Update()
    {
        if (cutsceneActive)
        {
            if (cutsceneCamera != null)
            {
                //point the camera at the center of the combatants
                Vector3 centerPoint = Vector3.zero;
                int count = 0;
                foreach(var beat in combatBeats)            {
                    if(beat.attacker != null) {centerPoint += beat.attacker.position; count++;}
                    if(beat.defender != null) {centerPoint += beat.defender.position; count++;}
                }              centerPoint /= count;
                cameraRig.transform.LookAt(centerPoint);
            }

            //Automatically return to fighting idle animation
            if(thisBeat != null && currentIndex < combatBeats.Length)
            {
                GetCombatants(thisBeat,out var attacker, out var defender);
                
               
                if(attacker != null && defender != null)
                {
                    var defenderDefaultPose = defender.GetComponent<DefaultPose>()?.combatIdle ?? "FightingIdle";
                    var attackerDefaultPose = attacker.GetComponent<DefaultPose>()?.combatIdle ?? "FightingIdle";
                    if(!waitingForHit && attacker.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName(thisBeat.animation) && attacker.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        attacker.GetComponentInChildren<Animator>().Play(attackerDefaultPose);
                    }
                    if(!waitingForHit && defender.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName(thisBeat.hitPose) && defender.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        defender.GetComponentInChildren<Animator>().Play(defenderDefaultPose);
                    }
                }
            }

            //Wait for timer to expire
            if(waitingForHit) return;
            if(waitClock > 0f) {waitClock -= Time.deltaTime; return;}
            //Final wait has ended
            if(currentIndex >= combatBeats.Length){
                cutsceneActive = false; 
                var player = GameObject.FindGameObjectWithTag("Player").transform;
                //player.GetComponent<CharacterController>().enabled = false;
                //player.position = originalPosition; player.rotation = originalRotation;
                //player.GetComponent<CharacterController>().enabled = true;
                cutsceneCamera.transform.localRotation = Quaternion.identity;
                GameManager.Instance.DestroyCamera();
                CallNext(); 
                return;
            }
            else
            {
                thisBeat = combatBeats[currentIndex];
                currentIndex ++;
                GetCombatants(thisBeat,out var attacker, out var defender);
                
                if(thisBeat != null && attacker != null && defender != null)
                {
                    waitingForHit = true;
                    attacker.GetComponentInChildren<Animator>().Play(thisBeat.animation);
                    AudioManager.Instance.PlaySoundEffect(thisBeat.attackSound);
                    waitClock = thisBeat.delay + 0.4f;
                    //position attacker facing the defender 2m away
                    //what if defaultPose is in the child object?
                    Vector3 direction = (defender.position - attacker.position).normalized;
                    attacker.position = defender.position - direction * 2f;
                    attacker.LookAt(defender.position);

                }
            }
        }
    }

    public void GetCombatants(CombatBeat thisBeat, out Transform attacker, out Transform defender)
    {
        attacker = null;
        defender = null;

        if (thisBeat == null) return;
        if(thisBeat.attacker != null) attacker = thisBeat.attacker;
        if(thisBeat.defender != null) defender = thisBeat.defender;
        var player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (attacker == null)
                attacker = player.transform;

            if (defender == null)
                defender = player.transform;
        }

        if (thisBeat.reversed)
        {
            (attacker, defender) = (defender, attacker); // cleaner swap
        }
    }

    public void OnHit()
    {
        if(!waitingForHit) return;
        waitingForHit = false;
        GetCombatants(thisBeat,out var attacker, out var defender);
        if(thisBeat != null && attacker != null && defender != null)
        {
            AudioManager.Instance.PlaySoundEffect(thisBeat.sound);
            defender.transform.LookAt(attacker.transform);
            defender.GetComponentInChildren<Animator>().Play(thisBeat.hitPose);
            var effect = Instantiate(Resources.Load<GameObject>("Particles/Hit"), defender.transform);
            CameraShake(2f,0.2f);
        }
    }

    public void CameraShake(float intensity, float duration)
    {
       //camera shake using cinemachine
         var impulseSource = cameraRig.GetComponent<CinemachineImpulseSource>();
        if(impulseSource != null)        {
            impulseSource.GenerateImpulse(intensity);  
        }
    }

}
