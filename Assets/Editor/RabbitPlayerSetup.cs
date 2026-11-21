using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: CutieHack > Setup Rabbit Player
// Hides the Player's capsule mesh, adds the rabbit sprite as a child,
// and shrinks the collider to rabbit size (32x40 px at 32 px per unit = 1 x 1.25 units).
public static class RabbitPlayerSetup
{
    const string SpritePath = "Assets/Art/Rabbit/rabbit_00.png";
    const float PixelsPerUnit = 32f;

    [MenuItem("CutieHack/Setup Rabbit Player")]
    public static void Setup()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("No object tagged Player in the scene."); return; }

        // Pixel-art import settings for every rabbit frame.
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Rabbit" }))
        {
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = PixelsPerUnit;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
        }
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null) { Debug.LogError("Missing sprite: " + SpritePath); return; }

        Undo.RegisterFullObjectHierarchyUndo(player, "Setup Rabbit Player");

        MeshRenderer mesh = player.GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        Transform child = player.transform.Find("Sprite");
        if (child == null)
        {
            GameObject go = new GameObject("Sprite");
            Undo.RegisterCreatedObjectUndo(go, "Setup Rabbit Player");
            go.transform.SetParent(player.transform, false);
            child = go.transform;
        }
        child.localPosition = Vector3.zero;
        child.localScale = Vector3.one;
        SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
        if (sr == null) sr = child.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // Rabbit is 1 x 1.25 units; collider a bit narrower than the art so edges don't snag.
        player.transform.localScale = Vector3.one;
        CapsuleCollider cap = player.GetComponent<CapsuleCollider>();
        if (cap != null)
        {
            cap.direction = 1;
            cap.radius = 0.4f;
            cap.height = 1.2f;
            cap.center = Vector3.zero;
        }

        EditorSceneManager.MarkSceneDirty(player.scene);
        Selection.activeGameObject = player;
        Debug.Log("Player is now the rabbit. Save the scene with Cmd+S.");
    }
}
