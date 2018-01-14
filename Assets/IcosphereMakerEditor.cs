using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IcosphereMaker))]
public class IcosphereMakerEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        IcosphereMaker myScript = (IcosphereMaker)target;
        if(GUILayout.Button("Build Mesh"))
        {
            myScript.MakeIcoMesh();
        }
    }

}
