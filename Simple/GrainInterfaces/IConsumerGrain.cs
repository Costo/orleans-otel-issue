namespace GrainInterfaces;

public interface IConsumerGrain : IGrainWithGuidKey
{
}

public interface IProcessingGrain : IGrainWithGuidKey
{
    Task ProcessAsync(int item);
}
