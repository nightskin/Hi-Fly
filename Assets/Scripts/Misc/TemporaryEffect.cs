using UnityEngine;
public class TemporaryEffect : MonoBehaviour
{
    [SerializeField][Min(0)] float duration = 1.0f;

    float timer;

    void OnEnable()
    {
        timer = duration;
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
