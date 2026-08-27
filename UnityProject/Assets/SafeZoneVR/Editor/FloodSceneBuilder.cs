#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Assembles the playable "Alagamento em Casa" scene out of placeholder primitives, wired to
    /// the SafeZoneVR mission system. Run via the menu items below, or in batchmode with:
    /// -executeMethod SafeZoneVR.Editor.FloodSceneBuilder.BuildScene
    /// </summary>
    public static class FloodSceneBuilder
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_BasicScenePath = "Assets/Scenes/BasicScene.unity";
        const string k_ScenarioAssetPath = "Assets/SafeZoneVR/Data/Alagamento/Scenario_AlagamentoEmCasa.asset";
        const string k_MaterialFolder = "Assets/SafeZoneVR/Materials";

        static readonly Dictionary<Color, Material> s_MaterialCache = new Dictionary<Color, Material>();

        static readonly Color k_WallColor = new Color(0.85f, 0.83f, 0.78f);
        static readonly Color k_WoodColor = new Color(0.55f, 0.38f, 0.22f);
        static readonly Color k_MetalColor = new Color(0.5f, 0.5f, 0.55f);
        static readonly Color k_SafetyYellow = new Color(0.95f, 0.75f, 0.1f);
        static readonly Color k_SafeGreen = new Color(0.25f, 0.6f, 0.3f);
        static readonly Color k_DangerRed = new Color(0.75f, 0.15f, 0.15f);
        static readonly Color k_WaterBlue = new Color(0.2f, 0.5f, 0.85f);

        [MenuItem("SafeZone VR/Build All/Alagamento em Casa")]
        public static void BuildAll()
        {
            FloodScenarioDataBuilder.BuildData();
            BuildScene();
        }

        [MenuItem("SafeZone VR/Build Scene/Alagamento em Casa")]
        public static void BuildScene()
        {
            s_MaterialCache.Clear();

            EnsureFolder("Assets/Scenes/Fases");

            var scene = EditorSceneManager.OpenScene(k_BasicScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene, k_ScenePath);

            // Load data assets only after the scene is open: OpenScene in Single mode unloads
            // unused assets, which would turn references loaded earlier into destroyed objects
            // that silently serialize as null.
            var scenario = AssetDatabase.LoadAssetAtPath<ScenarioSO>(k_ScenarioAssetPath);
            if (scenario == null || scenario.steps.Count < 5)
            {
                Debug.LogError("[SafeZoneVR] ScenarioSO não encontrado ou incompleto. Rode 'SafeZone VR/Build Data/Alagamento em Casa' primeiro.");
                return;
            }

            var root = new GameObject("Fase_AlagamentoEmCasa").transform;
            var house = new GameObject("Casa").transform;
            house.SetParent(root, false);

            BuildWalls(house);

            var valuableSockets = BuildValuablesArea(house);
            var breakerKnob = BuildBreaker(house);
            var valveKnob = BuildValve(house);
            var kitSockets = BuildKitStation(house);
            var evacuationTrigger = BuildEvacuationZone(root);

            var managerGO = new GameObject("ScenarioManager");
            managerGO.transform.SetParent(root, false);
            var scenarioManager = managerGO.AddComponent<ScenarioManager>();
            AssignScenario(scenarioManager, scenario);

            var steps = scenario.steps;
            WireSocketGroup(managerGO, "Validator_Valuables", scenarioManager, steps[0], valuableSockets);
            WireKnobStep(managerGO, "Validator_PowerOff", scenarioManager, steps[1], breakerKnob, 0f, 0.05f);
            WireKnobStep(managerGO, "Validator_WaterValve", scenarioManager, steps[2], valveKnob, 0f, 0.06f);
            WireSocketGroup(managerGO, "Validator_Kit", scenarioManager, steps[3], kitSockets);
            WireTriggerZone(evacuationTrigger, scenarioManager, steps[4]);

            BuildWristUI(root, scenarioManager);
            BuildReportPanel(root, scenarioManager);

            var activeScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AddSceneToBuildSettings(k_ScenePath);

            if (!VerifyWiring(scenarioManager))
                return;

            Debug.Log("[SafeZoneVR] Cena 'Alagamento em Casa' criada em " + k_ScenePath);
        }

        /// <summary>
        /// Guards against references that silently serialize as null (for example when a data
        /// asset was unloaded before being assigned).
        /// </summary>
        static bool VerifyWiring(ScenarioManager scenarioManager)
        {
            var ok = true;

            if (scenarioManager.scenario == null)
            {
                Debug.LogError("[SafeZoneVR] Verificação falhou: ScenarioManager.m_Scenario está nulo.");
                ok = false;
            }

            ok &= VerifyStepAssigned<StepCompleteOnSocket>("m_Step");
            ok &= VerifyStepAssigned<StepCompleteOnKnobThreshold>("m_Step");
            ok &= VerifyStepAssigned<StepCompleteOnTriggerZone>("m_Step");

            if (!ok)
                Debug.LogError("[SafeZoneVR] A cena foi salva, mas com referências faltando. Corrija antes de jogar.");

            return ok;
        }

        static bool VerifyStepAssigned<T>(string propertyName) where T : MonoBehaviour
        {
            var ok = true;
            foreach (var validator in Object.FindObjectsByType<T>(FindObjectsSortMode.None))
            {
                var so = new SerializedObject(validator);
                if (so.FindProperty(propertyName).objectReferenceValue == null)
                {
                    Debug.LogError($"[SafeZoneVR] Verificação falhou: {validator.name}.{propertyName} está nulo.");
                    ok = false;
                }
            }
            return ok;
        }

        // ---------------------------------------------------------------
        // House structure
        // ---------------------------------------------------------------

        static void BuildWalls(Transform house)
        {
            CreateBox("Wall_West", house, new Vector3(-3f, 1.25f, -1f), new Vector3(0.2f, 2.5f, 4f), k_WallColor);
            CreateBox("Wall_East", house, new Vector3(3f, 1.25f, -1f), new Vector3(0.2f, 2.5f, 4f), k_WallColor);
            CreateBox("Wall_North", house, new Vector3(0f, 1.25f, -3f), new Vector3(6.2f, 2.5f, 0.2f), k_WallColor);
        }

        static SocketRequirement[] BuildValuablesArea(Transform house)
        {
            CreateBox("ValuablesTable", house, new Vector3(-2.2f, 0.25f, -2.3f), new Vector3(0.7f, 0.5f, 0.5f), k_WoodColor);

            MakeGrabbable(CreateBox("Item_Documentos", house, new Vector3(-2.35f, 0.55f, -2.3f), new Vector3(0.16f, 0.04f, 0.22f), new Color(0.9f, 0.85f, 0.7f)), "valuable_documents");
            MakeGrabbable(CreateBox("Item_PortaRetrato", house, new Vector3(-2.0f, 0.60f, -2.3f), new Vector3(0.14f, 0.18f, 0.04f), new Color(0.6f, 0.45f, 0.25f)), "valuable_photo");

            CreateBox("ShelfLedge", house, new Vector3(-2.82f, 1.75f, -2.3f), new Vector3(0.22f, 0.05f, 0.9f), k_WoodColor);

            var socketA = CreateSocket("Socket_Valuable_Documentos", house, new Vector3(-2.75f, 1.82f, -2.55f), new Color(0.9f, 0.85f, 0.7f));
            var socketB = CreateSocket("Socket_Valuable_Photo", house, new Vector3(-2.75f, 1.82f, -2.05f), new Color(0.6f, 0.45f, 0.25f));

            return new[]
            {
                new SocketRequirement { socket = socketA, requiredItemId = "valuable_documents" },
                new SocketRequirement { socket = socketB, requiredItemId = "valuable_photo" },
            };
        }

        static XRKnob BuildBreaker(Transform house)
        {
            var anchor = new GameObject("BreakerAnchor").transform;
            anchor.SetParent(house, false);
            anchor.localPosition = new Vector3(2.0f, 1.5f, -2.85f);

            CreateBox("BreakerPlate", anchor, new Vector3(0f, 0f, -0.03f), new Vector3(0.32f, 0.42f, 0.06f), new Color(0.18f, 0.18f, 0.2f));

            return CreateKnob("BreakerHandle", anchor, new Vector3(0f, 0f, 0.03f), Quaternion.Euler(-90f, 0f, 0f),
                new Vector3(0.09f, 0.03f, 0.09f), -60f, 60f, 120f, 1f, k_SafetyYellow);
        }

        static XRKnob BuildValve(Transform house)
        {
            var anchor = new GameObject("ValveAnchor").transform;
            anchor.SetParent(house, false);
            anchor.localPosition = new Vector3(2.0f, 0.9f, -1.6f);

            CreateCylinder("ValvePipe", anchor, new Vector3(0f, -0.4f, 0f), new Vector3(0.04f, 0.4f, 0.04f), k_MetalColor);

            // Starts open (value 1 = 0°) and closes with a half turn to -180°, so it reads as a
            // real valve without demanding a full 360° rotation from the player.
            return CreateKnob("ValveWheel", anchor, Vector3.zero, Quaternion.Euler(-90f, 0f, 0f),
                new Vector3(0.16f, 0.03f, 0.16f), -180f, 0f, 0f, 1f, k_DangerRed);
        }

        static SocketRequirement[] BuildKitStation(Transform house)
        {
            CreateBox("KitBench", house, new Vector3(0.3f, 0.4f, -1.0f), new Vector3(0.9f, 0.45f, 0.45f), k_WoodColor);

            // Bench top sits at y = 0.625; a Unity cylinder is 2 units tall, so its half-height
            // equals its Y scale. Rest each item just above the surface so nothing starts intersecting.
            MakeGrabbable(CreateCylinder("Item_Lanterna", house, new Vector3(0.0f, 0.79f, -1.0f), new Vector3(0.05f, 0.15f, 0.05f), k_SafetyYellow), "kit_flashlight");
            MakeGrabbable(CreateCylinder("Item_Agua", house, new Vector3(0.3f, 0.77f, -1.0f), new Vector3(0.045f, 0.13f, 0.045f), k_WaterBlue), "kit_water");
            MakeGrabbable(CreateBox("Item_Remedio", house, new Vector3(0.55f, 0.67f, -1.0f), new Vector3(0.1f, 0.06f, 0.07f), new Color(0.9f, 0.2f, 0.2f)), "kit_meds");

            CreateBox("Backpack", house, new Vector3(0.3f, 0.45f, -0.6f), new Vector3(0.35f, 0.4f, 0.25f), new Color(0.3f, 0.35f, 0.15f));

            var socketFlashlight = CreateSocket("Socket_Kit_Flashlight", house, new Vector3(0.12f, 0.68f, -0.6f), k_SafetyYellow);
            var socketWater = CreateSocket("Socket_Kit_Water", house, new Vector3(0.3f, 0.68f, -0.6f), k_WaterBlue);
            var socketMeds = CreateSocket("Socket_Kit_Meds", house, new Vector3(0.48f, 0.68f, -0.6f), new Color(0.9f, 0.2f, 0.2f));

            return new[]
            {
                new SocketRequirement { socket = socketFlashlight, requiredItemId = "kit_flashlight" },
                new SocketRequirement { socket = socketWater, requiredItemId = "kit_water" },
                new SocketRequirement { socket = socketMeds, requiredItemId = "kit_meds" },
            };
        }

        static GameObject BuildEvacuationZone(Transform root)
        {
            var platform = CreateBox("EvacuationPlatform", root, new Vector3(0f, 0.5f, 6f), new Vector3(3f, 1f, 3f), k_SafeGreen);
            var teleportArea = platform.AddComponent<TeleportationArea>();
            teleportArea.interactionLayers = 1 << 31; // "Teleport" layer, matches the ground plane's setup

            // No collider: it sits on top of the platform and would otherwise block the
            // teleport ray from reaching the TeleportationArea underneath.
            var decal = CreateBox("EvacuationMarkerDecal", root, new Vector3(0f, 1.01f, 6f), new Vector3(3f, 0.02f, 3f), new Color(0.6f, 0.9f, 0.6f));
            Object.DestroyImmediate(decal.GetComponent<Collider>());

            var triggerGO = new GameObject("EvacuationTriggerZone");
            triggerGO.transform.SetParent(root, false);
            triggerGO.transform.localPosition = new Vector3(0f, 1.6f, 6f);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.size = new Vector3(3f, 1.6f, 3f);
            triggerCollider.isTrigger = true;

            return triggerGO;
        }

        // ---------------------------------------------------------------
        // Mission wiring
        // ---------------------------------------------------------------

        static void AssignScenario(ScenarioManager scenarioManager, ScenarioSO scenario)
        {
            var so = new SerializedObject(scenarioManager);
            so.FindProperty("m_Scenario").objectReferenceValue = scenario;
            so.ApplyModifiedProperties();
        }

        static void WireSocketGroup(GameObject parent, string name, ScenarioManager scenarioManager, MissionStepSO step, SocketRequirement[] requirements)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var validator = go.AddComponent<StepCompleteOnSocket>();

            var so = new SerializedObject(validator);
            so.FindProperty("m_ScenarioManager").objectReferenceValue = scenarioManager;
            so.FindProperty("m_Step").objectReferenceValue = step;

            var requirementsProp = so.FindProperty("m_Requirements");
            requirementsProp.arraySize = requirements.Length;
            for (var i = 0; i < requirements.Length; i++)
            {
                var element = requirementsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("socket").objectReferenceValue = requirements[i].socket;
                element.FindPropertyRelative("requiredItemId").stringValue = requirements[i].requiredItemId;
            }

            so.ApplyModifiedProperties();
        }

        static void WireKnobStep(GameObject parent, string name, ScenarioManager scenarioManager, MissionStepSO step, XRKnob knob, float targetValue, float tolerance)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var validator = go.AddComponent<StepCompleteOnKnobThreshold>();

            var so = new SerializedObject(validator);
            so.FindProperty("m_ScenarioManager").objectReferenceValue = scenarioManager;
            so.FindProperty("m_Step").objectReferenceValue = step;
            so.FindProperty("m_Knob").objectReferenceValue = knob;
            so.FindProperty("m_TargetValue").floatValue = targetValue;
            so.FindProperty("m_Tolerance").floatValue = tolerance;
            so.ApplyModifiedProperties();
        }

        static void WireTriggerZone(GameObject triggerGO, ScenarioManager scenarioManager, MissionStepSO step)
        {
            var validator = triggerGO.AddComponent<StepCompleteOnTriggerZone>();
            var so = new SerializedObject(validator);
            so.FindProperty("m_ScenarioManager").objectReferenceValue = scenarioManager;
            so.FindProperty("m_Step").objectReferenceValue = step;
            so.ApplyModifiedProperties();
        }

        // ---------------------------------------------------------------
        // Diegetic UI
        // ---------------------------------------------------------------

        static void BuildWristUI(Transform root, ScenarioManager scenarioManager)
        {
            var canvasGO = new GameObject("WristUI", typeof(RectTransform));
            canvasGO.transform.SetParent(root, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var rect = canvasGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 150f);
            canvasGO.transform.localScale = Vector3.one * 0.0008f;

            AddBackground(canvasGO.transform, new Color(0.05f, 0.08f, 0.1f, 0.85f));

            var title = CreateText("Title", canvasGO.transform, new Vector2(0f, 50f), new Vector2(240f, 40f), 18f, TextAlignmentOptions.Center, Color.white);
            var instruction = CreateText("Instruction", canvasGO.transform, new Vector2(0f, 0f), new Vector2(240f, 80f), 13f, TextAlignmentOptions.Center, new Color(0.85f, 0.9f, 1f));
            var progress = CreateText("Progress", canvasGO.transform, new Vector2(0f, -55f), new Vector2(240f, 25f), 12f, TextAlignmentOptions.Center, new Color(0.6f, 0.85f, 0.6f));

            var wristUI = canvasGO.AddComponent<WristUIController>();
            var so = new SerializedObject(wristUI);
            so.FindProperty("m_ScenarioManager").objectReferenceValue = scenarioManager;
            so.FindProperty("m_TitleText").objectReferenceValue = title;
            so.FindProperty("m_InstructionText").objectReferenceValue = instruction;
            so.FindProperty("m_ProgressText").objectReferenceValue = progress;
            so.ApplyModifiedProperties();
        }

        static void BuildReportPanel(Transform root, ScenarioManager scenarioManager)
        {
            var canvasGO = new GameObject("ReportPanel", typeof(RectTransform));
            canvasGO.transform.SetParent(root, false);
            canvasGO.transform.localPosition = new Vector3(0f, 1.8f, 6.9f);
            canvasGO.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var rect = canvasGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500f, 700f);
            canvasGO.transform.localScale = Vector3.one * 0.0015f;

            // The toggled panel must be a child of the canvas, never the canvas itself: the
            // controller lives on the canvas and would unsubscribe itself when hidden.
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            AddBackground(panelGO.transform, new Color(0.95f, 0.95f, 0.9f, 0.97f));

            var summary = CreateText("Summary", panelGO.transform, new Vector2(0f, 260f), new Vector2(460f, 180f), 20f, TextAlignmentOptions.Center, new Color(0.1f, 0.1f, 0.1f));
            var recap = CreateText("Recap", panelGO.transform, new Vector2(0f, -80f), new Vector2(460f, 400f), 15f, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.15f, 0.15f));

            var reportController = canvasGO.AddComponent<EndReportPanelController>();
            var so = new SerializedObject(reportController);
            so.FindProperty("m_ScenarioManager").objectReferenceValue = scenarioManager;
            so.FindProperty("m_PanelRoot").objectReferenceValue = panelGO;
            so.FindProperty("m_SummaryText").objectReferenceValue = summary;
            so.FindProperty("m_RecapText").objectReferenceValue = recap;
            so.ApplyModifiedProperties();
        }

        static void AddBackground(Transform canvasTransform, Color color)
        {
            var bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(canvasTransform, false);
            var rect = bgGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = bgGO.AddComponent<Image>();
            image.color = color;
            bgGO.transform.SetAsFirstSibling();
        }

        static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = string.Empty;
            return text;
        }

        // ---------------------------------------------------------------
        // Primitive helpers
        // ---------------------------------------------------------------

        static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            ApplyColor(go, color);
            return go;
        }

        static GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            ApplyColor(go, color);
            return go;
        }

        static GameObject MakeGrabbable(GameObject go, string itemId)
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            go.AddComponent<XRGrabInteractable>();
            var marker = go.AddComponent<ObjectiveItemId>();
            marker.SetItemId(itemId);
            return go;
        }

        static XRSocketInteractor CreateSocket(string name, Transform parent, Vector3 localPosition, Color padColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var sphereCollider = go.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.09f;
            sphereCollider.isTrigger = true;

            var socket = go.AddComponent<XRSocketInteractor>();

            var pad = CreateCylinder("Pad", go.transform, Vector3.zero, new Vector3(0.12f, 0.005f, 0.12f), padColor);
            Object.DestroyImmediate(pad.GetComponent<Collider>());

            return socket;
        }

        static XRKnob CreateKnob(string name, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 handleScale,
            float minAngle, float maxAngle, float angleIncrement, float initialValue, Color color)
        {
            var pivot = new GameObject(name + " Pivot");
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = localPosition;
            pivot.transform.localRotation = localRotation;

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = name;
            handle.transform.SetParent(pivot.transform, false);
            handle.transform.localPosition = Vector3.zero;
            handle.transform.localRotation = Quaternion.identity;
            handle.transform.localScale = handleScale;
            ApplyColor(handle, color);

            var knob = handle.AddComponent<XRKnob>();
            var so = new SerializedObject(knob);
            so.FindProperty("m_Handle").objectReferenceValue = handle.transform;
            so.FindProperty("m_ClampedMotion").boolValue = true;
            so.FindProperty("m_MinAngle").floatValue = minAngle;
            so.FindProperty("m_MaxAngle").floatValue = maxAngle;
            so.FindProperty("m_AngleIncrement").floatValue = angleIncrement;
            so.FindProperty("m_Value").floatValue = initialValue;
            so.ApplyModifiedProperties();

            return knob;
        }

        static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = GetOrCreateMaterial(color);
        }

        static Material GetOrCreateMaterial(Color color)
        {
            if (s_MaterialCache.TryGetValue(color, out var cached))
                return cached;

            EnsureFolder(k_MaterialFolder);
            var path = $"{k_MaterialFolder}/Placeholder_{ColorUtility.ToHtmlStringRGB(color)}.mat";

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { color = color };
                AssetDatabase.CreateAsset(material, path);
            }

            s_MaterialCache[color] = material;
            return material;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var existing in scenes)
            {
                if (existing.path == scenePath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
