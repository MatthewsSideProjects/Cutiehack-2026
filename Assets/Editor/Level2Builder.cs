using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu: CutieHack > Build Level 2
// Level 2: Birthday Table. Built under a "Level2" object at x = 1000 in the same scene as Level 1;
// the Level 1 door sends her to Level2/Spawn.
// One tile = one unit, coordinates are local to Level2. Floor tops are at y = 0 unless noted.
//
// Section 1 (x 0-31):   one plate loops over a pit, then flickering candle flames.
// Section 2 (x 32-63):  a side-to-side plate, then an up-and-down plate to a ledge (top y = 5).
// Section 3 (x 64-87):  four plates bobbing up and down out of phase over a long pit.
// Section 4 (x 88-125): cake climb, inside the cake. Four cream floors zigzag upward, each with a
//                       4-tile hole and a sugar-cube step under it. Giant forks stab out of the back
//                       of the cake toward the camera on fixed loops, shaking first as a warning.
//
// Heights are tuned for Jump Force 10 with Gravity Mod 20: a jump rises about 2.5 units.
public static class Level2Builder
{
    const string RootName = "Level2";
    const float RootX = 1000f;

    static Material floorMat, plateMat, candleMat, flameMat, cakeMat, frostingMat, creamMat, insideMat, crumbMat, berryMat, forkMat;

