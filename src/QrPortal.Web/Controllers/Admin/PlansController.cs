using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;
using QrPortal.Web.ViewModels;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class PlansController(ISubscriptionPlanRepository plans) : Controller
{
    public async Task<IActionResult> Index(string? query, bool? isActive)
    {
        var vm = new PlanIndexVm
        {
            Query = query,
            IsActive = isActive,
            Items = await plans.SearchAsync(query, isActive)
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create() => View(new PlanFormVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await plans.CreateAsync(vm.ToEntity());
        TempData["SuccessMessage"] = "Piano creato con successo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var plan = await plans.GetByIdAsync(id);
        if (plan is null) return NotFound();
        return View(PlanFormVm.FromEntity(plan));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlanFormVm vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var current = await plans.GetByIdAsync(id);
        if (current is null) return NotFound();

        var updated = vm.ToEntity(current);
        updated.Id = id;
        await plans.UpdateAsync(updated);
        TempData["SuccessMessage"] = "Piano aggiornato.";
        return RedirectToAction(nameof(Index));
    }
}
