﻿using System.Diagnostics;

namespace Berberis.Messaging;

public sealed class ChannelStatsTracker
{
    internal static long GetTicks() => Stopwatch.GetTimestamp();
    internal static float MsRatio => (float) 1000 / Stopwatch.Frequency;

    private long _totalMessages;
    private long _lastMessages;

    private long _lastTicks;
    private object _syncObj = new();

    internal void IncNumOfMessages() => Interlocked.Increment(ref _totalMessages);

    public ChannelStats GetStats(bool reset = true)
    {
        var ticks = GetTicks();

        var totalMesssagesInc = Interlocked.Read(ref _totalMessages);

        long intervalMessagesInc;

        float timePassed;

        lock (_syncObj)
        {
            intervalMessagesInc = totalMesssagesInc - _lastMessages;

            timePassed = (ticks - _lastTicks) / Stopwatch.Frequency;

            if (reset)
            {
                _lastMessages = totalMesssagesInc;
                _lastTicks = ticks;
            }
        }

        return new ChannelStats(timePassed * 1000,
            intervalMessagesInc / timePassed,
            totalMesssagesInc);
    }
}
