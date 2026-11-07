using System.Threading;
using Cysharp.Threading.Tasks;

namespace RAXY.Sequence
{
    public interface ISequenceable
    {
        string Label { get; }

        bool OverrideNextActionTime => false;

        float NextActionTime => 0f;

        UniTask ExecuteAsync(CancellationToken ct = default);
    }
}
