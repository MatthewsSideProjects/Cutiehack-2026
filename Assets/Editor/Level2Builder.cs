using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu: CutieHack > Build Level 2
// Level 2: Birthday Table. Built under a "Level2" object at x = 1000 in the same scene as Level 1;
// the Level 1 door sends her to Level2/Spawn.
// One tile = one unit, coordinates are local to Level2. Floor tops are at y = 0 unless noted.
//
// Hot pink, mint and lemon over deep purple: polka-dot tablecloth, giant plates, a faded stuck
// clock and stretched balloons in the background (Dressing).
//
// Section 1 (x 0-31):   one plate loops over a pit, then flickering candle flames.
// Section 2 (x 32-63):  a side-to-side plate, then an up-and-down plate to a ledge (top y = 5).
// Section 3 (x 64-83):  ride a lift plate with a present on it, push it (Big) onto a cup as a step.
// Section 4 (x 84-103): crawl (Small) under a small table past two flickering flames.
// Section 5 (x 104-141): cake climb, inside the cake (local to the "Cake" child, shifted
//                       CakeShift right; the x values below are before the shift).
//                       Four cream floors zigzag upward, each with a
//                       4-tile hole and a gift-box step under it. Giant forks stab out of the back
//                       of the cake toward the camera on fixed loops, shaking first as a warning.
//                       Two gifts must be pushed into place (Big) first: one from outside the cake
//                       for the first climb, one for the last climb. Then hop up at Regular size.
//
// Heights are tuned for Jump Force 10 with Gravity Mod 20: a jump rises about 2.5 units.
public static class Level2Builder
{
    const string RootName = "Level2";
    const float RootX = 1000f;

    static Material floorMat, plateMat, candleMat, flameMat, cakeMat, frostingMat, creamMat, insideMat, crumbMat, berryMat, forkMat, ribbonMat;
    static Material purpleMat, lemonMat, mintMat, hotPinkMat, clockMat;

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
        ribbonMat = Level1Builder.GetMaterial("Level2Ribbon", new Color32(255, 220, 90, 255));
        purpleMat = Level1Builder.GetMaterial("Level2Purple", new Color32(58, 28, 88, 255));
        lemonMat = Level1Builder.GetMaterial("Level2Lemon", new Color32(255, 236, 110, 255));
        mintMat = Level1Builder.GetMaterial("Level2Mint", new Color32(135, 230, 190, 255));
        hotPinkMat = Level1Builder.GetMaterial("Level2HotPink", new Color32(255, 80, 165, 255));
        clockMat = Level1Builder.GetMaterial("Level2Clock", new Color32(150, 130, 170, 255)); // faded into the purple

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

        // ---- Section 3: ride and push (x 64-83) ----
        // Pit at x 64-71. A wide plate carries a present up from ledge height (5) to 7.5 and back.
        // Near the top she goes Big and pushes the present off onto the cup beside it (top 7).
        // The cup sits half a unit below the plate's peak so the present clears its edge for about
        // 1.5 s of each loop; level with the peak, it only lined up for an instant and jammed.
        // Standing on the present is the only way up to the high shelf (top 11): from the plate
        // or the bare cup a jump tops out at 10. Up and down only: presents are frozen sideways unless she's
        // Big, so a side-to-side plate would slide out from under one.
        Plate(root, "S3 lift", 67f, 4.75f, new Vector3(0f, 2.5f, 0f), 5f, 0f, 5f);
        Present(root, "S3 gift", 68.5f, 5f, 2);
        Box(root, "S3 cup", 71f, 2f, 2f, 10f, lemonMat, "Floor"); // y -3 to 7
        Tiles(root, "S3 high shelf", 72, 83, -3, 10); // top at y = 11
        Level1Builder.AddItem(root, "Hat", 76f, 11f); // memory item: reward for reaching the high shelf
        Level1Builder.AddCheckpoint(root, "Checkpoint 3", 80, 11f);

        // ---- Section 4: under the table (x 84-103) ----
        // Drop down to the tablecloth (top 5). A small table at x 88-100 has its cloth hanging to
        // 1 above the floor, so only Small fits under, past two flickering candle flames.
        // The cloth goes up to 14: too tall to jump over, even from the high shelf.
        Tiles(root, "Cake floor", 84, 141, -3, 4); // top at y = 5, the cake stands on it further right
        Box(root, "Table", 94f, 10f, 12f, 8f, mintMat, "Floor"); // y 6 to 14
        Deco(root, "Table top", 94f, 14.15f, 13f, 0.5f, -0.1f, lemonMat);
        TableFlame(root, "Table flame A", 91.5f, 0f);
        TableFlame(root, "Table flame B", 96.5f, 0.5f);
        Level1Builder.AddItem(root, "Card", 93.5f, 5f); // memory item: under the table, between the flames
        Level1Builder.AddCheckpoint(root, "Checkpoint 4", 102, 5f);

        // ---- Section 5: cake climb, shifted right to make room for sections 3 and 4 ----
        Transform cake = new GameObject("Cake").transform;
        cake.SetParent(root, false);
        cake.localPosition = new Vector3(CakeShift, 0f, 0f);
        BuildCake(cake);

