using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;

namespace CustomerService.Services
{
	public class RabbitMQConsumer
	{
		public void Consume()
		{
			var factory = new ConnectionFactory() { HostName = "localhost", Port= 5672, UserName="guest",Password="guest" };
			using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();

			channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

			var consumer = new EventingBasicConsumer(channel);

			consumer.Received += (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);
				var order = JsonSerializer.Deserialize<Order>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

				Console.WriteLine($"📥 Yeni Sipariş Alındı: {order?.Id}, Ürün ID: {order?.ProductId}, Müşteri ID: {order?.CustomerId}, Fiyat: {order?.TotalPrice} TL");
			};
			channel.BasicConsume(queue: "orders", autoAck: true, consumer: consumer);

			Console.WriteLine("🎧 RabbitMQ Dinleniyor...");
			Console.ReadLine();


		}
		public class Order
		{
			public int Id { get; set; }
			public int ProductId { get; set; }
			public int CustomerId { get; set; }
			public int Quantity { get; set; }
			public decimal TotalPrice { get; set; }
		}

	}
}
