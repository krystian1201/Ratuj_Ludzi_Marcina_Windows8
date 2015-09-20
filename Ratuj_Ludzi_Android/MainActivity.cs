
using System;
using System.Collections.Generic;
using System.Timers;
using Android.Animation;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Interop;

namespace Ratuj_Ludzi_Android
{
    [Activity(Label = "Ratuj_Ludzi_Android", MainLauncher = true, Icon = "@drawable/icon", 
        ScreenOrientation = ScreenOrientation.Landscape, Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity
    {
        private const int ENEMY_HEIGHT_IN_DP = 60;
        private const int TARGET_HEIGHT_IN_DP = 35;

        private int EnemyWidthInPixels
        {
            get
            {
                const double widthToHeightRatio = 0.77;

                return (int) (widthToHeightRatio* EnemyHeightInPixels);
            }
        }

        private int EnemyHeightInPixels
        {
            get
            {
                return convertDpToPixels(ENEMY_HEIGHT_IN_DP);
            }
        }

        private ImageView HumanView
        {
            get
            {
                //TODO: is 1 the proper id here?
                return _humanView ?? (_humanView = _playArea.FindViewById<ImageView>(1));
            }
        }

        private ImageView TargetView
        {
            get
            {
                return _targetView ?? (_targetView = _playArea.FindViewById<ImageView>(2));
            }
        }

        #region Private fields

        private Random _random;
        private Timer _enemyTimer;
        private Timer _targetTimer;
        private bool _humanCaptured;

        private Button _startButton;
        private RelativeLayout _playArea;
        private ProgressBar _progressBar;
        private ImageView _humanView;
        private ImageView _targetView;
        private List<ImageView> _enemies; 

        #endregion Private fields

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            _random = new Random();

            _enemyTimer = new Timer();
            _enemyTimer.Elapsed += enemyTimer_Tick;
            _enemyTimer.Interval = 2000;


            _targetTimer = new Timer();
            _targetTimer.Elapsed += targetTimer_Tick;
            _targetTimer.Interval = 200;

            _humanCaptured = false;

            _enemies = new List<ImageView>();
            
            _startButton = FindViewById<Button>(Resource.Id.buttonStart);
            _playArea = FindViewById<RelativeLayout>(Resource.Id.relativeLayoutPlayground);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            
        }

        #region Private methods

        protected void enemyTimer_Tick(object sender, ElapsedEventArgs e)
        {
            addEnemy();
        }

        private void addEnemy()
        {

            Dictionary<string, int> enemyMargins = getEnemyTopAndLeftMargins();

            var enemyView = addImageViewToPlayArea(Resource.Drawable.enemy, enemyMargins["left"], enemyMargins["top"], 
                EnemyHeightInPixels);

            _enemies.Add(enemyView);

            //TODO: why is it not enough to substract enemie's dimensions?
            int horizontalAnimationRange = _playArea.Width- EnemyWidthInPixels - 85;
            int verticalAnimationRange = _playArea.Height - EnemyHeightInPixels - 80;

            animateEnemy(enemyView, 0, horizontalAnimationRange, "LeftMargin");
            animateEnemy(enemyView, _random.Next(verticalAnimationRange),
                _random.Next(verticalAnimationRange), "TopMargin");
        }

        //returned margin values are in pixels
        private Dictionary<string, int> getEnemyTopAndLeftMargins()
        {
            int enemyTopMargin = _random.Next(0, _playArea.Height - EnemyHeightInPixels);
            int enemyLeftMargin = _random.Next(0, _playArea.Width - EnemyWidthInPixels);

            return new Dictionary<string, int>
            {
                {"top", enemyTopMargin},
                {"left", enemyLeftMargin }
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

        private void targetTimer_Tick(object sender, ElapsedEventArgs e)
        {
            _progressBar.Progress += 1;

            if (_progressBar.Progress >= _progressBar.Max)
                endTheGame();
        }

        private void endTheGame()
        {
                _enemyTimer.Stop();
                _targetTimer.Stop();

                _humanCaptured = false;

                RunOnUiThread(() => _startButton.Enabled = true);

                addGameOverText();
 
        }

        private void startGame()
        {

            _humanCaptured = false;

            _progressBar.Progress = 0;

            _startButton.Enabled = false;

 
            _playArea.RemoveAllViews();
            _humanView = null;
            _targetView = null;
            _enemies.RemoveAll(e => true);

            addImageViewToPlayArea(Resource.Drawable.human2, convertDpToPixels(500), convertDpToPixels(100), 1);
            addImageViewToPlayArea(Resource.Drawable.target3, convertDpToPixels(100), convertDpToPixels(200), 2);

            HumanView.Touch += humanOnTouch;

            _enemyTimer.Start();
            _targetTimer.Start();
        }


        private void humanOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {

            switch (touchEventArgs.Event.Action)
            {
                case MotionEventActions.Down:

                    if (_enemyTimer.Enabled)
                    {
                        _humanCaptured = true;
                    }

                    break;
                    
                case MotionEventActions.Move:

                    if (_humanCaptured)
                    {
                        moveHuman(touchEventArgs);
                    }

                    break;
            }

        }

        private void moveHuman(View.TouchEventArgs touchEventArgs)
        {
   
            int touchX = (int) touchEventArgs.Event.RawX;
            int touchY = (int) touchEventArgs.Event.RawY;

            RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams) HumanView.LayoutParameters;

            int deltaX = touchX - layoutParams.LeftMargin;
            int deltaY = touchY - layoutParams.TopMargin;

            //end the game when human exited the play area
            if (touchX > _playArea.Width || touchY > _playArea.Height)
            {
                endTheGame();
                return;
            }

            
            //dropping human when moving too fast
            if (Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)) > HumanView.Width*2) 
            {
                _humanCaptured = false;
                return;
            }


            //actual movement of human
            layoutParams.LeftMargin += deltaX - HumanView.Width / 2;
            layoutParams.TopMargin += deltaY - HumanView.Height / 2;

            HumanView.LayoutParameters = layoutParams;

            //detecting collision of human and enemy
            //TODO: should it come before moving human?
            if (humanHitEnemy())
            {
                endTheGame();
            }
            else if(humanEnteredTarget())
            {
                _progressBar.Progress = 0;

                moveHumanAndTargetToNewRandomLocations();

                _humanCaptured = false;
            }
        }

        private bool humanHitEnemy()
        {
            RelativeLayout.LayoutParams humanLayoutParams = (RelativeLayout.LayoutParams)HumanView.LayoutParameters;

            int humanX = humanLayoutParams.LeftMargin;
            int humanY = humanLayoutParams.TopMargin;

            foreach (var enemy in _enemies)
            {
                RelativeLayout.LayoutParams enemyLayoutParams = (RelativeLayout.LayoutParams)enemy.LayoutParameters;

                int enemyX = enemyLayoutParams.LeftMargin;
                int enemyY = enemyLayoutParams.TopMargin;

                if (humanX > enemyX && humanX < enemyX + EnemyWidthInPixels &&
                    humanY > enemyY && humanY < enemyY + EnemyHeightInPixels)
                {

                    return true;
                }
            }

            return false;
        }

        private bool humanEnteredTarget()
        {
            RelativeLayout.LayoutParams humanLayoutParams = (RelativeLayout.LayoutParams)HumanView.LayoutParameters;

            int humanX = humanLayoutParams.LeftMargin;
            int humanY = humanLayoutParams.TopMargin;


            RelativeLayout.LayoutParams targetLayoutParams = (RelativeLayout.LayoutParams)TargetView.LayoutParameters;

            int targetX = targetLayoutParams.LeftMargin;
            int targetY = targetLayoutParams.TopMargin;

            //TODO: those conversions don't look great
            int targetHeightInPixels = convertDpToPixels(TARGET_HEIGHT_IN_DP);
            int targetWidthInPixels = (int) (1.03*targetHeightInPixels);

            if (humanX > targetX && humanX < targetX + targetWidthInPixels &&
                    humanY > targetY && humanY < targetY + targetHeightInPixels)
            {

                return true;
            }

            return false;
        }

        private void moveHumanAndTargetToNewRandomLocations()
        {
            const int horizontalVerticalMin = 100;
            int horizontalMax = _playArea.Width - 100;
            int verticalMax = _playArea.Height - 100;

            RelativeLayout.LayoutParams humanLayoutParams = (RelativeLayout.LayoutParams)HumanView.LayoutParameters;
            humanLayoutParams.LeftMargin = _random.Next(horizontalVerticalMin, horizontalMax);
            humanLayoutParams.TopMargin = _random.Next(horizontalVerticalMin, verticalMax);

            RelativeLayout.LayoutParams targetLayoutParams = (RelativeLayout.LayoutParams)TargetView.LayoutParameters;
            targetLayoutParams.LeftMargin = _random.Next(horizontalVerticalMin, horizontalMax);
            targetLayoutParams.TopMargin = _random.Next(horizontalVerticalMin, verticalMax);
        }

        private ImageView addImageViewToPlayArea(int drawableId, int marginLeft, int marginTop,
            int? id = null, int controlHeight = ViewGroup.LayoutParams.WrapContent)
        {
            ImageView imageView = new ImageView(this);
            imageView.SetImageResource(drawableId);

            if (id != null)
            {
                imageView.Id = id.Value;
            }

            RelativeLayout.LayoutParams layoutParams =
                new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, controlHeight);

            //margin values are in pixels
            layoutParams.SetMargins(marginLeft, marginTop, 0, 0);
            imageView.LayoutParameters = layoutParams;

            RunOnUiThread(() => _playArea.AddView(imageView));

            return imageView;
        }

