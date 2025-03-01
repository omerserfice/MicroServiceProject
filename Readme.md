# Microservice 
Microservice Nedir?
Microservice mimarisi, bir uygulamanın bağımsız çalışan küçük servislerden oluştuğu bir yazılım mimarisidir.
Her servis, belirli bir işi yapar ve bağımsız olarak geliştirilip dağıtılabilir.

HTTP API ve RabbitMQ ile haberleşebilir.

Elimizde 3 servis olduğunu farzedelim 

- ProductService
- OrderService
- CustomeerServiee

OrderService -> ProductService 'e Http ile istek gönderip ürünün var olup olmadığını kontrol ediyor.

# Customer Service & RabbitMQ ile Event Driven(olay güdümlü) Mimari
Customer Service, kullanıcıların (müşterilerin) kayıtlarını tutacak ve sipariş oluşturulurken müşterinin var olup olmadığını kontrol eder.
RabbitMQ kullanarak event-driven yapısı.
Sipariş oluşturulduğunda CustomerService'e bir event gönder.

-RabbitMQ’yu Docker ile Çalıştırma
````
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
````

-OrderService'te RabbitMQ Yayıncı (Publisher) Ekleme

````
using RabbitMQ.Client;
using System.Text;

namespace OrderService.Services
{
    public class RabbitMQProducer
    {
        public void Publish(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: null, body: body);
        }
    }
}

````

# Sipariş oluştuğunda mesaj gönder (Publisher)  Yayıncı

 OrderService  bir sipariş oluşturulduğunda RabbitMQ'ya bir mesaj yayınlayacak.
 CustomerService RabbitMQ'dan mesajı dinleyerek yeni siparişi işleyecek.

````
public class RabbitMQProducer
	{
		public void Publish(string message)
		{
			var factory = new ConnectionFactory() { HostName = "localhost" };
			using var connnection = factory.CreateConnection();
			using var channel = connnection.CreateModel();

			channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

			var body = Encoding.UTF8.GetBytes(message);

			channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: null, body: body);
		}
	}
````

````
public async Task<IActionResult> CreateOrder([FromBody] Order order)
		{
			var customerResponse = await _httpClient.GetAsync($"{CustomerServiceUrl}/{order.CustomerId}");
			if (!customerResponse.IsSuccessStatusCode) return BadRequest("Müşteri bulunamadı!");

			var response = await _httpClient.GetAsync($"{ProductServiceUrl}/{order.ProductId}");
			if (!response.IsSuccessStatusCode) return BadRequest("Ürün bulunamadı.");

			var productJson = await response.Content.ReadAsStringAsync();
			var product = JsonSerializer.Deserialize<Product>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			order.TotalPrice = product.Price * order.Quantity;
			_repository.Add(order);

			// rabbit mq ya  sipariş mesajı gönder.

			var orderMesage = JsonSerializer.Serialize(order);
			_rabbitMQProducer.Publish(orderMesage);

			return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
		}
````

# CustomerService RabbitMQ Consumer (Dinleyici)
RabbitMQ'dan gelen sipariş mesajlarını dinleyen bir tüketici (consumer) oluşturuldu.

````
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using CustomerService.Models;

namespace CustomerService.Services
{
    public class RabbitMQConsumer
    {
        public void Consume()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = JsonSerializer.Deserialize<Order>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Console.WriteLine($" Yeni Sipariş Alındı: {order?.Id}, Ürün ID: {order?.ProductId}, Müşteri ID: {order?.CustomerId}, Fiyat: {order?.TotalPrice} TL");
            };

            channel.BasicConsume(queue: "orders", autoAck: true, consumer: consumer);

            Console.WriteLine(" RabbitMQ Dinleniyor...");
            Console.ReadLine();
        }
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

````
![Resim1](https://github.com/omerserfice/MicroServiceProject/blob/master/CustomerService/images/Screenshot_1.png)


