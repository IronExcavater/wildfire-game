using System;
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
        private readonly SortedSet<IJob> _pendingJobs = new();

        private const int MaxConcurrentJobs = 8;
        private int _runningJobs;
        private Camera _camera;

        public static Task<T> Enqueue<T>(JobBase<T> job)
        {
            if (TryGetExistingJob(job, out var existingJob))
                return existingJob.CompleteSource.Task;

            Instance._pendingJobs.Add(job);
            return job.CompleteSource.Task;
        }

        public static bool TryGetExistingJob<TComplete>(JobBase<TComplete> job, out JobBase<TComplete> existingJob)
        {
            Instance._pendingJobs.TryGetValue(job, out var ijob);
            existingJob = (JobBase<TComplete>)ijob;
            return existingJob != null;
        }

        private void Update()
        {
            RecalculatePriorities();

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
                await job.ExecuteAsync();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError($"Job {job.Type} at {job.Position} threw an exception: {e}");
            }
            finally
            {
                _runningJobs--;
            }
        }

        private void RecalculatePriorities()
        {
            foreach (var job in _pendingJobs.ToList())
            {
                var old = job.Priority;
                job.Priority = ComputePriority(job.Position);

                if (Math.Abs(old - job.Priority) > 0.01f)
                {
                    _pendingJobs.Remove(job);
                    _pendingJobs.Add(job);
                }
            }
        }

        private float ComputePriority(Vector2Int position)
        {
            if (_camera == null) _camera = Camera.main;

            var cameraChunk = WorldLoader.CameraChunkPosition();
            return (position - cameraChunk).magnitude;
        }
    }
}
