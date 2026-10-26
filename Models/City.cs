using System.ComponentModel.DataAnnotations;
namespace SakilaWebMVC.Models;

public class City
{
    public int CityId { get; set; }

    [Required(ErrorMessage = "El nombre de la ciudad es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    [Display(Name = "Ciudad")]
    public string CityName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un país.")]
    [Display(Name = "País")]
    public int CountryId { get; set; }

    public string CountryName { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
}
