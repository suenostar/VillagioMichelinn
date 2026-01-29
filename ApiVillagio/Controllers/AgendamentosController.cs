using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVillagio.Data;
using ApiVillagio.Models.Entities;

namespace ApiVillagio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgendamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgendamentosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Agendamentos.Include(a => a.Agencia).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var agendamento = await _context.Agendamentos.Include(a => a.Agencia)
                .FirstOrDefaultAsync(a => a.Id == id);
            return agendamento == null ? NotFound() : Ok(agendamento);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Agendamento agendamento)
        {
            _context.Agendamentos.Add(agendamento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = agendamento.Id }, agendamento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Agendamento agendamento)
        {
            if (id != agendamento.Id) return BadRequest();
            _context.Entry(agendamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento == null) return NotFound();
            _context.Agendamentos.Remove(agendamento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}