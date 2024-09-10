using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioClip[] playlist; 
    AudioSource music;
    
    void Start()
    {
        music = GetComponent<AudioSource>();
    }
    
}
