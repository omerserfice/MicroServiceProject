using CustomerService.Models;
using CustomerService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
		private readonly CustomerRepository _repository = new CustomerRepository();
		[HttpGet]
		public IActionResult GetCustomers()
		{
			return Ok(_repository.GetAll());
		}

		[HttpGet("{id}")]
		public IActionResult GetCustomer(int id)
		{
			var customer = _repository.GetById(id);
			if (customer == null) return NotFound();
			return Ok(customer);
		}

		[HttpPost]
		public IActionResult AddCustomer(Customer customer)
		{
			_repository.Add(customer);
			return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
		}

	}
}
