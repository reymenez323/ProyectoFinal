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
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProveedores()
        {
            var proveedores = await _context.Proveedores
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Contacto
                })
                .ToListAsync();

            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Contacto
                })
                .FirstOrDefaultAsync();

            if (proveedor == null)
                return NotFound(new { mensaje = "Proveedor no encontrado." });

            return Ok(proveedor);
        }

        [HttpPost]
        public async Task<ActionResult> PostProveedor([FromBody] Proveedor proveedor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Proveedores
                .AnyAsync(p => p.Nombre == proveedor.Nombre);

            if (existe)
                return BadRequest(new { mensaje = "Ya existe un proveedor con ese nombre." });

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, new
            {
                proveedor.Id,
                proveedor.Nombre,
                proveedor.Contacto
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProveedor(int id, [FromBody] Proveedor proveedor)
        {
            if (id != proveedor.Id)
                return BadRequest(new { mensaje = "El id de la ruta no coincide con el id del cuerpo." });

            var existe = await _context.Proveedores.AnyAsync(p => p.Id == id);
            if (!existe)
                return NotFound(new { mensaje = "Proveedor no encontrado." });

            var duplicado = await _context.Proveedores
                .AnyAsync(p => p.Nombre == proveedor.Nombre && p.Id != id);

            if (duplicado)
                return BadRequest(new { mensaje = "Ya existe otro proveedor con ese nombre." });

            _context.Entry(proveedor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound(new { mensaje = "Proveedor no encontrado." });

            var tieneProductos = await _context.Productos.AnyAsync(p => p.IdProveedor == id);
            if (tieneProductos)
                return BadRequest(new { mensaje = "No se puede eliminar el proveedor porque tiene productos asociados." });

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor eliminado correctamente." });
        }
    }
}