using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVillagio.Data;
using ApiVillagio.Models.Entities;

namespace ApiVillagio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Reservas.Include(r => r.Familia).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reserva = await _context.Reservas.Include(r => r.Familia)
                .FirstOrDefaultAsync(r => r.Id == id);
            return reserva == null ? NotFound() : Ok(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reserva);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Reserva reserva)
        {
            if (id != reserva.Id) return BadRequest();
            _context.Entry(reserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}