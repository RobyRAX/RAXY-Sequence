using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Sequence
{
    public enum SequenceTargetMode
    {
        ActorId,
        GameObjectName
    }

    public enum SequenceTransformStartMode
    {
        Current,
        Manual
    }

    [Serializable]
    public class SequenceObjectTarget
    {
        public static Func<string, Transform> ActorIdResolver { get; set; }

        [HorizontalGroup("Target", LabelWidth = 90)]
        [LabelText("Target Type")]
        public SequenceTargetMode targetMode = SequenceTargetMode.GameObjectName;

        [HorizontalGroup("Target")]
        [ShowIf(nameof(IsActorMode))]
        [HideLabel]
        public string actorId;

        [HorizontalGroup("Target")]
        [ShowIf(nameof(IsGameObjectMode))]
        [HideLabel]
        public string gameObjectName;

        public string TargetDescription => targetMode == SequenceTargetMode.ActorId
            ? $"ActorId '{actorId}'"
            : $"GameObject '{gameObjectName}'";

        public Transform Resolve()
        {
            if (targetMode == SequenceTargetMode.ActorId)
            {
                if (string.IsNullOrWhiteSpace(actorId))
                {
                    Debug.LogWarning("[SequenceObjectTarget] Actor ID is empty.");
                    return null;
                }

                if (ActorIdResolver == null)
                {
                    Debug.LogWarning("[SequenceObjectTarget] No actor ID resolver is registered.");
                    return null;
                }

                return ActorIdResolver(actorId);
            }

            if (targetMode == SequenceTargetMode.GameObjectName)
            {
                if (string.IsNullOrWhiteSpace(gameObjectName))
                {
                    Debug.LogWarning("[SequenceObjectTarget] GameObject name is empty.");
                    return null;
                }

                var go = GameObject.Find(gameObjectName);
                if (go == null)
                {
                    Debug.LogWarning($"[SequenceObjectTarget] No active GameObject named '{gameObjectName}' was found.");
                    return null;
                }

                return go.transform;
            }

            Debug.LogWarning($"[SequenceObjectTarget] Unsupported target mode: {targetMode}.");
            return null;
        }

        bool IsActorMode => targetMode == SequenceTargetMode.ActorId;
        bool IsGameObjectMode => targetMode == SequenceTargetMode.GameObjectName;
    }
}
