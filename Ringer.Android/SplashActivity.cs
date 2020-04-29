using Android.App;
using Android.Content;
using Android.OS;

namespace Ringer.Droid
{
    [Activity(Label = "링거",
        Icon = "@drawable/icon",
        Theme = "@style/RingerTheme.Splash",
        MainLauncher = false
    )]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }
    }
}
