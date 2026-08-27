using System.Collections.Generic;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Data for a full disaster-preparedness phase: an ordered set of mission steps plus the
    /// educational recap cards shown in the end-of-phase report (RF01/RF03).
    /// </summary>
    [CreateAssetMenu(fileName = "Scenario", menuName = "SafeZone VR/Scenario", order = 0)]
    public class ScenarioSO : ScriptableObject
    {
        [SerializeField]
        string m_ScenarioId;

        [SerializeField]
        string m_DisplayName;

        [SerializeField]
        List<MissionStepSO> m_Steps = new List<MissionStepSO>();

        [SerializeField]
        List<EducationCardSO> m_RecapCards = new List<EducationCardSO>();

        public string scenarioId => m_ScenarioId;
        public string displayName => m_DisplayName;
        public IReadOnlyList<MissionStepSO> steps => m_Steps;
        public IReadOnlyList<EducationCardSO> recapCards => m_RecapCards;

#if UNITY_EDITOR
        public void EditorInitialize(string scenarioId, string displayName, List<MissionStepSO> steps, List<EducationCardSO> recapCards)
        {
            m_ScenarioId = scenarioId;
            m_DisplayName = displayName;
            m_Steps = steps;
            m_RecapCards = recapCards;
        }
#endif
    }
}
