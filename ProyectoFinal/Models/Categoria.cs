using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
