using UnityEngine;

public class Explosion : MonoBehaviour
{
    
    void Update()
    {
        if(!GetComponent<ParticleSystem>().isEmitting)
        {
            gameObject.SetActive(false);
        }
    }
}
