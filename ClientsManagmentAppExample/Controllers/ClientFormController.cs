using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientsManagmentAppExample.Controllers
{
    public class ClientFormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHelpers _helpers;
        

        public ClientFormController(IHelpers helpers, ApplicationDbContext context)
        {
            _helpers = helpers;
            _context = context;
            

        }

        public IActionResult ClientInfoForm()
        {
            
            return View();
        }

        
        [HttpPost]
        public   IActionResult ClientInfoForm(FormModel model)
        {


            return View(model);
            
        }

        [HttpPost]
        public async Task<IActionResult> ServiceForm(FormModel model)
        {
            if (!ModelState.IsValid)
            {
                
                return View("ClientInfoForm", model);
            }

            

            if (model.Service == "SiteWeb")
            {
                return View("WebForm", model);
            }
            else if(model.Service == "Shop")
            {
                return View("ShopForm", model);
            }
            else if (model.Service == "WebApp")
            {
                return View("WebAppForm", model);
            }  
            else if (model.Service == "DeskApp")
            {
                return View("DeskAppForm", model);
            }
            else if (model.Service == "MobileApp")
            {
                return View("MobileAppForm", model);
            }
            else
            {
                return View("ClientInfoForm", model);
            }
           
        }


        [HttpPost]
        public async Task<IActionResult> WebForm(FormModel model)
        {

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> WebFormPart2(FormModel model)
        {

            return View(model);
           
        }

        [HttpPost]
        public async Task<IActionResult> WebSiteSummary(FormModel model)
        {
            model.Consent = false;
            return View(model);

        }

        [HttpPost]
        public IActionResult ShopForm(FormModel model)
        {
            return View(model);
        }
        
        [HttpPost]
        public IActionResult ShopForm2(FormModel model)
        {
            return View(model);
        } 
        
        [HttpPost]
        public IActionResult ShopFormSummary(FormModel model)
        {
            model.Consent = false;
            return View(model);
        }

        [HttpPost]
        public IActionResult WebAppForm(FormModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult WebAppForm2(FormModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult WebAppFormSummary(FormModel model)
        {
            model.Consent = false;
            return View(model);
        }

        [HttpPost]
        public IActionResult DeskAppForm(FormModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult DeskAppFormSummary(FormModel model)
        {
            model.Consent = false;
            return View(model);
        } 
        
        [HttpPost]
        public IActionResult MobileAppForm(FormModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult MobileFormSummary(FormModel model)
        {
            model.Consent = false;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveForm(FormModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Proszę poprawnie wypełnić wszystkie wymagane pola");
                return View("ClientInfoForm", model);
            }
            
            if(model.Consent != true)
            {
                ModelState.AddModelError("", "Proszę wyrazić zgodę na przetwarzanie danych osobowych");
                return View("ClientInfoForm", model);
            }

            await Task.Run(async () =>
            {
                model.CreationTime = DateTime.Now;
                model.Id = Guid.NewGuid().ToString();
                await _context.Forms.AddAsync(model);
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    string message = await _helpers.FormMessageBuilder(model);
                    List<Task> sendEmailstasks = new() 
                    {
                        _helpers.SendSMTPEmailAsync(model.ClientEmail, "Ankieta w sprawie usługi informatycznej", message),
                        _helpers.SendSMTPEmailAsync("kontakt@lotier.pl", "Ankieta w sprawie usługi informatycznej od: " + model.ClientName, message)
                    };
                    await Task.WhenAll(sendEmailstasks);
                }

            });
            
            return View("FormCompleted");

        }

        
        public IActionResult FormCompleted()
        {

            return View();

        }
    }
}
