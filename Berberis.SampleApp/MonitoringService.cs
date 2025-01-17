﻿using Berberis.Messaging;

namespace Berberis.SampleApp;

public sealed class MonitoringService : BackgroundService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly ICrossBar _xBar;

    public MonitoringService(ILogger<MonitoringService> logger, ICrossBar xBar)
    {
        _logger = logger;
        _xBar = xBar;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        //_xBar.TracingEnabled = true;

        ISubscription tracingSub = null;

        try
        {
            tracingSub = _xBar.Subscribe<MessageTrace>("$message.traces",
                msg =>
                {
                    _logger.LogInformation(msg.Body.ToString());

                    return ValueTask.CompletedTask;
                }, stoppingToken);
        }
        catch { }

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var channel in _xBar.GetChannels())
            {
                var chatStats = channel.Statistics.GetStats();

                _logger.LogInformation("Channel:{channel}, Type:{type}, LastBy: {lastBy}, LastAt: {lastAt}, PubIn: {pubIn:N2}, PubLt: {pubLt:N2}", channel.Name, channel.BodyType.Name, channel.LastPublishedBy, channel.LastPublishedAt.ToUniversalTime(), chatStats.PublishRateInterval, chatStats.PublishRateLongTerm);
                foreach (var subscription in _xBar.GetChannelSubscriptions(channel.Name))
                {
                    var stats = subscription.Statistics.GetStats();

                    var intervalStats = $"Int: {stats.IntervalMs:N0} ms; Enq: {stats.EnqueueRateInterval:N1} msg/s; Deq: {stats.DequeueRateInterval:N1} msg/s; Pcs: {stats.ProcessRateInterval:N1} msg/s; EnqT: {stats.TotalEnqueuedMessages:N0}; DeqT: {stats.TotalDequeuedMessages:N0}; PcsT: {stats.TotalProcessedMessages:N0}; AvgLat: {stats.AvgLatencyTimeMsInterval:N4} ms; Avg Svc: {stats.AvgServiceTimeMsInterval:N4} ms";
                    var longTermStats = $"Conf: {stats.ConflationRatioLongTerm:N4}; QLen: {stats.QueueLength:N0}; Lat/Rsp: {stats.LatencyToResponseTimeRatioLongTerm:N2}; Deq: {stats.DequeueRateLongTerm:N1} msg/s; Pcs: {stats.ProcessRateLongTerm:N2} msg/s; EAAM: {stats.EstimatedAvgActiveMessages:N4}; AvgLat: {stats.AvgLatencyTimeMsLongTerm:N4} ms; AvgSvc: {stats.AvgServiceTimeMsLongTerm:N4} ms";

                    _logger.LogInformation("--- Subscription: [{subName}], Interval Stats: {stats}", subscription.Name, intervalStats);
                    _logger.LogInformation("--- Subscription: [{subName}], Long-term Stats: {stats}", subscription.Name, longTermStats);

                    //var ms = new MemoryStream();
                    //var writer = new Utf8JsonWriter(ms);
                    //_xBar.MetricsToJson(writer);
                    //writer.Dispose();

                    //var str = Encoding.UTF8.GetString(ms.ToArray());

                    //_logger.LogInformation(str);
                }
            }

            await Task.Delay(1000, stoppingToken);
        }

        await tracingSub?.MessageLoop;
    }
}
