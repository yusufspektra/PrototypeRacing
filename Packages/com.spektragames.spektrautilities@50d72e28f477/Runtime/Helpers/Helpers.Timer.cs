using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    public static partial class Helpers
    {
        public static class Time
        {
            private static readonly List<Timer> _timers = new List<Timer>();
            private static readonly Queue<Timer> _unregisterQueue = new Queue<Timer>();
            private static readonly Queue<Timer> _registerQueue = new Queue<Timer>();
            private static bool _isRunning;

            public static void RegisterTimer(Timer timer)
            {
                if (timer.GetRemainingTime() <= 0)
                {
                    timer.OnCompleted?.Invoke();
                }
                else
                {
                    _registerQueue.Enqueue(timer);
                }
                
                if (_isRunning)
                    return;
                
                StartAllTimers();
            }

            public static void UnregisterTimer(Timer timer)
            {
                _unregisterQueue.Enqueue(timer);
            }

            private static void StartAllTimers()
            {
                foreach (var timer in _timers)
                {
                    timer.Start();
                }

                _isRunning = true;
                RunTimerLoop().Forget();
            }

            private static async UniTaskVoid RunTimerLoop()
            {
                float lastTickTime = UnityEngine.Time.realtimeSinceStartup;

                while (_isRunning)
                {
                    TryDequeuingStartTimers();
                    TryDequeuingUnregisteredTimers();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    // Calculate delta AFTER the delay
                    float currentTime = UnityEngine.Time.realtimeSinceStartup;
                    float deltaTime = currentTime - lastTickTime;
                    lastTickTime = currentTime;

                    TickAllTimers(deltaTime);
                }
            }

            private static void TryDequeuingUnregisteredTimers()
            {
                while (_unregisterQueue.Count > 0)
                {
                    var timer = _unregisterQueue.Dequeue();
                    _timers.Remove(timer);
                    if (_timers.Count == 0)
                    {
                        StopAllTimers();
                    }
                }
            }

            private static void TryDequeuingStartTimers()
            {
                while (_registerQueue.Count > 0)
                {
                    var timer = _registerQueue.Dequeue();
                    _timers.Add(timer);
                    timer.OnCompleted += () => UnregisterTimer(timer);
                    if (_isRunning)
                    {
                        timer.Start();
                    }
                }
            }

            private static void TickAllTimers(double deltaTime)
            {
                foreach (var timer in _timers)
                {
                    timer.Tick(deltaTime);
                }
            }

            private static void StopAllTimers()
            {
                _isRunning = false;
                _timers.Clear();
            }
        }

        public class Timer
        {
            public event Action OnStarted;
            public event Action<double> OnUpdated;
            public Action OnCompleted;

            private double _remainingTime;

            public Timer(double duration)
            {
                _remainingTime = duration;
            }

            public void Start()
            {
                OnStarted?.Invoke();
            }

            public void Tick(double deltaTime)
            {
                if (_remainingTime > 0)
                {
                    _remainingTime -= deltaTime;
                    OnUpdated?.Invoke(_remainingTime);
                    if (_remainingTime <= 0)
                    {
                        _remainingTime = 0;
                        OnCompleted?.Invoke();
                    }
                }
            }

            public double GetRemainingTime()
            {
                return _remainingTime;
            }
        }

        public enum TimeFormat
        {
            Seconds,
            MinutesAndSeconds,
            Hours
        }

        public static string FormatTime(float timeInSeconds, TimeFormat format)
        {
            switch (format)
            {
                case TimeFormat.Seconds:
                    return $"{timeInSeconds:F2}";
                case TimeFormat.MinutesAndSeconds:
                    int minutes = Mathf.FloorToInt(timeInSeconds / 60);
                    float seconds = timeInSeconds % 60;
                    return $"{minutes:00}:{seconds:00}";
                case TimeFormat.Hours:
                    int hours = Mathf.FloorToInt(timeInSeconds / 3600);
                    int remainingMinutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
                    float remainingSeconds = timeInSeconds % 60;
                    return $"{hours:00}:{remainingMinutes:00}:{remainingSeconds:00}";
                default:
                    return timeInSeconds.ToString();
            }
        }
    }
}