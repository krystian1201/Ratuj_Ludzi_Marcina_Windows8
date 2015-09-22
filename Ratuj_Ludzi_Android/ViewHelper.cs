
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace SaveHumans_Android
{
    public static class ViewHelper
    {

        private static Activity _activity;

        public static Activity Activity
        {
            set { _activity = value; }
        }

        public static ImageView CreateImageView(Context context, int drawableId, int marginLeft, int marginTop,
                int? id = null, int controlHeight = ViewGroup.LayoutParams.WrapContent)
        {
            ImageView imageView = new ImageView(context);
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

        //TODO: put activity into some constructor or setter?
        //TODO: find a better class for this method?
        public static int ConvertDpToPixels(int pixels)
        {
            float scale = _activity.ApplicationContext.Resources.DisplayMetrics.Density;

            return (int)(pixels * scale + 0.5f);
        }
    }
}