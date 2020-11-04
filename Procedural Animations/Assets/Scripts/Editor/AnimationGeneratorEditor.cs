using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AnimationGenerator))]
public class AnimationGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AnimationGenerator AnimGen = (AnimationGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate!"))
        {
            AnimGen.Generate();
        }
    }
}
