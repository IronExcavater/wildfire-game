using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Generation.Jobs;
using UnityEngine;
using Utilities;

namespace Generation
{
    public class JobManager : Singleton<JobManager>
    {
        private readonly ConcurrentQueue<IJob> _stagedJobs = new();
        private readonly SortedSet<IJob> _pendingJobs = new();
        private readonly ConcurrentDictionary<IJob, IJob> _jobLookup = new();

        private const int MaxConcurrentJobs = 8;
        private int _runningJobs;

        public static Task<T> Enqueue<T>(JobBase<T> job)
        {
            var actual = (JobBase<T>)Instance._jobLookup.GetOrAdd(job, job);

            if (!ReferenceEquals(actual, job))
            {
                if (!actual.IsRunning)
                    Debug.LogWarning($"Existing {actual} is stale");
                return actual.CompleteSource.Task;
            }

            Debug.Log($"Staged {job}");

            Instance._stagedJobs.Enqueue(job);
            return job.CompleteSource.Task;
        }

        public static bool TryGetExistingJob<TComplete>(JobBase<TComplete> job, out JobBase<TComplete> existingJob)
        {
            if (Instance._jobLookup.TryGetValue(job, out var ijob) && ijob is JobBase<TComplete> typed)
            {
                existingJob = typed;
                return true;
            }

            existingJob = null;
            return false;
        }

        public static void CancelAllJobsOfTypeAtPosition<TJob>(Vector2Int position) where TJob : IJob
        {
            foreach (var job in Instance._jobLookup.Values.Where(j => j.Position == position && j is TJob).ToList())
                TryCancelJob(job);
        }

        public static void CancelAllJobsAtPosition(Vector2Int position)
        {
            foreach (var job in Instance._jobLookup.Values.Where(j => j.Position == position).ToList())
                TryCancelJob(job);
        }

        public static void CancelAllJobs()
        {
            foreach (var job in Instance._jobLookup.Values.ToList())
                TryCancelJob(job);
        }

        public static void TryCancelJob(IJob job)
        {
            Instance._jobLookup.TryRemove(job, out _);
            job.Cancel();
        }

        protected override void Awake()
        {
            base.Awake();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    CancelAllJobs();
            };
            #endif
        }

        private void Update()
        {
            while (_stagedJobs.TryDequeue(out var job))
            {
                if (!job.IsRunning) continue;

                job.Priority = ComputePriority(job.Position);

                if (job.Parent != null)
                {
                    _ = RunJobAsync(job);
                    continue;
                }

                if (!_pendingJobs.Add(job))
                    throw new Exception($"Failed to enqueue {job.Type} job at {job.Position}");

                Debug.Log($"Enqueued {job}");
            }

            while (_runningJobs < MaxConcurrentJobs & _pendingJobs.Count > 0)
            {
                var job = _pendingJobs.Min;
                _pendingJobs.Remove(job);

                if (!job.IsRunning) continue;

                _ = RunJobAsync(job);
            }
        }

        private async Task RunJobAsync(IJob job)
        {
            try
            {
                Interlocked.Increment(ref _runningJobs);
                job.IsRunning = true;

                Debug.Log($"Started {job}");
                await job.Start();
                Debug.Log($"Completed {job}");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed {job} threw an exception: {e}");
            }
            finally
            {
                _jobLookup.TryRemove(job, out _);
                Interlocked.Decrement(ref _runningJobs);
            }
        }

        private float ComputePriority(Vector2Int position)
        {
            var cameraChunk = WorldLoader.CameraChunkPosition();
            return (position - cameraChunk).magnitude;
        }
    }
}
