using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Sequence
{
    [Serializable]
    public class MoveObject : SequenceableBase
    {
        [HideLabel]
        public SequenceObjectTarget target = new();

        [LabelText("Start Mode")]
        public SequenceTransformStartMode startMode = SequenceTransformStartMode.Current;

        [ShowIf(nameof(UsesManualStart))]
        [LabelText("Start")]
        public Vector3 startPosition;

        public Vector3 destination;

        [MinValue(0f)]
        [SuffixLabel("s", true)]
        public float duration;

        [Tooltip("If true, moves in local space (localPosition); otherwise world space (position).")]
        public bool isLocal;

        public Ease ease = Ease.Linear;

#if UNITY_EDITOR
        [TitleGroup("Helper")]
        [ShowInInspector]
        [OnValueChanged(nameof(OnHelperGameObjectChanged))]
        GameObject helper_gameObject;

        void OnHelperGameObjectChanged()
        {
            if (helper_gameObject == null)
                return;

            if (target.targetMode == SequenceTargetMode.GameObjectName)
                target.gameObjectName = helper_gameObject.name;
            else if (target.targetMode == SequenceTargetMode.ActorId)
            {
                if (helper_gameObject.TryGetComponent(out SequenceActor actor))
                {
                    target.actorId = actor.ActorId;
                }
            }
        }

        [HorizontalGroup("Helper/Btn")]
        [Button]
        void Find()
        {
            helper_gameObject = target.Resolve().gameObject;
        }

        [HorizontalGroup("Helper/Btn")]
        [Button]
        void SetStart()
        {
            var selected = helper_gameObject.transform;
            if (selected == null)
                return;

            startPosition = isLocal ? selected.localPosition : selected.position;
            startMode = SequenceTransformStartMode.Manual;
        }

        [HorizontalGroup("Helper/Btn")]
        [Button]
        void SetDestination()
        {
            var selected = helper_gameObject.transform;
            if (selected == null)
                return;

            destination = isLocal ? selected.localPosition : selected.position;
        }
#endif

        public override string Label =>
            $"MoveObject ({(target != null ? target.TargetDescription : "None")}) {StartDescription} -> {destination} ({(isLocal ? "Local" : "World")}, {duration}s)";

        bool UsesManualStart => startMode == SequenceTransformStartMode.Manual;

        string StartDescription => UsesManualStart ? startPosition.ToString() : "Current";

        public override async UniTask ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var objectTransform = target?.Resolve();
            if (objectTransform == null)
            {
                Debug.LogWarning($"[CutsceneDebug/Move] Target unresolved: {target?.TargetDescription ?? "null"}; frame={Time.frameCount}.");
                return;
            }

            Debug.Log($"[CutsceneDebug/Move] Start: target={objectTransform.name} id={objectTransform.GetInstanceID()} active={objectTransform.gameObject.activeInHierarchy} position={objectTransform.position} destination={destination} duration={duration} frame={Time.frameCount}.", objectTransform);

            bool hasCharCon = objectTransform.TryGetComponent(out CharacterController charCon);
            bool wasCharConEnabled = hasCharCon && charCon.enabled;

            if (wasCharConEnabled)
                charCon.enabled = false;

            try
            {
                if (UsesManualStart)
                {
                    if (isLocal)
                        objectTransform.localPosition = startPosition;
                    else
                        objectTransform.position = startPosition;
                }

                if (duration <= 0f)
                {
                    if (isLocal)
                        objectTransform.localPosition = destination;
                    else
                        objectTransform.position = destination;

                    Debug.Log($"[CutsceneDebug/Move] Set: target={objectTransform.name} position={objectTransform.position} frame={Time.frameCount}.", objectTransform);

                    await UniTask.Yield();

                    Debug.Log($"[CutsceneDebug/Move] After yield: target={objectTransform.name} position={objectTransform.position} frame={Time.frameCount}.", objectTransform);

                    return;
                }

                Tween tween = isLocal
                    ? objectTransform.DOLocalMove(destination, duration).SetEase(ease)
                    : objectTransform.DOMove(destination, duration).SetEase(ease);

                await tween.AwaitTweenAsync(ct);
                Debug.Log($"[CutsceneDebug/Move] Tween complete: target={objectTransform.name} position={objectTransform.position} frame={Time.frameCount}.", objectTransform);
            }
            finally
            {
                if (wasCharConEnabled && charCon != null)
                    charCon.enabled = true;
            }
        }
    }
}
