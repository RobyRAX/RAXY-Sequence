using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Sequence
{
    [Serializable]
    public class Wait : ISequenceable
    {
        [MinValue(0f)]
        [SuffixLabel("s", true)]
        [Tooltip("Duration to wait in seconds.")]
        public float duration;

        public string Label => $"Wait ({duration}s)";

        public async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (duration <= 0f)
                return;

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
        }
    }
}
