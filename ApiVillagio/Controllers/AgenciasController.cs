using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVillagio.Data;
using ApiVillagio.Models.DTOs;
using ApiVillagio.Models.Entities;

namespace ApiVillagio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenciasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgenciasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Todas as Agências
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Agencias.ToListAsync());

        // GET: Agência por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var agencia = await _context.Agencias.FindAsync(id);
            return agencia == null ? NotFound(new { message = "Agência não encontrada" }) : Ok(agencia);
        }

        // POST: Cadastro de Agência
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] Agencia agencia)
        {
            if (agencia == null ||
                string.IsNullOrWhiteSpace(agencia.Nome) ||
                string.IsNullOrWhiteSpace(agencia.Email) ||
                string.IsNullOrWhiteSpace(agencia.Telefone) ||
                string.IsNullOrWhiteSpace(agencia.Cnpj) ||
                string.IsNullOrWhiteSpace(agencia.Senha))
            {
                return BadRequest(new { message = "Todos os campos são obrigatórios." });
            }

            // Verifica duplicidade por CNPJ ou Email
            bool existe = await _context.Agencias.AnyAsync(a => a.Cnpj == agencia.Cnpj || a.Email == agencia.Email);
            if (existe)
            {
                return Conflict(new { message = "Já existe uma agência com este CNPJ ou Email." });
            }

            // (Opcional) Criptografar senha antes de salvar
            // agencia.Senha = BCrypt.Net.BCrypt.HashPassword(agencia.Senha);

            _context.Agencias.Add(agencia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Agência cadastrada com sucesso!", agencia.Id });
        }

        // PUT: Atualizar Agência
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Agencia agencia)
        {
            if (id != agencia.Id) return BadRequest(new { message = "ID inválido." });

            _context.Entry(agencia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Agência atualizada com sucesso!" });
        }

        // DELETE: Remover Agência
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var agencia = await _context.Agencias.FindAsync(id);
            if (agencia == null) return NotFound(new { message = "Agência não encontrada." });

            _context.Agencias.Remove(agencia);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Agência removida com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AgenciaLoginDto login)
        {
            if (string.IsNullOrWhiteSpace(login.Cnpj) || string.IsNullOrWhiteSpace(login.Senha))
                return BadRequest(new { message = "CNPJ e Senha são obrigatórios." });

            var agencia = await _context.Agencias
                .FirstOrDefaultAsync(a => a.Cnpj == login.Cnpj && a.Senha == login.Senha);

            if (agencia == null)
                return Unauthorized(new { message = "CNPJ ou senha inválidos." });

            return Ok(new { message = "Login realizado com sucesso!", agencia.Id, agencia.Nome });
        }
    }
}