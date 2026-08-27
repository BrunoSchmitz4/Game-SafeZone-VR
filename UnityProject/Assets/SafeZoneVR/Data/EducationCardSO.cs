using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// A single educational recap card shown on the end-of-phase report, sourced from the
    /// Defesa Civil "Recomendações gerais" for the scenario.
    /// </summary>
    [CreateAssetMenu(fileName = "EducationCard", menuName = "SafeZone VR/Education Card", order = 20)]
    public class EducationCardSO : ScriptableObject
    {
        [SerializeField]
        string m_Title;

        [SerializeField]
        [TextArea(2, 5)]
        string m_TipText;

        public string title => m_Title;
        public string tipText => m_TipText;

#if UNITY_EDITOR
        public void EditorInitialize(string title, string tipText)
        {
            m_Title = title;
            m_TipText = tipText;
        }
#endif
    }
}
