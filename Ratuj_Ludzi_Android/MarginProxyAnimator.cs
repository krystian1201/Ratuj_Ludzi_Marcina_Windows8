
using Android.Views;
using Java.Interop;
using Java.Lang;

namespace SaveHumans_Android
{
    public class MarginProxyAnimator : Object
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