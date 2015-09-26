
using System;
using System.Timers;
using Android.Widget;

namespace SaveHumans_Android
{
    

    public class Target
    {
        public const int HEIGHT_IN_DP = 35;

        #region Private fields

            private readonly Random _random;
            private Timer _timer;
            private ImageView _view;
            private readonly RelativeLayout _playArea;

        #endregion Private fields

        #region Public properties

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

        #endregion Public properties

        #region Public methods

            public Target(Random random, RelativeLayout playArea)
            {
                _playArea = playArea;
                _random = random;
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

            public void moveTargetToNewRandomLocation()
            {
                const int horizontalVerticalMin = 100;
                int horizontalMax = _playArea.Width - 100;
                int verticalMax = _playArea.Height - 100;

                RelativeLayout.LayoutParams targetLayoutParams = (RelativeLayout.LayoutParams)View.LayoutParameters;
                targetLayoutParams.LeftMargin = _random.Next(horizontalVerticalMin, horizontalMax);
                targetLayoutParams.TopMargin = _random.Next(horizontalVerticalMin, verticalMax);
            }

        #endregion Public methods

    }
}