using System;
using System.Collections.Generic;
using System.Timers;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Widget;
using Ratuj_Ludzi_Android;

namespace SaveHumans_Android
{
    public class Enemy
    {
        #region consts

            public const int HEIGHT_IN_DP = 60;

        #endregion consts

        #region Private fields

            private Timer _timer;

            private Random _random;

            private MainActivity _activity;

            private List<ImageView> _enemies;

        #endregion Private fields

        #region Private properties



        #endregion Private properties


        #region Public properties

            //TODO: should it be private?
            public int EnemyWidthInPixels
            {
                get
                {
                    const double widthToHeightRatio = 0.77;

                    return (int)(widthToHeightRatio * EnemyHeightInPixels);
                }
            }

            //TODO: should it be private?
            public int EnemyHeightInPixels
            {
                get
                {
                    return ViewHelper.ConvertDpToPixels(Enemy.HEIGHT_IN_DP);
                }
            }

            public Timer Timer
                {
                    get
                    {
                        return _timer ?? (_timer = new Timer());
                    }
                }

            public List<ImageView> Enemies
            {
                get
                {
                    return _enemies;
                }
            } 

        #endregion Public properties

        #region Public methods

            public Enemy(MainActivity activity, Random random)
            {
                _random = random;
                _activity = activity;

                _enemies = new List<ImageView>();
            }


            public void InitializeTimer(int interval, ElapsedEventHandler elapsedEventHandler)
            {
                Timer.Interval = interval;
                Timer.Elapsed += elapsedEventHandler;
            }


            public ImageView SetupNewEnemyView(Context context)
            {
                Dictionary<string, int> enemyMargins = getEnemyTopAndLeftMargins();

                ImageView enemyView =
                    ViewHelper.CreateImageView(context, Resource.Drawable.enemy, enemyMargins["left"], enemyMargins["top"],
                        EnemyHeightInPixels);

                _enemies.Add(enemyView);

                return enemyView;
            }

            public Dictionary<string, ObjectAnimator> SetupEnemyAnimations(ImageView enemyView)
            {
                Dictionary<string, int> animationRanges = getEnemyAnimationMaxRanges();

                var horizontalAnimation = 
                    setupEnemyAnimation(enemyView, 0, animationRanges["horizontal"], "LeftMargin");

                var verticalAnimation = 
                    setupEnemyAnimation(enemyView, _random.Next(animationRanges["vertical"]),
                        _random.Next(animationRanges["vertical"]), "TopMargin");

                return new Dictionary<string, ObjectAnimator>
                {
                    { "horizontal", horizontalAnimation },
                    { "vertical", verticalAnimation }
                };
            }


        public void RemoveAllEnemies()
        {
            _enemies.RemoveAll(e => true);
        }

        #endregion Public methods

        #region Private methods

            private Dictionary<string, int> getEnemyTopAndLeftMargins()
            {
                int enemyTopMargin = _random.Next(0, _activity.PlayArea.Height - EnemyHeightInPixels);
                int enemyLeftMargin = _random.Next(0, _activity.PlayArea.Width - EnemyWidthInPixels);

                return new Dictionary<string, int>
                    {
                        {"top", enemyTopMargin},
                        {"left", enemyLeftMargin }
                    };
            }


            private Dictionary<string, int> getEnemyAnimationMaxRanges()
            {
                //TODO: why is it not enough to substract enemie's dimensions?
                int horizontalAnimationRange = _activity.PlayArea.Width - EnemyWidthInPixels - 85;
                int verticalAnimationRange = _activity.PlayArea.Height - EnemyHeightInPixels - 80;

                return new Dictionary<string, int>
                        {
                            {"horizontal", horizontalAnimationRange},
                            {"vertical", verticalAnimationRange}
                        };
            }

            private ObjectAnimator setupEnemyAnimation(ImageView enemy, int from, int to, string propertyToAnimate)
            {

                ObjectAnimator anim = ObjectAnimator.OfInt(new MarginProxyAnimator(enemy), propertyToAnimate, from, to);
                anim.SetDuration(_random.Next(4000, 6000));
                anim.RepeatCount = int.MaxValue;
                anim.RepeatMode = ValueAnimatorRepeatMode.Reverse;

                return anim;
            }

        #endregion Private methods

    }
}