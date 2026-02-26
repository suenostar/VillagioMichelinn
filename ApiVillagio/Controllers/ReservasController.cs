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
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReservasController(AppDbContext db) => _db = db;

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
        public async Task<ActionResult<IEnumerable<Reserva>>> Get([FromQuery] int? familiaId = null)
        {
            var query = _db.Reservas.Include(r => r.Familia).AsQueryable();
            if (familiaId.HasValue)
                query = query.Where(r => r.FamiliaId == familiaId.Value);

            var list = await query
                .OrderBy(r => r.DataReserva)
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarReservaRequest req, CancellationToken ct)
        {
            if (req is null) return BadRequest("Payload vazio.");

            if (!TryCombineLocalDateTime(req.Data, req.Horario, out var dataHoraLocal))
                return BadRequest("Data/Horário inválidos. Use AAAA-MM-DD e HH:MM (24h).");

            var familiaExiste = await _db.Familias.AnyAsync(f => f.Id == req.FamiliaId, ct);
            if (!familiaExiste) return NotFound("Família não encontrada.");

            var reserva = new Reserva
            {
                FamiliaId = req.FamiliaId,
                DataReserva = dataHoraLocal
            };

            _db.Reservas.Add(reserva);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // Trata violação de índice único (duplicidade de FamiliaId+DataReserva)
                if (ex.InnerException?.Message.Contains("UQ_Reservas_Familia_Data", StringComparison.OrdinalIgnoreCase) == true
                    || ex.InnerException?.Message.Contains("IX_Reservas_FamiliaId_DataReserva", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Conflict("Já existe reserva para esta família neste dia/horário.");
                }
                throw;
            }

            return CreatedAtAction(nameof(Get), new { id = reserva.Id }, reserva);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var reserva = await _db.Reservas.FindAsync(new object?[] { id }, ct);
            if (reserva == null) return NotFound();

            _db.Reservas.Remove(reserva);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}