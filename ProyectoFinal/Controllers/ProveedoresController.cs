using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProveedoresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Proveedor>>> GetAll()
    {
        return Ok(await _context.Proveedores.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Proveedor>> GetById(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        return proveedor is null ? NotFound(new { message = "Proveedor no encontrado." }) : Ok(proveedor);
    }

    [HttpPost]
    public async Task<ActionResult<Proveedor>> Create([FromBody] Proveedor proveedor)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        proveedor.Nombre = proveedor.Nombre.Trim();
        proveedor.Contacto = proveedor.Contacto.Trim();

        var exists = await _context.Proveedores.AnyAsync(p => p.Nombre == proveedor.Nombre);
        if (exists)
        {
            return BadRequest(new { message = "Ya existe un proveedor con ese nombre." });
        }

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Proveedor proveedor)
    {
        if (id != proveedor.Id)
        {
            return BadRequest(new { message = "El ID de la ruta no coincide con el ID del objeto." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var current = await _context.Proveedores.FindAsync(id);
        if (current is null)
        {
            return NotFound(new { message = "Proveedor no encontrado." });
        }

        proveedor.Nombre = proveedor.Nombre.Trim();
        proveedor.Contacto = proveedor.Contacto.Trim();

        var exists = await _context.Proveedores.AnyAsync(p => p.Nombre == proveedor.Nombre && p.Id != id);
        if (exists)
        {
            return BadRequest(new { message = "Ya existe un proveedor con ese nombre." });
        }

        current.Nombre = proveedor.Nombre;
        current.Contacto = proveedor.Contacto;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor is null)
        {
            return NotFound(new { message = "Proveedor no encontrado." });
        }

        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdProveedor == id);
        if (tieneProductos)
        {
            return BadRequest(new { message = "No se puede eliminar el proveedor porque tiene productos asociados." });
        }

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
