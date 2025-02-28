using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;

namespace OrderService.Services
{
	public class RabbitMQProducer
	{
		public void Publish(string message)
		{
			var factory = new ConnectionFactory()
			{
				HostName = "localhost", 
				Port = 5672,
				UserName = "guest",
				Password = "guest"
			};
			using var connnection = factory.CreateConnection();
			using var channel = connnection.CreateModel();

			channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

			var body = Encoding.UTF8.GetBytes(message);

			channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: null, body: body);
		}
	}
}
