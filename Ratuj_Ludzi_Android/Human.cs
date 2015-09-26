
using System;
using Android.Views;
using Android.Widget;
using Ratuj_Ludzi_Android;

namespace SaveHumans_Android
{
    public class Human
    {

        #region Private fields

            private readonly RelativeLayout _playArea;
            private View _view;

            private Enemy _enemy;
            private Target _target;
            private MainActivity _activity;

            private bool _isCaptured;

            private readonly Random _random;

        #endregion Private fields

        #region Public properties

            public ImageView View
            {
                get
                {
                    //TODO: is 1 the proper id here?
                    return (ImageView)(_view ?? (_view = _playArea.FindViewById<ImageView>(1)));
                }
            }

            public RelativeLayout.LayoutParams HumanLayoutParams
            {
                get
                {
                    return (RelativeLayout.LayoutParams)View.LayoutParameters;
                }
            }

            public bool IsCaptured
            {
                get
                {
                    return _isCaptured;
                }
                set
                {
                    _isCaptured = value;
                }

            }

        #endregion Public properties

        #region Public methods

        //TODO: Human depends on too many classes
            public Human(Random random, RelativeLayout playArea, Enemy enemy, Target target, MainActivity activity)
            {
                _playArea = playArea;
                _enemy = enemy;
                _target = target;
                _activity = activity;
                _random = random;

                _view = ViewHelper.CreateImageView(Resource.Drawable.human2,
                            ViewHelper.ConvertDpToPixels(500), ViewHelper.ConvertDpToPixels(100), 1);

                _view.Touch += humanOnTouch;
            }

            //TODO: create an interface or abstract class? (both Human and Target have such method)
            public void SetViewToNull()
            {
                _view = null;
            }

        #endregion Public methods


        #region Private methods

            //TODO: check if sender is not HumanView or PlayArea
            //TODO: this method could be moved to MainActivity - then Human class
            //TOOO: wouldn't need MainActivity
            private void humanOnTouch(object sender, View.TouchEventArgs touchEventArgs)
            {
                switch (touchEventArgs.Event.Action)
                {
                    //TODO: is it needed on mobile version?
                    case MotionEventActions.Down:

                        if (_enemy.Timer.Enabled)
                        {
                            _isCaptured = true;
                        }

                        break;

                    case MotionEventActions.Move:

                        if (_isCaptured)
                        {
                            moveHuman(touchEventArgs);
                        }

                        break;
                }
            }

            private void moveHuman(View.TouchEventArgs touchEventArgs)
            {

                int touchX = (int)touchEventArgs.Event.RawX;
                int touchY = (int)touchEventArgs.Event.RawY;


                int deltaX = touchX - HumanLayoutParams.LeftMargin;
                int deltaY = touchY - HumanLayoutParams.TopMargin;


                //end the game when human exited the play area
                if (humanExitedPlayArea(touchX, touchY))
                {
                    _activity.EndTheGame();
                    return;
                }

                //dropping human when moving too fast
                if (humanMovesTooFast(deltaX, deltaY))
                {
                    _isCaptured = false;
                    return;
                }


                //actual movement of human
                moveHumanActually(HumanLayoutParams, deltaX, deltaY);

                //detecting collision of human and enemy
                //TODO: should it come before moving human? 
                if (humanHitAnyOfEnemies())
                {
                    _activity.EndTheGame();
                }
                else if (humanEnteredTarget())
                {
                    _activity.SetProgressBarValueToZero();

                    moveHumanToNewRandomLocation();
                    _target.moveTargetToNewRandomLocation();

                    _isCaptured = false;
                }
            }

            private bool humanExitedPlayArea(int touchX, int touchY)
            {
                return touchX > _playArea.Width || touchY > _playArea.Height;
            }

            private bool humanMovesTooFast(int deltaX, int deltaY)
            {
                return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)) > _view.Width * 2;
            }

            private void moveHumanActually(RelativeLayout.LayoutParams layoutParams, int deltaX, int deltaY)
            {
                layoutParams.LeftMargin += deltaX - _view.Width / 2;
                layoutParams.TopMargin += deltaY - _view.Height / 2;

                _view.LayoutParameters = layoutParams;
            }

            private bool humanHitAnyOfEnemies()
            {

                int humanX = HumanLayoutParams.LeftMargin;
                int humanY = HumanLayoutParams.TopMargin;

                foreach (var enemy in _enemy.Enemies)
                {
                    if (humanHitThisEnemy(enemy, humanX, humanY))
                        return true;
                }

                return false;
            }

            private bool humanHitThisEnemy(ImageView enemy, int humanX, int humanY)
            {
                RelativeLayout.LayoutParams enemyLayoutParams = (RelativeLayout.LayoutParams)enemy.LayoutParameters;

                int enemyX = enemyLayoutParams.LeftMargin;
                int enemyY = enemyLayoutParams.TopMargin;

                if (humanX > enemyX && humanX < enemyX + _enemy.EnemyWidthInPixels &&
                    humanY > enemyY && humanY < enemyY + _enemy.EnemyHeightInPixels)
                {
                    return true;
                }

                return false;
            }


            private bool humanEnteredTarget()
            {

                int humanX = HumanLayoutParams.LeftMargin;
                int humanY = HumanLayoutParams.TopMargin;


                RelativeLayout.LayoutParams targetLayoutParams = (RelativeLayout.LayoutParams)_target.View.LayoutParameters;

                int targetX = targetLayoutParams.LeftMargin;
                int targetY = targetLayoutParams.TopMargin;

                //TODO: those conversions don't look great (but they are used only in this place in code)
                int targetHeightInPixels = ViewHelper.ConvertDpToPixels(Target.HEIGHT_IN_DP);
                int targetWidthInPixels = (int)(1.03 * targetHeightInPixels);

                if (humanX > targetX && humanX < targetX + targetWidthInPixels &&
                        humanY > targetY && humanY < targetY + targetHeightInPixels)
                {

                    return true;
                }

                return false;
            }

            private void moveHumanToNewRandomLocation()
            {
                const int horizontalVerticalMin = 100;
                int horizontalMax = _playArea.Width - 100;
                int verticalMax = _playArea.Height - 100;


                HumanLayoutParams.LeftMargin = _random.Next(horizontalVerticalMin, horizontalMax);
                HumanLayoutParams.TopMargin = _random.Next(horizontalVerticalMin, verticalMax);

                
            }

        #endregion Private methods

    }
}