using UnityEngine;

public class EnemyDissolveEffect : MonoBehaviour
{
    float t;
    MeshRenderer renderer;

    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    void OnEnable()
    {
        gameObject.layer = 2;
        t = 0;
    }


    void Update()
    {
        if(t < 1)
        {
            t += Time.deltaTime;
        }
        else
        {
            gameObject.layer = 6;
            this.enabled = false;
        }

        for(int i = 0; i < renderer.materials.Length; i++) 
        {
            renderer.materials[i].SetFloat("_Value", t);
        }
        
    }
}
