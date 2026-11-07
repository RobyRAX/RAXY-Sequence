using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Sequence
{
    [Serializable]
    public abstract class SequenceableBase : ISequenceable
    {
        [HorizontalGroup("Scheduling", LabelWidth = 135)]
        [LabelText("Override Next Time")]
        [Tooltip("If enabled, the next action starts after Next Action Time seconds from this action's start, instead of waiting for this action to complete.")]
        [SerializeField]
        protected bool overrideNextActionTime;

        [HorizontalGroup("Scheduling")]
        [ShowIf(nameof(overrideNextActionTime))]
        [HideLabel]
        [SuffixLabel("s", true)]
        [MinValue(0f)]
        [Tooltip("Delay in seconds measured from this action's start before the next action begins.")]
        [SerializeField]
        protected float nextActionTime;

        public virtual bool OverrideNextActionTime
        {
            get => overrideNextActionTime;
            set => overrideNextActionTime = value;
        }

        public virtual float NextActionTime
        {
            get => nextActionTime;
            set => nextActionTime = value;
        }

        public abstract string Label { get; }

        public abstract UniTask ExecuteAsync(CancellationToken ct = default);
    }
}
