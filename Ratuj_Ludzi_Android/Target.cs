
using System.Timers;
using Android.Widget;

namespace SaveHumans_Android
{
    

    public class Target
    {
        public const int HEIGHT_IN_DP = 35;

        private Timer _timer;
        private ImageView _view;
        private RelativeLayout _playArea;

        public ImageView View
        {
            get
            {
                return _view ?? (_view = _playArea.FindViewById<ImageView>(2));
            }
        }

        public Timer Timer
        {
            get
            {
                return _timer ?? (_timer = new Timer());
            }
        }

        public Target(RelativeLayout playArea)
        {
            _playArea = playArea;
        }

        public void InitializeTimer(int interval, ElapsedEventHandler elapsedEventHandler)
        {
            Timer.Interval = interval;
            Timer.Elapsed += elapsedEventHandler;
        }

        public void SetViewToNull()
        {
            _view = null;
        }
    }
}