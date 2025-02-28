using CustomerService.Services;
using RabbitMQ.Client;

var factory = new ConnectionFactory()
{
	HostName = "localhost",
	Port = 5672,
	UserName = "guest",
	Password = "guest"
};

try
{
	using var connection = factory.CreateConnection();
	using var channel = connection.CreateModel();
	Console.WriteLine("✅ RabbitMQ bağlantısı başarılı!");
}
catch (Exception ex)
{
	Console.WriteLine($"❌ RabbitMQ bağlantı hatası: {ex.Message}");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Task.Run(() =>
{
	var rabbitMqConsumer = new RabbitMQConsumer();
	rabbitMqConsumer.Consume();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
