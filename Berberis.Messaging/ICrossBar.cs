﻿namespace Berberis.Messaging;

public interface ICrossBar
{
    IReadOnlyList<CrossBar.ChannelInfo> GetChannels();

    IReadOnlyCollection<CrossBar.SubscriptionInfo> GetChannelSubscriptions(string channelName);

    ValueTask Publish<TBody>(string channelName, TBody body);
    ValueTask Publish<TBody>(string channelName, TBody body, string from);
    ValueTask Publish<TBody>(string channelName, TBody body, string key, bool store);
    ValueTask Publish<TBody>(string channelName, TBody body, long correlationId);
    ValueTask Publish<TBody>(string channelName, TBody body, long correlationId, string key, bool store, string from);

    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler, CancellationToken token = default);
    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler, string subscriptionName, CancellationToken token = default);
    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler, bool fetchState, CancellationToken token = default);
    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler, string subscriptionName, bool fetchState, CancellationToken token = default);
    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler, bool fetchState, TimeSpan conflationInterval, CancellationToken token = default);
    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler, string subscriptionName, bool fetchState, TimeSpan conflationInterval, CancellationToken token = default);

    ISubscription Subscribe<TBody>(string channelName, Func<Message<TBody>, ValueTask> handler,
       string subscriptionName,
       bool fetchState,
       SlowConsumerStrategy slowConsumerStrategy,
       int? bufferCapacity,
       TimeSpan conflationInterval,
       CancellationToken token = default);

    bool TryDeleteMessage<TBody>(string channelName, string key, out Message<TBody> message);
    bool ResetStore<TBody>(string channelName);
    bool TryDeleteChannel(string channelName);

    long GetNextCorrelationId();

    bool MessageTracingEnabled { get; set; }

    bool PublishLoggingEnabled { get; set; }
}