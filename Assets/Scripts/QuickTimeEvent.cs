using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public enum Arrow{Left, Right, Up, Down}

public class QuickTimeEvent : MonoBehaviour
{
    public string pattern; //A pattern where spaces indicate "rest", and numbers indicate which "note" prefab to spawn
    public float trackLength;
    public Transform hitZone;
    public FloatValue gameDifficulty;
    public float goodWindow = 0.1f;
    public float perfectWindow = 0.05f;
    float pitch = 0.9f;
    float accuracy = 0;
    float endTimer = 0.5f;
    float accuracyPerNote;
    public float beatLength = 0.5f; //each beat is 0.5s
    int currentIndex = 0;
    float clock = 0.5f;
    float mininumAccuracy = 0.5f; //If you fail every note you still get 50%
    bool active = false;

    List<GameObject> nextArrow = new List<GameObject>();

    public void Initialize(string newPattern)
    {
        AudioManager.Instance.PlaySoundEffect("NoiseSweeper");
        pattern = newPattern;
        pitch = 0.9f;
        
        currentIndex = 0;
        beatLength = 0.6f - (gameDifficulty.value * 0.15f); //beat length decreases as difficulty increases, minimum of 0.5s
        clock = beatLength;
        if(gameDifficulty.value >= 2f){mininumAccuracy = 0f;}
        if(gameDifficulty.value >= 1f){mininumAccuracy = .25f;}
        accuracy = mininumAccuracy;
        goodWindow = beatLength * 0.35f; //good window is 35% of the beat length
        perfectWindow = beatLength * 0.15f; //perfect window is 15
        active = true;
        //find total notes based on the number of non-space characters in the pattern, and calculate accuracy per note
        if (!string.IsNullOrEmpty(pattern))
        {
            int totalNotes = pattern.Count(c => c != ' ');
            if (totalNotes > 0)
            {
                accuracyPerNote = (100f - mininumAccuracy) / totalNotes;
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
                if(arrowPosition.x < hitZone.position.x - 200f){ //100f is arbitrary large number to ensure the note is well past the hit zone
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
                                var effect = Instantiate(Resources.Load<GameObject>("BadArrow"), thisArrow.transform.position, Quaternion.identity);
                                effect.transform.parent = transform;
                                effect.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/ArrowTorn");
                                effect = Instantiate(Resources.Load<GameObject>("BadArrow"), thisArrow.transform.position, Quaternion.identity);
                                effect.transform.parent = transform;
                                effect.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/ArrowTorn2");
                            }else if(result <= goodWindow){
                                accuracy += accuracyPerNote/2f;
                                AudioManager.Instance.PlaySoundEffect("SynthHit");
                                var effect = Instantiate(Resources.Load<GameObject>("BadArrow"), thisArrow.transform.position, Quaternion.identity);
                                effect.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/ArrowTorn");
                                effect.transform.parent = transform;
                                effect = Instantiate(Resources.Load<GameObject>("BadArrow"), thisArrow.transform.position, Quaternion.identity);
                                effect.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/ArrowTorn2");
                                effect.transform.parent = transform;
                            }
                            else
                            {
                                AudioManager.Instance.PlaySoundEffect("Negative");
                                GetComponent<UIShake>().Shake(10);
                                var effect = Instantiate(Resources.Load<GameObject>("BadArrow"), thisArrow.transform.position, Quaternion.identity);
                                effect.transform.parent = transform;
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
            endTimer -= Time.deltaTime;
            if(endTimer <= 0f){
                if(BattleManager.Instance != null){
                    if(accuracy == 100){accuracy = 125; GameManager.Instance.ShowMessage("<color=yellow>PERFECT! +25%</color>"); AudioManager.Instance.PlaySoundEffect("ChaChing",pitch);}
                    BattleManager.Instance.waitingForQuickTime = false;
                    BattleManager.Instance.quickTimeMultiplier = accuracy/100f;
                    BattleManager.Instance.clock = 0f;
                }
                
                //destroy all instances of BadArrow
                var badArrows = GameObject.FindObjectsByType<UIGravity>(FindObjectsSortMode.None);
                foreach(var arrow in badArrows)
                {
                    Destroy(arrow.gameObject);
                }
                active = false;
            }
        }
        
    }
}
