using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlanetGenerator planet = (PlanetGenerator)target;

        if(GUILayout.Button("Generate"))
        {
            planet.Generate();
        }

    }
}
