using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Woods.Tools.Texture
{
    [CustomEditor(typeof(TextureData))]
    public class TextureDataEditor : Editor
    {
        PNGConverter PNGConverter = new();
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var textureData = (TextureData)target;
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Process"))
            {
                textureData.ProcessTexture(PNGConverter);

            }
            EditorGUILayout.EndVertical();


        }
    }
}