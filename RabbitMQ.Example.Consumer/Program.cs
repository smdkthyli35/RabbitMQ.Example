using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ.Example.Consumer
{
    class Program
    {
        private static string queuename = "queue_name";

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "127.0.0.1",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                queuename = args.Length > 0
                    ? args[0]
                    : "queue_name";
                EventingBasicConsumer consumer = new(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Received: {message}");
                };

                channel.BasicConsume(queuename, true, consumer);
                Console.WriteLine($"{queuename} Listening.. \n");
                Console.ReadLine();
            }
        }
    }
}
