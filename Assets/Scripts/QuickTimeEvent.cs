using System.Collections.Generic;
using UnityEngine;

public enum NoteDirection
{
    Up,
    Down,
    Left,
    Right
}

[System.Serializable]
public struct Note
{
    public float time;          // Time in seconds (or beats)
    public NoteDirection dir;
}

public class QuickTimeEvent : MonoBehaviour
{
    public GameObject arrowPrefab;
    public List<Note> notes;
    int nextNoteIndex = 0;
    AudioSource audioSource;
    float spawnOffset = 1.5f; // seconds before hit

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float songTime = audioSource.time;

        while (nextNoteIndex < notes.Count &&
            notes[nextNoteIndex].time <= songTime + spawnOffset)
        {
            //SpawnArrow(notes[nextNoteIndex]);
            nextNoteIndex++;
        }
    }
}
