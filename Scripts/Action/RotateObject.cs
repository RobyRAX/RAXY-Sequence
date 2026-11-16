using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Sequence
{
    public enum RotateDestinationMode
    {
        Euler,
        LookAt
    }

    [Serializable]
    public class RotateObject : SequenceableBase
    {
        [HideLabel]
        public SequenceObjectTarget target = new();

        [LabelText("Start Mode")]
        public SequenceTransformStartMode startMode = SequenceTransformStartMode.Current;

        [ShowIf(nameof(UsesManualStart))]
        [LabelText("Start")]
        public Vector3 startRotation;

        [LabelText("Destination Mode")]
        public RotateDestinationMode destinationMode = RotateDestinationMode.Euler;

        [ShowIf(nameof(IsEulerMode))]
        [Tooltip("Target rotation in Euler angles.")]
        [LabelText("To")]
        public Vector3 destinationRotation;

        [ShowIf(nameof(IsLookAtMode))]
        [TitleGroup("Look Target")]
        [HideLabel]
        public SequenceObjectTarget lookTarget = new();

        [ShowIf(nameof(IsLookAtMode))]
        [HorizontalGroup("Axes", Title = "Rotation Axes")]
        [LabelText("X")]
        public bool rotateX = true;

        [ShowIf(nameof(IsLookAtMode))]
        [HorizontalGroup("Axes")]
        [LabelText("Y")]
        public bool rotateY = true;

        [ShowIf(nameof(IsLookAtMode))]
        [HorizontalGroup("Axes")]
        [LabelText("Z")]
        public bool rotateZ = true;

        [MinValue(0f)]
        [SuffixLabel("s", true)]
        public float duration;

        [Tooltip("If true, rotates in local space (localEulerAngles); otherwise world space (eulerAngles).")]
        public bool isLocal;

        public RotateMode rotateMode = RotateMode.Fast;

        public Ease ease = Ease.Linear;

#if UNITY_EDITOR
        [TitleGroup("Helper")]
        [ShowInInspector]
        [OnValueChanged(nameof(OnHelperGameObjectChanged))]
        [LabelText("Target Object")]
        GameObject helper_gameObject;

        [TitleGroup("Helper")]
        [ShowIf(nameof(IsLookAtMode))]
        [ShowInInspector]
        [OnValueChanged(nameof(OnHelperLookTargetGameObjectChanged))]
        [LabelText("Look Target Object")]
        GameObject helper_lookTargetGameObject;

        void OnHelperGameObjectChanged()
        {
            if (helper_gameObject == null || target == null)
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

        void OnHelperLookTargetGameObjectChanged()
        {
            if (helper_lookTargetGameObject == null || lookTarget == null)
                return;

            if (lookTarget.targetMode == SequenceTargetMode.GameObjectName)
                lookTarget.gameObjectName = helper_lookTargetGameObject.name;
            else if (lookTarget.targetMode == SequenceTargetMode.ActorId)
            {
                if (helper_lookTargetGameObject.TryGetComponent(out SequenceActor actor))
                {
                    lookTarget.actorId = actor.ActorId;
                }
            }
        }

        [HorizontalGroup("Helper/Btn")]
        [Button]
        void Find()
        {
            var resolved = target?.Resolve();
            if (resolved != null)
                helper_gameObject = resolved.gameObject;
        }

        [HorizontalGroup("Helper/Btn")]
        [ShowIf(nameof(IsLookAtMode))]
        [Button("Find Look")]
        void FindLookTarget()
        {
            var resolved = lookTarget?.Resolve();
            if (resolved != null)
                helper_lookTargetGameObject = resolved.gameObject;
        }

        [HorizontalGroup("Helper/Btn")]
        [Button]
        void SetStart()
        {
            if (helper_gameObject == null)
                return;

            var selected = helper_gameObject.transform;
            if (selected == null)
                return;

            startRotation = isLocal ? selected.localEulerAngles : selected.eulerAngles;
            startMode = SequenceTransformStartMode.Manual;
        }

        [HorizontalGroup("Helper/Btn")]
        [ShowIf(nameof(IsEulerMode))]
        [Button]
        void SetDestination()
        {
            if (helper_gameObject == null)
                return;

            var selected = helper_gameObject.transform;
            if (selected == null)
                return;

            destinationRotation = isLocal ? selected.localEulerAngles : selected.eulerAngles;
        }
#endif

        public override string Label
        {
            get
            {
                string targetDesc = target != null ? target.TargetDescription : "None";
                string spaceDesc = isLocal ? "Local" : "World";

                if (destinationMode == RotateDestinationMode.LookAt)
                {
                    string lookDesc = lookTarget != null ? lookTarget.TargetDescription : "None";
                    return $"RotateObject ({targetDesc}) {StartDescription} -> LookAt ({lookDesc}, [{AxesDescription}]) ({spaceDesc}, {duration}s)";
                }

                return $"RotateObject ({targetDesc}) {StartDescription} -> {destinationRotation} ({spaceDesc}, {duration}s)";
            }
        }

        bool UsesManualStart => startMode == SequenceTransformStartMode.Manual;
        bool IsEulerMode => destinationMode == RotateDestinationMode.Euler;
        bool IsLookAtMode => destinationMode == RotateDestinationMode.LookAt;

        string StartDescription => UsesManualStart ? startRotation.ToString() : "Current";

        string AxesDescription
        {
            get
            {
                string axes = "";
                if (rotateX) axes += "X";
                if (rotateY) axes += "Y";
                if (rotateZ) axes += "Z";
                return string.IsNullOrEmpty(axes) ? "None" : axes;
            }
        }

        public override UniTask ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var t = target?.Resolve();
            if (t == null)
            {
                Debug.LogWarning("[RotateObject] Skipping rotate because target could not be resolved.");
                return UniTask.CompletedTask;
            }

            Vector3 destinationEuler;

            if (destinationMode == RotateDestinationMode.Euler)
            {
                if (UsesManualStart)
                {
                    if (isLocal)
                        t.localEulerAngles = startRotation;
                    else
                        t.eulerAngles = startRotation;
                }

                destinationEuler = destinationRotation;
            }
            else
            {
                if (!rotateX && !rotateY && !rotateZ)
                {
                    Debug.LogWarning("[RotateObject] Skipping rotate because no rotation axis is enabled.");
                    return UniTask.CompletedTask;
                }

                var lt = lookTarget?.Resolve();
                if (lt == null)
                {
                    Debug.LogWarning("[RotateObject] Skipping rotate because look target could not be resolved.");
                    return UniTask.CompletedTask;
                }

                Vector3 direction = lt.position - t.position;
                if (direction.sqrMagnitude <= 1e-6f)
                {
                    Debug.LogWarning("[RotateObject] Skipping rotate because source and look target positions coincide.");
                    return UniTask.CompletedTask;
                }

                if (UsesManualStart)
                {
                    if (isLocal)
                        t.localEulerAngles = startRotation;
                    else
                        t.eulerAngles = startRotation;
                }

                Vector3 up = Mathf.Abs(Vector3.Dot(direction.normalized, Vector3.up)) > 0.9999f ? Vector3.forward : Vector3.up;
                Quaternion worldLookRotation = Quaternion.LookRotation(direction, up);

                Vector3 lookEuler;
                if (isLocal && t.parent != null)
                {
                    Quaternion localLookRotation = Quaternion.Inverse(t.parent.rotation) * worldLookRotation;
                    lookEuler = localLookRotation.eulerAngles;
                }
                else
                {
                    lookEuler = worldLookRotation.eulerAngles;
                }

                Vector3 currentEuler = isLocal ? t.localEulerAngles : t.eulerAngles;

                destinationEuler = new Vector3(
                    rotateX ? lookEuler.x : currentEuler.x,
                    rotateY ? lookEuler.y : currentEuler.y,
                    rotateZ ? lookEuler.z : currentEuler.z
                );
            }

            if (duration <= 0f)
            {
                if (isLocal)
                    t.localEulerAngles = destinationEuler;
                else
                    t.eulerAngles = destinationEuler;

                return UniTask.CompletedTask;
            }

            Tween tween = isLocal
                ? t.DOLocalRotate(destinationEuler, duration, rotateMode).SetEase(ease)
                : t.DORotate(destinationEuler, duration, rotateMode).SetEase(ease);

            return tween.AwaitTweenAsync(ct);
        }
    }
}
