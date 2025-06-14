using UnityEngine;

public class ExpandContract : MonoBehaviour
{
    [SerializeField] float maxScale = 20;
    float t;

    void OnEnable()
    {
        t = 0;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.localScale = Vector3.one * (Mathf.Sin(t) * maxScale);
    }
}
