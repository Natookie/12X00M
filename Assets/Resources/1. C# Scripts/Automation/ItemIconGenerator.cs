using UnityEngine;
using UnityEditor;
using System.IO;

public static class ItemIconGenerator
{
    private const string OUTPUT_FOLDER = "Assets/Resources/2. Arts/Generated Icon/";
    private const int ICON_SIZE = 512;

    [MenuItem("Tools/Catalog/Generate Item Icon")]
    public static void GenerateSelectedItemIcon(){
        GameObject selected = Selection.activeGameObject;
        if(selected == null){
            Debug.LogError("No GameObject selected.");
            return;
        }

        Camera cam = GameObject.Find("PreviewCamera")?.GetComponent<Camera>();
        if(cam == null){
            Debug.LogError("PreviewCamera not found in the scene.");
            return;
        }

        if(cam.targetTexture == null){
            Debug.LogError("PreviewCamera has no RenderTexture assigned.");
            return;
        }

        if(!Directory.Exists(OUTPUT_FOLDER)) Directory.CreateDirectory(OUTPUT_FOLDER);

        GameObject previewRoot = new GameObject("PreviewInstance_TEMP");
        previewRoot.hideFlags = HideFlags.HideAndDontSave;
        previewRoot.transform.position = Vector3.zero;
        previewRoot.transform.rotation = Quaternion.identity;
        previewRoot.transform.localScale = Vector3.one;

        GameObject instance = Object.Instantiate(selected);
        instance.hideFlags = HideFlags.HideAndDontSave;
        instance.transform.SetParent(previewRoot.transform, false);

        ResetLocalTransform(instance);
        NormalizeTransform(instance);

        cam.Render();
        Texture2D tex = RenderToTexture(cam.targetTexture);

        SaveTextureAsSprite(tex, selected.name);
        Object.DestroyImmediate(previewRoot);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        Debug.Log($"Icon generated: {OUTPUT_FOLDER}{selected.name}.png");
    }

    private static void ResetLocalTransform(GameObject obj){
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }

    private static void NormalizeTransform(GameObject obj){
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if(renderers.Length == 0) return;

        Bounds bounds = new Bounds();
        bool initialized = false;

        foreach (Renderer r in renderers){
            if(!initialized){
                bounds = r.localBounds;
                initialized = true;
            }
            else bounds.Encapsulate(r.localBounds);
        }

        obj.transform.localPosition = -bounds.center;

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if(maxSize > 0f){
            float scale = 1f / maxSize;
            obj.transform.localScale = Vector3.one * scale;
        }
    }

    private static Texture2D RenderToTexture(RenderTexture rt){
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        return tex;
    }

    private static void SaveTextureAsSprite(Texture2D tex, string name){
        string path = OUTPUT_FOLDER + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }
}
