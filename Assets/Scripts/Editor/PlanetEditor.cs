using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(ProceduralPlanet))]
public class PlanetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ProceduralPlanet planet = (ProceduralPlanet)target;

        if(GUILayout.Button("Generate Planet"))
        {
            planet.Generate();
        }

    }
}
