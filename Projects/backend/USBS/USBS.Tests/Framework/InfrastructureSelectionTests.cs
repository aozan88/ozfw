using Microsoft.Extensions.Configuration;
using OzFwCaching;
using OzFwEventBus;
using USBS.Components.Caching;
using USBS.Components.EventManagement;
using USBS.ServiceDefaults;

namespace USBS.Tests.Framework;

/// <summary>Altyapının yapılandırmadan seçilmesi (EventBus:Provider, Cache:Provider) ve süreç içi uygulamalar.</summary>
public class InfrastructureSelectionTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value)).Build();

    private static ServiceProvider BuildEventBus(IConfiguration config)
    {
        var services = new ServiceCollection();
        services.AddOzFwEventBus(config);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(null, typeof(InMemoryEventBus))]
    [InlineData("inmemory", typeof(InMemoryEventBus))]
    [InlineData("RabbitMQ", typeof(RabbitMqEventBusProvider))]
    [InlineData("kafka", typeof(KafkaEventBusProvider))]
    [InlineData("none", typeof(NullEventBus))]
    public void EventBus_providerdan_secilir(string? provider, Type expected)
    {
        var config = provider == null ? Config() : Config((EventManagementExtensions.ProviderKey, provider));
        using var sp = BuildEventBus(config);
        Assert.IsType(expected, sp.GetRequiredService<IEventBus>());
    }

    [Fact]
    public void Bilinmeyen_provider_anlasilir_hata_verir()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => BuildEventBus(Config((EventManagementExtensions.ProviderKey, "sqs"))));
        Assert.Contains("sqs", ex.Message);
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddOzFwCache(Config((CachingServiceExtensions.ProviderKey, "memcached"))));
    }

    public sealed class TestEvent : Event
    {
        public string Text { get; init; } = "";
    }

    public sealed class Recorder
    {
        public List<string> Received { get; } = new();
    }

    public sealed class RecordingHandler : IEventHandler<TestEvent>
    {
        private readonly Recorder _recorder;
        public RecordingHandler(Recorder recorder) => _recorder = recorder;
        public Task HandleAsync(TestEvent @event)
        {
            _recorder.Received.Add(@event.Text);
            return Task.CompletedTask;
        }
    }

    public sealed class FailingHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent @event) => throw new InvalidOperationException("handler hatası");
    }

    [Fact]
    public async Task InMemory_bus_abonelere_scope_icinde_iletir()
    {
        var services = new ServiceCollection();
        services.AddOzFwEventBus(Config());
        services.AddSingleton<Recorder>();
        services.AddScoped<RecordingHandler>();
        services.AddScoped<FailingHandler>();
        using var sp = services.BuildServiceProvider();

        var bus = sp.GetRequiredService<IEventBus>();
        await bus.PublishAsync(new TestEvent { Text = "abone yok" });

        await bus.SubscribeAsync<TestEvent, RecordingHandler>();
        await bus.SubscribeAsync<TestEvent, FailingHandler>();
        await bus.SubscribeAsync<TestEvent, RecordingHandler>(); // tekrar abonelik çift teslim etmez
        await bus.PublishAsync(new TestEvent { Text = "bir" }); // FailingHandler hatası yayınlayana yansımaz

        await bus.UnsubscribeAsync<TestEvent, RecordingHandler>();
        await bus.PublishAsync(new TestEvent { Text = "iki" });

        Assert.Equal(new[] { "bir" }, sp.GetRequiredService<Recorder>().Received);
    }

    [Fact]
    public void Cache_providerdan_secilir()
    {
        using var memory = new ServiceCollection().AddOzFwCache(Config()).BuildServiceProvider();
        Assert.IsType<MemoryKonCache>(memory.GetRequiredService<IKonCache>());

        using var redis = new ServiceCollection().AddOzFwCache(Config((CachingServiceExtensions.ProviderKey, "redis"))).BuildServiceProvider();
        Assert.IsType<RedisCacheProvider>(redis.GetRequiredService<IKonCache>());
    }

    [Fact]
    public async Task Memory_cache_get_set_desen_ve_temizleme()
    {
        using var cache = new MemoryKonCache();

        Assert.Equal(42, await cache.GetAsync("sayi", () => Task.FromResult(42)));
        Assert.Equal(42, await cache.GetAsync("sayi", () => Task.FromResult(0))); // önbellekten
        await cache.SetAsync("kullanici:1", "a");
        await cache.SetAsync("kullanici:2", "b");
        await cache.SetAsync("urun:1", "c");

        await cache.RemoveByPatternAsync("kullanici:*");
        Assert.False(await cache.ExistsAsync("kullanici:1"));
        Assert.True(await cache.ExistsAsync("urun:1"));

        await cache.ClearAllAsync();
        Assert.False(await cache.ExistsAsync("urun:1"));
        Assert.Null(await cache.GetAsync<string>("urun:1"));

        await cache.SetAsync("kisa", "x", TimeSpan.FromMilliseconds(30));
        await Task.Delay(100);
        Assert.False(await cache.ExistsAsync("kisa"));
    }
}
