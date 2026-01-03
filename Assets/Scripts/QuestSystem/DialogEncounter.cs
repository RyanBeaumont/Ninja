using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogEncounter : ChainedInteractable
{
    public List<Dialog> dialog;
    public bool turnToFace = false;
    public bool snapPlayerPosition = false;
    Quaternion originalRotation;
    int originalPose;
    float turnSpeed = 5f;
    Animator animator;
    
    void Start(){
        originalRotation = transform.rotation;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (turnToFace)
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;

            // Only rotate if close enough
            if (Vector3.Distance(transform.position, player.position) < 4f)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0f; // ignore vertical
                if (direction.sqrMagnitude < 0.0001f) return;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                 // tweak as needed
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * turnSpeed
                );
            }else
            {
                // --- ROTATE BACK TO ORIGINAL ---
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    originalRotation,
                    Time.deltaTime * turnSpeed
                );
            }
        }
    }

    public override void Interact()
    {
        if (active)
        {
            if(animator != null)
                originalPose = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            DialogBox d = FindFirstObjectByType<DialogBox>();
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            var characterController = player.GetComponent<CharacterController>();
            characterController.enabled = false;
            if (snapPlayerPosition)
            {
                player.position = transform.position + originalRotation * Vector3.forward * 1.5f;
            }
            Vector3 direction = transform.position - player.position;
                direction.y = 0f; // ignore vertical
            player.transform.rotation = Quaternion.LookRotation(direction);
            characterController.enabled = true;
            d.StartDialog(dialog);
            d.OnDialogFinished += OnDialogFinished;
        }
    }

     private void OnDialogFinished()
    {
        // Unsubscribe to avoid duplicate calls
        var dialogBox = FindFirstObjectByType<DialogBox>();
        dialogBox.OnDialogFinished -= OnDialogFinished;
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        if(GetComponent<DefaultPose>() != null)
            GetComponent<DefaultPose>().PlayDefault();
        else if(animator != null)
            animator.Play(originalPose); //Default animation
        CallNext();
    }

}
