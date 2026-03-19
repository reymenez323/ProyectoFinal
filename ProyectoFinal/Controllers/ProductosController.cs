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
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProductos()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Precio,
                    p.Stock,
                    p.IdCategoria,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : null,
                    p.IdProveedor,
                    Proveedor = p.Proveedor != null ? p.Proveedor.Nombre : null
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Precio,
                    p.Stock,
                    p.IdCategoria,
                    Categoria = p.Categoria != null ? p.Categoria.Nombre : null,
                    p.IdProveedor,
                    Proveedor = p.Proveedor != null ? p.Proveedor.Nombre : null
                })
                .FirstOrDefaultAsync();

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado." });

            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult> PostProducto([FromBody] Producto producto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == producto.IdCategoria);
            if (!categoriaExiste)
                return BadRequest(new { mensaje = "La categoría indicada no existe." });

            var proveedorExiste = await _context.Proveedores.AnyAsync(p => p.Id == producto.IdProveedor);
            if (!proveedorExiste)
                return BadRequest(new { mensaje = "El proveedor indicado no existe." });

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, new
            {
                producto.Id,
                producto.Nombre,
                producto.Precio,
                producto.Stock,
                producto.IdCategoria,
                producto.IdProveedor
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] Producto producto)
        {
            if (id != producto.Id)
                return BadRequest(new { mensaje = "El id de la ruta no coincide con el id del cuerpo." });

            var existe = await _context.Productos.AnyAsync(p => p.Id == id);
            if (!existe)
                return NotFound(new { mensaje = "Producto no encontrado." });

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == producto.IdCategoria);
            if (!categoriaExiste)
                return BadRequest(new { mensaje = "La categoría indicada no existe." });

            var proveedorExiste = await _context.Proveedores.AnyAsync(p => p.Id == producto.IdProveedor);
            if (!proveedorExiste)
                return BadRequest(new { mensaje = "El proveedor indicado no existe." });

            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado." });

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto eliminado correctamente." });
        }

        [HttpGet("estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var productos = await _context.Productos.ToListAsync();

            if (!productos.Any())
            {
                return Ok(new
                {
                    productoMasCaro = (object?)null,
                    productoMasBarato = (object?)null,
                    sumaTotalPrecios = 0,
                    promedioPrecios = 0
                });
            }

            var productoMasCaro = productos.OrderByDescending(p => p.Precio).First();
            var productoMasBarato = productos.OrderBy(p => p.Precio).First();
            var sumaTotalPrecios = productos.Sum(p => p.Precio);
            var promedioPrecios = productos.Average(p => p.Precio);

            return Ok(new
            {
                productoMasCaro = new
                {
                    productoMasCaro.Id,
                    productoMasCaro.Nombre,
                    productoMasCaro.Precio
                },
                productoMasBarato = new
                {
                    productoMasBarato.Id,
                    productoMasBarato.Nombre,
                    productoMasBarato.Precio
                },
                sumaTotalPrecios,
                promedioPrecios
            });
        }

        [HttpGet("por-categoria/{idCategoria}")]
        public async Task<IActionResult> GetProductosPorCategoria(int idCategoria)
        {
            var existeCategoria = await _context.Categorias.AnyAsync(c => c.Id == idCategoria);
            if (!existeCategoria)
                return NotFound(new { mensaje = "Categoría no encontrada." });

            var productos = await _context.Productos
                .Where(p => p.IdCategoria == idCategoria)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Precio,
                    p.Stock
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("por-proveedor/{idProveedor}")]
        public async Task<IActionResult> GetProductosPorProveedor(int idProveedor)
        {
            var existeProveedor = await _context.Proveedores.AnyAsync(p => p.Id == idProveedor);
            if (!existeProveedor)
                return NotFound(new { mensaje = "Proveedor no encontrado." });

            var productos = await _context.Productos
                .Where(p => p.IdProveedor == idProveedor)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.Precio,
                    p.Stock
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("cantidad-total")]
        public async Task<IActionResult> GetCantidadTotal()
        {
            var total = await _context.Productos.CountAsync();
            return Ok(new { cantidadTotalProductos = total });
        }
    }
}