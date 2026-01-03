using UnityEngine;

public class BlockBot : MonoBehaviour
{
    Character character;
    float blockTimeout = 0f;

    void Start()
    {
        character = GetComponent<Character>();
    }


    void Update()
    {
        if(character.state == State.Stunned)
        {
            blockTimeout = 1f;
        }

        if(blockTimeout > 0f)
        {
            character.blockInput = true;
            blockTimeout -= Time.deltaTime;
            if(blockTimeout <= 0f)
            {
                character.blockInput = false;
            }
        }
    }
    
}
