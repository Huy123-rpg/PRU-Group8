#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class DogRunRigBuilder
{
    private const string Folder = "Assets/Scenes/Hóa/Animations";
    private static readonly string[] LegNames = { "BackNear", "BackFar", "FrontNear", "FrontFar" };
    private static readonly int[] LegX = { 56, 84, 112, 140 };

    [MenuItem("Tools/DogRacer/Build Run Animation")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building the rigs.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Scene6_Race.unity")
            throw new InvalidOperationException("Open Scene6_Race first. No other scene will be modified.");
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        AnimatorController controller = GetController();
        Sprite streak = LoadSprite("Assets/Scenes/Hóa/fx_speed_boost.png");
        Sprite lightning = LoadSprite("Assets/Scenes/Hóa/icon_lightning.png");
        Transform group = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).First(t => t.name == "4Dog");
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Build DogRacer running rigs");

        for (int index = 1; index <= 4; index++)
        {
            Transform dog = group.Find("Dog" + index);
            if (dog == null) throw new InvalidOperationException("Missing Dog" + index);
            SpriteRenderer originalRenderer = dog.GetComponent<SpriteRenderer>();
            if (originalRenderer == null || originalRenderer.sprite == null) throw new InvalidOperationException("Missing original dog sprite.");
            Sprite source = originalRenderer.sprite;
            DogRunSpriteSet set = GetParts(source, index);

            foreach (string owned in new[] { "__DogVisuals", "__DogBoost" })
            {
                Transform old = dog.Find(owned);
                if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            }
            SortingGroup sorting = dog.GetComponent<SortingGroup>();
            if (sorting == null) sorting = Undo.AddComponent<SortingGroup>(dog.gameObject);
            Undo.RecordObject(sorting, "Set dog sorting group");
            sorting.sortingLayerID = originalRenderer.sortingLayerID;
            sorting.sortingOrder = originalRenderer.sortingOrder;
            Transform visual = NewChild(dog, "__DogVisuals");
            var renderers = new List<SpriteRenderer>();
            SpriteRenderer body = Part(visual, "Body", set.body, originalRenderer, 0);
            body.transform.localPosition = PartPosition(source, set.body);
            renderers.Add(body);
            var legs = new Transform[4];
            for (int leg = 0; leg < 4; leg++)
            {
                SpriteRenderer renderer = Part(visual, LegNames[leg], set.legs[leg], originalRenderer, leg % 2 == 0 ? 1 : -1);
                renderer.transform.localPosition = PartPosition(source, set.legs[leg]);
                legs[leg] = renderer.transform;
                renderers.Add(renderer);
            }
            Transform boost = NewChild(dog, "__DogBoost");
            SpriteRenderer trail = Part(boost, "SpeedStreak", streak, originalRenderer, -3);
            trail.transform.localScale = Vector3.one * 0.7f;
            trail.transform.localPosition = new Vector3(source.bounds.min.x + 0.15f - streak.bounds.max.x * 0.7f,
                source.bounds.min.y + source.bounds.size.y * 0.45f - streak.bounds.center.y * 0.7f, 0f);
            SpriteRenderer badge = Part(boost, "LightningBadge", lightning, originalRenderer, 3);
            badge.transform.localScale = Vector3.one * 0.4f;
            badge.transform.localPosition = new Vector3(source.bounds.center.x + source.bounds.size.x * 0.23f - lightning.bounds.center.x * 0.4f,
                source.bounds.max.y + 0.1f - lightning.bounds.min.y * 0.4f, 0f);
            boost.gameObject.SetActive(false);

            Animator animator = dog.GetComponent<Animator>();
            if (animator == null) animator = Undo.AddComponent<Animator>(dog.gameObject);
            Undo.RecordObject(animator, "Assign running controller");
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            DogRunAnimator driver = dog.GetComponent<DogRunAnimator>();
            if (driver == null) driver = Undo.AddComponent<DogRunAnimator>(dog.gameObject);
            Undo.RecordObject(driver, "Wire dog visuals");
            driver.animator = animator;
            driver.visualRoot = visual;
            driver.legs = legs;
            driver.coloredParts = renderers.ToArray();
            driver.boostRoot = boost.gameObject;
            driver.boostStreak = trail;
            driver.boostBadge = badge.transform;
            EditorUtility.SetDirty(driver);
            Undo.RecordObject(originalRenderer, "Use articulated dog parts");
            originalRenderer.enabled = false;
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[DogRacer] Built four running rigs, Animator clips and boost effects from existing sprites.");
    }

    private static Transform NewChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Create dog visual");
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static SpriteRenderer Part(Transform parent, string name, Sprite sprite, SpriteRenderer source, int order)
    {
        Transform child = NewChild(parent, name);
        SpriteRenderer renderer = Undo.AddComponent<SpriteRenderer>(child.gameObject);
        renderer.sprite = sprite;
        renderer.sharedMaterial = source.sharedMaterial;
        renderer.color = source.color;
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = order;
        return renderer;
    }

    private static Vector3 PartPosition(Sprite original, Sprite part)
    {
        Vector2 pixel = part.rect.min + part.pivot - original.rect.min - original.pivot;
        return new Vector3(pixel.x / original.pixelsPerUnit, pixel.y / original.pixelsPerUnit, 0f);
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite == null) throw new InvalidOperationException("Missing sprite: " + path);
        return sprite;
    }

    private static DogRunSpriteSet GetParts(Sprite original, int index)
    {
        string path = Folder + "/DogRun_" + index + ".asset";
        DogRunSpriteSet set = AssetDatabase.LoadAssetAtPath<DogRunSpriteSet>(path);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<DogRunSpriteSet>();
            set.original = original;
            AssetDatabase.CreateAsset(set, path);
        }
        Sprite[] embedded = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (set.body == null)
        {
            set.body = embedded.FirstOrDefault(s => s.name == "Body");
            if (set.body == null)
                set.body = CreatePart(original, new Rect(original.rect.xMin, 35f, original.rect.width, original.rect.yMax - 35f), Vector2.zero, "Body", set);
        }
        if (set.legs == null || set.legs.Length != 4) set.legs = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            if (set.legs[i] != null) continue;
            set.legs[i] = embedded.FirstOrDefault(s => s.name == LegNames[i]);
            if (set.legs[i] == null)
                set.legs[i] = CreatePart(original, new Rect(LegX[i], original.rect.yMin, 14f, 35f - original.rect.yMin), new Vector2(0.5f, 1f), LegNames[i], set);
        }
        EditorUtility.SetDirty(set);
        return set;
    }

    private static Sprite CreatePart(Sprite source, Rect rect, Vector2 pivot, string name, DogRunSpriteSet owner)
    {
        Sprite sprite = Sprite.Create(source.texture, rect, pivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = name;
        AssetDatabase.AddObjectToAsset(sprite, owner);
        return sprite;
    }

    private static AnimatorController GetController()
    {
        string path = Folder + "/DogRun.controller";
        AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        AnimationClip idle = CreateClip(false);
        AnimationClip run = CreateClip(true);
        AnimatorController controller = existing != null ? existing : AnimatorController.CreateAnimatorControllerAtPath(path);
        if (!controller.parameters.Any(p => p.name == "Running")) controller.AddParameter("Running", AnimatorControllerParameterType.Bool);
        if (!controller.parameters.Any(p => p.name == "RunSpeed"))
            controller.AddParameter(new AnimatorControllerParameter { name = "RunSpeed", type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
        if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idleState = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Idle");
        if (idleState == null) idleState = machine.AddState("Idle");
        idleState.motion = idle;
        machine.defaultState = idleState;
        AnimatorState runState = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Run");
        if (runState == null) runState = machine.AddState("Run");
        runState.motion = run;
        runState.speedParameterActive = true;
        runState.speedParameter = "RunSpeed";
        AnimatorStateTransition toRun = idleState.transitions.FirstOrDefault(t => t.destinationState == runState);
        if (toRun == null) toRun = idleState.AddTransition(runState);
        toRun.hasExitTime = false;
        toRun.duration = 0.06f;
        toRun.conditions = new[] { new AnimatorCondition { mode = AnimatorConditionMode.If, parameter = "Running", threshold = 0f } };
        AnimatorStateTransition toIdle = runState.transitions.FirstOrDefault(t => t.destinationState == idleState);
        if (toIdle == null) toIdle = runState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0.06f;
        toIdle.conditions = new[] { new AnimatorCondition { mode = AnimatorConditionMode.IfNot, parameter = "Running", threshold = 0f } };
        return controller;
    }

    private static AnimationClip CreateClip(bool running)
    {
        string path = Folder + (running ? "/Run.anim" : "/Idle.anim");
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) return existing;
        var clip = new AnimationClip { name = running ? "Run" : "Idle", frameRate = 16f };
        const float duration = 0.56f;
        for (int leg = 0; leg < 4; leg++)
        {
            float phase = leg == 0 || leg == 3 ? 0f : Mathf.PI;
            SetCurve(clip, "__DogVisuals/" + LegNames[leg], "localEulerAnglesRaw.z", duration,
                t => running ? Mathf.Sin(t * Mathf.PI * 2f + phase) * 26f : 0f);
        }
        SetCurve(clip, "__DogVisuals", "m_LocalPosition.y", duration,
            t => running ? (1f - Mathf.Cos(t * Mathf.PI * 4f)) * 0.012f : 0f);
        SetCurve(clip, "__DogVisuals", "localEulerAnglesRaw.z", duration,
            t => running ? Mathf.Sin(t * Mathf.PI * 2f) * 1.2f : 0f);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void SetCurve(AnimationClip clip, string path, string property, float duration, Func<float, float> value)
    {
        var curve = new AnimationCurve();
        for (int i = 0; i <= 8; i++) curve.AddKey(duration * i / 8f, value(i / 8f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
    }
}
#endif
