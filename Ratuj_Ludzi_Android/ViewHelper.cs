
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace SaveHumans_Android
{
    public static class ViewHelper
    {

        #region Private fields

            private static Activity _activity;
            private static Context _context;

        #endregion Private fields

        #region Public properties

            public static Activity Activity
            {
                set { _activity = value; }
            }

            public static Context Context
            {
                set { _context = value; }
            }

        #endregion Public properties

        #region Public methods

            public static ImageView CreateImageView(int drawableId, int marginLeft, int marginTop,
                    int? id = null, int controlHeight = ViewGroup.LayoutParams.WrapContent)
            {
                ImageView imageView = new ImageView(_context);
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

                return imageView;
            }

            public static TextView CreateGameOverTextView()
            {
                TextView gameOverTextView = new TextView(_context);

                gameOverTextView.Text = "Game over";
                gameOverTextView.TextSize = 48;
                gameOverTextView.SetTextColor(Color.White);


                RelativeLayout.LayoutParams layoutParams =
                    new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);

                layoutParams.SetMargins(ConvertDpToPixels(200),
                    ConvertDpToPixels(150), 0, 0);

                gameOverTextView.LayoutParameters = layoutParams;
                gameOverTextView.BringToFront();

                return gameOverTextView;

            }

            //TODO: find a better class for this method?
            public static int ConvertDpToPixels(int pixels)
            {
                float scale = _activity.ApplicationContext.Resources.DisplayMetrics.Density;

                return (int)(pixels * scale + 0.5f);
            }

        #endregion Public methods
    }
}