    [MenuItem("CutieHack/Build Level 2")]
    public static void Build()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            bool ok = EditorUtility.DisplayDialog("Rebuild Level 2",
                "An object named \"" + RootName + "\" already exists. Delete it and everything inside it, then rebuild?",
                "Rebuild", "Cancel");
            if (!ok) return;
            Undo.DestroyObjectImmediate(existing);
        }

        floorMat = Level1Builder.GetMaterial("Level2Tablecloth", new Color32(240, 120, 170, 255));
        plateMat = Level1Builder.GetMaterial("Level2Plate", new Color32(235, 240, 245, 255));
        candleMat = Level1Builder.GetMaterial("Level2Candle", new Color32(150, 225, 200, 255));
        flameMat = Level1Builder.GetMaterial("Level2Flame", new Color32(255, 190, 60, 255));
        cakeMat = Level1Builder.GetMaterial("Level2Cake", new Color32(250, 225, 150, 255));
        frostingMat = Level1Builder.GetMaterial("Level2Frosting", new Color32(255, 170, 200, 255));
        creamMat = Level1Builder.GetMaterial("Level2Cream", new Color32(255, 250, 235, 255));
        insideMat = Level1Builder.GetMaterial("Level2CakeInside", new Color32(230, 185, 115, 255));
        crumbMat = Level1Builder.GetMaterial("Level2Crumb", new Color32(195, 145, 80, 255));
        berryMat = Level1Builder.GetMaterial("Level2Berry", new Color32(220, 60, 80, 255));
        forkMat = Level1Builder.GetMaterial("Level2Fork", new Color32(185, 192, 205, 255));

        GameObject rootGo = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootGo, "Build Level 2");
        Transform root = rootGo.transform;
        root.position = new Vector3(RootX, 0f, 0f);

        Transform spawn = new GameObject("Spawn").transform;
        spawn.SetParent(root, false);
        spawn.localPosition = new Vector3(2f, 2f, 0f);

        // ---- Section 1: slow plate + candle flames (x 0-31) ----
        Tiles(root, "Start floor", 0, 9, -3, -1);
        // pit at x 10-17
        Plate(root, "S1 plate", 11.5f, -0.25f, new Vector3(5f, 0f, 0f), 4f, 0f);
        Tiles(root, "S1 floor", 18, 31, -3, -1);
        Candle(root, "S1 candle A", 22, 0f, 0f);
        Candle(root, "S1 candle B", 26, 0f, 0.5f);
        Level1Builder.AddCheckpoint(root, "Checkpoint 1", 30);

        // ---- Section 2: side-to-side, then up-and-down to a ledge (x 32-63) ----
        // pit at x 32-51
        Plate(root, "S2 plate side", 33.5f, -0.25f, new Vector3(8f, 0f, 0f), 5f, 0f);
        Plate(root, "S2 plate lift", 49.5f, -0.25f, new Vector3(0f, 5f, 0f), 4f, 0.5f);
        Tiles(root, "S2 ledge", 52, 63, -3, 4); // top at y = 5
        Level1Builder.AddCheckpoint(root, "Checkpoint 2", 60, 5f);

        // ---- Section 3: four bobbing plates (x 64-87), ledge height 5 ----
        // pit at x 64-83
        for (int i = 0; i < 4; i++)
            Plate(root, "S3 plate " + (i + 1), 66.5f + 5f * i, 1.75f, new Vector3(0f, 4f, 0f), 3f, i * 0.25f);
        Tiles(root, "Cake floor", 84, 125, -3, 4); // top at y = 5, the cake stands on it
        Level1Builder.AddCheckpoint(root, "Checkpoint 3", 87, 5f);

        // ---- Section 4: cake climb (x 91-114) ----
        BuildCake(root);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        LinkLevel1Door(spawn);
        Selection.activeGameObject = rootGo;
        Debug.Log("Level 2 built under '" + RootName + "'. Save the scene with Cmd+S.");
    }

    // The cake is one tall layer cake (x 92-113) the player climbs inside.
    // Layer k (1-4) is a cream filling floor with its top at y = 5 + 4k and a 4-tile hole at one
    // end: odd layers at the right, even layers at the left. A 2-high sugar cube under each hole
    // splits the 4-unit climb into two 2-unit hops, leaving 3.5 units of headroom per floor.
    const int CakeLeft = 92, CakeRight = 113, Hole = 4, Layers = 4;
    const float LayerGap = 4f, Ground = 5f, StepH = 2f;

    static void BuildCake(Transform root)
    {
        float summit = Ground + LayerGap * Layers;
        float cakeCx = (CakeLeft + CakeRight + 1) / 2f;
        float cakeW = CakeRight + 1 - CakeLeft + 2f; // includes the side walls

        // Side walls of the cake. They stop 2.2 above the ground, leaving a little doorway on each side.
        float wallBottom = Ground + 2.2f;
        float wallY = (wallBottom + summit) / 2f, wallH = summit - wallBottom;
        Box(root, "Cake wall left", CakeLeft - 0.5f, wallY, 1f, wallH, cakeMat, null);
        Box(root, "Cake wall right", CakeRight + 1.5f, wallY, 1f, wallH, cakeMat, null);

        for (int k = 1; k <= Layers; k++)
        {
            bool holeRight = k % 2 == 1;
            float x1 = holeRight ? CakeLeft : CakeLeft + Hole;
            float x2 = holeRight ? CakeRight + 1 - Hole : CakeRight + 1;
            float top = Ground + LayerGap * k;
            Box(root, "Cake layer " + k, (x1 + x2) / 2f, top - 0.25f, x2 - x1, 0.5f, creamMat, "Floor");

            // Sugar cube on the floor below, under the inner half of this layer's hole.
            float stepX = holeRight ? CakeRight + 1 - Hole + 1f : CakeLeft + Hole - 1f;
            float below = top - LayerGap;
            Box(root, "Sugar cube " + k, stepX, below + StepH / 2f, 2f, StepH, creamMat, "Floor");
        }

        // ---- Looks (no colliders, behind the play layer) ----
        Deco(root, "Cake stand", cakeCx, Ground - 0.1f, cakeW + 5f, 0.4f, 1f, plateMat);
        // Cut-open sponge behind her, with crumbs, so it reads as the inside of the cake.
        Deco(root, "Cake inside", cakeCx, (Ground + summit) / 2f, cakeW, summit - Ground, 1f, insideMat);
        System.Random rng = new System.Random(7);
        for (int i = 0; i < 70; i++)
        {
            float x = CakeLeft + (float)rng.NextDouble() * (CakeRight + 1 - CakeLeft);
            float y = Ground + 0.3f + (float)rng.NextDouble() * (summit - Ground - 1.2f);
            float size = 0.15f + (float)rng.NextDouble() * 0.2f;
            Deco(root, "Crumb " + (i + 1), x, y, size, size, 0.9f, crumbMat);
        }
        for (int k = 1; k < Layers; k++) // cream stripes across the face, lined up with the platforms
        {
            Deco(root, "Cake cream " + k, cakeCx, Ground + LayerGap * k - 0.25f, cakeW, 0.7f, 0.45f, creamMat);
            for (float x = CakeLeft + 1.5f + k; x < CakeRight; x += 5f) // strawberry slices in the cream
                Deco(root, "Strawberry " + k, x, Ground + LayerGap * k - 0.7f, 0.9f, 0.5f, 0.4f, berryMat);
        }
        Deco(root, "Cake frosting", cakeCx, summit - 0.2f, cakeW + 0.4f, 1f, 0.45f, frostingMat);
        float[] drips = { 1.2f, 0.6f, 1.6f, 0.8f, 1.1f, 0.5f, 1.4f, 0.7f, 1.0f, 0.6f, 1.3f, 0.9f };
        for (int i = 0; i < drips.Length; i++)
        {
            float x = CakeLeft - 0.5f + i * 2f;
            Deco(root, "Frosting drip " + (i + 1), x, summit - 0.7f - drips[i] / 2f, 0.7f, drips[i], 0.45f, frostingMat);
        }
        int[] candleX = { 94, 97, 103 }; // clear of the forks at 100 and 106
        foreach (int cx in candleX)
        {
            Deco(root, "Cake candle " + cx, cx + 0.5f, summit + 1f, 0.5f, 2f, 1f, candleMat);
            Deco(root, "Cake candle flame " + cx, cx + 0.5f, summit + 2.35f, 0.35f, 0.6f, 1f, flameMat);
        }

        Level1Builder.AddCheckpoint(root, "Checkpoint 4", CakeLeft + Hole + 1, Ground + LayerGap * 2);

        // ---- Forks ----
        // Two per floor (and on top), stabbing out of the back of the cake through her path.
        // The tines are low, so she can jump over one or wait for it to pull back. Phases are staggered.
        int[] forkX = { 100, 106 };
        for (int floor = 0; floor <= Layers; floor++)
            for (int i = 0; i < forkX.Length; i++)
                Fork(root, "Fork " + (floor + 1) + (i == 0 ? "a" : "b"), forkX[i] + 0.5f,
                    Ground + LayerGap * floor + 0.75f, floor * 0.2f + i * 0.5f);

        // Summit: door at the right end of the top layer.
        DoorScript door = Level1Builder.AddDoor(root, "Door", CakeRight - 4, null, summit);
        door.itemsNeeded = 0;
    }

    // A fork that stabs out of the back of the cake toward the camera. Built facing the camera:
    // tine tips at its origin, the handle trailing back (+z) behind the sponge where it's hidden.
    // A dark hole in the sponge marks where it comes out.
    const float ForkHomeZ = 1.15f, ForkStab = 2.1f;
    static void Fork(Transform root, string name, float x, float y, float phase)
    {
        if (y < Ground + LayerGap * Layers) // no sponge behind the top floor to put a hole in
            Deco(root, name + " hole", x, y, 1.9f, 1.6f, 0.93f, crumbMat);

        GameObject go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(x, y, ForkHomeZ);

        float[] tineX = { -0.6f, -0.2f, 0.2f, 0.6f };
        for (int i = 0; i < tineX.Length; i++)
            ForkPart(go.transform, "Tine " + (i + 1), new Vector3(tineX[i], 0f, 0.7f), new Vector3(0.22f, 1.3f, 1.4f));
        ForkPart(go.transform, "Neck", new Vector3(0f, 0f, 1.7f), new Vector3(1.6f, 0.7f, 0.6f));
        ForkPart(go.transform, "Handle", new Vector3(0f, 0f, 4.5f), new Vector3(0.4f, 0.5f, 5f));

        go.AddComponent<Rigidbody>();
        ForkThrust ft = go.AddComponent<ForkThrust>();
        ft.stab = new Vector3(0f, 0f, -ForkStab); // tips go from behind the sponge to past her
        ft.peek = 0.19f;                          // while shaking, the tips poke out of the sponge
        ft.phase = phase;
    }

    static void ForkPart(Transform fork, string name, Vector3 pos, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(fork, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = forkMat;
        go.tag = "Obstacle";
    }

    // Decoration box: no collider, pushed back to z so it sits behind the play layer.
    static void Deco(Transform root, string name, float cx, float cy, float w, float h, float z, Material mat)
    {
        GameObject go = Box(root, name, cx, cy, w, h, mat, null);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.localPosition = new Vector3(cx, cy, z);
        go.transform.localScale = new Vector3(w, h, 0.1f);
    }

    static GameObject Tiles(Transform root, string name, int x1, int x2, int y1, int y2)
    {
        return Level1Builder.Add(root, name, x1, x2, y1, y2, "Floor", floorMat);
    }

    static GameObject Box(Transform root, string name, float cx, float cy, float w, float h, Material mat, string tag)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(cx, cy, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (tag != null) go.tag = tag;
        return go;
    }

    // A 3-wide, half-tile-thick plate that loops from its start by `offset`.
    static void Plate(Transform root, string name, float cx, float cy, Vector3 offset, float period, float phase)
    {
        GameObject go = Box(root, name, cx, cy, 3f, 0.5f, plateMat, "Floor");
        go.AddComponent<Rigidbody>();
        MovingPlate mp = go.AddComponent<MovingPlate>();
        mp.offset = offset;
        mp.period = period;
        mp.phase = phase;
    }

    // A candle you can stand on, with a tall flame on top that flickers on and off.
    static void Candle(Transform root, string name, int tileX, float floorY, float phase)
    {
        Box(root, name, tileX + 0.5f, floorY + 0.75f, 0.8f, 1.5f, candleMat, "Floor");
        GameObject flame = Box(root, name + " flame", tileX + 0.5f, floorY + 2.75f, 0.5f, 2.5f, flameMat, "Obstacle");
        FlameFlicker ff = flame.AddComponent<FlameFlicker>();
        ff.phase = phase;
    }

    static void LinkLevel1Door(Transform spawn)
    {
        GameObject level1 = GameObject.Find("Level1");
        Transform doorT = level1 != null ? level1.transform.Find("Door") : null;
        DoorScript door = doorT != null ? doorT.GetComponent<DoorScript>() : null;
        if (door == null)
        {
            Debug.LogWarning("No Level1/Door found. Build Level 1, then Level 2 again, so the door leads here.");
            return;
        }
        Undo.RecordObject(door, "Build Level 2");
        door.destination = spawn;
        EditorUtility.SetDirty(door);
    }
}
