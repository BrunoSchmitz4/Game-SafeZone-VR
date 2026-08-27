using Unity.VRTemplate;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Completes a mission step once an <see cref="XRKnob"/> (breaker switch, water valve, etc.)
    /// reaches a target value, e.g. fully closed/off (RF05).
    /// </summary>
    public class StepCompleteOnKnobThreshold : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        MissionStepSO m_Step;

        [SerializeField]
        XRKnob m_Knob;

        [SerializeField]
        [Range(0f, 1f)]
        float m_TargetValue;

        [SerializeField]
        [Range(0f, 0.5f)]
        float m_Tolerance = 0.05f;

        void OnEnable()
        {
            if (m_Knob != null)
                m_Knob.onValueChange.AddListener(OnValueChanged);
        }

        void OnDisable()
        {
            if (m_Knob != null)
                m_Knob.onValueChange.RemoveListener(OnValueChanged);
        }

        void OnValueChanged(float value)
        {
            if (m_ScenarioManager != null && Mathf.Abs(value - m_TargetValue) <= m_Tolerance)
                m_ScenarioManager.CompleteStep(m_Step);
        }
    }
}
