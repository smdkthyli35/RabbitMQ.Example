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
        private const string FANOUT_ROUTING_KEY = "fanout_routing_key";
        private const string EXCHANGE_FANOUT_NAME = "fanout_exchange_name";

        public HomeController()
        {

        }

        [HttpGet]
        public IActionResult Get()
        {
            //DirectExchange();
            HeaderExchange();
            connection.Close();
            return Ok();
        }

        [HttpGet]
        [Route("getfanout")]
        public IActionResult GetFanout(string queue_name)
        {
            FanoutExchange(queue_name);
            connection.Close();
            return Ok();
        }

        private void HeaderExchange()
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Users
            {
                Id = 1,
                Name = "Samed",
                Password = "1234"
            }));

            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Headers);
            channel.QueueDeclare(queue: QUEUE_NAME,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: true,
                                 arguments: null);
            channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, string.Empty, new Dictionary<string, object>
            {
                {"x-match","all"},
                {"op","convert"},
                {"format","png"}
            });

            var props = channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>()
            {
                {"op","convert"},
                {"format","png"}
            };

            channel.BasicPublish(EXCHANGE_NAME, string.Empty, props, data);
        }

        private void FanoutExchange(string queue_name)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Users
            {
                Id = 1,
                Name = "Admin",
                Password = "1234"
            }));

            channel.ExchangeDeclare(EXCHANGE_FANOUT_NAME, ExchangeType.Fanout);
            channel.QueueDeclare(queue: queue_name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue_name, EXCHANGE_FANOUT_NAME, FANOUT_ROUTING_KEY);
            channel.BasicPublish(EXCHANGE_FANOUT_NAME, FANOUT_ROUTING_KEY, null, data);
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
