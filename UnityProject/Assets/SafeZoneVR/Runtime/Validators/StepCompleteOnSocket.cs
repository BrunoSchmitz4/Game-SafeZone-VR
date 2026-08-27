using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SafeZoneVR
{
    /// <summary>
    /// One socket that must receive a specific objective item (matched via <see cref="ObjectiveItemId"/>)
    /// for the group to count as filled. Leave <see cref="requiredItemId"/> empty to accept any item.
    /// </summary>
    [Serializable]
    public struct SocketRequirement
    {
        public XRSocketInteractor socket;
        public string requiredItemId;
    }

    /// <summary>
    /// Completes a mission step once every socket in the group has received its required item
    /// (RF05 — validating interactions with environment objects). Used for grouped placements
    /// such as "move the valuables to the high shelf" or "pack the emergency kit".
    /// </summary>
    public class StepCompleteOnSocket : MonoBehaviour
    {
        [SerializeField]
        ScenarioManager m_ScenarioManager;

        [SerializeField]
        MissionStepSO m_Step;

        [SerializeField]
        SocketRequirement[] m_Requirements = Array.Empty<SocketRequirement>();

        readonly HashSet<int> m_Filled = new HashSet<int>();
        UnityAction<SelectEnterEventArgs>[] m_Handlers;

        void Awake()
        {
            m_Handlers = new UnityAction<SelectEnterEventArgs>[m_Requirements.Length];
            for (var i = 0; i < m_Requirements.Length; i++)
            {
                var index = i;
                m_Handlers[i] = args => OnSocketFilled(index, args);
            }
        }

        void OnEnable()
        {
            for (var i = 0; i < m_Requirements.Length; i++)
            {
                if (m_Requirements[i].socket != null)
                    m_Requirements[i].socket.selectEntered.AddListener(m_Handlers[i]);
            }
        }

        void OnDisable()
        {
            for (var i = 0; i < m_Requirements.Length; i++)
            {
                if (m_Requirements[i].socket != null)
                    m_Requirements[i].socket.selectEntered.RemoveListener(m_Handlers[i]);
            }
        }

        void OnSocketFilled(int index, SelectEnterEventArgs args)
        {
            var requiredId = m_Requirements[index].requiredItemId;
            if (!string.IsNullOrEmpty(requiredId))
            {
                var item = args.interactableObject.transform.GetComponent<ObjectiveItemId>();
                if (item == null || item.itemId != requiredId)
                    return;
            }

            m_Filled.Add(index);

            if (m_ScenarioManager != null && m_Filled.Count >= m_Requirements.Length)
                m_ScenarioManager.CompleteStep(m_Step);
        }
    }
}
