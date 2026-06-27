using UnityEngine;

public class Explosion : MonoBehaviour
{ 
    [SerializeField] ParticleSystem particleSystem;

    void FixedUpdate()
    {
        if(!particleSystem.isEmitting) gameObject.SetActive(false);
    }
}
