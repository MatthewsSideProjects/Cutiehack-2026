using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Put this file in Assets/Editor/ (create the Editor folder if it does not exist).
// Put grass_top.png, dirt.png, platform_brown.png and wall_gray.png in Assets/Art/Terrain/.
// Then use the menu: CutieHack > Build Level 1
// Compact version: 70 tiles wide, five sections of 14 tiles each.
// One tile = one Unity unit. Everything sits at Z = 0.
// The cubes keep their colliders. Where a sprite is found, the cube's mesh is hidden
// and a tiled sprite is drawn in its place.
public static class Level1Builder
{
    const string RootName = "Level1";
    const string MatFolder = "Assets/Art/Materials";
    const string SpriteFolder = "Assets/Art/Terrain";

    internal enum Look { None, Floor, Platform, Wall }

    static readonly Color Green = new Color32(110, 170, 90, 255);
    static readonly Color Brown = new Color32(170, 120, 70, 255);
    static readonly Color Gray = new Color32(150, 150, 160, 255);
    static readonly Color Red = new Color32(226, 75, 74, 255);
    static readonly Color Amber = new Color32(250, 199, 117, 255);
    static readonly Color Pink = new Color32(212, 83, 126, 255);
    static readonly Color Gold = new Color32(239, 159, 39, 255);

    static Material green, brown, gray, red, amber, pink, gold;
    static Sprite grassTop, dirt, platformSprite, wallSprite;
    static Component playerScript;

