using System.Globalization;
using System.Text.RegularExpressions;
using ApiVillagio.Data;
using ApiVillagio.Models.DTOs;
using ApiVillagio.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVillagio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgendamentosController : ControllerBase
    {
        private readonly DbContext _db;

        public AgendamentosController(DbContext db) => _db = db;

        private static bool TryCombineLocalDateTime(string isoDate, string time24, out DateTime dt)
        {
            dt = default;
            if (!Regex.IsMatch(isoDate ?? "", @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$"))
                return false;
            if (!Regex.IsMatch(time24 ?? "", @"^([01]\d|2[0-3]):[0-5]\d$"))
                return false;

            return DateTime.TryParseExact($"{isoDate} {time24}", "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Agendamento>>> Get([FromQuery] int? agenciaId = null)
        {
            var query = _db.Agendamentos.Include(a => a.Agencia).AsQueryable();
            if (agenciaId.HasValue)
                query = query.Where(a => a.AgenciaId == agenciaId.Value);

            var list = await query
                .OrderBy(a => a.Data)
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarAgendamentoRequest req, CancellationToken ct)
        {
            if (req is null) return BadRequest("Payload vazio.");

            if (!TryCombineLocalDateTime(req.Data, req.Horario, out var dataHoraLocal))
                return BadRequest("Data/Horário inválidos. Use AAAA-MM-DD e HH:MM (24h).");

            var agenciaExiste = await _db.Agencias.AnyAsync(a => a.Id == req.AgenciaId, ct);
            if (!agenciaExiste) return NotFound("Agência não encontrada.");

            var ag = new Agendamento
            {
                AgenciaId = req.AgenciaId,
                Data = dataHoraLocal
            };

            _db.Agendamentos.Add(ag);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // Trata duplicidade por Agência+Data (índice único)
                if (ex.InnerException?.Message.Contains("UQ_Agendamentos_Agencia_Data", StringComparison.OrdinalIgnoreCase) == true
                    || ex.InnerException?.Message.Contains("IX_Agendamentos_AgenciaId_Data", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Conflict("Já existe agendamento para esta agência neste dia/horário.");
                }
                throw;
            }

            return CreatedAtAction(nameof(Get), new { id = ag.Id }, ag);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ag = await _db.Agendamentos.FindAsync(new object?[] { id }, ct);
            if (ag == null) return NotFound();

            _db.Agendamentos.Remove(ag);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
