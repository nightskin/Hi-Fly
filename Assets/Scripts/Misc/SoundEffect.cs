using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    [SerializeField] AudioSource audio;
    void Start()
    {
        if(!audio) audio = GetComponent<AudioSource>();
        if(audio) audio.volume = GameSettings.sfxVolume;    
    }
}
