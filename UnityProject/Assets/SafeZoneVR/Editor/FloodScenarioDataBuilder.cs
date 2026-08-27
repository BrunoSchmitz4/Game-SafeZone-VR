#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Generates the ScenarioSO/MissionStepSO/EducationCardSO data assets for the
    /// "Alagamento em Casa" (home flood) phase, using real Defesa Civil guidance text.
    /// Run via the menu item below, or in batchmode with:
    /// -executeMethod SafeZoneVR.Editor.FloodScenarioDataBuilder.BuildData
    /// </summary>
    public static class FloodScenarioDataBuilder
    {
        const string k_DataFolder = "Assets/SafeZoneVR/Data/Alagamento";

        [MenuItem("SafeZone VR/Build Data/Alagamento em Casa")]
        public static void BuildData()
        {
            EnsureFolder(k_DataFolder);

            var step1 = CreateStep("step_protect_valuables", 0,
                "Proteja seus objetos de valor",
                "Pegue os documentos e a caixa de lembranças da mesa e guarde-os na prateleira alta, fora do alcance da água.");

            var step2 = CreateStep("step_power_off", 1,
                "Desligue o quadro de energia",
                "Gire a chave geral do quadro de energia para DESLIGADO antes que a água chegue perto de fios e tomadas.");

            var step3 = CreateStep("step_water_valve", 2,
                "Feche o registro de entrada de água",
                "Gire o registro até fechar por completo, evitando contaminação e desperdício durante a enchente.");

            var step4 = CreateStep("step_emergency_kit", 3,
                "Monte seu kit de emergência",
                "Pegue a lanterna, a água potável e os remédios da bancada e guarde-os na mochila.");

            var step5 = CreateStep("step_evacuate", 4,
                "Evacue para um local alto e seguro",
                "Use o teleporte para sair de casa e alcançar o ponto seguro elevado, longe da água.");

            var card1 = CreateCard("Mantenha a calma",
                "Mantenha a calma e siga rigorosamente as orientações da Defesa Civil e demais autoridades da sua cidade.");

            var card2 = CreateCard("Desconfie de mensagens não oficiais",
                "Acompanhe informações oficiais por sites, rádio, SMS ou carro de som da Defesa Civil. Mensagens não oficiais nas redes sociais podem ser falsas e causar pânico.");

            var card3 = CreateCard("Nunca atravesse ruas alagadas",
                "A força da água pode arrastar pessoas e veículos, mesmo em pontos que parecem rasos. Procure um local alto e espere a água baixar.");

            var card4 = CreateCard("Telefones úteis",
                "Defesa Civil: 199  •  Bombeiros: 193  •  Polícia Militar: 190  •  SAMU: 192");

            var scenario = ScriptableObject.CreateInstance<ScenarioSO>();
            scenario.EditorInitialize(
                "alagamento_em_casa",
                "Alagamento em Casa",
                new List<MissionStepSO> { step1, step2, step3, step4, step5 },
                new List<EducationCardSO> { card1, card2, card3, card4 });
            CreateAsset(scenario, "Scenario_AlagamentoEmCasa.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SafeZoneVR] Dados da fase 'Alagamento em Casa' gerados em " + k_DataFolder);
        }

        static MissionStepSO CreateStep(string id, int order, string title, string instruction)
        {
            var step = ScriptableObject.CreateInstance<MissionStepSO>();
            step.EditorInitialize(id, order, title, instruction);
            CreateAsset(step, $"Step_{order:00}_{id}.asset");
            return step;
        }

        static EducationCardSO CreateCard(string title, string tip)
        {
            var card = ScriptableObject.CreateInstance<EducationCardSO>();
            card.EditorInitialize(title, tip);
            CreateAsset(card, $"Card_{SanitizeFileName(title)}.asset");
            return card;
        }

        static void CreateAsset(Object asset, string fileName)
        {
            var path = $"{k_DataFolder}/{fileName}";
            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }

        static string SanitizeFileName(string value)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                value = value.Replace(invalidChar, '_');
            return value.Replace(" ", "");
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
    }
}
#endif
