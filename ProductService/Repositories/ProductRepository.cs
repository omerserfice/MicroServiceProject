using ProductService.Models;

namespace ProductService.Repositories
{
	public class ProductRepository
	{
		private readonly List<Product> _products = new List<Product>
		{
			new Product{Id=1,Name="Laptop",Price=15000},
			new Product{Id=2,Name="Mouse",Price=500}
		};

		public IEnumerable<Product> GetAll() => _products;
		public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

		public void Add(Product product)
		{
			product.Id = _products.Max(p => p.Id) + 1;
			_products.Add(product);
		}
	}
}
