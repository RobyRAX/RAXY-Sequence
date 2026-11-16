using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RAXY.Sequence
{
    public class SequenceRunner
    {
        public static async UniTask RunAsync(
            IReadOnlyList<ISequenceable> actions,
            CancellationToken ct = default)
        {
            if (actions == null || actions.Count == 0)
                return;

            List<UniTask> startedTasks = null;
            Exception caughtException = null;

            try
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var action = actions[i];
                    if (action == null)
                        continue;

                    UniTask actionTask = action.ExecuteAsync(ct);

                    if (i < actions.Count - 1)
                    {
                        if (!action.OverrideNextActionTime)
                        {
                            await actionTask;
                        }
                        else
                        {
                            startedTasks ??= new List<UniTask>();
                            startedTasks.Add(actionTask);

                            float delay = Mathf.Max(0f, action.NextActionTimeOverrider);
                            if (delay > 0f)
                            {
                                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
                            }
                        }
                    }
                    else
                    {
                        if (!action.OverrideNextActionTime)
                        {
                            await actionTask;
                        }
                        else
                        {
                            startedTasks ??= new List<UniTask>();
                            startedTasks.Add(actionTask);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            if (startedTasks != null && startedTasks.Count > 0)
            {
                try
                {
                    await UniTask.WhenAll(startedTasks);
                }
                catch (Exception ex)
                {
                    if (caughtException == null)
                        caughtException = ex;
                }
            }

            if (caughtException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(caughtException).Throw();
            }
        }
    }
}
