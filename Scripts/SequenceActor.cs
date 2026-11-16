using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace RAXY.Sequence
{
    [DisallowMultipleComponent]
    [MovedFrom(true, "RAXY.Narrative", "RAXY.Narrative", "NarrativeActor")]
    public class SequenceActor : MonoBehaviour
    {
        [SerializeField]
        string actorId;

        public string ActorId
        {
            get => actorId;
            set => actorId = value;
        }

        static readonly List<SequenceActor> activeActors = new();

        public static IReadOnlyList<SequenceActor> ActiveActors => activeActors;

        void OnEnable()
        {
            if (!activeActors.Contains(this))
                activeActors.Add(this);
        }

        void OnDisable()
        {
            activeActors.Remove(this);
        }

        void OnDestroy()
        {
            activeActors.Remove(this);
        }

        public static Transform FindActorTransform(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[SequenceActor] Actor ID is null or empty.");
                return null;
            }

            SequenceActor found = null;
            int count = 0;

            for (int i = 0; i < activeActors.Count; i++)
            {
                var actor = activeActors[i];
                if (actor != null && actor.ActorId == id)
                {
                    found = actor;
                    count++;
                }
            }

            if (count == 0)
            {
                var all = Object.FindObjectsByType<SequenceActor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < all.Length; i++)
                {
                    var actor = all[i];
                    if (actor != null && actor.ActorId == id)
                    {
                        found = actor;
                        count++;
                    }
                }
            }

            if (count == 0)
            {
                Debug.LogWarning($"[SequenceActor] No active SequenceActor found with actorId '{id}'.");
                return null;
            }

            if (count > 1)
            {
                Debug.LogWarning($"[SequenceActor] Multiple SequenceActors found with actorId '{id}' ({count} found). Skipping resolution to avoid ambiguous mutation.");
                return null;
            }

            return found != null ? found.transform : null;
        }
    }
}
