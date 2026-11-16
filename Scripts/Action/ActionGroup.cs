using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Sequence
{
    [Serializable]
    public class ActionGroup : SequenceableBase
    {
        [TitleGroup("Group Actions")]
        [SerializeReference]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ShowIndexLabels = true, ShowFoldout = true, ListElementLabelName = "Label")]
        public List<ISequenceable> actions = new();

        public override string Label =>
            $"ActionGroup ({(actions != null ? actions.Count : 0)} actions)";

        public override UniTask ExecuteAsync(CancellationToken ct = default)
        {
            return SequenceRunner.RunAsync(actions, ct);
        }
    }
}
