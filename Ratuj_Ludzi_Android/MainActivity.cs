using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Ratuj_Ludzi_Android
{
    [Activity(Label = "Ratuj_Ludzi_Android", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : Activity
    {
        

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            
        }
    }
}

