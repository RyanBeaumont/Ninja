using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogEncounter : ChainedInteractable
{
    public List<Dialog> dialog;
    public bool turnToFace = false;
    bool currentlyTurningToFace = false;
    public bool snapPlayerPosition = false;
    Quaternion originalRotation;
    int originalPose;
    float turnSpeed = 5f;
    Animator animator;
    List<GameObject> spawnedCharacters = new List<GameObject>();
    
    void Start(){
        originalRotation = transform.rotation;
        animator = GetComponent<Animator>();
        currentlyTurningToFace = true;
    }

    void Update()
    {
        if (turnToFace && currentlyTurningToFace)
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
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            float spacing = 1f; // distance between party members
            int count = YourParty.instance.partyMembers.Count - 1;
            float startOffset = -(count - 1) * 0.5f;
            
            //You can index party leader by saying party[0]
            foreach(Dialog dd in dialog)
            {
                if(dd.character == null && dd.name == $"party[0]"){dd.name = YourParty.instance.GetPartyMember(YourParty.instance.partyMembers[0]).memberName;}
                if(dd.character == null && dd.name.Equals(name)){dd.character = this.transform;}
            }

            if(animator != null)
                originalPose = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            DialogBox d = FindFirstObjectByType<DialogBox>();
            var characterController = player.GetComponent<CharacterController>();
            characterController.enabled = false;
            currentlyTurningToFace = false;
            if (snapPlayerPosition)
            {
                transform.rotation = originalRotation;
                player.position = transform.position + transform.forward * 1.5f;
            }
            Vector3 direction = transform.position - player.position;
            direction.y = 0f; // ignore vertical
            player.transform.rotation = Quaternion.LookRotation(direction);
            characterController.enabled = true;

            //Spawn other player objects
            if(YourParty.instance.partyMembers.Count > 1)
            {
                for(int i=1; i<=count; i++)
                {
                    var partyMember =  YourParty.instance.GetPartyMember(YourParty.instance.partyMembers[i]);
                    var modelToSpawn = partyMember.modelName;
                    GameObject thisPartyMember = Instantiate(Resources.Load<GameObject>($"Characters/{modelToSpawn}"));
                    float offsetIndex = startOffset + (i - 1);

                    Vector3 offset =  player.right * offsetIndex * spacing - player.forward * 1.5f + Vector3.down * 0.5f; // slightly behind player

                    thisPartyMember.transform.position = player.position + offset;
                    thisPartyMember.transform.rotation = player.rotation;
                    spawnedCharacters.Add(thisPartyMember);

                    //Replace dialog by name
                    foreach(Dialog dd in dialog)
                    {
                        if(dd.character == null && dd.name == partyMember.memberName) dd.character = thisPartyMember.transform;
                        //Replace dialog by index
                        if(dd.character == null && dd.name == $"party[{i}]"){
                            dd.name = partyMember.memberName;
                            dd.character = thisPartyMember.transform;
                        }
                    }

                    
                }
            }

            d.StartDialog(dialog);
            d.OnDialogFinished += OnDialogFinished;
        }
    }

     private void OnDialogFinished()
    {
        // Unsubscribe to avoid duplicate calls
        currentlyTurningToFace = true;
        var dialogBox = FindFirstObjectByType<DialogBox>();
        dialogBox.OnDialogFinished -= OnDialogFinished;
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        if(GetComponentInChildren<DefaultPose>() != null)
            GetComponentInChildren<DefaultPose>().PlayDefault();
        else if(animator != null)
            animator.Play(originalPose); //Default animation
        CallNext();
        foreach(GameObject g in spawnedCharacters) Destroy(g.gameObject);
        spawnedCharacters.Clear();
    }

}
