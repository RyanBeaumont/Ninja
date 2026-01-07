using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Transform cameraTransform;   // Assign Main Camera here
    Character character;
    float leftMouseDownTime = 0f;
    bool leftMouseHoldSent = false;
    public float leftMouseHoldThreshold = 0.25f; // seconds to consider a hold
    [SerializeField] string weapon = "None";


    void Start()
    {
        character = GetComponent<Character>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        if(cameraTransform != null)
        character.SetMotion(input,cameraTransform.forward);
        else
        cameraTransform = Camera.main.transform;

        if(Input.GetKeyDown(KeyCode.Space)){
            var controller = GetComponent<CharacterController>();
            // Allow jumping from a hanging state — treat Space as Jump when hanging
            if(character != null && character.state == State.Hanging)
            {
                character.SetInput(GameplayInput.Jump);
            }
            
            character.SetInput(GameplayInput.Jump);
     
        }
        /*
        if(Input.GetKey(KeyCode.LeftShift))
            character.SetInput(GameplayInput.Dash);
        if(Input.GetKey(KeyCode.Mouse1))
            character.blockInput = true; else character.blockInput = false;

        if(Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)) character.crouchInput = true; else character.crouchInput = false;

        if(Input.GetKeyDown(KeyCode.E)) character.SetInput(GameplayInput.Attack,"Jab");
        // Left mouse button: detect tap vs hold
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseDownTime = Time.time;
            leftMouseHoldSent = false;
        }

        if (Input.GetMouseButton(0))
        {
            // If held long enough and we haven't sent the hold action yet, send it once
            if (!leftMouseHoldSent && Time.time - leftMouseDownTime >= leftMouseHoldThreshold)
            {
                if(character.crouchInput)
                    character.SetInput(GameplayInput.Attack, "Sweep");
                else
                {
                    if(weapon == "None"){
                        character.SetInput(GameplayInput.Attack, "Kick");
                    }
                }
                leftMouseHoldSent = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // If we already sent the hold action, do nothing on release. Otherwise it's a tap.
            if (!leftMouseHoldSent)
            {
                if(character.crouchInput)
                    character.SetInput(GameplayInput.Attack, "Uppercut");
                else
                {
                    if(weapon == "None"){
                        if(character.comboStep == 0) character.SetInput(GameplayInput.Attack, "Punch");
                        else if(character.comboStep == 1) character.SetInput(GameplayInput.Attack, "PunchCombo");
                        else{character.SetInput(GameplayInput.Attack, "Kick");}
                    }
                }
            }
        }
        */
        
        
    }
}
