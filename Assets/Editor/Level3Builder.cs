using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu: CutieHack > Build Level 3
// Level 3: The Empty House. Packing day. Built under a "Level3" object at x = 2000 in the same scene;
// the Level 2 door (top of the cake) sends her to Level3/Spawn.
// One tile = one unit, coordinates are local to Level3. Floor tops are at y = 0 unless noted.
// Grays and beiges, one warm door. A dark wall creeps in from the left the moment she arrives
// and keeps coming until the end (CreepingWall).
//
// 1 Hallway   (x 0-30):    low ceiling, crawl (Small) under a lintel.
// 2 Kitchen   (x 30-60):   push a crate (Big) to the counter, hop up (Regular), crawl (Small)
//                          between the counter and the upper cabinet.
// 3 Stairs    (x 60-88):   push the crate off the upper ledge, hop over it to get behind it,
//                          push it into the gap so it lands on a hidden shelf, flush with the floor.
// 4 Her room  (x 88-116):  pale rectangles where pictures hung, a window of light, both items.
// 5 Front door(x 116-175): crawl under a lintel, bridge a pit with a crate, final leap off a
//                          step over a wide gap, door at Regular size with both items.
//
// Sizes (scale 1.25, capsule 1.2 tall): Small ~0.75 tall, Regular ~1.5, Big ~3 (and 2 wide).
// A jump rises about 2.5 and carries about 7 units at a full run. Lintel gaps are 1 unit.
public static class Level3Builder
{
    const string RootName = "Level3";
    const float RootX = 2000f;
    const float Top = 16f; // ceilings and end walls are solid up to here, so there's nothing to stand on above

    static Material floorMat, wallpaperMat, ceilingMat, boxMat, crateMat, counterMat, cabinetMat, paleMat, windowMat, creepMat, warmMat;

