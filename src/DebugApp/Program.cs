// See https://aka.ms/new-console-template for more information

using DebugApp;
using MicroHomeAssistantClient.Extensions;
using MicroHomeAssistantClient.Internal.Json;
using MicroHomeAssistantClient.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
        services
            .ConfigureNetDaemonJsonOptions(options =>
                {
                    options.SerializerOptions.AddContext<TestServiceDataSerializationContext>();
                }
                )
            .AddMicroHomeAssistantClient()
            .AddHostedService<DebugService>()
            
    )
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json");
        config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true);
        config.AddEnvironmentVariables();
    })
    .Build()
    .RunAsync()
    .ConfigureAwait(false);

    
        // try
        // {
            // var wsClient = new WebSocketClientImpl();
            //
            // var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            //
            // await wsClient.ConnectAsync(new Uri("ws://localhost:8124/api/websocket"), tokenSource.Token);
            // // await wsClient.SendAsync(Encoding.UTF8.GetBytes("Hello, World on websocket!"), WebSocketMessageType.Text, true, tokenSource.Token);
            // var mem = new Memory<byte>(new byte[1024]);
            // var result = await wsClient.ReceiveAsync(mem, tokenSource.Token);
            //
            // var resultMsg = Encoding.UTF8.GetString(mem.Span.Slice(0, result.Count));
            // Console.WriteLine($"Result from WS: {resultMsg}");
            //
            // var hassMessage = JsonSerializer.Deserialize<HassMessage>(
            //         resultMsg, HassMessageSerializationContext.Default.HassMessage);
            //
            // var hassMessageSubject = new Subject<HassMessage>();
            //
            // // result.
            // IObservable<HassMessage> hassMessageObservable = hassMessageSubject;
            // Console.WriteLine($"Result from WS: {hassMessage}");
            //
            // hassMessageObservable.Where(n => n.Type == "auth_required").Subscribe(msg =>
            // {
            //     Console.WriteLine($"Received message: {msg}");
            // });
            //
            // hassMessageSubject.OnNext(hassMessage);
            //
            // Console.ReadLine();

        // }
        // catch (System.Exception ex)
        // {
        //
        //     Console.WriteLine($"Failed to connect! : {ex.Message}");
        // }

        // Console.WriteLine("Connected!");
//     }
//
//
// }