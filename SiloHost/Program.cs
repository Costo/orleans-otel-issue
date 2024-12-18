using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317", EnvironmentVariableTarget.Process);

try
{
    AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

    var host = new HostBuilder()
        .UseOrleans(ConfigureSilo)
        .ConfigureLogging(logging => logging.AddConsole())
        .ConfigureServices(services =>
        {
            services.AddOpenTelemetry()
                .WithTracing(b =>
                {
                    b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("silo"));
                    b.AddSource("Azure.*");
                    b.AddSource("Microsoft.Orleans.Runtime");
                    b.AddSource("Microsoft.Orleans.Application");
                    b.AddSource("Consumer");
                    b.AddOtlpExporter();
                });
        })
        .Build();

    await host.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static void ConfigureSilo(HostBuilderContext context, ISiloBuilder siloBuilder)
{
    var secrets = Secrets.LoadFromFile()!;
    siloBuilder
        .AddActivityPropagation()
        .UseLocalhostClustering(serviceId: Constants.ServiceId, clusterId: Constants.ClusterId)
        .AddAzureTableGrainStorage(
            "PubSubStore",
            options => options.ConfigureTableServiceClient(secrets.DataConnectionString))
        .AddEventHubStreams(Constants.StreamProvider, (ISiloEventHubStreamConfigurator configurator) =>
        {
            configurator.ConfigureEventHub(builder => builder.Configure(options =>
            {
                options.ConfigureEventHubConnection(
                    secrets.EventHubConnectionString,
                    Constants.EHPath,
                    Constants.EHConsumerGroup);
            }));
            configurator.UseAzureTableCheckpointer(
                builder => builder.Configure(options =>
            {
                options.ConfigureTableServiceClient(secrets.DataConnectionString);
                options.PersistInterval = TimeSpan.FromSeconds(10);
            }));
        });
}
