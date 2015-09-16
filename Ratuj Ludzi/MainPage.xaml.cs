using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Ratuj_Ludzi.Common;


namespace Ratuj_Ludzi
{
   
    public sealed partial class MainPage : Page
    {
        #region Private fields

        private Random _random = new Random();
        private DispatcherTimer _enemyTimer = new DispatcherTimer();
        private DispatcherTimer _targetTimer = new DispatcherTimer();
        private bool _humanCaptured = false;

        private NavigationHelper _navigationHelper;
        private ObservableDictionary _defaultViewModel = new ObservableDictionary();

        #endregion Private fields


        public MainPage()
        {
            this.InitializeComponent();

            _enemyTimer.Tick += enemyTimer_Tick;
            _enemyTimer.Interval = TimeSpan.FromSeconds(2);

            _targetTimer.Tick += targetTimer_Tick;
            _targetTimer.Interval = TimeSpan.FromSeconds(.1);
            this._navigationHelper = new NavigationHelper(this);
            this._navigationHelper.LoadState += navigationHelper_LoadState;
            this._navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private void targetTimer_Tick(object sender, object e)
        {
            progressBar.Value += 1;
            if (progressBar.Value >= progressBar.Maximum)
                EndTheGame();
        }

        private void EndTheGame()
        {
            if (!playArea.Children.Contains(gameOverText))
            {
                _enemyTimer.Stop();
                _targetTimer.Stop();
                _humanCaptured = false;
                startButton.Visibility = Visibility.Visible;
                playArea.Children.Add(gameOverText);
            }
        }

        private void enemyTimer_Tick(object sender, object e)
        {
            AddEnemy();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            human.IsHitTestVisible = true;
            _humanCaptured = false;
            progressBar.Value = 0;
            startButton.Visibility = Visibility.Collapsed;
            playArea.Children.Clear();
            playArea.Children.Add(target);
            playArea.Children.Add(human);
            _enemyTimer.Start();
            _targetTimer.Start();
        }

        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl();
            enemy.Template = Resources["EnemyTemplate"] as ControlTemplate;
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, _random.Next((int)playArea.ActualHeight - 100), 
                _random.Next((int)playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);

            enemy.PointerEntered += enemy_PointerEntered;
        }

        void enemy_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_humanCaptured)
                EndTheGame();
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(_random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            storyboard.Children.Add(animation);
            storyboard.Begin();
            
        }

        private void human_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_enemyTimer.IsEnabled)
            {
                _humanCaptured = true;
                human.IsHitTestVisible = false;
            }
        }

        private void target_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_targetTimer.IsEnabled && _humanCaptured)
            {
                progressBar.Value = 0;
                Canvas.SetLeft(target, _random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(target, _random.Next(100, (int)playArea.ActualHeight - 100));
                Canvas.SetLeft(human, _random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(target, _random.Next(100, (int)playArea.ActualHeight - 100));
                _humanCaptured = false;
                human.IsHitTestVisible = true;
                
            }
        }

        private void playArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_humanCaptured)
            {
                Point pointerPosition = e.GetCurrentPoint(null).Position;
                Point relativePosition = grid.TransformToVisual(playArea).TransformPoint(pointerPosition);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth *3)
                    || (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * 3))
                {
                    _humanCaptured = false;
                    human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 2);
                }
            }
        }

        private void playArea_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_humanCaptured)
                EndTheGame();
        }

        #region NavigationHelper registration

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this._defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this._navigationHelper; }
        }

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
