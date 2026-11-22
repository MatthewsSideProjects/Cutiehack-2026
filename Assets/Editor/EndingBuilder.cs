using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// CutieHack > Build Ending Scene: makes Assets/Scenes/Ending.unity, the scene the last door of Level 3 loads.
// Sunrise bedroom: peach and gold, soft low light. The window is open, the bed is made, the moving boxes are
// unpacked, and the cat figurine from the garden sits on the sill. No background: that is custom pixel art.
// It also puts GameLevels and Ending in the build list, which SceneManager.LoadScene needs.
public static class EndingBuilder
{
    const string ScenePath = "Assets/Scenes/Ending.unity";
    const string LevelsPath = "Assets/Scenes/GameLevels.unity";

    [MenuItem("CutieHack/Build Ending Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material wood = Level1Builder.GetMaterial("EndWood", new Color32(196, 140, 98, 255));
        Material woodDark = Level1Builder.GetMaterial("EndWoodDark", new Color32(150, 100, 70, 255));
        Material sheet = Level1Builder.GetMaterial("EndSheet", new Color32(255, 236, 220, 255));
        Material blanket = Level1Builder.GetMaterial("EndBlanket", new Color32(244, 170, 150, 255));
        Material frame = Level1Builder.GetMaterial("EndFrame", new Color32(255, 246, 232, 255));
        Material glow = Level1Builder.GetMaterial("EndGlow", new Color32(255, 214, 130, 255));
        Material card = Level1Builder.GetMaterial("EndCardboard", new Color32(214, 172, 120, 255));
        Material cat = Level1Builder.GetMaterial("GardenCat", new Color32(112, 100, 116, 255)); // same as Level 1's cat
        glow.EnableKeyword("_EMISSION");
        glow.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.45f) * 1.5f);

        // Camera and light
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(255, 214, 180, 255); // peach, until the pixel-art background goes in
        camGo.transform.position = new Vector3(0f, 3.5f, -12f);
        camGo.AddComponent<AudioListener>();

