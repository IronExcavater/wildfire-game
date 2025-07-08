using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Generation.Jobs
{
    public enum ChunkJobType
    {
        GenerateChunk,
        LoadChunk,
        BuildTerrain,
        UnloadChunk
    }

    public abstract class IJob : IComparable<IJob>, IEquatable<IJob>
    {
        public ChunkJobType Type { get; }
        public Vector2Int Position { get; }
        public float Priority { get; set; }
        public bool IsRunning { get; set; } = true;

        public IJob(ChunkJobType type, Vector2Int position)
        {
            Type = type;
            Position = position;
        }

        public virtual int CompareTo(IJob other) =>
            (Priority, Position.x, Position.y, (int)Type).CompareTo(
                (other.Priority, other.Position.x, other.Position.y, (int)other.Type));

        public bool Equals(IJob other) =>
            other != null &&
            Position.Equals(other.Position) &&
            Type.Equals(other.Type);
        public override bool Equals(object obj) => obj is IJob other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Position, (int)Type);

        public abstract Task Start();
        public abstract void Cancel();

        public override string ToString() => $"{Type} job at {Position}";
    }

    public abstract class JobBase<TComplete> : IJob
    {
        public TaskCompletionSource<TComplete> CompleteSource { get; } = new();
        public CancellationTokenSource CancelSource { get; } = new();

        protected JobBase(ChunkJobType type, Vector2Int position) : base(type, position)
        {
        }

        public override void Cancel()
        {
            if (IsRunning)
                Debug.Log($"Cancelled {ToString()}");
            CancelSource.Cancel();
            IsRunning = false;
        }
    }
}
