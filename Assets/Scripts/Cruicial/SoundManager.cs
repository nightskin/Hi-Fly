using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioSource audioSource;
    public static float sfxVolume = 0.5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = sfxVolume;
    }

}
