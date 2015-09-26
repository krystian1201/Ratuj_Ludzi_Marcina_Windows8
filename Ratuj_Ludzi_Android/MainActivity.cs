
using System;
using System.Collections.Generic;
using System.Timers;
using Android.Animation;
using Android.App;
using Android.Content.PM;
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
            
            private Button _startButton;
            private RelativeLayout _playArea;
            private ProgressBar _progressBar;
            
            private Target _target;
            private Human _human;
            private Enemy _enemy;

        #endregion Private fields


        #region Private properties
  

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

            private Human Human
            {
                get
                {
                    return _human ?? (_human = new Human(_random, PlayArea, _enemy, _target, this));
                }
                set
                {
                    _human = value;
                }
            }

            private Target Target
            {
                get
                {
                    return _target ?? (_target = new Target(_random, PlayArea));
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


            ViewHelper.Activity = this;
            ViewHelper.Context = this;


            Human.IsCaptured = false;

        }

        #region Public methods

            [Export("startButton_Click")]
            public void startButton_Click(View view)
            {
                startGame();
            }

            //TODO: it doesn't look good that this method has to be public
            public void EndTheGame()
            {
                Enemy.Timer.Stop();
                Target.Timer.Stop();

                Human.IsCaptured = false;

                RunOnUiThread(() => StartButton.Enabled = true);

                TextView gameOverTextView = ViewHelper.CreateGameOverTextView();

                RunOnUiThread(() => PlayArea.AddView(gameOverTextView));

            }

            public void SetProgressBarValueToZero()
            {
                ProgressBar.Progress = 0;
            }

        #endregion Public methods

        #region Private methods

            private void initializeTimers()
            {
                //TODO: make sth with timer to make it more generic (interface? abstract class?)

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

                RunOnUiThread(() => PlayArea.AddView(enemyView));
            }

            private void targetTimer_Tick(object sender, ElapsedEventArgs e)
            {
                ProgressBar.Progress += 1;

                if (ProgressBar.Progress >= ProgressBar.Max)
                    EndTheGame();
            }

            private void startGame()
            {
                resetControls();

                Human.IsCaptured = false;

                Enemy.Timer.Start();
                Target.Timer.Start();
            }

            private void resetControls()
            {
                ProgressBar.Progress = 0;

                StartButton.Enabled = false;

                PlayArea.RemoveAllViews();
                Human.SetViewToNull();
                Target.SetViewToNull();
                Enemy.RemoveAllEnemies();

                Human = new Human(_random, PlayArea, Enemy, Target, this);

                RunOnUiThread(() => PlayArea.AddView(Human.View));


                var targetImageView = ViewHelper.CreateImageView(Resource.Drawable.target3,
                    ViewHelper.ConvertDpToPixels(100), ViewHelper.ConvertDpToPixels(200), 2);

                RunOnUiThread(() => PlayArea.AddView(targetImageView));

            }

        #endregion Private methods

    }

}

