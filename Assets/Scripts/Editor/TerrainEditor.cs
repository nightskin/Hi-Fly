using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TerrainGenerator level = (TerrainGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            level.Generate();
        }

    }
}
