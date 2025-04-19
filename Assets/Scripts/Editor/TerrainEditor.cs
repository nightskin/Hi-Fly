using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TerrainGenerator island = (TerrainGenerator)target;

        if(GUILayout.Button("Generate"))
        {
            island.Generate();
        }

    }
}
