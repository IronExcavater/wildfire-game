using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<IJob> _jobLookup = new();

        private const int MaxConcurrentJobs = 10000;
        private int _runningJobs;

        public static Task<T> Enqueue<T>(JobBase<T> job)
        {
            if (TryGetExistingJob(job, out var existingJob))
                return existingJob.CompleteSource.Task;

            Debug.Log($"Staged {job.Type} job at {job.Position}");

            Instance._stagedJobs.Enqueue(job);
            Instance._jobLookup.Add(job);
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
            foreach (var job in Instance._jobLookup.Where(j => j.Position == position && j is TJob).ToList())
                job.Cancel();
        }

        public static void CancelAllJobsAtPosition(Vector2Int position)
        {
            foreach (var job in Instance._jobLookup.Where(j => j.Position == position).ToList())
                job.Cancel();
        }

        public static void CancelAllJobs()
        {
            foreach (var job in Instance._jobLookup.ToList())
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
                job.Priority = ComputePriority(job.Position);

                if (_pendingJobs.Add(job))
                {
                    Debug.Log($"Enqueued {job}");
                    continue;
                }
                Debug.LogError($"Failed to enqueue {job.Type} job at {job.Position}");
            }

            while (_runningJobs < MaxConcurrentJobs & _pendingJobs.Count > 0)
            {
                var job = _pendingJobs.Min;
                _pendingJobs.Remove(job);

                _runningJobs++;
                job.IsRunning = true;

                _ = RunJobAsync(job);
            }
        }

        private async Task RunJobAsync(IJob job)
        {
            try
            {
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
                _jobLookup.Remove(job);
                _runningJobs--;
            }
        }

        private float ComputePriority(Vector2Int position)
        {
            var cameraChunk = WorldLoader.CameraChunkPosition();
            return (position - cameraChunk).magnitude;
        }
    }
}
