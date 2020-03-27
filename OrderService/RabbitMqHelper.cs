using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService
{
    public class RabbitMqHelper
    {
        #region 单例模式
        public static ConnectionFactory Factory { get; set; } = new ConnectionFactory() { HostName = "localhost", UserName = "city", Password = "12345678" };
        private static RabbitMqHelper _singleModel;
        private static readonly object SynObject = new object();

        private RabbitMqHelper()
        {
        }

        public static RabbitMqHelper SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new RabbitMqHelper();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public void Send(object message)
        {
            using (var connection = Factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
                    Console.WriteLine("Sent：{0}", message);
                }
            }
        }


        public void Receive()
        {
            using (var connection = Factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        if (!string.IsNullOrEmpty(message))
                        {
                            dynamic obj = JsonConvert.DeserializeObject<object>(message);
                            Console.WriteLine("Receive：{0}", obj.Word);
                        }
                    };
                    channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }

    }
}
