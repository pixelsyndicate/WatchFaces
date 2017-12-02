using System;
using Android.Content;

namespace WatchFaceTools
{
    /// <summary>
    /// Time zone broadcast receiver. OnReceive is called when the time zone changes:
    /// </summary>
    public class TimeZoneReceiver : BroadcastReceiver
    {
        public Action<Intent> Receive { get; set; }

        public override void OnReceive(Context context, Intent intent)
        {
            Receive?.Invoke(intent);
        }
    }
}