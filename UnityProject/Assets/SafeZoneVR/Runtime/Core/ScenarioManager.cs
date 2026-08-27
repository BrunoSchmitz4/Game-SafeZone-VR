using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SafeZoneVR
{
    [Serializable]
    public class MissionStepEvent : UnityEvent<MissionStepSO> { }

    /// <summary>
    /// Drives a single <see cref="ScenarioSO"/> at runtime. Validators call <see cref="CompleteStep"/>
    /// when the player performs the matching real-world action; this manager tracks progress,
    /// timing and ordering for the RF03 end-of-phase report and raises events for the diegetic UI.
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField]
        ScenarioSO m_Scenario;

        [SerializeField]
        MissionStepEvent m_OnStepChanged = new MissionStepEvent();

        [SerializeField]
        MissionStepEvent m_OnStepCompleted = new MissionStepEvent();

        [SerializeField]
        UnityEvent m_OnScenarioComplete = new UnityEvent();

        readonly HashSet<string> m_CompletedStepIds = new HashSet<string>();
        readonly List<MissionStepSO> m_CompletionOrder = new List<MissionStepSO>();

        float m_StartTime;

        public ScenarioSO scenario => m_Scenario;
        public MissionStepEvent onStepChanged => m_OnStepChanged;
        public MissionStepEvent onStepCompleted => m_OnStepCompleted;
        public UnityEvent onScenarioComplete => m_OnScenarioComplete;

        public MissionStepSO currentStep { get; private set; }
        public bool isComplete { get; private set; }
        public float elapsedSeconds => Time.time - m_StartTime;
        public int completedCount => m_CompletedStepIds.Count;
        public int totalSteps => m_Scenario != null ? m_Scenario.steps.Count : 0;
        public int outOfOrderCount { get; private set; }
        public int repeatCount { get; private set; }
        public IReadOnlyList<MissionStepSO> completionOrder => m_CompletionOrder;

        void Start()
        {
            m_StartTime = Time.time;

            if (m_Scenario == null || m_Scenario.steps.Count == 0)
                return;

            currentStep = m_Scenario.steps[0];
            m_OnStepChanged.Invoke(currentStep);
        }

        /// <summary>
        /// Called by objective validators (sockets, knobs, trigger zones, grabs) when the player
        /// performs the real-world action a step represents.
        /// </summary>
        public void CompleteStep(MissionStepSO step)
        {
            if (step == null || m_Scenario == null || isComplete)
                return;

            if (m_CompletedStepIds.Contains(step.stepId))
            {
                repeatCount++;
                return;
            }

            var steps = m_Scenario.steps;
            var expectedIndex = m_CompletedStepIds.Count;
            var actualIndex = IndexOfStep(steps, step);
            if (actualIndex != expectedIndex)
                outOfOrderCount++;

            m_CompletedStepIds.Add(step.stepId);
            m_CompletionOrder.Add(step);
            m_OnStepCompleted.Invoke(step);

            var nextStep = FindNextIncompleteStep(steps);
            if (nextStep != null)
            {
                currentStep = nextStep;
                m_OnStepChanged.Invoke(currentStep);
            }
            else
            {
                currentStep = null;
                isComplete = true;
                m_OnScenarioComplete.Invoke();
            }
        }

        static int IndexOfStep(IReadOnlyList<MissionStepSO> steps, MissionStepSO step)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] == step)
                    return i;
            }
            return -1;
        }

        MissionStepSO FindNextIncompleteStep(IReadOnlyList<MissionStepSO> steps)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (!m_CompletedStepIds.Contains(steps[i].stepId))
                    return steps[i];
            }
            return null;
        }
    }
}
