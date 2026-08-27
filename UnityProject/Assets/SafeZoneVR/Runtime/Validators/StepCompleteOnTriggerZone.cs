using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Completes a mission step when the player's rig enters this trigger volume, e.g. reaching
    /// the evacuation point or a safe/high-ground area (RF04/RF05).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StepCompleteOnTriggerZone : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        MissionStepSO m_Step;

        void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null)
                zoneCollider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_ScenarioManager == null)
                return;

            if (other.GetComponentInParent<CharacterController>() != null)
                m_ScenarioManager.CompleteStep(m_Step);
        }
    }
}
