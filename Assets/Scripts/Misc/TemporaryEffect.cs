using UnityEngine;
public class TemporaryEffect : MonoBehaviour
{
    [SerializeField][Min(0)] float durationInSeconds = 1.0f;

    float timer;

    void OnEnable()
    {
        timer = durationInSeconds;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            gameObject.SetActive(false);
        }
    }

}
