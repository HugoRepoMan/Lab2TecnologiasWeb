using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using SakilaWebMVC.Data;
using SakilaWebMVC.Models;

namespace SakilaWebMVC.Controllers;

public class CityController(SakilaRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetCitiesAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetCityAsync(id);
        return model is null ? NotFound() : View(model);
    }

    private async Task LoadCountriesAsync(int selected = 0)
    {
        ViewBag.Countries = new SelectList(await repository.GetCountriesAsync(),
            nameof(Country.CountryId), nameof(Country.CountryName), selected);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCountriesAsync();
        return View(new City());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(City model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await repository.InsertCityAsync(model);
                TempData["Success"] = "Ciudad creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException)
            {
                ModelState.AddModelError("", "No se pudo crear la ciudad. Comprueba que el país exista.");
            }
        }
        await LoadCountriesAsync(model.CountryId);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await repository.GetCityAsync(id);
        if (model is null) return NotFound();
        await LoadCountriesAsync(model.CountryId);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, City model)
    {
        if (id != model.CityId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                if (await repository.UpdateCityAsync(model) == 0) return NotFound();
                TempData["Success"] = "Ciudad actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException)
            {
                ModelState.AddModelError("", "No se pudo actualizar la ciudad o el país seleccionado no existe.");
            }
        }
        await LoadCountriesAsync(model.CountryId);
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var model = await repository.GetCityAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            if (await repository.DeleteCityAsync(id) == 0)
                TempData["Error"] = "La ciudad no existe.";
            else TempData["Success"] = "Ciudad eliminada correctamente.";
        }
        catch (SqlException)
        {
            TempData["Error"] = "No se puede eliminar la ciudad porque tiene registros relacionados en Sakila.";
        }
        return RedirectToAction(nameof(Index));
    }
}
