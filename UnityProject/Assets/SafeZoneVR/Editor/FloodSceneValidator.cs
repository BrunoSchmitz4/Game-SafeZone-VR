#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VRTemplate;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Checks that the "Alagamento em Casa" phase is actually playable: every mission step has a
    /// validator that can fire it, every validator points at the right data, the XR rig can reach
    /// and interact with the objectives, and the mission state machine runs from the first step to
    /// the end-of-phase report. Run via the menu item, or in batchmode with:
    /// -executeMethod SafeZoneVR.Editor.FloodSceneValidator.ValidateBatch
    /// </summary>
    public static class FloodSceneValidator
    {
        const string k_ScenePath = "Assets/Scenes/Fases/Alagamento_EmCasa.unity";
        const string k_ReportPath = "Logs/SafeZoneVR_Validacao.txt";
        const int k_TeleportLayer = 31;

        static readonly StringBuilder s_Report = new StringBuilder();
        static int s_Failures;
        static int s_Warnings;
        static int s_Checks;

        [MenuItem("SafeZone VR/Validar Fase/Alagamento em Casa")]
        public static void Validate()
        {
            if (SceneManager.GetActiveScene().path != k_ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);
            }

            RunAllChecks();
        }

        /// <summary>Entry point for headless runs; exits non-zero when a check fails.</summary>
        public static void ValidateBatch()
        {
            EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);
            RunAllChecks();
            EditorApplication.Exit(s_Failures == 0 ? 0 : 1);
        }

        static void RunAllChecks()
        {
            s_Report.Clear();
            s_Failures = 0;
            s_Warnings = 0;
            s_Checks = 0;

            Section("Dados da fase");
            var manager = CheckScenarioData(out var steps);

            Section("Validadores das missoes");
            if (manager != null)
                CheckValidators(manager, steps);

            Section("Rig do jogador (XR)");
            CheckPlayerRig();

            Section("Interface diegetica");
            CheckUI(manager);

            Section("Locomocao e alcance");
            CheckReachability();

            Section("Maquina de estados da missao");
            if (manager != null && manager.scenario != null)
                SimulateRun(manager.scenario);

            WriteReport();
        }

        // ---------------------------------------------------------------
        // Data
        // ---------------------------------------------------------------

        static ScenarioManager CheckScenarioData(out List<MissionStepSO> steps)
        {
            steps = new List<MissionStepSO>();

            var managers = Object.FindObjectsByType<ScenarioManager>(FindObjectsSortMode.None);
            if (!Check(managers.Length == 1, "Existe exatamente 1 ScenarioManager na cena (achei " + managers.Length + ")"))
                return null;

            var manager = managers[0];
            if (!Check(manager.scenario != null, "ScenarioManager tem um ScenarioSO atribuido"))
                return null;

            var scenario = manager.scenario;
            Check(scenario.steps.Count > 0, "Cenario '" + scenario.displayName + "' tem missoes (" + scenario.steps.Count + ")");
            Check(scenario.recapCards.Count > 0, "Cenario tem cards educativos (" + scenario.recapCards.Count + ")");

            var ids = new HashSet<string>();
            for (var i = 0; i < scenario.steps.Count; i++)
            {
                var step = scenario.steps[i];
                if (!Check(step != null, "Missao " + i + " nao e nula"))
                    continue;

                steps.Add(step);
                Check(!string.IsNullOrEmpty(step.stepId), "Missao " + i + " ('" + step.title + "') tem stepId");
                Check(ids.Add(step.stepId), "stepId '" + step.stepId + "' e unico");
                Check(!string.IsNullOrEmpty(step.instructionText), "Missao '" + step.title + "' tem texto de instrucao");
            }

            foreach (var card in scenario.recapCards)
                Check(card != null && !string.IsNullOrEmpty(card.tipText), "Card '" + (card != null ? card.title : "nulo") + "' tem texto");

            return manager;
        }

        // ---------------------------------------------------------------
        // Validators
        // ---------------------------------------------------------------

        static void CheckValidators(ScenarioManager manager, List<MissionStepSO> steps)
        {
            var covered = new Dictionary<MissionStepSO, List<string>>();
            foreach (var step in steps)
                covered[step] = new List<string>();

            foreach (var validator in Object.FindObjectsByType<StepCompleteOnSocket>(FindObjectsSortMode.None))
                CheckSocketValidator(validator, manager, covered);

            foreach (var validator in Object.FindObjectsByType<StepCompleteOnKnobThreshold>(FindObjectsSortMode.None))
                CheckKnobValidator(validator, manager, covered);

            foreach (var validator in Object.FindObjectsByType<StepCompleteOnTriggerZone>(FindObjectsSortMode.None))
                CheckTriggerValidator(validator, manager, covered);

            foreach (var validator in Object.FindObjectsByType<StepCompleteOnGrab>(FindObjectsSortMode.None))
                Register(covered, Step(validator, "m_Step"), validator.name, manager, Manager(validator));

            foreach (var pair in covered)
            {
                var names = string.Join(", ", pair.Value);
                Check(pair.Value.Count > 0,
                    "Missao '" + pair.Key.title + "' tem quem a conclua" +
                    (pair.Value.Count > 0 ? " (" + names + ")" : " - NENHUM validador aponta para ela"));

                if (pair.Value.Count > 1)
                    Warn("Missao '" + pair.Key.title + "' e concluida por mais de um validador (" + names + ")");
            }
        }

        static void CheckSocketValidator(StepCompleteOnSocket validator, ScenarioManager manager, Dictionary<MissionStepSO, List<string>> covered)
        {
            Register(covered, Step(validator, "m_Step"), validator.name, manager, Manager(validator));

            var so = new SerializedObject(validator);
            var requirements = so.FindProperty("m_Requirements");
            Check(requirements.arraySize > 0, validator.name + " tem sockets configurados");

            var itemIds = Object.FindObjectsByType<ObjectiveItemId>(FindObjectsSortMode.None);

            for (var i = 0; i < requirements.arraySize; i++)
            {
                var element = requirements.GetArrayElementAtIndex(i);
                var socket = element.FindPropertyRelative("socket").objectReferenceValue as XRSocketInteractor;
                var requiredId = element.FindPropertyRelative("requiredItemId").stringValue;

                if (!Check(socket != null, validator.name + ": socket " + i + " atribuido"))
                    continue;

                var socketCollider = socket.GetComponent<Collider>();
                Check(socketCollider != null && socketCollider.isTrigger,
                    validator.name + ": '" + socket.name + "' tem collider marcado como trigger");

                if (string.IsNullOrEmpty(requiredId))
                {
                    Warn(validator.name + ": '" + socket.name + "' aceita qualquer objeto (requiredItemId vazio)");
                    continue;
                }

                var item = itemIds.FirstOrDefault(x => x.itemId == requiredId);
                if (!Check(item != null, validator.name + ": existe na cena um objeto com id '" + requiredId + "'"))
                    continue;

                Check(item.GetComponent<XRGrabInteractable>() != null, "'" + item.name + "' e agarravel (XRGrabInteractable)");
                Check(item.GetComponent<Rigidbody>() != null, "'" + item.name + "' tem Rigidbody");
                Check(item.GetComponent<Collider>() != null, "'" + item.name + "' tem Collider");
            }
        }

        static void CheckKnobValidator(StepCompleteOnKnobThreshold validator, ScenarioManager manager, Dictionary<MissionStepSO, List<string>> covered)
        {
            Register(covered, Step(validator, "m_Step"), validator.name, manager, Manager(validator));

            var so = new SerializedObject(validator);
            var knob = so.FindProperty("m_Knob").objectReferenceValue as XRKnob;
            var target = so.FindProperty("m_TargetValue").floatValue;
            var tolerance = so.FindProperty("m_Tolerance").floatValue;

            if (!Check(knob != null, validator.name + ": XRKnob atribuido"))
                return;

            Check(knob.GetComponentInChildren<Collider>() != null, "'" + knob.name + "' tem collider para ser agarrado");

            // XRKnob.Start() republishes its serialized value, so a knob already sitting on the
            // target would complete the step the instant the scene loads.
            var startValue = new SerializedObject(knob).FindProperty("m_Value").floatValue;
            Check(Mathf.Abs(startValue - target) > tolerance,
                "'" + knob.name + "' comeca fora do alvo (valor inicial " + startValue.ToString("0.00") +
                ", alvo " + target.ToString("0.00") + " +/- " + tolerance.ToString("0.00") + ") - a missao nao se auto-completa");
        }

        static void CheckTriggerValidator(StepCompleteOnTriggerZone validator, ScenarioManager manager, Dictionary<MissionStepSO, List<string>> covered)
        {
            Register(covered, Step(validator, "m_Step"), validator.name, manager, Manager(validator));

            var zoneCollider = validator.GetComponent<Collider>();
            if (!Check(zoneCollider != null, validator.name + ": tem Collider"))
                return;

            Check(zoneCollider.isTrigger, validator.name + ": collider esta marcado como trigger");

            // StepCompleteOnTriggerZone only reacts to a CharacterController, which lives on the rig.
            var origin = Object.FindFirstObjectByType<XROrigin>();
            Check(origin != null && origin.GetComponentInChildren<CharacterController>(true) != null,
                "O rig do jogador tem CharacterController (necessario para a zona de evacuacao disparar)");
        }

        static void Register(Dictionary<MissionStepSO, List<string>> covered, MissionStepSO step, string validatorName, ScenarioManager manager, ScenarioManager assigned)
        {
            Check(assigned == manager, validatorName + ": aponta para o ScenarioManager da cena");

            if (!Check(step != null, validatorName + ": tem uma missao atribuida"))
                return;

            if (covered.ContainsKey(step))
                covered[step].Add(validatorName);
            else
                Fail(validatorName + ": a missao '" + step.name + "' nao pertence ao cenario desta fase");
        }

        static MissionStepSO Step(MonoBehaviour validator, string property)
        {
            return new SerializedObject(validator).FindProperty(property).objectReferenceValue as MissionStepSO;
        }

        static ScenarioManager Manager(MonoBehaviour validator)
        {
            return new SerializedObject(validator).FindProperty("m_ScenarioManager").objectReferenceValue as ScenarioManager;
        }

        // ---------------------------------------------------------------
        // Rig, UI, reachability
        // ---------------------------------------------------------------

        static void CheckPlayerRig()
        {
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (!Check(origin != null, "Existe um XR Origin (rig do jogador) na cena"))
                return;

            Check(origin.Camera != null, "O rig tem camera");
            Check(origin.Camera != null && origin.Camera.CompareTag("MainCamera"), "A camera do rig esta com a tag MainCamera");
            Check(origin.GetComponentInChildren<CharacterController>(true) != null, "O rig tem CharacterController");
            Check(Object.FindFirstObjectByType<XRInteractionManager>() != null, "Existe um XR Interaction Manager");
            Check(Object.FindFirstObjectByType<EventSystem>() != null, "Existe um EventSystem (para a UI responder)");

            var hasLeftController = origin.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Left Controller");
            Check(hasLeftController, "O rig tem um filho chamado 'Left Controller' (a UI de pulso se prende nele)");

            var teleportInteractors = origin.GetComponentsInChildren<XRRayInteractor>(true)
                .Where(x => x.name.Contains("Teleport")).ToArray();
            Check(teleportInteractors.Length > 0, "O rig tem interactor de teleporte");
            foreach (var interactor in teleportInteractors)
            {
                Check((interactor.interactionLayers.value & (1 << k_TeleportLayer)) != 0,
                    "'" + interactor.name + "' usa a interaction layer Teleport");
            }
        }

        static void CheckUI(ScenarioManager manager)
        {
            var wrist = Object.FindFirstObjectByType<WristUIController>(FindObjectsInactive.Include);
            if (Check(wrist != null, "Existe a UI de pulso (WristUIController)"))
            {
                var so = new SerializedObject(wrist);
                Check(so.FindProperty("m_ScenarioManager").objectReferenceValue == manager, "UI de pulso aponta para o ScenarioManager");
                foreach (var field in new[] { "m_TitleText", "m_InstructionText", "m_ProgressText" })
                    Check(so.FindProperty(field).objectReferenceValue != null, "UI de pulso: " + field + " atribuido");

                var canvas = wrist.GetComponentInParent<Canvas>();
                Check(canvas != null && canvas.renderMode == RenderMode.WorldSpace,
                    "UI de pulso esta em Canvas World Space (Overlay nao aparece em VR)");
            }

            var report = Object.FindFirstObjectByType<EndReportPanelController>(FindObjectsInactive.Include);
            if (Check(report != null, "Existe o painel de relatorio final"))
            {
                var so = new SerializedObject(report);
                Check(so.FindProperty("m_ScenarioManager").objectReferenceValue == manager, "Relatorio aponta para o ScenarioManager");

                var panelRoot = so.FindProperty("m_PanelRoot").objectReferenceValue as GameObject;
                if (Check(panelRoot != null, "Relatorio: m_PanelRoot atribuido"))
                {
                    // The controller hides m_PanelRoot in OnEnable; if that were its own GameObject
                    // it would disable itself and never show the report.
                    Check(panelRoot != report.gameObject,
                        "Relatorio: m_PanelRoot e um filho, nao o proprio objeto do script (senao o painel nunca reaparece)");
                }

                foreach (var field in new[] { "m_SummaryText", "m_RecapText" })
                    Check(so.FindProperty(field).objectReferenceValue != null, "Relatorio: " + field + " atribuido");

                var canvas = report.GetComponentInChildren<Canvas>(true);
                Check(canvas != null && canvas.renderMode == RenderMode.WorldSpace, "Relatorio esta em Canvas World Space");
            }
        }

        static void CheckReachability()
        {
            var areas = Object.FindObjectsByType<TeleportationArea>(FindObjectsSortMode.None);
            Check(areas.Length > 0, "Existe area de teleporte na fase (" + areas.Length + ")");

            foreach (var area in areas)
            {
                Check((area.interactionLayers.value & (1 << k_TeleportLayer)) != 0,
                    "'" + area.name + "' esta na interaction layer Teleport");
            }

            // The evacuation step only fires if the player can actually stand inside the zone.
            var zone = Object.FindFirstObjectByType<StepCompleteOnTriggerZone>();
            if (zone == null)
                return;

            var zoneCollider = zone.GetComponent<Collider>();
            if (zoneCollider == null)
                return;

            var reachable = areas.Any(area =>
            {
                var areaCollider = area.GetComponent<Collider>();
                return areaCollider != null && areaCollider.bounds.Intersects(zoneCollider.bounds);
            });
            Check(reachable, "A zona de evacuacao tem uma area de teleporte dentro dela (o jogador consegue chegar la)");

            var floorTeleport = areas.Any(area =>
            {
                var areaCollider = area.GetComponent<Collider>();
                return areaCollider != null && areaCollider.bounds.size.x * areaCollider.bounds.size.z > 30f;
            });
            if (!floorTeleport)
                Warn("Nenhuma area de teleporte cobre o chao da casa - dentro do comodo so da para andar com o analogico (locomocao continua)");
        }

        // ---------------------------------------------------------------
        // Mission state machine
        // ---------------------------------------------------------------

        /// <summary>
        /// Drives a throwaway <see cref="ScenarioManager"/> through the phase to prove the state
        /// machine advances, reports progress and finishes - without entering play mode.
        /// </summary>
        static void SimulateRun(ScenarioSO scenario)
        {
            var probe = new GameObject("SafeZoneVR_ValidationProbe") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var manager = probe.AddComponent<ScenarioManager>();
                var so = new SerializedObject(manager);
                so.FindProperty("m_Scenario").objectReferenceValue = scenario;
                so.ApplyModifiedPropertiesWithoutUndo();

                var announced = new List<MissionStepSO>();
                manager.onStepChanged.AddListener(step => announced.Add(step));

                var completedAtEnd = 0;
                manager.onScenarioComplete.AddListener(() => completedAtEnd = manager.completedCount);

                foreach (var step in scenario.steps)
                    manager.CompleteStep(step);

                Check(manager.isComplete, "Concluir as missoes na ordem termina a fase");
                Check(completedAtEnd == scenario.steps.Count,
                    "Todas as " + scenario.steps.Count + " missoes contam como concluidas (contei " + completedAtEnd + ")");
                Check(manager.outOfOrderCount == 0,
                    "Jogar na ordem correta nao registra erro de ordem (registrou " + manager.outOfOrderCount + ")");
                Check(announced.Count == scenario.steps.Count - 1,
                    "A UI e avisada a cada troca de missao (" + announced.Count + " avisos apos o passo inicial)");

                // Repeating a finished step must not double-count.
                var before = manager.completedCount;
                manager.CompleteStep(scenario.steps[0]);
                Check(manager.completedCount == before, "Repetir uma missao ja concluida nao conta de novo");
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }

            var outOfOrderProbe = new GameObject("SafeZoneVR_ValidationProbeB") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var manager = outOfOrderProbe.AddComponent<ScenarioManager>();
                var so = new SerializedObject(manager);
                so.FindProperty("m_Scenario").objectReferenceValue = scenario;
                so.ApplyModifiedPropertiesWithoutUndo();

                foreach (var step in scenario.steps.Reverse())
                    manager.CompleteStep(step);

                Check(manager.isComplete, "Concluir fora de ordem tambem termina a fase");
                Check(manager.outOfOrderCount > 0, "Jogar fora de ordem e registrado no relatorio final");
            }
            finally
            {
                Object.DestroyImmediate(outOfOrderProbe);
            }
        }

        // ---------------------------------------------------------------
        // Reporting
        // ---------------------------------------------------------------

        static void Section(string title)
        {
            s_Report.AppendLine();
            s_Report.AppendLine("--- " + title + " ---");
        }

        static bool Check(bool condition, string description)
        {
            s_Checks++;
            if (condition)
            {
                s_Report.AppendLine("  OK    " + description);
            }
            else
            {
                s_Failures++;
                s_Report.AppendLine("  FALHA " + description);
            }
            return condition;
        }

        static void Fail(string description)
        {
            s_Checks++;
            s_Failures++;
            s_Report.AppendLine("  FALHA " + description);
        }

        static void Warn(string description)
        {
            s_Warnings++;
            s_Report.AppendLine("  AVISO " + description);
        }

        static void WriteReport()
        {
            var header = "SafeZone VR - validacao da fase 'Alagamento em Casa'\n" +
                s_Checks + " verificacoes | " + s_Failures + " falhas | " + s_Warnings + " avisos";

            var full = header + "\n" + s_Report;

            Directory.CreateDirectory(Path.GetDirectoryName(k_ReportPath));
            File.WriteAllText(k_ReportPath, full, new UTF8Encoding(false));

            if (s_Failures > 0)
                Debug.LogError("[SafeZoneVR] " + full);
            else if (s_Warnings > 0)
                Debug.LogWarning("[SafeZoneVR] " + full);
            else
                Debug.Log("[SafeZoneVR] " + full);
        }
    }
}
#endif
