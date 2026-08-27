using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SafeZoneVR
{
    /// <summary>
    /// Completes a mission step the first time this object is grabbed (RF05). Useful for
    /// single-item pickups that don't require placing the item anywhere specific.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class StepCompleteOnGrab : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        MissionStepSO m_Step;

        XRGrabInteractable m_Interactable;

        void Awake()
        {
            m_Interactable = GetComponent<XRGrabInteractable>();
        }

        void OnEnable()
        {
            m_Interactable.selectEntered.AddListener(OnGrabbed);
        }

        void OnDisable()
        {
            m_Interactable.selectEntered.RemoveListener(OnGrabbed);
        }

        void OnGrabbed(SelectEnterEventArgs args)
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.CompleteStep(m_Step);
        }
    }
}
