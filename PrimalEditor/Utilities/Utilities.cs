using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Represents a unique identifier for an entity
    /// </summary>
    public static class ID
    {
        /// <summary>
        /// The value of an invalid ID.
        /// </summary>
        public static int INVALID_ID => -1;
        /// <summary>
        /// Determines whether the specified ID is valid.
        /// </summary>
        /// <param name="id">The ID to check.</param>
        /// <returns>
        /// <c>true</c> if the ID is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(int id) => id != INVALID_ID;
    }
    /// <summary>
    /// Useful for accessing non-standard utilities helpful for mathematics
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// The value of the epsilon constant.
        /// </summary>
        public static float Epsilon => 0.00001f;
        /// <summary>
        /// Determines whether the two specified floats are equal, within the specified epsilon.
        /// </summary>
        /// <param name="value">The first float.</param>
        /// <param name="other">The second float.</param>
        /// <returns>
        /// <c>true</c> if the two floats are equal, within the specified epsilon; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTheSameAs(this float value, float other) => Math.Abs(value - other) < Epsilon;
        /// <summary>
        /// Determines whether the two specified nullable floats are equal, within the specified epsilon.
        /// </summary>
        /// <param name="value">The first nullable float.</param>
        /// <param name="other">The second nullable float.</param>
        /// <returns>
        /// <c>true</c> if the two nullable floats are equal, within the specified epsilon; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTheSameAs(this float? value, float? other)
        {
            if (!value.HasValue || !other.HasValue) return false;
            return Math.Abs(value.Value - other.Value) < Epsilon;
        }
    }
    class DelayEventTimerArgs : EventArgs
    {
        public bool RepeatEvent { get; set; }
        public IEnumerable<object> Data { get; set; }
        public DelayEventTimerArgs(IEnumerable<object> data) => Data = data;
    }
    class DelayEventTimer
    {
        private readonly DispatcherTimer _timer;
        private readonly TimeSpan _delay;
        private readonly List<Object> _data = new();
        private DateTime _lastEventTime = DateTime.Now;
        public event EventHandler<DelayEventTimerArgs>? Triggered;
        public void Trigger(object? data = null)
        {
            if (data != null) _data.Add(data);
            _lastEventTime = DateTime.Now;
            _timer.IsEnabled = true;
        }
        public void Disable() => _timer.IsEnabled = false;
        private void OnTimerTick(object? sender, EventArgs e)
        {
            if ((DateTime.Now - _lastEventTime) < _delay) return;
            var eventArgs = new DelayEventTimerArgs(_data);
            Triggered?.Invoke(this, eventArgs);
            if (!eventArgs.RepeatEvent) _data.Clear();
            _timer.IsEnabled = eventArgs.RepeatEvent;
        }
        public DelayEventTimer(TimeSpan delay, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _delay = delay;
            _timer = new DispatcherTimer(priority)
            {
                Interval = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 0.5)
            };
            _timer.Tick += OnTimerTick;
        }
    }
}
