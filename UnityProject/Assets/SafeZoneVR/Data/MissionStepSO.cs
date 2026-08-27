using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// A single objective within a <see cref="ScenarioSO"/>, mapped 1:1 to a real Defesa Civil recommendation.
    /// </summary>
    [CreateAssetMenu(fileName = "MissionStep", menuName = "SafeZone VR/Mission Step", order = 10)]
    public class MissionStepSO : ScriptableObject
    {
        [SerializeField]
        string m_StepId;

        [SerializeField]
        int m_OrderIndex;

        [SerializeField]
        string m_Title;

        [SerializeField]
        [TextArea(2, 4)]
        string m_InstructionText;

        public string stepId => m_StepId;
        public int orderIndex => m_OrderIndex;
        public string title => m_Title;
        public string instructionText => m_InstructionText;

#if UNITY_EDITOR
        public void EditorInitialize(string stepId, int orderIndex, string title, string instructionText)
        {
            m_StepId = stepId;
            m_OrderIndex = orderIndex;
            m_Title = title;
            m_InstructionText = instructionText;
        }
#endif
    }
}
