using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
		private readonly ProductRepository _repository = new ProductRepository();

        [HttpGet]
        public IActionResult GetProducts()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _repository.GetById(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            _repository.Add(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
	}
}
