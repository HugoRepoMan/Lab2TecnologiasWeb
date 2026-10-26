using System.ComponentModel.DataAnnotations;
namespace SakilaWebMVC.Models;

public class Country
{
    public int CountryId { get; set; }

    [Required(ErrorMessage = "El nombre del país es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "País")]
    public string CountryName { get; set; } = string.Empty;

    [Display(Name = "Última actualización")]
    public DateTime LastUpdate { get; set; }
    public int CityCount { get; set; }
}
