using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    [SerializeField] AudioSource audio;
    void Awake()
    {
        if(!audio) audio = GetComponent<AudioSource>();
        if(audio) audio.volume = GameSettings.sfxVolume;    
    }
}
