using Common;
using System.Diagnostics;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Grains;

// ImplicitStreamSubscription attribute here is to subscribe implicitely to all stream within
// a given namespace: whenever some data is pushed to the streams of namespace Constants.StreamNamespace,
// a grain of type ConsumerGrain with the same guid of the stream will receive the message.
// Even if no activations of the grain currently exist, the runtime will automatically
// create a new one and send the message to it.
[ImplicitStreamSubscription(Constants.StreamNamespace)]
public class ConsumerGrain : IGrainBase, IConsumerGrain, IStreamSubscriptionObserver, IAsyncObserver<int>
{
    private static readonly ActivitySource source = new ActivitySource("Consumer");
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<IConsumerGrain> _logger;

    public IGrainContext GrainContext { get; }

    public ConsumerGrain(IGrainContext grainContext, IGrainFactory grainFactory, ILogger<IConsumerGrain> logger)
    {
        GrainContext = grainContext;
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public Task OnCompletedAsync()
    {
        _logger.LogInformation("OnCompletedAsync");
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        _logger.LogInformation("OnErrorAsync: {Exception}", ex);
        return Task.CompletedTask;
    }

    public async Task OnNextAsync(int item, StreamSequenceToken? token = null)
    {
        using var activity = source.StartActivity("consumer_onnextasync");
        _logger.LogInformation("OnNextAsync: item: {Item}, token = {Token}", item, token);
        await _grainFactory.GetGrain<IProcessingGrain>(GrainContext.GrainId.GetGuidKey(out var _)).ProcessAsync(item);
    }

    // Called when a subscription is added
    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
    {
        // Plug our LoggerObserver to the stream
        var handle = handleFactory.Create<int>();
        await handle.ResumeAsync(this);
    }


    public Task OnActivateAsync(CancellationToken token)
    {
        _logger.LogInformation("OnActivateAsync");
        return Task.CompletedTask;
    }
}

public class ProcessingGrain : IGrainBase, IProcessingGrain
{
    private readonly ILogger<IProcessingGrain> _logger;

    public ProcessingGrain(IGrainContext grainContext, ILogger<IProcessingGrain> logger)
    {
        GrainContext = grainContext;
        _logger = logger;
    }

    public IGrainContext GrainContext { get; }

    public Task ProcessAsync(int item)
    {
        _logger.LogInformation("Processing item: {Item}", item);
        return Task.CompletedTask;
    }
}
