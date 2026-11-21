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
    // Normal-form size. 1.25 makes her about 1.25 x 1.5 units, too tall for the 1-tile gaps.
    // Shrink/grow later can just multiply this scale.
    const float BaseScale = 1.25f;

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

        // Sprite is 1 x 1.25 units at scale 1; collider a bit narrower than the art so edges don't snag.
        // The whole Player is scaled, so sprite and collider grow together.
        player.transform.localScale = Vector3.one * BaseScale;
        CapsuleCollider cap = player.GetComponent<CapsuleCollider>();
        if (cap != null)
        {
            cap.direction = 1;
            cap.radius = 0.4f;
            cap.height = 1.2f;
            cap.center = Vector3.zero;
            // No friction, so she slides down walls instead of sticking to them mid-jump.
            cap.sharedMaterial = GetSlipperyMaterial();
        }
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Idle: rabbit_00..03. Run: rabbit_04..08.
        BaseMovement move = player.GetComponent<BaseMovement>();
        if (move != null)
        {
            move.idleFrames = LoadFrames(0, 3);
            move.runFrames = LoadFrames(4, 8);
            EditorUtility.SetDirty(move);
        }

        EditorSceneManager.MarkSceneDirty(player.scene);
        Selection.activeGameObject = player;
        Debug.Log("Player is now the rabbit. Save the scene with Cmd+S.");
    }

    static Sprite[] LoadFrames(int first, int last)
    {
        Sprite[] frames = new Sprite[last - first + 1];
        for (int i = first; i <= last; i++)
            frames[i - first] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Rabbit/rabbit_" + i.ToString("00") + ".png");
        return frames;
    }

    static PhysicsMaterial GetSlipperyMaterial()
    {
        const string path = "Assets/Art/Materials/PlayerNoFriction.asset";
        PhysicsMaterial mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
        if (mat != null) return mat;
        mat = new PhysicsMaterial("PlayerNoFriction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
