using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVillagio.Data;
using ApiVillagio.Models.Entities;

namespace ApiVillagio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PagamentosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Pagamentos.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pagamento = await _context.Pagamentos.FindAsync(id);
            return pagamento == null ? NotFound() : Ok(pagamento);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Pagamento pagamento)
        {
            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = pagamento.Id }, pagamento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Pagamento pagamento)
        {
            if (id != pagamento.Id) return BadRequest();
            _context.Entry(pagamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pagamento = await _context.Pagamentos.FindAsync(id);
            if (pagamento == null) return NotFound();
            _context.Pagamentos.Remove(pagamento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}