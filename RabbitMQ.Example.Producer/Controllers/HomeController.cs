using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Example.Producer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Example.Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private IConnection connection;
        private string Url = $"amqp://guest:guest@localhost:5672";
        private IModel channel => CreateChannel();
        private const string EXCHANGE_NAME = "exchange_name";
        private const string QUEUE_NAME = "queue_name";

        public HomeController()
        {

        }

        [HttpGet]
        public IActionResult Get()
        {
            DirectExchange();
            connection.Close();
            return Ok();
        }

        private void DirectExchange()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Users
            {
                Id = 1,
                Name = "Admin",
                Password = "1234"
            }));

            channel.ExchangeDeclare(exchange: EXCHANGE_NAME,
                                    type: ExchangeType.Direct);

            channel.QueueDeclare(queue: QUEUE_NAME,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, QUEUE_NAME);
            channel.BasicPublish(EXCHANGE_NAME, QUEUE_NAME, null, data);
        }

        private IModel CreateChannel()
        {
            if (connection is null)
            {
                connection = GetConnection();
                return connection.CreateModel();
            }
            else
            {
                return connection.CreateModel();
            }
        }

        private IConnection GetConnection()
        {
            ConnectionFactory factory = new()
            {
                Uri = new Uri(Url, UriKind.RelativeOrAbsolute)
            };

            return factory.CreateConnection();
        }
    }
}
