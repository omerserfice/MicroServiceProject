using OrderService.Models;

namespace OrderService.Repositories
{
	public class OrderRepository
	{
		private static readonly List<Order> _orders = new List<Order>();

		public IEnumerable<Order> GetAll() => _orders;
		public Order? GetById(int id) => _orders.FirstOrDefault(o => o.Id == id);

		public void Add(Order order)
		{
			order.Id = _orders.Count + 1;
			_orders.Add(order);
		}
	}
}