        Dressing(root);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        LinkLevel1Door(spawn);
        Selection.activeGameObject = rootGo;
        Debug.Log("Level 2 built under '" + RootName + "'. Save the scene with Cmd+S.");
    }

    // The cake is one tall layer cake (x 92-113) the player climbs inside.
    // Layer k (1-4) is a cream filling floor with its top at y = 5 + 4k and a 4-tile hole at one
    // end: odd layers at the right, even layers at the left. A 2-high gift box under each hole
    // splits the 4-unit climb into two 2-unit hops, leaving 3.5 units of headroom per floor.
    // Cake coordinates are local to the "Cake" child, which sits CakeShift to the right in Level2.
    const float CakeShift = 16f;
    const int CakeLeft = 92, CakeRight = 113, Hole = 4, Layers = 4;
    const float LayerGap = 4f, Ground = 5f;

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

            // Gift box step on the floor below, under the inner half of this layer's hole.
            // Layer 1 and the top layer have none in place: she has to push one there (see below).
            if (k == 1 || k == Layers) continue;
            float stepX = holeRight ? CakeRight + 1 - Hole + 1f : CakeLeft + Hole - 1f;
            Present(root, "Gift step " + k, stepX, top - LayerGap, k);
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
        int[] candleX = { 94, 99, 103 };
        foreach (int cx in candleX)
        {
            Deco(root, "Cake candle " + cx, cx + 0.5f, summit + 1f, 0.5f, 2f, 1f, candleMat);
            Deco(root, "Cake candle flame " + cx, cx + 0.5f, summit + 2.35f, 0.35f, 0.6f, 1f, flameMat);
        }

        Level1Builder.AddCheckpoint(root, "Checkpoint 5", CakeLeft + Hole + 1, Ground + LayerGap * 2);

        // ---- Forks ----
        // Stabbing out of the back of the cake through her path, spread out and on different timings.
        // The tines are low, so she can jump over one or wait for it to pull back.
        // (floor, x, phase). Floor 0 is the ground inside the cake; none on the top.
        var forks = new (int floor, int x, float phase)[]
        {
            (0, 97, 0.0f), (0, 104, 0.45f),
            (1, 107, 0.2f), (1, 101, 0.7f),
            (2, 103, 0.35f), (2, 108, 0.85f),
            (3, 100, 0.5f), // in the present's path: the present blocks it while it's in front
        };
        foreach (var f in forks)
            Fork(root, "Fork " + f.floor + "-" + f.x, f.x + 0.5f, Ground + LayerGap * f.floor + 0.75f, f.phase);

        // Ground floor: no step under layer 1's hole. The gift sits outside, past the cake's right
        // doorway. Walk through to the end, grow Big, push it back in until she hits the wall
        // (it slides under; she can't), then go Regular, walk in, climb it and jump up the hole.
        Present(root, "Gift outside", 120f, Ground, 1);
        // Floor 3: push this one (as Big) left under the top layer's hole, then climb (as Regular).
        Present(root, "Gift top", 106f, Ground + LayerGap * 3, 4);

        // Summit: door at the right end of the top layer.
        // Leads to Level 3 if it's built; building Level 3 also links this door.
        DoorScript door = Level1Builder.AddDoor(root, "Door", CakeRight - 4, Level3Builder.GetSpawn(), summit);
        door.itemsNeeded = 2; // card + hat
    }

    // A fork that stabs out of the back of the cake toward the camera. Built facing the camera:
    // tine tips at its origin, the handle trailing back (+z) behind the sponge where it's hidden.
    // A dark hole in the sponge marks where it comes out.
    const float ForkHomeZ = 1.15f, ForkStab = 2.1f;
    static void Fork(Transform root, string name, float x, float y, float phase)
    {
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

    // A 2x2 gift box on a floor at y = floorY: pushable in Big form, and a step to stand on.
    static readonly Color32[] GiftColors =
    {
        new Color32(120, 200, 230, 255), new Color32(170, 130, 230, 255),
        new Color32(120, 210, 150, 255), new Color32(250, 150, 110, 255),
    };
    static void Present(Transform root, string name, float x, float floorY, int colorIndex)
    {
        Color32 c = GiftColors[colorIndex % GiftColors.Length];
        Material mat = Level1Builder.GetMaterial("Level2Gift" + colorIndex % GiftColors.Length, c);
        GameObject go = Box(root, name, x, floorY + 1f, 2f, 2f, mat, "Floor");
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 2f;
        go.AddComponent<Pushable>();
        // Ribbon: children scaled against the 2x2 parent, no colliders.
        foreach (Vector3 size in new[] { new Vector3(0.18f, 1.01f, 1.02f), new Vector3(1.01f, 0.18f, 1.02f) })
        {
            GameObject ribbon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(ribbon.GetComponent<Collider>());
            ribbon.name = "Ribbon";
            ribbon.transform.SetParent(go.transform, false);
            ribbon.transform.localScale = size;
            ribbon.GetComponent<Renderer>().sharedMaterial = ribbonMat;
        }
    }

    // A short flame standing on the floor under the table (top y = 5), flickering on and off.
    static void TableFlame(Transform root, string name, float x, float phase)
    {
        GameObject flame = Box(root, name, x, 5.45f, 0.4f, 0.9f, flameMat, "Obstacle");
        FlameFlicker ff = flame.AddComponent<FlameFlicker>();
        ff.onTime = 0.9f;
        ff.offTime = 1.3f;
        ff.phase = phase;
    }

    // Background only, no colliders: deep purple backdrop, polka dots on the tablecloth,
    // giant plates, a faded clock stuck at one time, and stretched balloons.
    static void Dressing(Transform root)
    {
        Deco(root, "Backdrop", 70f, 14f, 170f, 40f, 3f, purpleMat);

        // Polka dots on the front of every tablecloth floor (x1, x2, bottom, top) and the small table.
        var cloth = new (float x1, float x2, float y1, float y2)[]
        {
            (0f, 10f, -3f, 0f), (18f, 32f, -3f, 0f), (52f, 64f, -3f, 5f),
            (72f, 84f, -3f, 11f), (84f, 142f, -3f, 5f), (88f, 100f, 6f, 14f),
        };
        int n = 0;
        foreach (var c in cloth)
        {
            int row = 0;
            for (float y = c.y2 - 0.7f; y > c.y1 + 0.3f; y -= 1.4f, row++)
                for (float x = c.x1 + 0.6f + (row % 2) * 0.7f; x < c.x2 - 0.3f; x += 1.4f)
                    Shape(root, PrimitiveType.Sphere, "Dot " + ++n, new Vector3(x, y, -0.53f),
                        new Vector3(0.45f, 0.45f, 0.05f), 0f, n % 2 == 0 ? lemonMat : mintMat);
        }

        // Giant plates: a white rim with a cream middle.
        var plates = new (float x, float y, float d)[] { (14f, 9f, 9f), (44f, 14f, 11f), (80f, 19f, 10f) };
        foreach (var p in plates)
        {
            Disc(root, "Giant plate " + p.x, p.x, p.y, p.d, 2.5f, plateMat);
            Disc(root, "Giant plate middle " + p.x, p.x, p.y, p.d * 0.7f, 2.45f, creamMat);
        }

        // Faded clock, stuck: the hands never move.
        Vector3 clock = new Vector3(60f, 17f, 2.4f);
        Disc(root, "Clock", clock.x, clock.y, 7f, clock.z, clockMat);
        for (int h = 0; h < 12; h++)
            Hand(root, "Clock tick " + h, clock, h * 30f, 2.8f, 0.6f, 0.15f, purpleMat);
        Hand(root, "Clock hour hand", clock, 100f, 0f, 1.8f, 0.3f, purpleMat);
        Hand(root, "Clock minute hand", clock, 230f, 0f, 2.7f, 0.18f, purpleMat);

        // Stretched balloons on strings, leaning a little.
        var balloons = new (float x, float y, float tilt, Material mat)[]
        {
            (6f, 11f, 6f, hotPinkMat), (27f, 14f, -4f, mintMat), (36f, 10f, 8f, lemonMat),
            (68f, 13f, -6f, hotPinkMat), (92f, 18f, 5f, lemonMat), (101f, 15f, -8f, mintMat),
        };
        foreach (var b in balloons)
        {
            Shape(root, PrimitiveType.Sphere, "Balloon " + b.x, new Vector3(b.x, b.y, 2.2f),
                new Vector3(1.4f, 3.2f, 0.3f), b.tilt, b.mat);
            Shape(root, PrimitiveType.Cube, "Balloon string " + b.x, new Vector3(b.x, b.y - 3.6f, 2.25f),
                new Vector3(0.06f, 4f, 0.05f), 0f, plateMat);
        }
    }

    // A flat circle facing the camera.
    static void Disc(Transform root, string name, float x, float y, float d, float z, Material mat)
    {
        GameObject go = Shape(root, PrimitiveType.Cylinder, name, new Vector3(x, y, z), new Vector3(d, 0.02f, d), 0f, mat);
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // A bar on the clock face pointing at `angle` (degrees clockwise from 12), starting `from` out from the middle.
    static void Hand(Transform root, string name, Vector3 center, float angle, float from, float length, float width, Material mat)
    {
        float a = angle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f);
        Vector3 pos = center + dir * (from + length / 2f) + new Vector3(0f, 0f, -0.05f);
        Shape(root, PrimitiveType.Cube, name, pos, new Vector3(width, length, 0.02f), -angle, mat);
    }

    static GameObject Shape(Transform root, PrimitiveType type, string name, Vector3 pos, Vector3 scale, float tilt, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(root, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
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
    static void Plate(Transform root, string name, float cx, float cy, Vector3 offset, float period, float phase, float width = 3f)
    {
        GameObject go = Box(root, name, cx, cy, width, 0.5f, plateMat, "Floor");
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
