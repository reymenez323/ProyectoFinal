using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Select(c => new
                {
                    c.Id,
                    c.Nombre
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada." });

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult> PostCategoria([FromBody] Categoria categoria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Categorias
                .AnyAsync(c => c.Nombre == categoria.Nombre);

            if (existe)
                return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre." });

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, new
            {
                categoria.Id,
                categoria.Nombre
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, [FromBody] Categoria categoria)
        {
            if (id != categoria.Id)
                return BadRequest(new { mensaje = "El id de la ruta no coincide con el id del cuerpo." });

            var existe = await _context.Categorias.AnyAsync(c => c.Id == id);
            if (!existe)
                return NotFound(new { mensaje = "Categoría no encontrada." });

            var duplicada = await _context.Categorias
                .AnyAsync(c => c.Nombre == categoria.Nombre && c.Id != id);

            if (duplicada)
                return BadRequest(new { mensaje = "Ya existe otra categoría con ese nombre." });

            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Categoría actualizada correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada." });

            var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
            if (tieneProductos)
                return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados." });

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Categoría eliminada correctamente." });
        }
    }
}