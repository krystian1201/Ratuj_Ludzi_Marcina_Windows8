
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

        #region Private fields

            private Random _random;
            
            private bool _humanCaptured;

            private Button _startButton;
            private RelativeLayout _playArea;
            private ProgressBar _progressBar;
            private ImageView _humanView;
            
            
            private Target _target;
            private Enemy _enemy;

        #endregion Private fields


        #region Private properties

            

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

            public RelativeLayout PlayArea
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

            private Enemy Enemy
            {
                get
                {
                    return _enemy ?? (_enemy = new Enemy(this, _random));
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

            ViewHelper.Activity = this;
        }


        #region Private methods

            private void initializeTimers()
            {
                //TODO: make sth with timer to make it more generic (interface?)

                Enemy.InitializeTimer(2000, enemyTimer_Tick);

                Target.InitializeTimer(200, targetTimer_Tick);
            }

            private void enemyTimer_Tick(object sender, ElapsedEventArgs e)
            {
                ImageView enemyView = Enemy.SetupNewEnemyView(this);

                Dictionary<string, ObjectAnimator> animations =
                    Enemy.SetupEnemyAnimations(enemyView);

                RunOnUiThread(() => animations["horizontal"].Start());
                RunOnUiThread(() => animations["vertical"].Start());

                //TODO: Is it needed?
                RunOnUiThread(() => PlayArea.AddView(enemyView));
            }


            private void targetTimer_Tick(object sender, ElapsedEventArgs e)
            {
                ProgressBar.Progress += 1;

                if (ProgressBar.Progress >= ProgressBar.Max)
                    endTheGame();
            }

            private void endTheGame()
            {
                    Enemy.Timer.Stop();
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

                layoutParams.SetMargins(ViewHelper.ConvertDpToPixels(200), 
                    ViewHelper.ConvertDpToPixels(150), 0, 0);

                gameOverTextView.LayoutParameters = layoutParams;
                gameOverTextView.BringToFront();

                RunOnUiThread(() => PlayArea.AddView(gameOverTextView));
            }

            private void startGame()
            {

                resetControls();

                _humanCaptured = false;

                Enemy.Timer.Start();
                Target.Timer.Start();
            }

            private void resetControls()
            {
                ProgressBar.Progress = 0;

                StartButton.Enabled = false;


                PlayArea.RemoveAllViews();
                _humanView = null;
                Target.SetViewToNull();
                Enemy.RemoveAllEnemies();


                var humanImageView = ViewHelper.CreateImageView(this, Resource.Drawable.human2, 
                    ViewHelper.ConvertDpToPixels(500), ViewHelper.ConvertDpToPixels(100), 1);

                RunOnUiThread(() => PlayArea.AddView(humanImageView));


                var targetImageView = ViewHelper.CreateImageView(this, Resource.Drawable.target3,
                    ViewHelper.ConvertDpToPixels(100), ViewHelper.ConvertDpToPixels(200), 2);

                RunOnUiThread(() => PlayArea.AddView(targetImageView));

                HumanView.Touch += humanOnTouch;
            }


            private void humanOnTouch(object sender, View.TouchEventArgs touchEventArgs)
            {
                switch (touchEventArgs.Event.Action)
                {
                    //TODO: is it needed on mobile version?
                    case MotionEventActions.Down:

                        if (Enemy.Timer.Enabled)
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

                foreach (var enemy in Enemy.Enemies)
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

                if (humanX > enemyX && humanX < enemyX + Enemy.EnemyWidthInPixels &&
                    humanY > enemyY && humanY < enemyY + Enemy.EnemyHeightInPixels)
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
                int targetHeightInPixels = ViewHelper.ConvertDpToPixels(Target.HEIGHT_IN_DP);
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

