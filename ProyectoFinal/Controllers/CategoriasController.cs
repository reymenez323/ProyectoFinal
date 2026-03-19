using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetAll()
    {
        return Ok(await _context.Categorias.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        return categoria is null ? NotFound(new { message = "Categoría no encontrada." }) : Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<Categoria>> Create([FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var nombre = categoria.Nombre.Trim();
        var exists = await _context.Categorias.AnyAsync(c => c.Nombre == nombre);
        if (exists)
        {
            return BadRequest(new { message = "Ya existe una categoría con ese nombre." });
        }

        categoria.Nombre = nombre;
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria categoria)
    {
        if (id != categoria.Id)
        {
            return BadRequest(new { message = "El ID de la ruta no coincide con el ID del objeto." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var current = await _context.Categorias.FindAsync(id);
        if (current is null)
        {
            return NotFound(new { message = "Categoría no encontrada." });
        }

        var nombre = categoria.Nombre.Trim();
        var exists = await _context.Categorias.AnyAsync(c => c.Nombre == nombre && c.Id != id);
        if (exists)
        {
            return BadRequest(new { message = "Ya existe una categoría con ese nombre." });
        }

        current.Nombre = nombre;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new { message = "Categoría no encontrada." });
        }

        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
        if (tieneProductos)
        {
            return BadRequest(new { message = "No se puede eliminar la categoría porque tiene productos asociados." });
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
