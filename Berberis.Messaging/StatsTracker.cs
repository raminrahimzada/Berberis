﻿using System.Diagnostics;

namespace Berberis.Messaging;

public sealed class StatsTracker
{
    internal static long GetTicks() => Stopwatch.GetTimestamp();
    internal static float TicksToTimeMs(long ticks) => ticks * MsRatio;
    internal static float MsRatio => 1000f / Stopwatch.Frequency;

    private long _totalMessagesEnqueued;
    private long _lastMessagesEnqueued;

    private long _totalMessagesDequeued;
    private long _lastMessagesDequeued;

    private long _totalMessagesProcessed;
    private long _lastMessagesProcessed;

    private long _totalLatencyTicks;
    private long _lastLatencyTicks;

    private long _totalServiceTicks;
    private long _lastServiceTicks;

    private long _totalInterDequeueTime;
    private long _lastInterDequeueTime;

    private long _totalInterProcessTime;
    private long _lastInterProcessTime;

    private long _lastTicks;
    private readonly object _syncObj = new();

    internal void IncNumOfEnqueuedMessages() => Interlocked.Increment(ref _totalMessagesEnqueued);

    internal void IncNumOfDequeuedMessages() => Interlocked.Increment(ref _totalMessagesDequeued);

    internal void IncNumOfProcessedMessages() => Interlocked.Increment(ref _totalMessagesProcessed);

    internal long RecordLatencyAndInterDequeueTime(long startTicks)
    {
        var nowTicks = GetTicks();
        var latency = nowTicks - startTicks;
        Interlocked.Add(ref _totalLatencyTicks, latency);

        if (_lastInterDequeueTime > 0)
        {
            Interlocked.Add(ref _totalInterDequeueTime, (nowTicks - _lastInterDequeueTime));
        }

        _lastInterDequeueTime = nowTicks;

        return latency;
    }

    internal long RecordServiceAndInterProcessTime(long startTicks)
    {
        var nowTicks = GetTicks();
        var svcTime = nowTicks - startTicks;
        Interlocked.Add(ref _totalServiceTicks, svcTime);

        if (_lastInterProcessTime > 0)
        {
            Interlocked.Add(ref _totalInterProcessTime, (nowTicks - _lastInterProcessTime));
        }

        _lastInterProcessTime = nowTicks;

        return svcTime;
    }

    public Stats GetStats(bool reset = true)
    {
        var ticks = GetTicks();

        var totalMessagesEnqueued = Interlocked.Read(ref _totalMessagesEnqueued);
        var totalMessagesDequeued = Interlocked.Read(ref _totalMessagesDequeued);
        var totalMessagesProcessed = Interlocked.Read(ref _totalMessagesProcessed);

        var totalLatencyTicks = Interlocked.Read(ref _totalLatencyTicks);
        var totalServiceTicks = Interlocked.Read(ref _totalServiceTicks);

        long intervalMessagesEnqueued;
        long intervalMessagesDequeued;
        long intervalMessagesProcessed;

        long intervalLatencyTicks;
        long intervalSvcTicks;

        float timePassed;

        lock (_syncObj)
        {
            intervalMessagesEnqueued = totalMessagesEnqueued - _lastMessagesEnqueued;
            intervalMessagesDequeued = totalMessagesDequeued - _lastMessagesDequeued;
            intervalMessagesProcessed = totalMessagesProcessed - _lastMessagesProcessed;

            intervalLatencyTicks = totalLatencyTicks - _lastLatencyTicks;
            intervalSvcTicks = totalServiceTicks - _lastServiceTicks;

            timePassed = (float) (ticks - _lastTicks) / Stopwatch.Frequency;

            if (reset)
            {
                _lastMessagesEnqueued = totalMessagesEnqueued;
                _lastMessagesDequeued = totalMessagesDequeued;
                _lastMessagesProcessed = totalMessagesProcessed;

                _lastLatencyTicks = totalLatencyTicks;
                _lastServiceTicks = totalServiceTicks;

                _lastTicks = ticks;
            }
        }

        var intervalLatencyTimeMs = intervalLatencyTicks * MsRatio;
        var intervalSvcTimeMs = intervalSvcTicks * MsRatio;

        var avgLatencyTimeMs = intervalMessagesDequeued == 0 ? 0 : intervalLatencyTimeMs / intervalMessagesDequeued;
        var avgServiceTimeMs = intervalMessagesProcessed == 0 ? 0 : intervalSvcTimeMs / intervalMessagesProcessed;

        var totalLatencyTimeMs = totalLatencyTicks * MsRatio;
        var totalInterDequeueTimeMs = Interlocked.Read(ref _totalInterDequeueTime) * MsRatio;
        var totalServiceTimeMs = totalServiceTicks * MsRatio;
        var totalInterProcessTimeMs = Interlocked.Read(ref _totalInterProcessTime) * MsRatio;

        return new Stats(timePassed * 1000,
            intervalMessagesEnqueued / timePassed,
            intervalMessagesDequeued / timePassed,
            intervalMessagesProcessed / timePassed,
            totalMessagesEnqueued,
            totalMessagesDequeued,
            totalInterDequeueTimeMs,
            totalMessagesProcessed,
            totalInterProcessTimeMs,
            totalLatencyTimeMs,
            avgLatencyTimeMs,
            totalServiceTimeMs,
            avgServiceTimeMs);
    }
}
