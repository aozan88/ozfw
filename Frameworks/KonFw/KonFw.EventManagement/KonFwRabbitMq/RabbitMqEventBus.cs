using KonFwEventBus;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace KonFwRabbitMq
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly RabbitMqConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqEventBus(RabbitMqConfiguration config, IServiceProvider serviceProvider)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public virtual async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (!_config.IsEnabled) return;

            var factory = CreateConnectionFactory();
           var connection = await factory.CreateConnectionAsync();  
           var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: _config.ExchangeName,
                type: _config.ExchangeType,
                durable: _config.Durable,
                autoDelete: _config.AutoDelete);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

            var properties = new BasicProperties
            {
                Persistent = _config.PersistentDelivery
            };

            await channel.BasicPublishAsync(
                exchange: _config.ExchangeName,
                routingKey: typeof(TEvent).Name,
                mandatory: false, 
                basicProperties: properties,
                body: body);

            Console.WriteLine($"[RabbitMQ Publish] Event published: {typeof(TEvent).Name}");
            await Task.CompletedTask;
        }

        public virtual async Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            if (!_config.IsEnabled) return;

            var factory = CreateConnectionFactory();
            var  connection = await factory.CreateConnectionAsync();
            var  channel = await connection.CreateChannelAsync();

           await channel.ExchangeDeclareAsync(
                exchange: _config.ExchangeName,
                type: _config.ExchangeType,
                durable: _config.Durable,
                autoDelete: _config.AutoDelete);

            var queueName = $"{_config.QueueName}_{typeof(TEvent).Name}";
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: _config.Durable,
                exclusive: false,
                autoDelete: _config.AutoDelete);

          await  channel.QueueBindAsync(
                queue: queueName,
                exchange: _config.ExchangeName,
                routingKey: typeof(TEvent).Name);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<TEvent>(message);

                if (@event != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                    await handler.HandleAsync(@event);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer).ConfigureAwait(false);
            Console.WriteLine($"[RabbitMQ Subscribe] Subscribed to event: {typeof(TEvent).Name}");
            await Task.CompletedTask;
        }

        public virtual Task UnsubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            Console.WriteLine($"[RabbitMQ Unsubscribe] Unsubscribed from event: {typeof(TEvent).Name}");
            return Task.CompletedTask;
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = _config.HostName,
                Port = _config.Port,
                VirtualHost = _config.VirtualHost,
                UserName = _config.Username,
                Password = _config.Password
            };
        }
    }
}