using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Event;
using Sirenix.OdinInspector;

namespace RAXY.Sequence
{
    [Serializable]
    public class TriggerEventSO : SequenceableBase
    {
        [HideLabel]
        [HideReferenceObjectPicker]
        public EventSoRaiser eventRaiser;

        public override string Label => "TriggerEventSO";

        public override UniTask ExecuteAsync(CancellationToken ct = default)
        {
            eventRaiser?.Raise();
            return UniTask.CompletedTask;
        }
    }
}
