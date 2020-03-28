using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace OrderService
{
    public partial class OrderService : ServiceBase
    {
        #region 全局变量
        private static ConnectionFactory _factory { get; set; }
        #endregion 全局变量

        public OrderService()
        {
            InitializeComponent();
            _factory = new ConnectionFactory() { HostName = "localhost", UserName = "city", Password = "12345678" };
            _factory.AutomaticRecoveryEnabled = true;//启用自动恢复
            _factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);//重试间隔
        }

        #region 定时器配置

        private System.Timers.Timer _Timer;
        public int ExecuteInterval
        {
            get
            {
                int time = 0;
                string intervaltime = ConfigurationManager.AppSettings["ExecuteInterval"];
                if (!string.IsNullOrEmpty(intervaltime))
                {
                    int.TryParse(intervaltime, out time);
                }
                if (time == 0)
                {
                    time = 10;
                }
                //毫秒
                return time;
            }
        }

        #endregion 定时器配置

        #region 服务内置函数

        protected override void OnStart(string[] args)
        {
            _Timer = new System.Timers.Timer(10000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Enabled = true;
        }

        protected override void OnStop()
        {
        }

        #endregion 服务内置函数

        #region 定时任务
        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _Timer.Enabled = false;
            try
            {
                ReceiveQueue();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                //_Timer.Interval = ExecuteInterval;
                //_Timer.Enabled = true;
                //_Timer.Start();
            }
        }
        #endregion 定时任务

        #region 业务函数
        public static void ReceiveQueue()
        {
            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "orderlist", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                if (!string.IsNullOrEmpty(message))
                {
                    dynamic order = JsonConvert.DeserializeObject<object>(message);
                }
            };
            channel.BasicConsume(queue: "orderlist", autoAck: true, consumer: consumer);
        }
        #endregion 业务函数
    }
}
