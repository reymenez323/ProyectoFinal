using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .AsNoTracking()
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return producto is null ? NotFound(new { message = "Producto no encontrado." }) : Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.IdCategoria))
        {
            return BadRequest(new { message = "La categoría especificada no existe." });
        }

        if (!await _context.Proveedores.AnyAsync(p => p.Id == producto.IdProveedor))
        {
            return BadRequest(new { message = "El proveedor especificado no existe." });
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var created = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .FirstAsync(p => p.Id == producto.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
    {
        if (id != producto.Id)
        {
            return BadRequest(new { message = "El ID de la ruta no coincide con el ID del objeto." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var current = await _context.Productos.FindAsync(id);
        if (current is null)
        {
            return NotFound(new { message = "Producto no encontrado." });
        }

        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.IdCategoria))
        {
            return BadRequest(new { message = "La categoría especificada no existe." });
        }

        if (!await _context.Proveedores.AnyAsync(p => p.Id == producto.IdProveedor))
        {
            return BadRequest(new { message = "El proveedor especificado no existe." });
        }

        current.Nombre = producto.Nombre;
        current.Precio = producto.Precio;
        current.Stock = producto.Stock;
        current.IdCategoria = producto.IdCategoria;
        current.IdProveedor = producto.IdProveedor;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound(new { message = "Producto no encontrado." });
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticas()
    {
        if (!await _context.Productos.AnyAsync())
        {
            return Ok(new
            {
                message = "No hay productos registrados.",
                precioMasAlto = (object?)null,
                precioMasBajo = (object?)null,
                sumaTotalPrecios = 0,
                precioPromedio = 0,
                cantidadTotalProductos = 0
            });
        }

        var precioMasAlto = await _context.Productos.MaxAsync(p => p.Precio);
        var precioMasBajo = await _context.Productos.MinAsync(p => p.Precio);
        var sumaTotal = await _context.Productos.SumAsync(p => p.Precio);
        var promedio = await _context.Productos.AverageAsync(p => p.Precio);

        var productoMasAlto = await _context.Productos.FirstOrDefaultAsync(p => p.Precio == precioMasAlto);
        var productoMasBajo = await _context.Productos.FirstOrDefaultAsync(p => p.Precio == precioMasBajo);

        return Ok(new
        {
            precioMasAlto = new { producto = productoMasAlto?.Nombre, precio = precioMasAlto },
            precioMasBajo = new { producto = productoMasBajo?.Nombre, precio = precioMasBajo },
            sumaTotalPrecios = sumaTotal,
            precioPromedio = Math.Round(promedio, 2),
            cantidadTotalProductos = await _context.Productos.CountAsync()
        });
    }

    [HttpGet("por-categoria/{categoriaId:int}")]
    public async Task<IActionResult> GetPorCategoria(int categoriaId)
    {
        var categoria = await _context.Categorias.FindAsync(categoriaId);
        if (categoria is null)
        {
            return NotFound(new { message = "La categoría especificada no existe." });
        }

        var productos = await _context.Productos
            .Where(p => p.IdCategoria == categoriaId)
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .AsNoTracking()
            .ToListAsync();

        return Ok(new { categoria = categoria.Nombre, cantidad = productos.Count, productos });
    }

    [HttpGet("por-proveedor/{proveedorId:int}")]
    public async Task<IActionResult> GetPorProveedor(int proveedorId)
    {
        var proveedor = await _context.Proveedores.FindAsync(proveedorId);
        if (proveedor is null)
        {
            return NotFound(new { message = "El proveedor especificado no existe." });
        }

        var productos = await _context.Productos
            .Where(p => p.IdProveedor == proveedorId)
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .AsNoTracking()
            .ToListAsync();

        return Ok(new { proveedor = proveedor.Nombre, cantidad = productos.Count, productos });
    }

    [HttpGet("cantidad-total")]
    public async Task<IActionResult> GetCantidadTotal()
    {
        var cantidad = await _context.Productos.CountAsync();
        return Ok(new
        {
            cantidadTotalProductos = cantidad,
            stockTotal = await _context.Productos.SumAsync(p => p.Stock)
        });
    }
}
