using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers
{
    public class EventDelayer
    {
        private readonly DispatcherTimer _timer;

        public EventDelayer(double seconds) : this(TimeSpan.FromSeconds(seconds))
        {
        }

        public EventDelayer(TimeSpan interval)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            Interval = interval;
        }

        public EventDelayer() : this(0.1)
        {
        }

        public TimeSpan Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool ResetWhenDelayed { get; set; }

        public void Delay()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
            else
            {
                if (ResetWhenDelayed)
                {
                    _timer.Stop();
                    _timer.Start();
                }
            }
        }


        private void _timer_Tick(object sender, object e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
            OnArrived();
        }

        public event EventHandler Arrived;
        protected void OnArrived()
        {
            Arrived?.Invoke(this, EventArgs.Empty);
        }

    }

    public class EventWaiter
    {
        private DateTime _lastTime;

        public EventWaiter(double seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
        }

        public EventWaiter(TimeSpan interval)
        {
            Interval = interval;
        }

        public EventWaiter()
        {
            Interval = TimeSpan.FromSeconds(0.1d);
        }

        public TimeSpan Interval { get; set; }

        public bool IsEnabled
        {
            get
            {
                if (DateTime.Now - _lastTime > Interval)
                {
                    _lastTime = DateTime.Now;
                    return true;
                }
                return false;
            }
        }

        public void Reset()
        {
            _lastTime = DateTime.Now;
        }
    }
}
