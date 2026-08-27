using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Diegetic "smart wristband" panel showing the current objective (RF02/RF04). Attaches itself
    /// to the left controller of the XR rig at runtime so it never relies on a fixed scene hierarchy;
    /// falls back to a camera-relative placement if no rig is found.
    /// </summary>
    public class WristUIController : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        TMP_Text m_TitleText;

        [SerializeField]
        TMP_Text m_InstructionText;

        [SerializeField]
        TMP_Text m_ProgressText;

        [SerializeField]
        Vector3 m_WristLocalPosition = new Vector3(0f, 0.04f, 0.05f);

        [SerializeField]
        Vector3 m_WristLocalEuler = new Vector3(60f, 0f, 0f);

        [SerializeField]
        Vector3 m_CameraFallbackLocalPosition = new Vector3(-0.15f, -0.15f, 0.5f);

        void OnEnable()
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.onStepChanged.AddListener(OnStepChanged);

            AttachToRig();
        }

        void OnDisable()
        {
            if (m_ScenarioManager != null)
                m_ScenarioManager.onStepChanged.RemoveListener(OnStepChanged);
        }

        void AttachToRig()
        {
            var origin = FindFirstObjectByType<XROrigin>();
            var anchor = origin != null ? FindLeftController(origin.transform) : null;

            if (anchor != null)
            {
                transform.SetParent(anchor, false);
                transform.localPosition = m_WristLocalPosition;
                transform.localEulerAngles = m_WristLocalEuler;
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                transform.SetParent(mainCamera.transform, false);
                transform.localPosition = m_CameraFallbackLocalPosition;
                transform.localRotation = Quaternion.identity;
            }
        }

        static Transform FindLeftController(Transform root)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child.name == "Left Controller")
                    return child;
            }
            return null;
        }

        void OnStepChanged(MissionStepSO step)
        {
            if (step == null)
                return;

            if (m_TitleText != null)
                m_TitleText.text = step.title;

            if (m_InstructionText != null)
                m_InstructionText.text = step.instructionText;

            if (m_ProgressText != null && m_ScenarioManager != null)
                m_ProgressText.text = $"Passo {m_ScenarioManager.completedCount + 1} de {m_ScenarioManager.totalSteps}";
        }
    }
}
