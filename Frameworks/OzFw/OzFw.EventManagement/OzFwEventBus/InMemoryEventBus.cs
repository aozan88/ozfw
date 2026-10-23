using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace OzFwEventBus
{
    /// <summary>
    /// Süreç içi event bus: broker gerektirmez (yerel geliştirme, testler, tek süreçli çalıştırma).
    /// Abonelik ve handler çözümleme RabbitMqEventBus ile aynıdır: her olay için yeni bir DI scope'unda
    /// THandler çözülür. Handler hataları yayınlayana yansıtılmaz, loglanır (broker davranışına yakın).
    /// </summary>
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, byte>> _handlers = new();

        public InMemoryEventBus(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public virtual async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var handlerTypes) || handlerTypes.IsEmpty)
                return;

            foreach (var handlerType in handlerTypes.Keys)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var handler = (IEventHandler<TEvent>)scope.ServiceProvider.GetRequiredService(handlerType);
                    await handler.HandleAsync(@event);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InMemoryEventBus] {handlerType.Name} '{typeof(TEvent).Name}' olayını işlerken hata: {ex.Message}");
                }
            }
        }

        public virtual Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            _handlers.GetOrAdd(typeof(TEvent), _ => new()).TryAdd(typeof(THandler), 0);
            return Task.CompletedTask;
        }

        public virtual Task UnsubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlerTypes))
                handlerTypes.TryRemove(typeof(THandler), out _);
            return Task.CompletedTask;
        }
    }

    /// <summary>Olay altyapısı kapalıyken (EventBus:Provider = none) kullanılan, hiçbir şey yapmayan bus.</summary>
    public class NullEventBus : IEventBus
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent => Task.CompletedTask;

        public Task SubscribeAsync<TEvent, THandler>() where TEvent : IEvent where THandler : IEventHandler<TEvent> => Task.CompletedTask;

        public Task UnsubscribeAsync<TEvent, THandler>() where TEvent : IEvent where THandler : IEventHandler<TEvent> => Task.CompletedTask;
    }
}
