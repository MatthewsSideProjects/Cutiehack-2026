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
    static Material soil, grass, hedge, hedgeLight, pot, potRim, stone, moss, thorn, wood, rose, peach, cream,
        pollenGold, stem, logMat;
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

        // ---- Garden look: peach, soft greens and rose in golden-hour light ----
        soil = GetMaterial("GardenSoil", new Color32(196, 140, 110, 255));
        grass = GetMaterial("GardenGrass", new Color32(170, 214, 140, 255));
        hedge = GetMaterial("GardenHedge", new Color32(98, 150, 98, 255));
        hedgeLight = GetMaterial("GardenHedgeLight", new Color32(196, 210, 120, 255));
        pot = GetMaterial("GardenPot", new Color32(214, 124, 94, 255));
        potRim = GetMaterial("GardenPotRim", new Color32(238, 156, 118, 255));
        stone = GetMaterial("GardenStone", new Color32(186, 172, 166, 255));
        moss = GetMaterial("GardenMoss", new Color32(146, 186, 112, 255));
        thorn = GetMaterial("GardenThorn", new Color32(118, 58, 78, 255));
        wood = GetMaterial("GardenWood", new Color32(232, 186, 146, 255));
        rose = GetMaterial("GardenRose", new Color32(236, 118, 142, 255));
        peach = GetMaterial("GardenPeach", new Color32(252, 192, 160, 255));
        cream = GetMaterial("GardenCream", new Color32(255, 230, 205, 255));
        pollenGold = GetMaterial("GardenGold", new Color32(246, 202, 92, 255));
        stem = GetMaterial("GardenStem", new Color32(120, 176, 104, 255));
        logMat = GetMaterial("GardenLog", new Color32(168, 112, 82, 255));

        // ---- Floors and end hedges (tile coordinates, both ends included) ----
        Ground(root, "Floor A", 0, 5, -3, -1);    // small gap at tile 6
        Ground(root, "Floor A2", 7, 9, -3, -1);   // wider gap at tiles 10-12
        Ground(root, "Floor B", 13, 34, -3, -1);  // up to the trench
        Ground(root, "Trench bottom", 35, 36, -3, -3); // trench is 2 wide, 2 deep
        Ground(root, "Floor C", 37, 69, -3, -1);
        Hedge(root, "Left hedge", -2, -1, -3, 7, null);
        Hedge(root, "Right hedge", 70, 71, -3, 7, null);

        // ---- Section 1: walk and jump (tiles 0-13) ----
        AddCheckpoint(root, "Checkpoint 1", 2);   // first thing she touches, so the lamp is explained early
        Pot(root, "S1 pot", 4, 1);
        Pot(root, "S1 tall pot", 8, 2);
        AddCheckpoint(root, "Checkpoint 2", 13);

        // ---- Section 2: hedge gap (tiles 14-27) ----
        // The hedge reaches far above her jump; the only way through is the 1-high gap at its roots.
        Hedge(root, "S2 hedge", 16, 19, 1, 7, null);
        Pot(root, "S2 pot", 21, 1);
        Hedge(root, "S2 nook roof", 23, 25, 1, 1, "Floor");
        Hedge(root, "S2 nook end", 25, 25, 0, 0, null);
        AddItem(root, "Teacup", 24, 0);
        AddCheckpoint(root, "Checkpoint 3", 27);

        // ---- Section 3: push the stone (tiles 28-41) ----
        AddPushable(root, "S3 stone", 31, 32, 0, 1); // push (Big) into the trench to bridge it
        AddCheckpoint(root, "Checkpoint 4", 40);

        // ---- Section 4: thorns and step (tiles 42-55) ----
        // Thorns hang from a trellis down to 1 above the ground, so only Small crawls under.
        // Past them there are 3 tiles of room to go Big and push the stone right, against the
        // garden wall (top 4), as a step.
        Trellis(root, 41.5f, 46.5f);
        Thorns(root, "S4 thorns", 42, 44, 1, 3);
        AddPushable(root, "S4 stone", 48, 49, 0, 1);
        Add(root, "S4 garden wall", 53, 55, 0, 3, "Floor", stone);
        Deco(root, PrimitiveType.Cube, new Vector3(54.5f, 4.08f, 0f), new Vector3(3.1f, 0.2f, 1.1f), 0f, moss);
        AddItem(root, "Gloves", 54, 4);
        AddCheckpoint(root, "Checkpoint 5", 57);

        // ---- Section 5: ending (tiles 56-69) ----
        // A hollow log to crawl through as Small, then back to Regular for the door.
        Add(root, "S5 log", 60, 62, 1, 2, null, logMat);
        Deco(root, PrimitiveType.Cube, new Vector3(61.5f, 3.05f, 0f), new Vector3(3f, 0.15f, 1.05f), 0f, moss);
        AddDoor(root, "Door", 67, GetLevel2Spawn()).itemsNeeded = 2; // teacup + gloves

        Dressing(root);

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

    // A stone only Big can push, with a mossy cap. The cap is a child so it moves with the stone.
    static void AddPushable(Transform root, string name, int x1, int x2, int y1, int y2)
    {
        GameObject go = Add(root, name, x1, x2, y1, y2, "Floor", stone);
        go.transform.localScale -= new Vector3(0.1f, 0f, 0f); // a hair narrower than the trench, or it rests on both edges
        go.AddComponent<Rigidbody>().mass = 2f;
        go.AddComponent<Pushable>();
        Deco(go.transform, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(1.04f, 0.14f, 1.04f), 0f, moss);
    }

    // Ground: soil with a grass lip along the top and a few tufts along the front.
    static void Ground(Transform root, string name, int x1, int x2, int y1, int y2)
    {
        Add(root, name, x1, x2, y1, y2, "Floor", soil);
        float w = x2 - x1 + 1;
        Deco(root, PrimitiveType.Cube, new Vector3(x1 + w / 2f, y2 + 0.85f, 0f), new Vector3(w + 0.1f, 0.35f, 1.1f), 0f, grass);
        for (float x = x1 + 0.7f; x < x2 + 1; x += 2.3f)
            for (int i = -1; i <= 1; i++)
                Deco(root, PrimitiveType.Cube, new Vector3(x + i * 0.12f, y2 + 1.1f, -0.52f), new Vector3(0.07f, 0.35f, 0.02f), i * 22f, grass);
    }

    // A golden-hour hedge: dark green block with lit leaf clumps along its top and front.
    static void Hedge(Transform root, string name, int x1, int x2, int y1, int y2, string tag)
    {
        Add(root, name, x1, x2, y1, y2, tag, hedge);
        for (float x = x1 + 0.5f; x <= x2 + 0.6f; x += 0.9f)
        {
            Deco(root, PrimitiveType.Sphere, new Vector3(x, y2 + 0.95f, -0.1f), new Vector3(1f, 0.7f, 1f), 0f, hedgeLight);
            for (float y = y1 + 0.6f; y < y2 + 0.6f; y += 1.3f)
                Deco(root, PrimitiveType.Sphere, new Vector3(x + (y % 2f) * 0.3f, y, -0.52f), new Vector3(0.8f, 0.7f, 0.05f), 0f, y > y2 - 0.5f ? hedgeLight : hedge);
        }
    }

    // A terracotta flower pot `tall` tiles high with a wider rim and a small sprout on top (decor only).
    static void Pot(Transform root, string name, int tileX, int tall)
    {
        Add(root, name, tileX, tileX, 0, tall - 1, "Floor", pot);
        Deco(root, PrimitiveType.Cube, new Vector3(tileX + 0.5f, tall - 0.15f, 0f), new Vector3(1.3f, 0.3f, 1.2f), 0f, potRim);
        Deco(root, PrimitiveType.Sphere, new Vector3(tileX + 0.5f, tall + 0.25f, 0.3f), new Vector3(0.5f, 0.4f, 0.3f), 0f, rose);
    }

    // Hanging thorns: an Obstacle block with spikes along its underside.
    static void Thorns(Transform root, string name, int x1, int x2, int y1, int y2)
    {
        Add(root, name, x1, x2, y1, y2, "Obstacle", thorn);
        for (float x = x1 + 0.25f; x < x2 + 1; x += 0.5f)
            Deco(root, PrimitiveType.Cube, new Vector3(x, y1 - 0.02f, 0f), new Vector3(0.18f, 0.18f, 0.9f), 45f, thorn);
        for (float x = x1 + 0.6f; x < x2 + 1; x += 1.2f)
            Deco(root, PrimitiveType.Sphere, new Vector3(x, y2 + 0.3f, -0.55f), new Vector3(0.45f, 0.45f, 0.1f), 0f, rose);
    }

    // Wooden trellis behind the thorns, from x1 to x2, with roses climbing it.
    static void Trellis(Transform root, float x1, float x2)
    {
        float z = 0.8f, top = 4.6f;
        Deco(root, PrimitiveType.Cube, new Vector3(x1, top / 2f, z), new Vector3(0.25f, top, 0.2f), 0f, wood);
        Deco(root, PrimitiveType.Cube, new Vector3(x2, top / 2f, z), new Vector3(0.25f, top, 0.2f), 0f, wood);
        Deco(root, PrimitiveType.Cube, new Vector3((x1 + x2) / 2f, top, z), new Vector3(x2 - x1 + 0.6f, 0.25f, 0.2f), 0f, wood);
        for (float x = x1 + 0.8f; x < x2; x += 1.2f)
        {
            Deco(root, PrimitiveType.Cube, new Vector3(x, top / 2f, z + 0.05f), new Vector3(0.08f, top * 1.3f, 0.05f), 30f, wood);
            Deco(root, PrimitiveType.Cube, new Vector3(x, top / 2f, z + 0.05f), new Vector3(0.08f, top * 1.3f, 0.05f), -30f, wood);
            Deco(root, PrimitiveType.Sphere, new Vector3(x + 0.3f, top + 0.3f, z - 0.2f), new Vector3(0.5f, 0.5f, 0.3f), 0f, rose);
        }
    }

    // An oversized flower standing behind the play area: stem, two leaves, five petals and a center.
    static void Flower(Transform root, float x, float height, float size, float z, Material petals)
    {
        Deco(root, PrimitiveType.Cube, new Vector3(x, height / 2f, z), new Vector3(0.18f * size, height, 0.1f), 0f, stem);
        Deco(root, PrimitiveType.Sphere, new Vector3(x + 0.35f * size, height * 0.4f, z), new Vector3(0.8f * size, 0.25f * size, 0.1f), 25f, stem);
        Deco(root, PrimitiveType.Sphere, new Vector3(x - 0.35f * size, height * 0.6f, z), new Vector3(0.8f * size, 0.25f * size, 0.1f), -25f, stem);
        for (int i = 0; i < 5; i++)
        {
            float a = i * 72f * Mathf.Deg2Rad;
            Deco(root, PrimitiveType.Sphere, new Vector3(x + Mathf.Sin(a) * 0.55f * size, height + Mathf.Cos(a) * 0.55f * size, z),
                new Vector3(0.75f * size, 0.75f * size, 0.1f), 0f, petals);
        }
        Deco(root, PrimitiveType.Sphere, new Vector3(x, height, z - 0.05f), new Vector3(0.5f * size, 0.5f * size, 0.1f), 0f, pollenGold);
    }

    // Big flowers behind the path and pollen drifting over the whole garden, in front of the painted backdrop.
    static void Dressing(Transform root)
    {
        // Painted garden (Assets/Art/level_1_frame.jpg) behind the path, repeated sideways between the
        // end hedges (x -2 to 72). The camera is flat (orthographic), so no perspective maths: 16 units tall,
        // and the planters' bottom (72% down the picture) sits on the floor top at y 0, so centre y = 3.5.
        Backdrop(root, "Assets/Art/level_1_frame.jpg", 35f, 0.71875f * 16f - 8f, 15f, 74f, 16f);

        Flower(root, 6f, 3.5f, 1.3f, 1.6f, peach);
        Flower(root, 11.5f, 5f, 1.6f, 2.2f, rose);
        Flower(root, 21f, 2.5f, 0.9f, 1.2f, cream);
        Flower(root, 26f, 4.2f, 1.4f, 2f, peach);
        Flower(root, 34f, 3.2f, 1.2f, 1.5f, rose);
        Flower(root, 38.5f, 5.5f, 1.7f, 2.4f, cream);
        Flower(root, 51f, 6f, 1.5f, 2.2f, peach);
        Flower(root, 58.5f, 3f, 1f, 1.3f, rose);
        Flower(root, 64.5f, 4.6f, 1.5f, 2f, cream);

        GameObject go = new GameObject("Pollen");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(35f, 4f, 0.3f);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 9f;
        main.startSpeed = 0.15f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        main.startColor = (Color)new Color32(255, 236, 170, 220);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.prewarm = true;
        var emission = ps.emission;
        emission.rateOverTime = 20f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(72f, 8f, 0.5f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.2f;
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        string path = MatFolder + "/GardenPollen.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = m;
    }

    // Painted background art (1363x768-ish jpg) as a sprite, height h units, repeated sideways to width w.
    internal static void Backdrop(Transform root, string path, float cx, float cy, float z, float w, float h)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Sprite sprite = LoadSprite(path, tex != null ? tex.height / h : 768f / h);
        if (sprite == null) return;
        GameObject go = new GameObject("Backdrop art");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(cx, cy, z);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(w, h);
        sr.sortingOrder = -100;
    }

    // Decoration: a primitive without a collider.
    static GameObject Deco(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, float rotZ, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name = "Deco";
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
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

    // A memory item: its pixel-art sprite (Assets/Art/Items/<name>.png) with a trigger to pick it up.
    // The door at the end of the level stays locked until she has them.
    internal static void AddItem(Transform root, string name, float tileX, float tileY)
    {
        Sprite sprite = LoadSprite("Assets/Art/Items/" + name.ToLower() + ".png", 16f);
        GameObject go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(tileX + 0.5f, tileY + 0.5f, 0f);
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;
        CollectableScript c = go.AddComponent<CollectableScript>();
        c.playerScript = playerScript as BaseMovement;
        if (c.playerScript == null) c.playerScript = Object.FindFirstObjectByType<BaseMovement>();
        c.name = name;
        c.displayImage = sprite;
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

    internal static Sprite LoadSprite(string path, float ppu)
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