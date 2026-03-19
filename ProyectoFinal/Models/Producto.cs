using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models;

public class Producto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public int IdProveedor { get; set; }

    [Required]
    public int IdCategoria { get; set; }

    public Proveedor? Proveedor { get; set; }
    public Categoria? Categoria { get; set; }
}
