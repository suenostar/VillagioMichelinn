using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVillagio.Data;
using ApiVillagio.Models.DTOs;
using ApiVillagio.Models.Entities;

namespace ApiVillagio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FamiliasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Todas as famílias
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Familias.ToListAsync());

        // GET: Família por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var familia = await _context.Familias.FindAsync(id);
            return familia == null ? NotFound(new { message = "Família não encontrada" }) : Ok(familia);
        }

        // POST: Cadastro de Família
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] Familia familia)
        {
            if (familia == null ||
                string.IsNullOrWhiteSpace(familia.NomeResponsavel) ||
                string.IsNullOrWhiteSpace(familia.Telefone) ||
                string.IsNullOrWhiteSpace(familia.Senha))
            {
                return BadRequest(new { message = "Todos os campos são obrigatórios." });
            }


            // familia.Senha = BCrypt.Net.BCrypt.HashPassword(familia.Senha);

            _context.Familias.Add(familia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Família cadastrada com sucesso!", familia.Id });
        }

        // PUT: Atualizar Família
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Familia familia)
        {
            if (id != familia.Id) return BadRequest(new { message = "ID inválido." });

            _context.Entry(familia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Família atualizada com sucesso!" });
        }

        // DELETE: Remover Família
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var familia = await _context.Familias.FindAsync(id);
            if (familia == null) return NotFound(new { message = "Família não encontrada." });

            _context.Familias.Remove(familia);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Família removida com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FamiliaLoginDto login)
        {
            if (string.IsNullOrWhiteSpace(login.Telefone) || string.IsNullOrWhiteSpace(login.Senha))
                return BadRequest(new { message = "Telefone e Senha são obrigatórios." });

            var familia = await _context.Familias
                .FirstOrDefaultAsync(f => f.Telefone == login.Telefone && f.Senha == login.Senha);

            if (familia == null)
                return Unauthorized(new { message = "Telefone ou senha inválidos." });

            return Ok(new { message = "Login realizado com sucesso!", familia.Id, familia.NomeResponsavel });
        }
    }
}