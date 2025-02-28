using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Repositories;
using OrderService.Services;
using System.Text.Json;

namespace OrderService.Controllers
{
	[Route("api/orders")]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly OrderRepository _repository = new OrderRepository();

		private readonly HttpClient _httpClient = new HttpClient();

		private readonly RabbitMQProducer _rabbitMQProducer = new RabbitMQProducer();

		private const string ProductServiceUrl = "https://localhost:7187/api/products";
		private const string CustomerServiceUrl = "https://localhost:7167/api/customers";

		

		[HttpGet]
		public IActionResult GetOrders()
		{
			return Ok(_repository.GetAll());
		}
		[HttpGet("{id}")]
		public IActionResult GetOrder(int id)
		{
			var order = _repository.GetById(id);
			if (order == null) return NotFound();
			return Ok(order);
		}
		[HttpPost]
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
	}
	public class Product 
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
	}

}
