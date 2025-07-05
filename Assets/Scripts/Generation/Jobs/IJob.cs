using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Generation.Jobs
{
    public enum ChunkJobType
    {
        Generate,
        Load,
        Build,
        Unload
    }

    public abstract class IJob : IComparable<IJob>
    {
        public ChunkJobType Type { get; }
        public Vector2Int Position { get; }
        public float Priority { get; set; }
        public bool IsRunning { get; set; }

        public int CompareTo(IJob other) => Priority.CompareTo(other.Priority);

        public abstract Task ExecuteAsync();
    }

    public abstract class JobBase<TComplete> : IJob, IEquatable<JobBase<TComplete>>
    {
        public TaskCompletionSource<TComplete> CompleteSource { get; } = new();
        public CancellationTokenSource CancelSource { get; } = new();

        public bool Equals(JobBase<TComplete> other) => other != null && Position.Equals(other.Position) && Type.Equals(other.Type);
        public override bool Equals(object obj) => obj is JobBase<TComplete> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Position, (int)Type);
    }
}
