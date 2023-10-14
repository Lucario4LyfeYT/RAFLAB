using UnityEngine;
using UnityEditor;


public class Credits : EditorWindow
{
    internal void showWindow()
    {
        GetWindow<Credits>("Credits");
    }

    private void OnGUI()
    {
        GUILayout.Label("Coded by qkms and liamriley101", EditorStyles.boldLabel);
        GUILayout.Label("Mesh Copier - qkms", EditorStyles.boldLabel);
        GUILayout.Label("Material Copier - qkms", EditorStyles.boldLabel);
        GUILayout.Label("Avatar Grabber - qkms", EditorStyles.boldLabel);
        GUILayout.Label("Texture grabber - liamriley101 (modified by qkms)", EditorStyles.boldLabel);
        GUILayout.Label("Audio Capture - liamriley101 (modified by qkms)", EditorStyles.boldLabel);
        GUILayout.Label("RAFLAB (Recreate Avatar From Loaded Asset Bundle)", EditorStyles.boldLabel);
        GUILayout.Label("is a tool that allows you to take a loaded asset", EditorStyles.boldLabel);
        GUILayout.Label("and recreate it into a working project state", EditorStyles.boldLabel);

    }
}