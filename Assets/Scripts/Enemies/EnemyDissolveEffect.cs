using UnityEngine;

public class EnemyDissolveEffect : MonoBehaviour
{
    float t;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;

    void OnEnable()
    {
        if (!mesh && !skinnedMesh) return;


        t = 0;

        if (mesh)
        {
            for (int i = 0; i < mesh.materials.Length; i++)
            {
                mesh.materials[i].SetFloat("_Value", t);
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
    
    void Update()
    {
        if (!mesh && !skinnedMesh) return;

        if (t < 1)
        {
            t += Time.deltaTime;
        }
        else
        {
            this.enabled = false;
        }


        if (mesh)
        {
            for (int i = 0; i < mesh.materials.Length; i++)
            {
                mesh.materials[i].SetFloat("_Value", t);
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