    [MenuItem("CutieHack/Build Level 1")]
    public static void Build()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            bool ok = EditorUtility.DisplayDialog(
                "Rebuild Level 1",
                "An object named \"" + RootName + "\" already exists. Delete it and everything inside it, then rebuild?",
                "Rebuild", "Cancel");
            if (!ok) return;
            Undo.DestroyObjectImmediate(existing);
        }

        green = GetMaterial("LevelGreen", Green);
        brown = GetMaterial("LevelBrown", Brown);
        gray = GetMaterial("LevelGray", Gray);
        red = GetMaterial("LevelRed", Red);
        amber = GetMaterial("LevelAmber", Amber);
        pink = GetMaterial("LevelPink", Pink);
        gold = GetMaterial("LevelGold", Gold);

        grassTop = LoadSprite("grass_top");
        dirt = LoadSprite("dirt");
        platformSprite = LoadSprite("platform_brown");
        wallSprite = LoadSprite("wall_gray");

        GameObject rootGo = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootGo, "Build Level 1");
        Transform root = rootGo.transform;
        root.position = Vector3.zero;

        GameObject playerGo = GameObject.Find("Player");
        playerScript = null;
        if (playerGo != null)
        {
            playerScript = playerGo.GetComponent("BaseMovement");
            playerGo.transform.position = new Vector3(2f, 2f, 0f);
            try { playerGo.tag = "Player"; }
            catch (UnityException) { Debug.LogWarning("Tag 'Player' is not defined."); }
        }

        // Camera follows the player along x.
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraScript follow = cam.GetComponent<CameraScript>();
            if (follow == null) follow = Undo.AddComponent<CameraScript>(cam.gameObject);
            follow.player = playerGo;
            cam.transform.position = new Vector3(2f, 2f, -10f);
        }

        // ---- Floors and end walls (tile coordinates, both ends included) ----
        Add(root, "Floor A", 0, 9, -3, -1, "Floor", green, look: Look.Floor);          // before the gap
        Add(root, "Floor B", 13, 34, -3, -1, "Floor", green, look: Look.Floor);        // after the gap, up to the trench
        Add(root, "Trench bottom", 35, 36, -3, -3, "Floor", green, look: Look.Floor);  // trench is 2 wide, 2 deep
        Add(root, "Floor C", 37, 69, -3, -1, "Floor", green, look: Look.Floor);        // after the trench
        Add(root, "Left wall", -1, -1, -3, 3, null, gray, look: Look.Wall);
        Add(root, "Right wall", 70, 70, -3, 3, null, gray, look: Look.Wall);

        // ---- Section 1: jump (tiles 0-13) ----
        Add(root, "S1 step", 4, 4, 0, 0, "Floor", brown, look: Look.Platform);
        Add(root, "S1 wall", 7, 7, 0, 1, null, gray, look: Look.Wall);
        // gap at tiles 10-12 is simply empty space between Floor A and Floor B
        AddCheckpoint(root, "Checkpoint 1", 13);

        // ---- Section 2: shrink (tiles 14-27) ----
        Add(root, "S2 tunnel ceiling", 16, 19, 1, 2, null, gray, look: Look.Wall);
        Add(root, "S2 step", 21, 21, 0, 0, "Floor", brown, look: Look.Platform);
        Add(root, "S2 nook roof", 23, 25, 1, 1, "Floor", brown, look: Look.Platform);
        Add(root, "S2 nook end wall", 25, 25, 0, 0, null, gray, look: Look.Wall);
        AddItem(root, "Teacup", 24, 0, gold);
        AddCheckpoint(root, "Checkpoint 2", 27);

        // ---- Section 3: grow and push (tiles 28-41) ----
        Add(root, "S3 block", 31, 32, 0, 1, "Floor", brown, look: Look.Platform);
        AddCheckpoint(root, "Checkpoint 3", 41);

        // ---- Section 4: block and ledge (tiles 42-55) ----
        Add(root, "S4 block", 44, 45, 0, 1, "Floor", brown, look: Look.Platform);
        Add(root, "S4 pillar", 49, 52, 0, 2, "Floor", gray, look: Look.Wall);
        AddItem(root, "Gloves", 50, 3, pink);
        AddCheckpoint(root, "Checkpoint 4", 55);

        // ---- Section 5: door (tiles 56-69) ----
        Add(root, "S5 crawl ceiling", 60, 62, 1, 2, null, gray, look: Look.Wall);
        AddDoor(root, "Door", 67, GetLevel2Spawn());

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = rootGo;
        Debug.Log("Level 1 built under '" + RootName + "'. Save the scene with Cmd+S.");
    }

    internal static GameObject Add(Transform root, string name, int x1, int x2, int y1, int y2,
        string tag, Material mat, bool trigger = false, Look look = Look.None)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(root, false);
        float w = x2 - x1 + 1;
        float h = y2 - y1 + 1;
        go.transform.localPosition = new Vector3((x1 + x2 + 1) / 2f, (y1 + y2 + 1) / 2f, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (trigger) go.GetComponent<Collider>().isTrigger = true;
        SetTag(go, tag);
        ApplyLook(root, name, x1, x2, y1, y2, go, look);
        return go;
    }

    static void ApplyLook(Transform root, string name, int x1, int x2, int y1, int y2, GameObject cube, Look look)
    {
        float w = x2 - x1 + 1;
        float h = y2 - y1 + 1;
        float cx = (x1 + x2 + 1) / 2f;
        float cy = (y1 + y2 + 1) / 2f;
        bool applied = false;

        if (look == Look.Floor && grassTop != null && dirt != null)
        {
            if (h >= 2f)
            {
                AddSprite(root, name + " grass", grassTop, cx, y2 + 0.5f, w, 1f);
                AddSprite(root, name + " dirt", dirt, cx, (y1 + y2) / 2f, w, h - 1f);
            }
            else
            {
                AddSprite(root, name + " dirt", dirt, cx, y1 + 0.5f, w, 1f);
            }
            applied = true;
        }
        else if (look == Look.Platform && platformSprite != null)
        {
            AddSprite(root, name + " look", platformSprite, cx, cy, w, h);
            applied = true;
        }
        else if (look == Look.Wall && wallSprite != null)
        {
            AddSprite(root, name + " look", wallSprite, cx, cy, w, h);
            applied = true;
        }

        if (applied) cube.GetComponent<MeshRenderer>().enabled = false;
    }

    static void AddSprite(Transform root, string name, Sprite sprite, float cx, float cy, float w, float h)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(cx, cy, 0.5f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(w, h);
    }

    static void AddItem(Transform root, string name, int tileX, int tileY, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(tileX + 0.5f, tileY + 0.5f, 0f);
        go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.GetComponent<Collider>().isTrigger = true;
    }

    // Lamppost checkpoint: 1 x 2.5 units (32x80 px at 32 px per unit), standing on the floor at tileX.
    internal static void AddCheckpoint(Transform root, string name, int tileX, float floorY = 0f)
    {
        Sprite off = LoadSprite("Assets/Art/Lamp/lamp_off.png", 32f);
        Sprite on = LoadSprite("Assets/Art/Lamp/lamp_on.png", 32f);

        GameObject go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(tileX + 0.5f, floorY + 1.25f, 0f);
        BoxCollider box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(1f, 2.5f, 1f);

        if (off != null)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = off;
        }
        else
        {
            Debug.LogWarning("Missing lamp sprites, " + name + " is invisible.");
        }

        CheckpointDetection cp = go.AddComponent<CheckpointDetection>();
        cp.playerScript = playerScript as BaseMovement;
        cp.offSprite = off;
        cp.onSprite = on;
    }

    // Door: 1.5 x 2.5 units (48x80 px at 32 px per unit), standing on the floor at tileX.
    // The trigger is narrow so she has to walk into the middle of the doorway.
    internal static DoorScript AddDoor(Transform root, string name, int tileX, Transform destination, float floorY = 0f)
    {
        Sprite closed = LoadSprite("Assets/Art/Door/door_closed.png", 32f);
        Sprite open = LoadSprite("Assets/Art/Door/door_open.png", 32f);

        GameObject go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(tileX + 0.5f, floorY + 1.25f, 0.2f);
        BoxCollider box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(0.5f, 2.5f, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = closed;

        DoorScript door = go.AddComponent<DoorScript>();
        door.closedSprite = closed;
        door.openSprite = open;
        door.destination = destination;
        return door;
    }

    // Level 2 lives in this same scene, 1000 units to the right. It is a separate root object,
    // so rebuilding Level 1 never touches it. Created once with a starter floor and a spawn point.
    const float Level2X = 1000f;
    internal static Transform GetLevel2Spawn()
    {
        GameObject level2 = GameObject.Find("Level2");
        if (level2 == null)
        {
            level2 = new GameObject("Level2");
            Undo.RegisterCreatedObjectUndo(level2, "Build Level 1");
            level2.transform.position = new Vector3(Level2X, 0f, 0f);
            Add(level2.transform, "Start floor", 0, 9, -3, -1, "Floor", green, look: Look.Floor);
        }
        Transform spawn = level2.transform.Find("Spawn");
        if (spawn == null)
        {
            spawn = new GameObject("Spawn").transform;
            spawn.SetParent(level2.transform, false);
            spawn.localPosition = new Vector3(2f, 2f, 0f);
        }
        return spawn;
    }

    static void SetTag(GameObject go, string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;
        try { go.tag = tag; }
        catch (UnityException) { Debug.LogWarning("Tag '" + tag + "' is not defined, left " + go.name + " untagged."); }
    }

    static Sprite LoadSprite(string file)
    {
        return LoadSprite(SpriteFolder + "/" + file + ".png", 16f);
    }

    static Sprite LoadSprite(string path, float ppu)
    {
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null)
        {
            Debug.LogWarning("Missing sprite: " + path + " (using plain colored cubes for that look).");
            return null;
        }

        TextureImporterSettings s = new TextureImporterSettings();
        imp.ReadTextureSettings(s);
        bool needsChange =
            imp.textureType != TextureImporterType.Sprite ||
            imp.spriteImportMode != SpriteImportMode.Single ||
            imp.spritePixelsPerUnit != ppu ||
            imp.filterMode != FilterMode.Point ||
            imp.textureCompression != TextureImporterCompression.Uncompressed ||
            s.spriteMeshType != SpriteMeshType.FullRect;

        if (needsChange)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    internal static Material GetMaterial(string name, Color color)
    {
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "Materials");
        string path = MatFolder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            AssetDatabase.CreateFolder(parent, child);
    }
}