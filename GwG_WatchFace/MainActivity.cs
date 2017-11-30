
using Android;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.Wearable.Views;
using Android.Widget;

namespace WatchFace
{
    [Activity(Label = "WatchFace", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
       // int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var main = Resource.Layout.Main;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var v = FindViewById<WatchViewStub>(Resource.Id.watch_view_stub);
            v.LayoutInflated += delegate
            {

                // Get our button from the layout resource,
                // and attach an event to it
                Button button = FindViewById<Button>(Resource.Id.myButton);
                button.Text = "Install Completed. Click to accept.";
                button.Click += delegate
                {
                    var notification = new NotificationCompat.Builder(this)
                        .SetContentTitle("User Acceptance")
                        .SetContentText("Thank you. Enjoy the Watch Face!")
                        .SetSmallIcon(Android.Resource.Drawable.StatNotifyVoicemail)
                        .SetGroup("group_key_demo").Build();

                    var manager = NotificationManagerCompat.From(this);
                    manager.Notify(1, notification);
                    button.Text = "Swipe to dismiss";
                };
            };
        }
    }
}


