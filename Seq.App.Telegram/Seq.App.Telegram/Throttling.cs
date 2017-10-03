using System;
using System.Collections.Concurrent;

namespace Seq.App.Telegram
{
    public class Throttling<T>
    {
        readonly ConcurrentDictionary<T, DateTime> _lastSeen = new ConcurrentDictionary<T, DateTime>();


        public bool TryBegin(T key, TimeSpan period)
        {
            if (period <= TimeSpan.Zero)
                return true;
            var res = false;
            _lastSeen.AddOrUpdate(
                key: key,
                addValueFactory: type =>
                {
                    res = true;
                    return DateTime.Now;
                },
                updateValueFactory: (type, lastTime) =>
                {
                    var now = DateTime.Now;
                    if (now - lastTime > period)
                    {
                        res = true;
                        return now;
                    }
                    return lastTime;
                });
            return res;
        }
    }
}
