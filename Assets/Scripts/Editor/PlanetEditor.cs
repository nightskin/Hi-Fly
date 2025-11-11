using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Planet planet = (Planet)target;

        if(GUILayout.Button("Generate"))
        {
            planet.Generate();
        }

    }
}
