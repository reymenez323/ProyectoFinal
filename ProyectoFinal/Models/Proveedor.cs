using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models;

public class Proveedor
{
    public int Id { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Contacto { get; set; } = string.Empty;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