        GameObject sunGo = new GameObject("Sunrise");
        Light sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.82f, 0.6f);
        sun.intensity = 1.3f;
        sun.shadows = LightShadows.Soft;
        sunGo.transform.rotation = Quaternion.Euler(18f, -40f, 0f); // low morning sun through the window

        GameObject fillGo = new GameObject("Window glow");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.color = new Color(1f, 0.85f, 0.6f);
        fill.range = 12f;
        fill.intensity = 2f;
        fillGo.transform.position = new Vector3(5f, 5f, -1.5f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color32(200, 160, 140, 255);

        Transform room = new GameObject("Bedroom").transform;

        // Floor
        Box(room, "Floor", 0f, -0.5f, 0f, 30f, 1f, 4f, 0f, wood);

        // Window: frame, glowing sunrise pane, one sash swung open, sill
        Box(room, "Window pane", 5f, 5f, 0.95f, 3.4f, 3.4f, 0.05f, 0f, glow);
        Box(room, "Frame top", 5f, 6.8f, 0.8f, 3.8f, 0.25f, 0.3f, 0f, frame);
        Box(room, "Frame left", 3.2f, 5f, 0.8f, 0.25f, 3.8f, 0.3f, 0f, frame);
        Box(room, "Frame right", 6.8f, 5f, 0.8f, 0.25f, 3.8f, 0.3f, 0f, frame);
        Box(room, "Frame middle", 5f, 5f, 0.8f, 0.12f, 3.4f, 0.2f, 0f, frame);
        GameObject sash = Box(room, "Open sash", 7.4f, 5f, 0.1f, 1.7f, 3.3f, 0.08f, 0f, frame);
        sash.transform.rotation = Quaternion.Euler(0f, -55f, 0f); // swung open into the room
        Box(room, "Sill", 5f, 3.1f, 0.5f, 4.4f, 0.25f, 1f, 0f, frame);

        // Cat figurine on the sill
        Transform c = new GameObject("Cat figurine").transform;
        c.SetParent(room, false);
        c.localPosition = new Vector3(4.2f, 3.23f, 0.4f);
        Part(c, PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.45f, 0.42f, 0.35f), 0f, cat);
        Part(c, PrimitiveType.Sphere, new Vector3(0.05f, 0.55f, 0f), new Vector3(0.32f, 0.3f, 0.3f), 0f, cat);
        Part(c, PrimitiveType.Cube, new Vector3(-0.06f, 0.72f, 0f), new Vector3(0.09f, 0.12f, 0.06f), 20f, cat);
        Part(c, PrimitiveType.Cube, new Vector3(0.16f, 0.72f, 0f), new Vector3(0.09f, 0.12f, 0.06f), -20f, cat);
        Part(c, PrimitiveType.Capsule, new Vector3(-0.28f, 0.15f, 0f), new Vector3(0.08f, 0.2f, 0.08f), -60f, cat);

        // Bed: frame, mattress, pillow, folded-back blanket, headboard
        Box(room, "Bed frame", -5f, 0.4f, 0.2f, 6f, 0.8f, 2.6f, 0f, woodDark);
        Box(room, "Headboard", -7.9f, 1.6f, 0.2f, 0.3f, 3.2f, 2.6f, 0f, woodDark);
        Box(room, "Footboard", -2.1f, 1f, 0.2f, 0.3f, 2f, 2.6f, 0f, woodDark);
        Box(room, "Mattress", -5f, 1.05f, 0.2f, 5.6f, 0.5f, 2.4f, 0f, sheet);
        Box(room, "Pillow", -7f, 1.45f, 0.2f, 1.2f, 0.35f, 1.6f, 0f, sheet);
        Box(room, "Blanket", -4.2f, 1.35f, 0.15f, 4f, 0.15f, 2.5f, 0f, blanket);
        Box(room, "Blanket fold", -6.1f, 1.45f, 0.15f, 0.4f, 0.12f, 2.5f, 0f, blanket);

        // Unpacked boxes: open flaps, and the last one flattened against the wall
        OpenBox(room, -0.2f, 0f, 0f, card);
        OpenBox(room, 1.6f, 0f, 8f, card);
        OpenBox(room, 0.7f, 1.2f, -6f, card); // stacked
        Box(room, "Flattened box", 9.5f, 1.2f, 0.7f, 1.6f, 2.4f, 0.08f, -8f, card);

        // Dust drifting in the sunbeam
        GameObject dust = new GameObject("Sunbeam dust");
        dust.transform.SetParent(room, false);
        dust.transform.position = new Vector3(3f, 3f, -0.5f);
        ParticleSystem ps = dust.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 8f;
        main.startSpeed = 0.1f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = (Color)new Color32(255, 230, 170, 200);
        main.maxParticles = 80;
        main.prewarm = true;
        var em = ps.emission; em.rateOverTime = 10f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(8f, 6f, 1f);
        var noise = ps.noise; noise.enabled = true; noise.strength = 0.15f; noise.frequency = 0.3f;
        Material dustMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/EndDust.mat");
        if (dustMat == null)
        {
            dustMat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(dustMat, "Assets/Art/Materials/EndDust.mat");
        }
        dust.GetComponent<ParticleSystemRenderer>().sharedMaterial = dustMat;

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuild(LevelsPath, ScenePath);
        Debug.Log("Ending scene saved to " + ScenePath + " and added to the build list with GameLevels.");
    }

    static void OpenBox(Transform room, float x, float y, float tilt, Material mat)
    {
        Box(room, "Open box", x, y + 0.6f, 0.2f, 1.6f, 1.2f, 1.4f, tilt, mat);
        Box(room, "Flap", x - 1.07f, y + 1.42f, 0.2f, 0.7f, 0.06f, 1.4f, -40f, mat); // flaps folded open, up and out
        Box(room, "Flap", x + 1.07f, y + 1.42f, 0.2f, 0.7f, 0.06f, 1.4f, 40f, mat);
    }

    static GameObject Box(Transform parent, string name, float x, float y, float z, float w, float h, float d, float tilt, Material mat)
    {
        GameObject go = Part(parent, PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(w, h, d), tilt, mat);
        go.name = name;
        return go;
    }

    static GameObject Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, float tilt, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(go.GetComponent<Collider>()); // looks only, nothing to stand on yet
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void AddToBuild(params string[] paths)
    {
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string p in paths)
            if (!list.Exists(s => s.path == p)) list.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
