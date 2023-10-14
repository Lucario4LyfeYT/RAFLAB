using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public class RAFLAB : EditorWindow
{
    //Mesh Variables
    private GameObject parentGameObject;
    private string MeshFolderPath = "Assets/RAFLAB/Avatar DATA/Meshes";
    private string copiedMeshSkipPath;

    //Material Variables
    private GameObject sourceGameObject;
    private DefaultAsset destinationFolder;
    private Dictionary<Material, Material> copiedMaterials = new Dictionary<Material, Material>();

    //Texture Variables
    private DefaultAsset sourceFolder; // Changed to DefaultAsset type
    private DefaultAsset saveFolder;   // Changed to DefaultAsset type
    private int append;

    //Avatar Copy Variables
    private GameObject sourceGameObject1;
    private DefaultAsset destinationFolder1;

    //Audio Variables
    private Transform parentObject;
    private DefaultAsset saveFolderPath;
    private List<int> copiedAudioClipInstanceIDs = new List<int>(); // Track copied audio clip instance IDs
    public int ii;

    private bool a = false;

    [MenuItem("Custom/RAFLAB V3")]
    public static void ShowWindow()
    {
        Credits credits = new Credits();
        credits.showWindow();
        GetWindow<RAFLAB>("RAFLAB V3");
    }

    private void OnGUI()
    {

        if (destinationFolder != null && sourceFolder != destinationFolder)
        {
            sourceFolder = destinationFolder;
        }

        GUILayout.Label("Coded by qkms and liamriley101", EditorStyles.boldLabel);

        GUILayout.Label("Mesh Copier V2:", EditorStyles.boldLabel);
        parentGameObject = EditorGUILayout.ObjectField(parentGameObject, typeof(GameObject), true) as GameObject;

        GUILayout.Space(10);

        if (GUILayout.Button("Select Destination Folder for copied meshes"))
        {
            string newFolderPath = EditorUtility.SaveFolderPanel("Select Destination Folder", "", "");
            if (!string.IsNullOrEmpty(newFolderPath))
            {
                MeshFolderPath = "Assets" + newFolderPath.Replace(Application.dataPath, "");
            }
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Path to copied meshes:", MeshFolderPath);

        GUILayout.Space(10);

        GUILayout.Label("Material Copier V2", EditorStyles.boldLabel);

        sourceGameObject = (GameObject)EditorGUILayout.ObjectField("Source GameObject", sourceGameObject, typeof(GameObject), true);
        destinationFolder = (DefaultAsset)EditorGUILayout.ObjectField("Destination Folder", destinationFolder, typeof(DefaultAsset), true);

        GUILayout.Space(10);

        GUILayout.Label("Texture Grabber V1", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Source Folder:");

        // Replaced the sourceFolderPath with a DefaultAsset field
        sourceFolder = EditorGUILayout.ObjectField(sourceFolder, typeof(DefaultAsset), false) as DefaultAsset;

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Save Folder:");

        // Replaced the saveFolderPath with a DefaultAsset field
        saveFolder = EditorGUILayout.ObjectField(saveFolder, typeof(DefaultAsset), false) as DefaultAsset;

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Copy Avatar from Animator V2", EditorStyles.boldLabel);
        sourceGameObject1 = EditorGUILayout.ObjectField("Source GameObject", sourceGameObject1, typeof(GameObject), true) as GameObject;
        destinationFolder1 = EditorGUILayout.ObjectField("Destination Folder", destinationFolder1, typeof(DefaultAsset), true) as DefaultAsset;

        GUILayout.Space(10);

        GUILayout.Label("Audio Capture V2", EditorStyles.boldLabel);

        parentObject = EditorGUILayout.ObjectField("Parent Object:", parentObject, typeof(Transform), true) as Transform;

        saveFolderPath = EditorGUILayout.ObjectField("Destination Folder", saveFolderPath, typeof(DefaultAsset), false) as DefaultAsset;
        
        if (GUILayout.Button("Recreate entire avatar"))
        {
            if (parentGameObject == null || MeshFolderPath == null || sourceGameObject == null || destinationFolder == null || sourceFolder == null || saveFolder == null || sourceGameObject1 == null || destinationFolder1 == null || parentObject == null || saveFolderPath == null)
            {
                Debug.LogError("Please setup all the properties before pressing this button.");
            }
            else
            {
                //reset values so they dont save for next time
                copiedMaterials.Clear();
                copiedAudioClipInstanceIDs.Clear();
                append = 0;
                ii = 0;
                CopyMeshes(); //Meshes
                CopyMaterialsFromChildren(); //Materials
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                a = true;
            }

        }
        if (a)
        {
            if (GUILayout.Button("Click when shaders are reapplied"))
            {
                //Textures
                string sourceFolderPath = AssetDatabase.GetAssetPath(sourceFolder);
                string[] materialPaths = Directory.GetFiles(sourceFolderPath, "*.mat");
                append = 0;
                foreach (string materialPath in materialPaths)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (material != null)
                    {
                        CaptureMaterialTextures(material);
                    }
                }
                CopyAvatar(); //Avatar
                CaptureAudioFromChildren(); //Audio
                parentGameObject = null;
                a = false;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Successfully Recreated Avatar");
            }
        }
        if (GUILayout.Button("Clear last recreated avatar"))
        {

            //Delete Mesh's
            string[] bGUIDS = AssetDatabase.FindAssets("t:Mesh", new[] { MeshFolderPath });
            foreach (string guid in bGUIDS)
            {
                string meshPath = AssetDatabase.GUIDToAssetPath(guid);
                File.Delete(meshPath);
            }

            //Delete Materials
            bGUIDS = AssetDatabase.FindAssets("t:Material", new[] { AssetDatabase.GetAssetPath(destinationFolder) });
            foreach (string guid in bGUIDS)
            {
                string matPaths = AssetDatabase.GUIDToAssetPath(guid);
                File.Delete(matPaths);
            }

            //Delete Textures
            bGUIDS = AssetDatabase.FindAssets("t:Texture", new[] { AssetDatabase.GetAssetPath(saveFolder) });
            foreach (string guid in bGUIDS)
            {
                string texPaths = AssetDatabase.GUIDToAssetPath(guid);
                File.Delete(texPaths);
            }

            //Delete Avatars
            bGUIDS = AssetDatabase.FindAssets("t:Avatar", new[] { AssetDatabase.GetAssetPath(destinationFolder1) });
            foreach (string guid in bGUIDS)
            {
                string aviPaths = AssetDatabase.GUIDToAssetPath(guid);
                File.Delete(aviPaths);
            }

            //Delete Audios
            bGUIDS = AssetDatabase.FindAssets("t:AudioClip", new[] { AssetDatabase.GetAssetPath(saveFolderPath) });
            foreach (string guid in bGUIDS)
            {
                string audPaths = AssetDatabase.GUIDToAssetPath(guid);
                File.Delete(audPaths);
            }
            AssetDatabase.Refresh();
        }
    }

    //Mesh Methods V2

    private void CopyMeshes()
    { 

        MeshFilter[] meshFilters = parentGameObject.GetComponentsInChildren<MeshFilter>(true);
        SkinnedMeshRenderer[] skinnedMeshRenderers = parentGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if ((meshFilters == null || meshFilters.Length == 0) && (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0))
        {
            Debug.LogError("No meshes found in the selected parent GameObject.");
            return;
        }


        int meshCount = 0;

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
                continue;

            if (MeshCompare(meshFilter.sharedMesh, "Assets/RAFLAB/Default Meshes"))
            {
                Debug.LogWarning("Skipping default asset: " + meshFilter.sharedMesh.name);
                continue;
            }
            if (MeshCompare(meshFilter.sharedMesh, MeshFolderPath))
            {
                Debug.LogWarning("Skipping Already Copied Mesh: " + meshFilter.sharedMesh.name);
                meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(copiedMeshSkipPath);
                continue;
            }


            string meshPath = MeshFolderPath + "/" + meshFilter.name + meshCount + ".asset";
            Mesh meshCopy = Instantiate(meshFilter.sharedMesh);
            AssetDatabase.CreateAsset(meshCopy, meshPath);
            meshFilter.sharedMesh = meshCopy;
            Debug.Log("Mesh copied and saved to: " + meshPath);

            meshCount++;
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh == null) //|| !uniqueMeshes.Add(skinnedMeshRenderer.sharedMesh))
                continue;

            if (MeshCompare(skinnedMeshRenderer.sharedMesh, "Assets/RAFLAB/Default Meshes"))
            {
                Debug.LogWarning("Skipping Default Asset: " + skinnedMeshRenderer.sharedMesh.name);
                continue;
            }
            if (MeshCompare(skinnedMeshRenderer.sharedMesh, MeshFolderPath))
            {
                Debug.LogWarning("Skipping Already Copied Mesh: " + skinnedMeshRenderer.sharedMesh.name);
                skinnedMeshRenderer.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(copiedMeshSkipPath);
                continue;
            }

            string meshPath = MeshFolderPath + "/" + skinnedMeshRenderer.name + meshCount + ".asset";

            Mesh meshCopy = Instantiate(skinnedMeshRenderer.sharedMesh);
            AssetDatabase.CreateAsset(meshCopy, meshPath);
            skinnedMeshRenderer.sharedMesh = meshCopy;
            Debug.Log("Mesh copied and saved to: " + meshPath);

            meshCount++;
        }
    }

    //Checks for default meshes/already copied meshes and does not create them if returns true
    private bool MeshCompare(Mesh mesh, string folderPath)
    {
        string[] defaultMeshPaths = AssetDatabase.FindAssets("t:Mesh", new[] { folderPath });
        Vector3[] targetVertices = mesh.vertices;

        foreach (var defaultMeshPath in defaultMeshPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(defaultMeshPath);
            Mesh defaultMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            Vector3[] defaultVertices = defaultMesh.vertices;

            if (AreVerticesEqual(defaultVertices, targetVertices))
            {
                copiedMeshSkipPath = assetPath;
                return true;
            }
        }
        return false;
    }

    //Compares the vertices of 2 meshes to see if they're the same
    private bool AreVerticesEqual(Vector3[] vertices1, Vector3[] vertices2)
    {
        if (vertices1.Length != vertices2.Length)
        {
            return false;
        }

        for (int i = 0; i < vertices1.Length; i++)
        {
            if (vertices1[i] != vertices2[i])
            {
                return false;
            }
        }

        return true;
    }

    //Material methods V2
    private void CopyMaterialsFromChildren()
    {
        if (sourceGameObject == null || destinationFolder == null)
        {
            Debug.LogError("Please select the source GameObject and destination folder.");
            return;
        }

        string destinationFolderPath = AssetDatabase.GetAssetPath(destinationFolder);
        if (!AssetDatabase.IsValidFolder(destinationFolderPath))
        {
            Debug.LogError("Please select a valid destination folder.");
            return;
        }

        copiedMaterials.Clear();

        TraverseChildrenAndCopyMaterials(sourceGameObject.transform, destinationFolderPath);

        Debug.Log("Materials copied successfully.");
    }

    private void TraverseChildrenAndCopyMaterials(Transform currentTransform, string destinationFolderPath)
    {
        Renderer renderer = currentTransform.GetComponent<Renderer>();
        ParticleSystem particleSystem = currentTransform.GetComponent<ParticleSystem>();

        if (renderer != null)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null)
                {
                    CopyMaterial(material, destinationFolderPath);

                    // Apply the copied material to the renderer
                    Material[] newMaterials = renderer.sharedMaterials;
                    newMaterials[i] = copiedMaterials[material];
                    renderer.sharedMaterials = newMaterials;
                }
            }
        }

        if (particleSystem != null)
        {
            var particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                var materials = new Material[particleSystem.trails.enabled ? 2 : 1];
                materials[0] = particleRenderer.sharedMaterial;
                if (particleSystem.trails.enabled)
                    materials[1] = particleRenderer.trailMaterial;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material != null)
                    {
                        CopyMaterial(material, destinationFolderPath);

                        // Apply the copied material to the renderer
                        Material[] newMaterials = particleRenderer.sharedMaterials;
                        newMaterials[i] = copiedMaterials[material];
                        particleRenderer.sharedMaterials = newMaterials;
                    }
                }
            }
        }

        // Traverse children
        foreach (Transform child in currentTransform)
        {
            TraverseChildrenAndCopyMaterials(child, destinationFolderPath);
        }
    }

    private void CopyMaterial(Material sourceMaterial, string destinationFolderPath)
    {

        if (sourceMaterial == null || copiedMaterials.ContainsKey(sourceMaterial))
            return;

        string destinationPath = destinationFolderPath + "/" + sourceMaterial.name + ".mat";

        // Check if a material with the same name already exists in the destination folder
        int duplicateCount = 1;
        while (System.IO.File.Exists(destinationPath))
        {
            string duplicateSuffix = "_" + duplicateCount.ToString();
            destinationPath = destinationFolderPath + "/" + sourceMaterial.name + duplicateSuffix + ".mat";
            duplicateCount++;
        }

        Material newMaterial = new Material(sourceMaterial.shader);
        newMaterial.CopyPropertiesFromMaterial(sourceMaterial);
        newMaterial.shader = sourceMaterial.shader; //New Line as of 10/11/2023)

        // Save the copied material
        AssetDatabase.CreateAsset(newMaterial, destinationPath);

        copiedMaterials.Add(sourceMaterial, newMaterial);
    }

    //Texture Methods
    private void CaptureMaterialTextures(Material material)
    {
        string[] propertyNames = material.GetTexturePropertyNames();

        for (int i = 0; i < propertyNames.Length; i++)
        {
            string propertyName = propertyNames[i];
            Texture texture = material.GetTexture(propertyName);

            if (texture != null)
            {
                // Convert the texture to Texture2D.
                Texture2D tex2D = TextureToTexture2D(texture);

                if (tex2D != null)
                {
                    // Save the captured image to the specified folder.
                    string saveFolderPath = AssetDatabase.GetAssetPath(saveFolder);
                    string fullPath = Path.Combine(saveFolderPath, append +  propertyName + ".png");
                    append++;
                    byte[] bytes = tex2D.EncodeToPNG();
                    File.WriteAllBytes(fullPath, bytes);
                    AssetDatabase.ImportAsset(fullPath);
                    Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
                    material.SetTexture(propertyName, newTexture);

                    Debug.Log("Texture captured and saved to: " + fullPath);
                }
            }
        }
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height);
        Graphics.Blit(texture, renderTexture);

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D tex2D = new Texture2D(texture.width, texture.height);
        tex2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex2D.Apply();

        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(renderTexture);

        return tex2D;
    }

    //Avatar Copy Methods V2
    private void CopyAvatar()
    {
        string folderPath = AssetDatabase.GetAssetPath(destinationFolder1);

        Animator[] animators = sourceGameObject1.GetComponentsInChildren<Animator>(true);

        if (animators.Length == 1)
        {
            if (animators[0].avatar == null) return;
            else
            {
                Avatar newAvatar = UnityEngine.Object.Instantiate(animators[0].avatar);
                string avatarName = animators[0].avatar.name + "_Avatar.asset";
                string avatarPath = folderPath + "/" + avatarName;
                AssetDatabase.CreateAsset(newAvatar, avatarPath);
                animators[0].avatar = newAvatar;
                return;
            }
        }

        HashSet<Avatar> avatarSet = new HashSet<Avatar>();

        //Setup the hashset
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i].avatar == null) continue;
            if (avatarSet.Contains(animators[i].avatar)) continue;
            avatarSet.Add(animators[i].avatar);
        }

        //Set HashSet to array
        Avatar[] avset = avatarSet.ToArray();

        //Scan through the avset array and set all animator.avatar's to the one in the array
        
        foreach (Animator animator in animators)
        {
            if (animator.avatar == null) continue;
            for (int i = 0; i < avset.Length; i++) 
                if (animator.avatar == avset[i]) animator.avatar = avset[i];
        }

        //Instantiate everything from the array
        for (int i = 0; i < avset.Length; i++)
        {
            Avatar newAvatar = UnityEngine.Object.Instantiate(avset[i]);
            string avatarName = avset[i].name + "_Avatar.asset";
            string avatarPath = folderPath + "/" + avatarName;
            AssetDatabase.CreateAsset(newAvatar, avatarPath);
            foreach(Animator animator in animators)
            {
                if (animator.avatar == null) continue;
                if (animator.avatar == avset[i]) animator.avatar = newAvatar;
            }
        }

        Debug.Log("Avatar copy completed!");
    }

