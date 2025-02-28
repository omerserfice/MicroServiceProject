using CustomerService.Models;

namespace CustomerService.Repositories
{
	public class CustomerRepository
	{
		private static readonly List<Customer> _customers = new List<Customer>
		{
			new Customer { Id = 1, Name = "Ömer Serfice", Email = "srfcomr@gmail.com" },
			new Customer { Id = 2, Name = "Zeynep Demir", Email = "zeynep@example.com" }
		};
		public IEnumerable<Customer> GetAll() => _customers;

		public Customer? GetById(int id) => _customers.FirstOrDefault(c => c.Id == id);
		public void Add(Customer customer)
		{
			customer.Id = _customers.Max(c => c.Id) + 1;
			_customers.Add(customer);
		}


	}
}
