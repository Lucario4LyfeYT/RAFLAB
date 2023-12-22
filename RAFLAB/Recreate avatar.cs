#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RAFLAB : EditorWindow
{
    //Parent Gameobject
    private GameObject parentGameObject;

    //Mesh Variables
    private DefaultAsset MeshFolderPath;
    private List<Mesh> meshesnShit = new List<Mesh>();

    //Material Variables
    private DefaultAsset destinationFolder;
    private Dictionary<Material, Material> copiedMaterials = new Dictionary<Material, Material>(); //YES THIS IS NEEDED STOP TRYING TO DELETE IT DUMBASS (past qkms)

    //Texture Variables
    private DefaultAsset sourceFolder; // Changed to DefaultAsset type
    private DefaultAsset saveFolder;   // Changed to DefaultAsset type
    private int append;

    //Avatar Copy Variables
    private DefaultAsset destinationFolder1;

    //Audio Variables
    private DefaultAsset saveFolderPath;
    private List<int> copiedAudioClipInstanceIDs = new List<int>(); // Track copied audio clip instance IDs
    public int ii;

    private bool a = false;

    [MenuItem("Custom/RAFLAB V5")]
    public static void ShowWindow()
    {
        Credits credits = new Credits();
        credits.showWindow();
        GetWindow<RAFLAB>("RAFLAB V5");
    }

    private void OnGUI()
    {
        GUILayout.Label("Coded by qkms and liamriley101", EditorStyles.boldLabel);
        parentGameObject = EditorGUILayout.ObjectField("Parent Object", parentGameObject, typeof(GameObject), true) as GameObject;
        DisplayMeshCopierSection();
        DisplayMaterialCopierSection();
        DisplayTextureGrabberSection();
        DisplayAvatarCopySection();
        DisplayAudioCaptureSection();
        if (GUILayout.Button("Recreate entire avatar"))
            HandleRecreateAvatar();
        if (a && GUILayout.Button("Click when shaders are reapplied"))
            HandleShadersReapplied();
        if (GUILayout.Button("Clear last recreated avatar"))
            ClearLastRecreatedAvatar();
    }

    private void DisplayMeshCopierSection()
    {
        GUILayout.Label("Mesh Copier V4:", EditorStyles.boldLabel);
        MeshFolderPath = (DefaultAsset)EditorGUILayout.ObjectField("Destination Folder", MeshFolderPath, typeof(DefaultAsset), true);
        GUILayout.Space(10);
    }

    private void DisplayMaterialCopierSection()
    {
        GUILayout.Label("Material Copier V2", EditorStyles.boldLabel);
        sourceFolder = destinationFolder = (DefaultAsset)EditorGUILayout.ObjectField("Destination Folder", destinationFolder, typeof(DefaultAsset), true);
        GUILayout.Space(10);
    }

    private void DisplayTextureGrabberSection()
    {
        GUILayout.Label("Texture Grabber V3", EditorStyles.boldLabel);
        sourceFolder = EditorGUILayout.ObjectField("Source Folder:", sourceFolder, typeof(DefaultAsset), false) as DefaultAsset;
        saveFolder = EditorGUILayout.ObjectField("Save Folder:", saveFolder, typeof(DefaultAsset), false) as DefaultAsset;
        GUILayout.Space(10);
    }

    private void DisplayAvatarCopySection()
    {
        GUILayout.Label("Copy Avatar from Animator V3", EditorStyles.boldLabel);
        destinationFolder1 = EditorGUILayout.ObjectField("Destination Folder", destinationFolder1, typeof(DefaultAsset), true) as DefaultAsset;
        GUILayout.Space(10);
    }

    private void DisplayAudioCaptureSection()
    {
        GUILayout.Label("Audio Capture V2", EditorStyles.boldLabel);
        saveFolderPath = EditorGUILayout.ObjectField("Destination Folder", saveFolderPath, typeof(DefaultAsset), false) as DefaultAsset;
    }

    private void HandleRecreateAvatar()
    {
        if (parentGameObject == null || MeshFolderPath == null || destinationFolder == null || sourceFolder == null || saveFolder == null || destinationFolder1 == null || saveFolderPath == null)
            Debug.LogError("Please setup all the properties before pressing this button.");
        else
        {
            ResetValues();
            StartAssetCopying();
            AssetDatabase.Refresh();
            a = true;
        }
    }

    private void ResetValues()
    {
        copiedMaterials.Clear();
        copiedAudioClipInstanceIDs.Clear();
        meshesnShit.Clear();
        append = 0;
        ii = 0;
        a = false;
}

    private void StartAssetCopying()
    {
        CopyMeshes();
        CopyMaterialsFromChildren();
        CopyAvatar();
    }

    private void HandleShadersReapplied()
    {
        ProcessTextures();
        CaptureAudioFromChildren();
        FinalizeAvatarRecreation();
    }

    private void ProcessTextures()
    {
        string sourceFolderPath = AssetDatabase.GetAssetPath(sourceFolder);
        string[] materialPaths = Directory.GetFiles(sourceFolderPath, "*.mat", SearchOption.AllDirectories);
        append = 0;
        foreach (string materialPath in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
                CaptureMaterialTextures(material);
        }
    }

    private void FinalizeAvatarRecreation()
    {
        parentGameObject = null;
        a = false;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Successfully Recreated Avatar");
    }

    private void ClearLastRecreatedAvatar()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Mesh", new[] { AssetDatabase.GetAssetPath(MeshFolderPath) }))
            File.Delete(AssetDatabase.GUIDToAssetPath(guid));

        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { AssetDatabase.GetAssetPath(destinationFolder) }))
            File.Delete(AssetDatabase.GUIDToAssetPath(guid));

        foreach (string guid in AssetDatabase.FindAssets("t:Texture", new[] { AssetDatabase.GetAssetPath(saveFolder) }))
            File.Delete(AssetDatabase.GUIDToAssetPath(guid));

        foreach (string guid in AssetDatabase.FindAssets("t:Avatar", new[] { AssetDatabase.GetAssetPath(destinationFolder1) }))
            File.Delete(AssetDatabase.GUIDToAssetPath(guid));

        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AssetDatabase.GetAssetPath(saveFolderPath) }))
            File.Delete(AssetDatabase.GUIDToAssetPath(guid));
        AssetDatabase.Refresh();

    }

    //Mesh Methods V4

    private void CopyMeshes()
    {
        var allMeshRenderers = parentGameObject.GetComponentsInChildren<Renderer>(true)
            .Where(r => r is MeshRenderer || r is SkinnedMeshRenderer)
            .ToArray();

        if (allMeshRenderers.Length == 0)
        {
            Debug.LogError("No meshes found in the selected parent GameObject.");
            return;
        }

        foreach (var renderer in allMeshRenderers)
        {
            Mesh sharedMesh = GetSharedMesh(renderer);
            int meshIndex = MeshCompare(sharedMesh);

            if (meshIndex >= 0)
                AssignMeshToRenderer(renderer, meshesnShit[meshIndex]);
            else
            {
                Mesh newMesh = Instantiate(sharedMesh);
                meshesnShit.Add(newMesh);
                AssignMeshToRenderer(renderer, newMesh);
            }
        }

        // Save each mesh as an asset
        for (int i = 0; i < meshesnShit.Count; i++)
        {
            string meshPath = AssetDatabase.GetAssetPath(MeshFolderPath) + "/" + meshesnShit[i].name + i + ".asset";
            AssetDatabase.CreateAsset(meshesnShit[i], meshPath);
            Debug.Log("Mesh copied and saved to: " + meshPath);
        }
        meshesnShit.Clear();
    }

    private Mesh GetSharedMesh(Renderer renderer)
    {
        if (renderer is MeshRenderer meshRenderer && meshRenderer.GetComponent<MeshFilter>())
            return meshRenderer.GetComponent<MeshFilter>().sharedMesh;
        else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            return skinnedMeshRenderer.sharedMesh;
        return null;
    }

    private void AssignMeshToRenderer(Renderer renderer, Mesh mesh)
    {
        if (renderer is MeshRenderer meshRenderer && meshRenderer.GetComponent<MeshFilter>())
            meshRenderer.GetComponent<MeshFilter>().sharedMesh = mesh;
        else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            skinnedMeshRenderer.sharedMesh = mesh;
    }

    private int MeshCompare(Mesh mesh)
    {
        Vector3[] targetVertices = mesh.vertices;
        int counttt = -1;

        foreach (Mesh defaultMesh in meshesnShit)
        {
            Vector3[] defaultVertices = defaultMesh.vertices;
            counttt++;

            if (AreVerticesEqual(defaultVertices, targetVertices))
                return counttt;
        }
        return -1;
    }

    private bool AreVerticesEqual(Vector3[] vertices1, Vector3[] vertices2)
    {
        if (vertices1.Length != vertices2.Length)
            return false;

        for (int i = 0; i < vertices1.Length; i++)
            if (vertices1[i] != vertices2[i])
                return false;

        return true;
    }

    //Material methods V2

    private void CopyMaterialsFromChildren()
    {
        if (parentGameObject == null || destinationFolder == null)
        {
            Debug.LogError("Please select the source GameObject and destination folder.");
            return;
        }

        string destinationFolderPath = AssetDatabase.GetAssetPath(destinationFolder);

        copiedMaterials.Clear();

        TraverseChildrenAndCopyMaterials(parentGameObject.transform, destinationFolderPath);

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
                    CopyMaterial(material, destinationFolderPath, currentTransform.name);

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
                        CopyMaterial(material, destinationFolderPath, currentTransform.name);

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
            TraverseChildrenAndCopyMaterials(child, destinationFolderPath);
    }

    private void CopyMaterial(Material sourceMaterial, string destinationFolderPath, string name)
    {

        if (sourceMaterial == null || copiedMaterials.ContainsKey(sourceMaterial))
            return;

        string ndame = sourceMaterial.shader.name.Replace("/", "");

        if (ndame.StartsWith("."))
            ndame = ndame.Substring(1, ndame.Length - 1);

        Directory.CreateDirectory(destinationFolderPath + "/" + ndame);

        string destinationPath = destinationFolderPath + "/" + ndame + "/" + sourceMaterial.name + ".mat";

        int duplicateCount = 0;
        while (System.IO.File.Exists(destinationPath))
        {
            string duplicateSuffix = "_" + duplicateCount.ToString();
            destinationPath = destinationFolderPath + "/" + ndame + "/" + sourceMaterial.name + duplicateSuffix + ".mat";
            duplicateCount++;
        }


        Material newMaterial = new Material(sourceMaterial.shader);
        newMaterial.CopyPropertiesFromMaterial(sourceMaterial);

        AssetDatabase.CreateAsset(newMaterial, destinationPath);
        newMaterial.shader = sourceMaterial.shader;

        copiedMaterials.Add(sourceMaterial, newMaterial);
    }

    //Texture Methods V3

    private void CaptureMaterialTextures(Material material)
    {
        string[] propertyNames = material.GetTexturePropertyNames();

        for (int i = 0; i < propertyNames.Length; i++)
        {
            string propertyName = propertyNames[i];
            Texture texture = material.GetTexture(propertyName);

            if (texture != null)
            {
                string saveFolderPath = AssetDatabase.GetAssetPath(saveFolder);

                if (texture is Texture2DArray)
                {
                    // Handle Texture2DArray as per your existing logic
                    HandleTexture2DArray(texture, propertyName, saveFolderPath, material);
                }
                else
                {
                    Texture2D existingTexture = FindExistingTexture(texture, saveFolderPath);
                    if (existingTexture == null)
                    {
                        existingTexture = SaveNewTexture(texture, saveFolderPath);
                        material.SetTexture(propertyName, existingTexture);
                    }

                    if (existingTexture != null)
                    {
                        material.SetTexture(propertyName, existingTexture);
                    }
                }
            }
        }
    }

    private void HandleTexture2DArray(Texture texture, string propertyName, string folderPath, Material material)
    {
        Texture2DArray newArray = Instantiate((Texture2DArray)texture);

        // Save the new Texture2DArray to the specified folder.
        string saveFolderPath = AssetDatabase.GetAssetPath(saveFolder);
        string fullPath = Path.Combine(saveFolderPath, append + propertyName + "_Array.asset");
        append++;
        AssetDatabase.CreateAsset(newArray, fullPath);
        material.SetTexture(propertyName, newArray);

        Debug.Log("Texture2DArray saved to: " + fullPath);
    }

    private Texture2D FindExistingTexture(Texture texture, string folderPath)
    {
        string instanceIdFileName = texture.GetInstanceID().ToString() + ".png";
        string potentialPath = Path.Combine(folderPath, instanceIdFileName);

        if (File.Exists(potentialPath))
        {
            string relativePath = "Assets" + potentialPath.Substring(Application.dataPath.Length);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
        }

        return null;
    }

    private Texture2D SaveNewTexture(Texture texture, string folderPath)
    {
        Texture2D tex2D = TextureToTexture2D(texture);
        if (tex2D != null)
        {
            string filename = texture.GetInstanceID().ToString() + ".png";
            string fullPath = Path.Combine(folderPath, filename);
            byte[] bytes = tex2D.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.ImportAsset(fullPath);
            Debug.Log("Texture saved to: " + fullPath);

            //string relativePath = "Assets" + fullPath.Substring(Application.dataPath.Length);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
        }

        return null;
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


    //Avatar Copy Methods V3
    private void CopyAvatar()
    {
        string folderPath = AssetDatabase.GetAssetPath(destinationFolder1);
        Animator[] animators = parentGameObject.GetComponentsInChildren<Animator>(true);

        if (animators.Length == 0)
        {
            Debug.Log("No animators found.");
            return;
        }

        Dictionary<Avatar, Avatar> avatarMap = new Dictionary<Avatar, Avatar>();

        foreach (Animator animator in animators)
        {
            if (animator.avatar == null) continue;

            // Check if the avatar has already been cloned
            if (!avatarMap.TryGetValue(animator.avatar, out Avatar newAvatar))
            {
                newAvatar = UnityEngine.Object.Instantiate(animator.avatar);
                string avatarName = animator.avatar.name + "_Avatar.asset";
                string avatarPath = Path.Combine(folderPath, avatarName);
                AssetDatabase.CreateAsset(newAvatar, avatarPath);

                avatarMap[animator.avatar] = newAvatar;
            }

            animator.avatar = newAvatar;
        }

        Debug.Log("Avatar copy completed!");
    }

    //Audio Methods V2

    private void CaptureAudioFromChildren()
    {
        if (parentGameObject == null)
        {
            Debug.LogError("Parent Object is not selected.");
            return;
        }

        AudioSource[] audioSources = parentGameObject.GetComponentsInChildren<AudioSource>(true);

        foreach (AudioSource source in audioSources)
        {
            AudioClip clip = source.clip;
            if (clip == null) continue;

            int clipID = clip.GetInstanceID();
            if (copiedAudioClipInstanceIDs.Contains(clipID))
            {
                Debug.Log($"Audio clip {clip.name} (Instance ID: {clipID}) already copied.");
                continue;
            }

            CaptureAudio(source);
            copiedAudioClipInstanceIDs.Add(clipID);
        }
    }

    private void CaptureAudio(AudioSource source)
    {
        if (source == null || source.clip == null || saveFolderPath == null)
        {
            Debug.LogError("Invalid Audio Source, Clip, or Save Folder.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(saveFolderPath);
        string fullPath = Path.Combine(folderPath, $"{source.clip.name}_{ii}.wav").Replace("\\", "/");
        ii++;

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        SavWav.Save(fullPath, source.clip);
        Debug.Log("Audio captured and saved to: " + fullPath);
    }

    public static class SavWav
    {
        public static void Save(string path, AudioClip clip)
        {
            if (!path.ToLower().EndsWith(".wav")) path += ".wav";

            Debug.Log("Saving to: " + path);

            using (FileStream fileStream = CreateEmpty(path))
            {
                float[] audioData = new float[clip.samples * clip.channels];
                clip.GetData(audioData, 0);
                ConvertAndWrite(fileStream, audioData);
                WriteHeader(fileStream, clip.samples, clip.channels, clip.frequency);
            }
        }

        static FileStream CreateEmpty(string filepath)
        {
            var fileStream = new FileStream(filepath, FileMode.Create);
            byte emptyByte = new byte();

            for (int i = 0; i < 44; i++) // we need to write 44 bytes of silence at the beginning of the file to make it a valid wav file.
                fileStream.WriteByte(emptyByte);

            return fileStream;
        }

        static void ConvertAndWrite(FileStream fileStream, float[] audioData)
        {
            Int16[] intData = new Int16[audioData.Length];
            // Convert float to Int16
            for (int i = 0; i < audioData.Length; i++) intData[i] = (short)(audioData[i] * 32767);

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
#endif
