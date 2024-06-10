using System;
using System.Text;
using System.Text.Json;
using HotelBooking.ScheduledTasks.DTOs;
using HotelBooking.ScheduledTasks.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HotelBooking.ScheduledTasks.Services
{
    public class QueueService : IQueueService
    {
        private readonly string _hostName = "localhost"; // Update with your RabbitMQ host name
        private readonly string _queueName = "hotelReservations";

        public void SendReservationMessage(ReservationMessage message)
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
            }
        }

        public void ReceiveReservationMessages(Action<ReservationMessage> processMessage)
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<ReservationMessage>(Encoding.UTF8.GetString(body));
                    processMessage(message);
                };

                channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            }
        }
    }
}