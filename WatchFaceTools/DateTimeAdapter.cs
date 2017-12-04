using System;

namespace WatchFaceTools
{
    public class DateTimeAdapter : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public long UnixNow => (Int32)(UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        public DateTime Now => DateTime.Now;
    }

    /// <summary>
    /// Calling a static class like DateTime.Now will get ugly in tests, where we would have to change the system date.
    /// So we are going to use the Adapter pattern, wrap the DateTime class in an injectable interface.
    /// </summary>
    public interface IDateTime
    {
        DateTime UtcNow { get; }
        long UnixNow { get; }
        DateTime Now { get; }
    }
}