using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Transform cameraTransform;   // Assign Main Camera here
    Character character;

    void Start()
    {
        character = GetComponent<Character>();
    }

    void Update()
    {
        // Skip input handling during dialogs and cutscenes
        GameplayState currentState = GameManager.Instance.GetGameplayState();
        if(currentState != GameplayState.FreeMovement)
        {
            print("Player input ignored due to gameplay state: " + currentState);
            return;
        }


        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        if(cameraTransform != null)
            character.SetMotion(input, cameraTransform.forward);
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

        
        
    }
}