//Audio Methods V2

private void CaptureAudioFromChildren()
    {
        if (parentObject != null)
        {
            AudioSource[] audioSources = parentObject.GetComponentsInChildren<AudioSource>(true);

            foreach (AudioSource childAudioSource in audioSources)
            {
                AudioClip audioClip = childAudioSource.clip;

                if (audioClip != null)
                {
                    int instanceID = audioClip.GetInstanceID();

                    if (!copiedAudioClipInstanceIDs.Contains(instanceID))
                    {
                        CaptureAudio(childAudioSource);
                        copiedAudioClipInstanceIDs.Add(instanceID);
                    }
                    else
                    {
                        Debug.Log("Audio clip with instance ID " + instanceID + " has already been copied.");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Please select a Parent Object.");
        }
    }

    private void CaptureAudio(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.clip != null && saveFolderPath != null)
        {
            string folderPath = AssetDatabase.GetAssetPath(saveFolderPath);
            string originalClipName = audioSource.clip.name;
            string fileName = originalClipName + ii + ".wav";
            ii++;
            string fullPath = Path.Combine(folderPath, fileName).Replace("\\", "/");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            AudioClip clip = audioSource.clip;
            float[] audioData = new float[clip.samples * clip.channels];
            clip.GetData(audioData, 0);
            String filePathfull = SavWav.Save(fileName, audioData, clip.samples, clip.channels, clip.frequency, folderPath);
            Debug.Log("Audio captured and saved to: " + fullPath);

        }
        else
        {
            Debug.LogError("Please select an Audio Source with a valid AudioClip and a save folder.");
        }
    }

    public static class SavWav
    {
        public static String Save(string filename, float[] audioData, int sampleCount, int channelCount, int frequency, string saveFolderPath)
        {
            if (!filename.ToLower().EndsWith(".wav"))
            {
                filename += ".wav";
            }

            var filepath = Path.Combine(saveFolderPath, filename);

            Debug.Log("Saving to: " + filepath);

            var fileStream = CreateEmpty(filepath);

            ConvertAndWrite(fileStream, audioData);

            WriteHeader(fileStream, sampleCount, channelCount, frequency);

            fileStream.Close();

            return filepath;
        }

        static FileStream CreateEmpty(string filepath)
        {
            var fileStream = new FileStream(filepath, FileMode.Create);
            byte emptyByte = new byte();

            for (int i = 0; i < 44; i++) // we need to write 44 bytes of silence at the beginning of the file to make it a valid wav file.
            {
                fileStream.WriteByte(emptyByte);
            }

            return fileStream;
        }

        static void ConvertAndWrite(FileStream fileStream, float[] audioData)
        {
            Int16[] intData = new Int16[audioData.Length];
            // Convert float to Int16
            for (int i = 0; i < audioData.Length; i++)
            {
                intData[i] = (short)(audioData[i] * 32767);
            }

            // Write to file
            byte[] bytesData = new byte[intData.Length * 2];
            Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
            fileStream.Write(bytesData, 0, bytesData.Length);
        }

        static void WriteHeader(FileStream fileStream, int sampleCount, int channelCount, int frequency)
        {
            var hz = frequency;
            var channels = channelCount;
            var samples = sampleCount;
            var length = samples * channels * 2; // 2 bytes per sample (16 bit)

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            byte[] chunkSize = BitConverter.GetBytes(length + 36);
            fileStream.Write(chunkSize, 0, 4);

            byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            byte[] subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);

            UInt16 one = 1;

            byte[] audioFormat = BitConverter.GetBytes(one);
            fileStream.Write(audioFormat, 0, 2);

            byte[] numChannels = BitConverter.GetBytes(channels);
            fileStream.Write(numChannels, 0, 2);

            byte[] sampleRate = BitConverter.GetBytes(hz);
            fileStream.Write(sampleRate, 0, 4);

            byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
            fileStream.Write(byteRate, 0, 4);

            UInt16 blockAlign = (ushort)(channels * 2);
            fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            byte[] bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(dataString, 0, 4);

            byte[] subChunk2 = BitConverter.GetBytes(length);
            fileStream.Write(subChunk2, 0, 4);
        }
    }
}