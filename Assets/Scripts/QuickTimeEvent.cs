using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Arrow{Left, Right, Up, Down}

public class QuickTimeEvent : MonoBehaviour
{
    public string pattern; //A pattern where spaces indicate "rest", and numbers indicate which "note" prefab to spawn
    public float trackLength;
    public Transform hitZone;
    public float goodWindow = 0.1f;
    public float perfectWindow = 0.05f;
    float pitch = 0.9f;
    float accuracy = 0;
    float accuracyPerNote;
    public float beatLength = 0.5f; //each beat is 0.5s
    int currentIndex = 0;
    float clock = 0.5f;
    bool active = false;

    List<GameObject> nextArrow = new List<GameObject>();

    public void Initialize(string newPattern)
    {
        AudioManager.Instance.PlaySoundEffect("NoiseSweeper");
        pattern = newPattern;
        pitch = 0.9f;
        accuracy = 0;
        currentIndex = 0;
        clock = 0.5f;
        active = true;
        //find total notes based on the number of non-space characters in the pattern, and calculate accuracy per note
        if (!string.IsNullOrEmpty(pattern))
        {
            int totalNotes = pattern.Count(c => c != ' ');
            if (totalNotes > 0)
            {
                accuracyPerNote = 100f / totalNotes;
            }
            else
            {
                accuracyPerNote = 0f; // No notes to hit
            }
        }
        else
        {
            accuracyPerNote = 0f; // Empty pattern
        }
    }

    void Update()
    {
        if(!active) return;
        if(clock > 0f) clock -= Time.deltaTime;
        else
        {
            clock += beatLength;

            //Spawn note - only if we haven't reached the end of the pattern
            if (currentIndex < pattern.Length)
            {
                char note = pattern[currentIndex];
                if(note != ' ')
                {
                    var notePrefab = Resources.Load<GameObject>($"Note{note}");
                    if(notePrefab != null){
                        AudioManager.Instance.PlaySoundEffect("HitRattle");
                        var thisNote = Instantiate(notePrefab,transform);
                        thisNote.transform.position += new Vector3(trackLength,0f);
                        nextArrow.Add(thisNote);
                    }
                    
                }
                currentIndex ++;
            }
        }


        if(nextArrow.Count > 0){

            //Destroy the arrow if it's 100 past the hit zone
            var currentArrow = nextArrow[0];
            if (currentArrow != null) // Check if object still exists
            {
                
                var arrowPosition = currentArrow.transform.position;
                if(arrowPosition.x < hitZone.position.x - 100f){ //100f is arbitrary large number to ensure the note is well past the hit zone
                    Destroy(currentArrow);
                    nextArrow.RemoveAt(0);
                }
                
            }
            else
            {
                // Object was destroyed elsewhere, remove from list
                nextArrow.RemoveAt(0);
            }

            // Only process input if we still have arrows after cleanup
            if (nextArrow.Count > 0)
            {
                bool pressed = false;
                float result = 100f;
                var firstArrow = nextArrow[0];
                
                if (firstArrow != null) // Double-check the object exists
                {
                    ArrowMovement thisArrow = firstArrow.GetComponent<ArrowMovement>();
                    if (thisArrow != null) // Check component exists
                    {
                        if(Input.GetKeyDown(KeyCode.W)){ result = thisArrow.ProcessHit(Arrow.Up); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.A)){ result = thisArrow.ProcessHit(Arrow.Left); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.S)){ result = thisArrow.ProcessHit(Arrow.Down); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.D)){ result = thisArrow.ProcessHit(Arrow.Right); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.UpArrow)){ result = thisArrow.ProcessHit(Arrow.Up); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.LeftArrow)){ result = thisArrow.ProcessHit(Arrow.Left); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.RightArrow)){ result = thisArrow.ProcessHit(Arrow.Right); pressed = true;}
                        if(Input.GetKeyDown(KeyCode.DownArrow)){ result = thisArrow.ProcessHit(Arrow.Down); pressed = true;}

                        if(pressed){
                            if(result <= perfectWindow){
                                accuracy += accuracyPerNote;
                                AudioManager.Instance.PlaySoundEffect("StrongPunch",pitch);
                                AudioManager.Instance.PlaySoundEffect("Energy",pitch);
                                pitch += 0.1f;
                            }else if(result <= goodWindow){
                                accuracy += accuracyPerNote/2f;
                                AudioManager.Instance.PlaySoundEffect("SynthHit");
                            }
                            else
                            {
                                AudioManager.Instance.PlaySoundEffect("Negative");
                                GetComponent<UIShake>().Shake(10);
                            }
                            Destroy(thisArrow.gameObject);
                            nextArrow.RemoveAt(0);
                        }
                    }
                }
            }
        }
        
        //check if we are done with the pattern and have no more arrows on screen, then calculate final accuracy and apply multiplier
        if(currentIndex >= pattern.Length && nextArrow.Count == 0)
        {
            if(BattleManager.Instance != null){
                if(accuracy == 100){accuracy = 125; GameManager.Instance.ShowMessage("PERFECT! +25%"); AudioManager.Instance.PlaySoundEffect("ChaChing",pitch);}
                BattleManager.Instance.waitingForQuickTime = false;
                BattleManager.Instance.quickTimeMultiplier = accuracy/100f;
                BattleManager.Instance.clock = 0f;
            }
            active = false;
        }
        
    }
}
