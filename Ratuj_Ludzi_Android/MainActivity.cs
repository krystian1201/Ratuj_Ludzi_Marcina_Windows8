
using System;
using System.Collections.Generic;
using System.Timers;
using Android.Animation;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Ratuj_Ludzi_Android;

namespace SaveHumans_Android
{
    [Activity(Label = "Save Humans", MainLauncher = true, Icon = "@drawable/icon", 
        ScreenOrientation = ScreenOrientation.Landscape, Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity
    {
        #region Private consts

            private const int ENEMY_HEIGHT_IN_DP = 60;
            private const int TARGET_HEIGHT_IN_DP = 35;

        #endregion Private consts

        
        #region Private fields

            private Random _random;
            private Timer _enemyTimer;
            
            private bool _humanCaptured;

            private Button _startButton;
            private RelativeLayout _playArea;
            private ProgressBar _progressBar;
            private ImageView _humanView;
            
            private List<ImageView> _enemies;
            private Target _target;

        #endregion Private fields


        #region Private properties

            private int EnemyWidthInPixels
            {
                get
                {
                    const double widthToHeightRatio = 0.77;

                    return (int)(widthToHeightRatio * EnemyHeightInPixels);
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

            

            private Button StartButton
            {
                get
                {
                    return _startButton ?? (_startButton = FindViewById<Button>(Resource.Id.buttonStart));
                }
            }

            private RelativeLayout PlayArea
            {
                get
                {
                    return _playArea ?? (_playArea = FindViewById<RelativeLayout>(Resource.Id.relativeLayoutPlayground));
                }
            
            }

            private ProgressBar ProgressBar
            {
                get
                {
                    return _progressBar ?? (_progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar1));
                }
            }

            private RelativeLayout.LayoutParams HumanLayoutParams
            {
                get
                {
                    return (RelativeLayout.LayoutParams) HumanView.LayoutParameters;
                }
            }

            private Target Target
            {
                get
                {
                    return _target ?? (_target = new Target(PlayArea));
                }
            }

        #endregion Private properties


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            _random = new Random();

            initializeTimers();

            _humanCaptured = false;

            _enemies = new List<ImageView>();
            
        }


        #region Private methods

            private void initializeTimers()
            {
                _enemyTimer = new Timer();
                _enemyTimer.Elapsed += enemyTimer_Tick;
                _enemyTimer.Interval = 2000;

                Target.InitializeTimer(200, targetTimer_Tick);
            }

            private void enemyTimer_Tick(object sender, ElapsedEventArgs e)
            {
                addEnemy();
            }

            private void addEnemy()
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

            private void targetTimer_Tick(object sender, ElapsedEventArgs e)
            {
                ProgressBar.Progress += 1;

                if (ProgressBar.Progress >= ProgressBar.Max)
                    endTheGame();
            }

            private void endTheGame()
            {
                    _enemyTimer.Stop();
                    Target.Timer.Stop();

                    _humanCaptured = false;

                    RunOnUiThread(() => StartButton.Enabled = true);

                    addGameOverText();
 
            }

            private void addGameOverText()
            {
                TextView gameOverTextView = new TextView(this);

                gameOverTextView.Text = "Game over";
                gameOverTextView.TextSize = 48;
                gameOverTextView.SetTextColor(Color.White);


                RelativeLayout.LayoutParams layoutParams =
                    new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);

                layoutParams.SetMargins(convertDpToPixels(200), convertDpToPixels(150), 0, 0);
                gameOverTextView.LayoutParameters = layoutParams;
                gameOverTextView.BringToFront();

                RunOnUiThread(() => PlayArea.AddView(gameOverTextView));
            }

            private void startGame()
            {

                resetControls();

                _humanCaptured = false;

                _enemyTimer.Start();
                Target.Timer.Start();
            }

            private void resetControls()
            {
                ProgressBar.Progress = 0;

                StartButton.Enabled = false;


                PlayArea.RemoveAllViews();
                _humanView = null;
                Target.SetViewToNull();
                _enemies.RemoveAll(e => true);


                addImageViewToPlayArea(Resource.Drawable.human2, convertDpToPixels(500), convertDpToPixels(100), 1);
                addImageViewToPlayArea(Resource.Drawable.target3, convertDpToPixels(100), convertDpToPixels(200), 2);

                HumanView.Touch += humanOnTouch;
            }


            private void humanOnTouch(object sender, View.TouchEventArgs touchEventArgs)
            {
                switch (touchEventArgs.Event.Action)
                {
                    //TODO: is it needed on mobile version?
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

                
                int deltaX = touchX - HumanLayoutParams.LeftMargin;
                int deltaY = touchY - HumanLayoutParams.TopMargin;


                //end the game when human exited the play area
                if (humanExitedPlayArea(touchX, touchY))
                {
                    endTheGame();
                    return;
                }

                //dropping human when moving too fast
                if (humanMovesTooFast(deltaX, deltaY)) 
                {
                    _humanCaptured = false;
                    return;
                }


                //actual movement of human
                moveHuman(HumanLayoutParams, deltaX, deltaY);

                //detecting collision of human and enemy
                //TODO: should it come before moving human?
                if (humanHitAnyOfEnemies())
                {
                    endTheGame();
                }
                else if(humanEnteredTarget())
                {
                    ProgressBar.Progress = 0;

                    moveHumanAndTargetToNewRandomLocations();

                    _humanCaptured = false;
                }
            }

        
            private bool humanExitedPlayArea(int touchX, int touchY)
            {
                return touchX > PlayArea.Width || touchY > PlayArea.Height;
            }

            private bool humanMovesTooFast(int deltaX, int deltaY)
            {
                return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)) > HumanView.Width * 2;
            }

            private void moveHuman(RelativeLayout.LayoutParams layoutParams, int deltaX, int deltaY)
            {
                layoutParams.LeftMargin += deltaX - HumanView.Width / 2;
                layoutParams.TopMargin += deltaY - HumanView.Height / 2;

                HumanView.LayoutParameters = layoutParams;
            }

            private bool humanHitAnyOfEnemies()
            {

                int humanX = HumanLayoutParams.LeftMargin;
                int humanY = HumanLayoutParams.TopMargin;

                foreach (var enemy in _enemies)
                {
                    if (humanHitThisEnemy(enemy, humanX, humanY))
                        return true;
                }

                return false;
            }

            private bool humanHitThisEnemy(ImageView enemy, int humanX, int humanY)
            {
                RelativeLayout.LayoutParams enemyLayoutParams = (RelativeLayout.LayoutParams) enemy.LayoutParameters;

                int enemyX = enemyLayoutParams.LeftMargin;
                int enemyY = enemyLayoutParams.TopMargin;

                if (humanX > enemyX && humanX < enemyX + EnemyWidthInPixels &&
                    humanY > enemyY && humanY < enemyY + EnemyHeightInPixels)
                {
                    return true;
                }

                return false;
            }


            private bool humanEnteredTarget()
            {

                int humanX = HumanLayoutParams.LeftMargin;
                int humanY = HumanLayoutParams.TopMargin;


                RelativeLayout.LayoutParams targetLayoutParams = (RelativeLayout.LayoutParams)Target.View.LayoutParameters;

                int targetX = targetLayoutParams.LeftMargin;
                int targetY = targetLayoutParams.TopMargin;

                //TODO: those conversions don't look great (but they are used only in this place in code)
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
                int horizontalMax = PlayArea.Width - 100;
                int verticalMax = PlayArea.Height - 100;


                HumanLayoutParams.LeftMargin = _random.Next(horizontalVerticalMin, horizontalMax);
                HumanLayoutParams.TopMargin = _random.Next(horizontalVerticalMin, verticalMax);

                RelativeLayout.LayoutParams targetLayoutParams = (RelativeLayout.LayoutParams)Target.View.LayoutParameters;
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

                RunOnUiThread(() => PlayArea.AddView(imageView));

                return imageView;
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

}

