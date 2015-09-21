using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ratuj_Ludzi_Android;

namespace SaveHumans_Android
{
    public class Enemy
    {
        public const int HEIGHT_IN_DP = 60;

        private Timer _timer;

        private Random _random;

        public Timer Timer
        {
            get
            {
                return _timer ?? (_timer = new Timer());
            }
        }

        public Enemy(Random random)
        {
            _random = random;
        }

        public void InitializeTimer(int interval, ElapsedEventHandler elapsedEventHandler)
        {
            Timer.Interval = interval;
            Timer.Elapsed += elapsedEventHandler;
        }


        public void AddEnemy()
        {

            Dictionary<string, int> enemyMargins = getEnemyTopAndLeftMargins();

            ImageView enemyView =
                addImageViewToPlayArea(Resource.Drawable.enemy, enemyMargins["left"], enemyMargins["top"],
                    EnemyHeightInPixels);

            _enemies.Add(enemyView);


            Dictionary<string, int> animationRanges = getEnemyAnimationMaxRanges();

            animateEnemy(enemyView, 0, animationRanges["horizontal"], "LeftMargin");
            animateEnemy(enemyView, _random.Next(animationRanges["vertical"]),
                _random.Next(animationRanges["vertical"]), "TopMargin");
        }


        //returned margin values are in pixels
        private Dictionary<string, int> getEnemyTopAndLeftMargins()
        {
            int enemyTopMargin = _random.Next(0, PlayArea.Height - EnemyHeightInPixels);
            int enemyLeftMargin = _random.Next(0, PlayArea.Width - EnemyWidthInPixels);

            return new Dictionary<string, int>
                {
                    {"top", enemyTopMargin},
                    {"left", enemyLeftMargin }
                };
        }


        private Dictionary<string, int> getEnemyAnimationMaxRanges()
        {
            //TODO: why is it not enough to substract enemie's dimensions?
            int horizontalAnimationRange = PlayArea.Width - EnemyWidthInPixels - 85;
            int verticalAnimationRange = PlayArea.Height - EnemyHeightInPixels - 80;

            return new Dictionary<string, int>
                    {
                        {"horizontal", horizontalAnimationRange},
                        {"vertical", verticalAnimationRange}
                    };
        }

        private void animateEnemy(ImageView enemy, int from, int to, string propertyToAnimate)
        {

            ObjectAnimator anim = ObjectAnimator.OfInt(new MarginProxyAnimator(enemy), propertyToAnimate, from, to);
            anim.SetDuration(_random.Next(4000, 6000));
            anim.RepeatCount = int.MaxValue;
            anim.RepeatMode = ValueAnimatorRepeatMode.Reverse;

            RunOnUiThread(() => anim.Start());
        }
    }
}