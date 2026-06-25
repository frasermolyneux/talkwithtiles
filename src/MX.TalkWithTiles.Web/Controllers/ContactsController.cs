using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;
using MX.TalkWithTiles.Web.Extensions;

namespace MX.TalkWithTiles.Web.Controllers;

[Authorize]
public class ContactsController(
    ILogger<ContactsController> logger,
    IContactsRepository contactsRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var contacts = await contactsRepository.GetContacts(new ContactsFilterModel
        {
            UserId = User.GetUserGuid()
        });

        return View(contacts);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteContact(Guid contactId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("User has deleted a contact '{ContactId}'", contactId);
        await contactsRepository.DeleteContact(User.GetUserGuid(), contactId);
        return RedirectToAction("Index");
    }
}