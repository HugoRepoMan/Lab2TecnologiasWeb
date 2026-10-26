using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SakilaWebMVC.Data;
using SakilaWebMVC.Models;

namespace SakilaWebMVC.Controllers;

public class CountryController(SakilaRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetCountriesAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await repository.GetCountryAsync(id);
        return model is null ? NotFound() : View(model);
    }

    public IActionResult Create() => View(new Country());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Country model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await repository.InsertCountryAsync(model);
            TempData["Success"] = "País creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException)
        {
            ModelState.AddModelError("", "No se pudo crear el país. Revisa los datos y las restricciones de la base de datos.");
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await repository.GetCountryAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Country model)
    {
        if (id != model.CountryId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        try
        {
            if (await repository.UpdateCountryAsync(model) == 0) return NotFound();
            TempData["Success"] = "País actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException)
        {
            ModelState.AddModelError("", "No se pudo actualizar el país.");
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var model = await repository.GetCountryAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            if (await repository.DeleteCountryAsync(id) == 0)
            {
                TempData["Error"] = "No se eliminó el país: no existe o tiene ciudades asociadas. La integridad referencial se conserva.";
            }
            else TempData["Success"] = "País eliminado correctamente.";
        }
        catch (SqlException)
        {
            TempData["Error"] = "No se pudo eliminar el país porque existen registros relacionados en Sakila.";
        }
        return RedirectToAction(nameof(Index));
    }
}