    [MenuItem("CutieHack/Build Level 3")]
    public static void Build()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            bool ok = EditorUtility.DisplayDialog("Rebuild Level 3",
                "An object named \"" + RootName + "\" already exists. Delete it and everything inside it, then rebuild?",
                "Rebuild", "Cancel");
            if (!ok) return;
            Undo.DestroyObjectImmediate(existing);
        }

        floorMat = Level1Builder.GetMaterial("Level3Floor", new Color32(196, 182, 158, 255));
        wallpaperMat = Level1Builder.GetMaterial("Level3Wallpaper", new Color32(168, 166, 162, 255));
        ceilingMat = Level1Builder.GetMaterial("Level3Ceiling", new Color32(140, 138, 135, 255));
        boxMat = Level1Builder.GetMaterial("Level3Box", new Color32(178, 158, 122, 255));
        crateMat = Level1Builder.GetMaterial("Level3Crate", new Color32(205, 180, 135, 255));
        counterMat = Level1Builder.GetMaterial("Level3Counter", new Color32(128, 128, 130, 255));
        cabinetMat = Level1Builder.GetMaterial("Level3Cabinet", new Color32(210, 200, 182, 255));
        paleMat = Level1Builder.GetMaterial("Level3Pale", new Color32(214, 211, 204, 255));
        windowMat = Level1Builder.GetMaterial("Level3Window", new Color32(255, 248, 222, 255));
        creepMat = Level1Builder.GetMaterial("Level3Creep", new Color32(38, 36, 40, 255));
        warmMat = Level1Builder.GetMaterial("Level3Warm", new Color32(245, 160, 70, 255));

        GameObject rootGo = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootGo, "Build Level 3");
        Transform root = rootGo.transform;
        root.position = new Vector3(RootX, 0f, 0f);

        Transform spawn = new GameObject("Spawn").transform;
        spawn.SetParent(root, false);
        spawn.localPosition = new Vector3(2f, 1.5f, 0f);

        // ---- Floors, gaps, end walls ----
        Solid(root, "Back wall", -4, -1, -3, Top, wallpaperMat);
        Solid(root, "Floor A", -4, 80, -3, 0, floorMat);   // hallway, kitchen, stairs
        Solid(root, "Floor B", 88, 130, -3, 0, floorMat);  // gap 80-88 under the stairs ledge
        Solid(root, "Floor C", 138, 150, -3, 0, floorMat); // pit 130-138 by the front door
        Solid(root, "Floor D", 158, 175, -3, 0, floorMat); // final gap 150-158
        Solid(root, "Front wall", 172, 175, 0, Top, wallpaperMat);

        // ---- 1 Hallway: low ceiling, lintel ----
        Solid(root, "Hallway ceiling", -4, 30, 4.5f, Top, ceilingMat);
        Solid(root, "Hallway lintel", 20, 24, 1f, 4.5f, ceilingMat);
        Level1Builder.AddCheckpoint(root, "Checkpoint 1", 27);

        // ---- 2 Kitchen: crate to the counter, crawl under the cabinet ----
        Solid(root, "Kitchen ceiling", 30, 60, 8f, Top, ceilingMat);
        Crate(root, "Kitchen crate", 36f, 0f);
        Solid(root, "Counter", 44, 58, 0f, 4f, counterMat);   // 4 high: only reachable from the crate
        Solid(root, "Upper cabinet", 46, 56, 5f, 8f, cabinetMat); // 1-unit gap above the counter
        Level1Builder.AddCheckpoint(root, "Checkpoint 2", 60);

        // ---- 3 Stairs: crate off the ledge, then into the gap ----
        Solid(root, "Stairs ceiling", 60, 88, 12f, Top, ceilingMat);
        Solid(root, "Step 1", 62, 64, 0f, 2f, floorMat);
        Solid(root, "Step 2", 64, 66, 0f, 4f, floorMat);
        Solid(root, "Upper ledge", 66, 74, 5f, 6f, floorMat); // open underneath, so she can get behind the crate
        Crate(root, "Stairs crate", 71f, 6f);
        // Hidden shelf in the gap: the crate falls straight down onto it when its left edge passes x 80,
        // and sits flush with the floor. That leaves a 6-wide jump; without it the gap is 8.
        Solid(root, "Stairs shelf", 80, 82, -4f, -2f, floorMat);
        Level1Builder.AddCheckpoint(root, "Checkpoint 3", 90);

        // ---- 4 Her empty room ----
        Solid(root, "Room ceiling", 88, 116, 7f, Top, ceilingMat);
        Level1Builder.AddItem(root, "Photo", 98f, 0.5f);
        Level1Builder.AddItem(root, "Bunny", 108f, 0.5f);
        Level1Builder.AddCheckpoint(root, "Checkpoint 4", 114);

        // ---- 5 Front door: lintel, bridge, final leap ----
        Solid(root, "Front hall ceiling", 116, 142, 5.5f, Top, ceilingMat);
        Solid(root, "Front lintel", 118, 122, 1f, 5.5f, ceilingMat);
        Crate(root, "Front crate", 126f, 0f);
        Solid(root, "Front shelf", 130, 132, -4f, -2f, floorMat); // same trick as the stairs
        Level1Builder.AddCheckpoint(root, "Checkpoint 5", 139);
        Solid(root, "Entry ceiling", 142, 175, 8f, Top, ceilingMat);
        Solid(root, "Final step", 144, 150, 0f, 2f, floorMat); // jump off this: 8 wide, a little drop on the far side
        DoorScript door = Level1Builder.AddDoor(root, "Door", 164, null);
        door.itemsNeeded = 2;
        Deco(root, "Door glow", 164.5f, 1.6f, 2.6f, 3.4f, 0.6f, 0f, warmMat);

        // ---- The wall ----
        GameObject wall = Box(root, "Creeping wall", -3f, 5f, 4f, 22f, creepMat, "Obstacle");
        wall.transform.localScale = new Vector3(4f, 22f, 1.4f);
        CreepingWall cw = wall.AddComponent<CreepingWall>();
        cw.startX = RootX - 1f;
        cw.endX = RootX + 150f; // stops at the final gap

        Dressing(root);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        LinkLevel2Door(spawn);
        Selection.activeGameObject = rootGo;
        Debug.Log("Level 3 built under '" + RootName + "'. Save the scene with Cmd+S.");
    }

    // Looks only, behind the play layer. Stacked boxes and picture marks lean more the further in
    // she gets (the house distorts), except in her room, which stays still.
    static void Dressing(Transform root)
    {
        Deco(root, "Wallpaper", 85f, 6f, 180f, 20f, 1.5f, 0f, wallpaperMat);

        System.Random rng = new System.Random(3);
        float[] stackX = { 6f, 13f, 33f, 40f, 61f, 78f, 124f, 140f, 154f, 168f };
        foreach (float sx in stackX)
        {
            float lean = 1f + sx / 30f; // degrees, grows toward the front door
            float y = 0f;
            int count = 2 + rng.Next(4);
            for (int i = 0; i < count; i++)
            {
                float w = 1.6f + (float)rng.NextDouble();
                float h = 1.1f + (float)rng.NextDouble() * 0.6f;
                float tilt = ((float)rng.NextDouble() * 2f - 1f) * lean;
                Deco(root, "Box stack " + sx + "-" + i, sx + (float)rng.NextDouble() * 0.6f, y + h / 2f, w, h, 1.2f, tilt, boxMat);
                y += h;
            }
        }

        // Pale rectangles where pictures hung: a few in the hall, the most in her room.
        var marks = new (float x, float y, float w, float h, float tilt)[]
        {
            (9f, 2.8f, 1.6f, 1.2f, 4f), (16f, 2.6f, 1.0f, 1.4f, -6f), (52f, 6.2f, 2.0f, 1.3f, 9f),
            (92f, 4.2f, 1.4f, 1.8f, 0f), (95f, 3.6f, 2.2f, 1.5f, 0f), (99f, 4.8f, 1.0f, 1.0f, 0f),
            (110f, 4.0f, 1.8f, 2.2f, 0f), (113f, 3.4f, 1.2f, 1.0f, 0f), (134f, 3.4f, 1.6f, 1.2f, -12f),
        };
        for (int i = 0; i < marks.Length; i++)
            Deco(root, "Picture mark " + (i + 1), marks[i].x, marks[i].y, marks[i].w, marks[i].h, 1.4f, marks[i].tilt, paleMat);

        // Window of light in her room, with a slanted beam down to the floor.
        Deco(root, "Window", 103.5f, 4.2f, 3f, 2.6f, 1.4f, 0f, windowMat);
        Deco(root, "Window beam", 102f, 2f, 3f, 4.4f, 1.3f, 18f, windowMat);
    }

    // A 2x2 crate standing on a floor at y = floorY: pushable in Big form, and a step to stand on.
    static void Crate(Transform root, string name, float x, float floorY)
    {
        GameObject go = Box(root, name, x, floorY + 1f, 2f, 2f, crateMat, "Floor");
        go.AddComponent<Rigidbody>().mass = 2f;
        go.AddComponent<Pushable>();
    }

    // Solid block between edges x1..x2 and y1..y2, tagged Floor so she can stand on it.
    static GameObject Solid(Transform root, string name, float x1, float x2, float y1, float y2, Material mat)
    {
        return Box(root, name, (x1 + x2) / 2f, (y1 + y2) / 2f, x2 - x1, y2 - y1, mat, "Floor");
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

    // Decoration: no collider, at depth z behind the play layer, tilted by `tilt` degrees.
    static void Deco(Transform root, string name, float cx, float cy, float w, float h, float z, float tilt, Material mat)
    {
        GameObject go = Box(root, name, cx, cy, w, h, mat, null);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.localPosition = new Vector3(cx, cy, z);
        go.transform.localScale = new Vector3(w, h, 0.1f);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    internal static Transform GetSpawn()
    {
        GameObject level3 = GameObject.Find(RootName);
        return level3 != null ? level3.transform.Find("Spawn") : null;
    }

    static void LinkLevel2Door(Transform spawn)
    {
        GameObject level2 = GameObject.Find("Level2");
        Transform doorT = level2 != null ? level2.transform.Find("Cake/Door") : null;
        DoorScript door = doorT != null ? doorT.GetComponent<DoorScript>() : null;
        if (door == null)
        {
            Debug.LogWarning("No Level2/Door found. Build Level 2, then Level 3 again, so the cake door leads here.");
            return;
        }
        Undo.RecordObject(door, "Build Level 3");
        door.destination = spawn;
        EditorUtility.SetDirty(door);
    }
}