        private void addGameOverText()
        {
            TextView gameOverTextView = new TextView(this);
            gameOverTextView.Text = "Koniec gry";
            gameOverTextView.TextSize = 48;
            gameOverTextView.SetTextColor(Color.White);

            RelativeLayout.LayoutParams layoutParams = 
                new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(convertDpToPixels(200), convertDpToPixels(150), 0, 0);
            gameOverTextView.LayoutParameters = layoutParams;
            gameOverTextView.BringToFront();

            RunOnUiThread(() => _playArea.AddView(gameOverTextView));
        }

        private int convertDpToPixels(int pixels)
        {
            float scale = ApplicationContext.Resources.DisplayMetrics.Density;

            return (int)(pixels * scale + 0.5f);
        }

        #endregion Private methods

        #region Public methods

        [Export("startButton_Click")]
        public void startButton_Click(View view)
        {
            startGame();
        }

        #endregion Public methods

    }

    public class MarginProxyAnimator : Java.Lang.Object
    {
        private readonly View _view;

        public MarginProxyAnimator(View view)
        {
            _view = view;
        }

        [Export]
        public int getTopMargin()
        {
            var lp = (ViewGroup.MarginLayoutParams)_view.LayoutParameters;
            return lp.TopMargin;
        }

        [Export]
        public void setTopMargin(int margin)
        {
            var lp = (ViewGroup.MarginLayoutParams)_view.LayoutParameters;
            lp.SetMargins(lp.LeftMargin, margin, lp.RightMargin, lp.BottomMargin);
            _view.RequestLayout();
        }

        [Export]
        public int getLefMargin()
        {
            var lp = (ViewGroup.MarginLayoutParams)_view.LayoutParameters;
            return lp.LeftMargin;
        }

        [Export]
        public void setLeftMargin(int margin)
        {
            var lp = (ViewGroup.MarginLayoutParams)_view.LayoutParameters;
            lp.SetMargins(margin, lp.TopMargin, lp.RightMargin, lp.BottomMargin);

            _view.RequestLayout();
        }
    }
}

