using System.Text;
using TMPro;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Diegetic "clipboard" performance report shown when the scenario is completed (RF03):
    /// time taken, steps completed vs. out-of-order attempts, plus the real Defesa Civil
    /// recap cards for the scenario.
    /// </summary>
    public class EndReportPanelController : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        GameObject m_PanelRoot;

        [SerializeField]
        TMP_Text m_SummaryText;

        [SerializeField]
        TMP_Text m_RecapText;

        void OnEnable()
        {
            if (m_PanelRoot != null)
                m_PanelRoot.SetActive(false);

            if (m_ScenarioManager != null)
                m_ScenarioManager.onScenarioComplete.AddListener(ShowReport);
        }

        void OnDisable()
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.onScenarioComplete.RemoveListener(ShowReport);
        }

        void ShowReport()
        {
            if (m_PanelRoot != null)
                m_PanelRoot.SetActive(true);

            if (m_SummaryText != null)
                m_SummaryText.text = BuildSummary();

            if (m_RecapText != null)
                m_RecapText.text = BuildRecap();
        }

        string BuildSummary()
        {
            var minutes = Mathf.FloorToInt(m_ScenarioManager.elapsedSeconds / 60f);
            var seconds = Mathf.FloorToInt(m_ScenarioManager.elapsedSeconds % 60f);

            return "Fase concluída!\n" +
                $"Tempo: {minutes:00}:{seconds:00}\n" +
                $"Passos concluídos: {m_ScenarioManager.completedCount}/{m_ScenarioManager.totalSteps}\n" +
                $"Fora de ordem: {m_ScenarioManager.outOfOrderCount}";
        }

        string BuildRecap()
        {
            if (m_ScenarioManager.scenario == null)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var card in m_ScenarioManager.scenario.recapCards)
            {
                builder.AppendLine($"• {card.title}");
                builder.AppendLine(card.tipText);
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
