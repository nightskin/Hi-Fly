using UnityEngine;

public class EnemyDissolveEffect : MonoBehaviour
{
    float t;
    [SerializeField] MeshRenderer renderer;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;

    void OnEnable()
    {
        t = 0;
    }
    
    void Update()
    {
        if (!renderer && !skinnedMesh) return;

        if (t < 1)
        {
            t += Time.deltaTime;
        }
        else
        {
            this.enabled = false;
        }


        if (renderer)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials[i].SetFloat("_Value", t);
            }
        }
        else if (skinnedMesh)
        {
            for (int i = 0; i < skinnedMesh.materials.Length; i++)
            {
                skinnedMesh.materials[i].SetFloat("_Value", t);
            }
        }
    }
}
