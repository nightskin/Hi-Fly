using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    void Awake()
    {
        GetComponent<AudioSource>().volume = SoundManager.sfxVolume;    
    }
}
