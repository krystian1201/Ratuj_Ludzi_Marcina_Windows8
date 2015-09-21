using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SaveHumans_Android
{
    public class ImageViewHelper
    {
        public ImageView AddImageViewToPlayArea(Context context, int drawableId, int marginLeft, int marginTop,
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

            RunOnUiThread(() => PlayArea.AddView(imageView));

            return imageView;
        }
    }
}