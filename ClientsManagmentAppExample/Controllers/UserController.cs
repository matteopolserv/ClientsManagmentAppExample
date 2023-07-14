using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MimeKit;
using System.IO;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ClientsManagmentAppExample.Controllers
{
    [Authorize(Roles = "user")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHelpers _helpers;
        private readonly IFileService _fileService;
        private readonly IMediator _mediaor;

        public UserController(IUserRepository userRepository, UserManager<IdentityUser> userManager, IHelpers helpers, IFileService fileService, IMediator mediator)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _helpers = helpers;
            _fileService = fileService;
            _mediaor = mediator;
        }

        [Authorize]
        private async Task<IdentityUser> GetCurrentUserAsync()
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            IdentityUser user = await _userManager.FindByIdAsync(userId);

            return user;
        }

        [Authorize]
        public IActionResult UserDashboard()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetAllForms()
        {
            List<FormModel> forms = await _userRepository.GetAllFormsAsync();
            if (forms.Count < 1)
            {
                ModelState.AddModelError("", "Brak formularzy");
                return RedirectToAction("UserDashboard", "User");
            }
            return View(forms);
        }


        [Authorize]
        public async Task<IActionResult> GetFormDetails(string id)
        {
            FormModel model = await _userRepository.GetFormDetailsAsync(id);
            
            
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> GetAllClients()
        {
            List<ClientModel> clients = await _userRepository.GetAllClientsAsync();
            if (clients.Count < 1)
            {
                ModelState.AddModelError("", "Brak klientów");
                return RedirectToAction("AddClient", "User");
            }
            return View(clients);
        }

        [Authorize]
        public async Task<IActionResult> GetClientDetails(string id)
        {
            ClientModel model = await _userRepository.GetClientDetailsAsync(id);
            if(model == null || model.IsVisible != true)
            {
                ModelState.AddModelError("", "Nie ma takiego klienta");
                return RedirectToAction("GetAllClients", "User");
            }
            return View (model);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult>UpdateClient(ClientModel client)
        {
            if (!ModelState.IsValid)
            {
                return View("GetClientDetails", client.ClientId);
            }
            IdentityUser curUser = await GetCurrentUserAsync();

           
            IdentityUser userClient;

            if (client.UserId != null)
            {
                userClient = await _userManager.FindByIdAsync(client.UserId);
                client.ClientEmail = userClient.Email;
                
            }
            else
            {
                userClient = await _userManager.FindByEmailAsync(client.ClientEmail);
                if(userClient != null)
                {
                    client.ClientEmail = userClient.Email;
                    client.UserId = userClient.Id;
                }
                
            }


            client.IsVisible = true;
            client.UpdatedBy = curUser.UserName;
            client.UpdatedById = curUser.Id;
            
            await _userRepository.UpdateClientInfoAsync(client);

            var users = await _userManager.GetUsersInRoleAsync("user");
            
            foreach(var user in users)
            {
                if (user.Email != null) 
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Zmiana danych klienta " + client.ClientName, "Dane klienta " + client.ClientName + " zostały zmienione przez " + client.UpdatedBy);
                }
            }

            return RedirectToAction("GetClientDetails", new {id = client.ClientId});
        }

        [Authorize]
        public IActionResult AddClient()
        {
            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddClient(ClientModel client)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction ("AddClient", "User");
            }
           
            IdentityUser curUser = await GetCurrentUserAsync();
            if(curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if(await _userManager.FindByEmailAsync(client.ClientEmail) != null)
            {
                var clientUser = await _userManager.FindByEmailAsync(client.ClientEmail);
                client.UserId = clientUser.Id;
            }

            client.CreatedById = curUser.Id;
            client.CreatedBy = curUser.UserName;
            
            await _userRepository.AddNewClientAsync(client);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy klient " + client.ClientName, "Nowy klient " + client.ClientName + " został dodany przez " + client.CreatedBy);
                }
            }

            return RedirectToAction("GetAllClients", "User");

              
        }

        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveClientConfirmation(string clientId)
        {
            var client = await _userRepository.GetClientDetailsAsync(clientId);

            return View(client);
        }
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveCooperatorConfirmation(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            return View(user);
        }
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveClient(string id, string clientName)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var client = await _userRepository.GetClientDetailsAsync(id);
            if (!clientName.Equals(client.ClientName))
            {
                return RedirectToAction("GetAllClients", "User");
            }
            client.UpdatedBy = curUser.UserName;
            client.UpdatedById = curUser.Id;
            client.IsVisible = false;
            await _userRepository.UpdateClientInfoAsync(client);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Klient usunięty: " + client.ClientName, "Klient " + client.ClientName + " został usunięty przez " + client.UpdatedBy);
                }
            }

            return RedirectToAction("GetAllClients", "User");
        }
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveCooperator(string userId, string userName)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var cooperator = await _userManager.FindByIdAsync(userId);
            if (!userName.Equals(cooperator.UserName))
            {
                return RedirectToAction("GetAllClients", "User");
            }

            await _userRepository.RemoveCooperatorAsync(userId);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Współpracownik usunięty: " + cooperator.UserName, "Współpracownik " + cooperator.UserName + " został usunięty przez " + cooperator.UserName);
                }
            }

            return RedirectToAction("GetAllClients", "User");
        }


        [Authorize]
        public async Task<IActionResult> GetAllProjects()
        {
            List<ProjectModel> projects = await _userRepository.GetAllProjectsAsync();
            
            if(projects.Count() < 1)
            {
                ModelState.AddModelError("", "Brak projektów");
                return RedirectToAction("AddProject", "User");
            }
            
            return View(projects);
        }

        
        [Authorize]
        public async Task<IActionResult> GetProjectDetails(string projectId)
        {
            ProjectModel model = await _userRepository.GetProjectDetailsAsync(projectId);
            if(model.IsVisible != true || model == null)
            {
                ModelState.AddModelError("", "Nie ma takiego projektu");
                return RedirectToAction("GetAllProjects", "User"); 
            }
            return View(model);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateProject(ProjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", "User",new { projectId = model.ProjectId });
            }
            IdentityUser curUser = await GetCurrentUserAsync();

            ProjectModel project = await _userRepository.GetProjectDetailsAsync(model.ProjectId);

            project.UpdatedBy = curUser.UserName;
            project.UpdatedById = curUser.Id;
            project.ProjectDescription = model.ProjectDescription;
            project.ProjectName = model.ProjectName;
            project.DeadLine = model.DeadLine;
            project.ClientName = model.ClientName;
            project.ClientId = model.ClientId;
            project.Price = model.Price;
            project.ProjectStatus = model.ProjectStatus;

            await _userRepository.UpdateProjectInfoAsync(project);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały zmieniony przez " + project.UpdatedBy);
                }
            }

            if (project.AssginedTo != null)
            {
                List<string> receivers = project.AssginedTo.Split("|").ToList();
                foreach (var receiver in receivers)
                {
                    if (receiver != null && receiver != "")
                    {
                        var receiverUser = await _userManager.FindByIdAsync(receiver);
                        string receiverEmail = receiverUser.Email;
                        await _helpers.SendSMTPEmailAsync(receiverEmail, "Projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały zmieniony przez " + project.UpdatedBy);

                    }
                }
            }
            return RedirectToAction("GetProjectDetails", "User", new { projectId = model.ProjectId });
        }

        [Authorize]
        public IActionResult AddProject()
        {
            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddProject(ProjectModel project)
        {
            if (!ModelState.IsValid)
            {
                return View("AddProject", project);
            }
            
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            project.CreatedBy = curUser.UserName;
            project.CreatedById = curUser.Id;
            project.IsVisible = true;

            await _userRepository.AddNewProjectAsync(project);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały dodany przez " + project.CreatedBy);
                }
            }

            return RedirectToAction("GetAllProjects", "User");


        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveProjectConfirmation(string id)
        {
            var project = await _userRepository.GetProjectDetailsAsync(id);

            return View(project);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveProject(string projectId, string projectName)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var project = await _userRepository.GetProjectDetailsAsync(projectId);
            if (!projectName.Equals(project.ProjectName))
            {
                return RedirectToAction("GetAllProjects", "User");
            }
            var credentials = await _userRepository.GetProjectCredentialsAsync(projectId);
            project.UpdatedBy = curUser.UserName;
            project.UpdatedById = curUser.Id;
            project.IsVisible = false;
            credentials.IsVisible = false;
            await _userRepository.RemoveProject(projectId);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały usunięty przez " + project.UpdatedBy);
                }
            }

            return RedirectToAction("GetAllProjects", "User");


        }
        
        [Authorize]
        public async Task<IActionResult> ArichveProject(string projectId)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var project = await _userRepository.GetProjectDetailsAsync(projectId);
           
            var credentials = await _userRepository.GetProjectCredentialsAsync(projectId);
            project.UpdatedBy = curUser.UserName;
            project.UpdatedById = curUser.Id;
            project.IsVisible = false;
            credentials.IsVisible = false;
            await _userRepository.UpdateProjectInfoAsync(project);
            await _userRepository.UpdateProjectCredentialsAsync(credentials);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały zarchiwizowany przez " + project.UpdatedBy);
                }
            }

            return RedirectToAction("GetAllProjects", "User");


        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DeArichveProject(string projectId)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var project = await _userRepository.GetProjectDetailsAsync(projectId);

            var credentials = await _userRepository.GetProjectCredentialsAsync(projectId);
            project.UpdatedBy = curUser.UserName;
            project.UpdatedById = curUser.Id;
            project.IsVisible = true;
            credentials.IsVisible = true;
            await _userRepository.UpdateProjectInfoAsync(project);
            await _userRepository.UpdateProjectCredentialsAsync(credentials);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały zdezarchiwizowany przez " + project.UpdatedBy);
                }
            }

            return RedirectToAction("GetAllProjects", "User");


        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddUpdate(string id, string description)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (await _helpers.CheckPassword(description, curUser.UserName))
            {
                return RedirectToAction("FatalError", "Home");
            }
            
            
            

            var project = await _userRepository.GetProjectDetailsAsync(id);
            if (project == null)
            {
                return RedirectToAction("GetAllProjects", "User");
            }
            UpdatesModel update = new() 
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = description,
                CreatedBy = curUser.UserName,
                CreatedById = curUser.Id,
            };

            
            
            await _userRepository.AddUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + update.ProjectName, "Do projektu " + update.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "\n\r<br/>" + update.Description);
                }
            }
            project.UpdatedDate = DateTime.Now;
            await _userRepository.UpdateProjectInfoAsync(project);
            if (project.AssginedTo != null)
            {
                List<string> receivers = project.AssginedTo.Split("|").ToList();
                foreach(var receiver in receivers)
                {
                    if(receiver != null && receiver != "")
                    {
                        var receiverUser = await _userManager.FindByIdAsync(receiver);
                        string receiverEmail = receiverUser.Email;
                        await _helpers.SendSMTPEmailAsync(receiverEmail, "Nowa aktualizacja " + update.ProjectName, "Do projektu " + update.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "\n\r<br/>" + update.Description);

                    }
                }
            }

            return RedirectToAction("GetProjectDetails", "User", new { projectId = project.ProjectId });
        }
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditUpdate(string id)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var update = await _userRepository.GetUpdateDetails(id);
            if (update == null)
            {
                return RedirectToAction("GetAllProjects", "User");
            }
            
            
            if(update.CreatedById != curUser.Id)
            {
                return RedirectToAction("GetAllProjects", "User");
            }
            else
            {
               
                return View(update);

            }
           
        }  
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateUpdate(string id, string description)
        {
            if(description == null)
            {
                return RedirectToAction("UserDashboard", "User");
            }
            
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (await _helpers.CheckPassword(description, curUser.UserName))
            {
                return RedirectToAction("FatalError", "Home");
            }

            UpdatesModel update = await _userRepository.GetUpdateDetails(id);
            if(update.CreatedDate.Value.AddDays(1) < DateTime.Now)
            {
                return RedirectToAction("GetProjectDetails", "User", new { projectId = update.ProjectId });
            }
            update.Description = description;
            update.UpdatedBy = curUser.UserName;
            update.UpdatedById = curUser.Id;
            
            
            await _userRepository.UpdateUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Edycja aktualizacji projektu: " + update.ProjectName, "Aktualizacja w projekcie " + update.ProjectName + " została edytowana przez " + update.UpdatedBy + "\n\r<br/>" + update.Description);
                }
            }

            var project = await _userRepository.GetProjectDetailsAsync(update.ProjectId);
            project.UpdatedDate = DateTime.Now;
            await _userRepository.UpdateProjectInfoAsync(project);
            if (project.AssginedTo != null)
            {
                List<string> receivers = project.AssginedTo.Split("|").ToList();
                foreach (var receiver in receivers)
                {
                    if (receiver != null && receiver != "")
                    {
                        var receiverUser = await _userManager.FindByIdAsync(receiver);
                        string receiverEmail = receiverUser.Email;
                        await _helpers.SendSMTPEmailAsync(receiverEmail, "Nowa aktualizacja " + update.ProjectName, "Do projektu " + update.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "\n\r<br/>" + update.Description);

                    }
                }
            }

            return RedirectToAction("GetProjectDetails", "User", new { projectId = update.ProjectId });
        } 
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveUpdate(string id)
        {
            IdentityUser curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UpdatesModel update = await _userRepository.GetUpdateDetails(id);
            var project = await _userRepository.GetProjectDetailsAsync(update.ProjectId);
            if (project == null || project.IsVisible != true)
            {
                return RedirectToAction("GetAllProjects", "User");
            }

            project.UpdatedDate = DateTime.Now;
            await _userRepository.UpdateProjectInfoAsync(project);

            update.IsVisible = false;
            update.UpdatedBy = curUser.UserName;
            update.UpdatedById = curUser.Id;

            await _userRepository.UpdateUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Usunięcie aktualizacji projektu: " + update.ProjectName, "Aktualizacja w projekcie " + update.ProjectName + " została usunięta przez " + update.UpdatedBy);
                }
            }

            return RedirectToAction("GetProjectDetails", "User", new { projectId = update.ProjectId });
        }
        
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetProjectCredentials(string projectId)
        {
            

            var credentials = await _userRepository.GetProjectCredentialsAsync(projectId);
            
            if(credentials.FtpPassword != null) 
            { 
                credentials.FtpPassword = await _helpers.DecryptString(credentials.FtpPassword);
            }
            
            if(credentials.PanelPassword != null) 
            { 
                credentials.PanelPassword = await _helpers.DecryptString(credentials.PanelPassword);
            }
              
            if(credentials.HostingPassword != null) 
            { 
                credentials.HostingPassword = await _helpers.DecryptString(credentials.HostingPassword);
            }
                
            if(credentials.DomainOperatorPassword != null) 
            { 
                credentials.DomainOperatorPassword = await _helpers.DecryptString(credentials.DomainOperatorPassword);
            }
                 
            if(credentials.Key1Password != null) 
            { 
                credentials.Key1Password = await _helpers.DecryptString(credentials.Key1Password);
            }
                 
            if(credentials.Key2Password != null) 
            { 
                credentials.Key2Password = await _helpers.DecryptString(credentials.Key2Password);
            }
                 
            if(credentials.Key3Password != null) 
            { 
                credentials.Key3Password = await _helpers.DecryptString(credentials.Key3Password);
            }

            return View(credentials);
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateProjectCredentials(CredentialsModel credentials) 
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", "User", new { projectId = credentials.ProjectId });
            }
            
            if (credentials.PanelPassword != null)
            {
                credentials.PanelPassword = await _helpers.EncryptString(credentials.PanelPassword);
            }
            
            if (credentials.FtpPassword != null)
            {
                credentials.FtpPassword = await _helpers.EncryptString(credentials.FtpPassword);
            }
            if (credentials.DomainOperatorPassword != null)
            {
                credentials.DomainOperatorPassword = await _helpers.EncryptString(credentials.DomainOperatorPassword);
            }
            if (credentials.HostingPassword != null)
            {
                credentials.HostingPassword = await _helpers.EncryptString(credentials.HostingPassword);
            }
            if (credentials.Key1Password != null)
            {
                credentials.Key1Password = await _helpers.EncryptString(credentials.Key1Password);
            } 
            if (credentials.Key2Password != null)
            {
                credentials.Key2Password = await _helpers.EncryptString(credentials.Key2Password);
            } 
            if (credentials.Key3Password != null)
            {
                credentials.Key3Password = await _helpers.EncryptString(credentials.Key3Password);
            }
            
            
            var curUser = await GetCurrentUserAsync();
            if(curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            credentials.UpdatedBy = curUser.UserName;
            credentials.UpdatedById = curUser.Id;

            await _userRepository.UpdateProjectCredentialsAsync(credentials);


            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
                }
            }
            return RedirectToAction("GetProjectDetails", new { projectId = credentials.ProjectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetProjectClientCredentials(string projectId)
        {


            var credentials = await _userRepository.GetProjectClientCredentialsAsync(projectId);

            if (credentials.FtpPassword != null)
            {
                credentials.FtpPassword = await _helpers.DecryptString(credentials.FtpPassword);
            }

            if (credentials.PanelPassword != null)
            {
                credentials.PanelPassword = await _helpers.DecryptString(credentials.PanelPassword);
            }

            if (credentials.HostingPassword != null)
            {
                credentials.HostingPassword = await _helpers.DecryptString(credentials.HostingPassword);
            }

            if (credentials.DomainOperatorPassword != null)
            {
                credentials.DomainOperatorPassword = await _helpers.DecryptString(credentials.DomainOperatorPassword);
            }

            if (credentials.Key1Password != null)
            {
                credentials.Key1Password = await _helpers.DecryptString(credentials.Key1Password);
            }

            if (credentials.Key2Password != null)
            {
                credentials.Key2Password = await _helpers.DecryptString(credentials.Key2Password);
            }

            if (credentials.Key3Password != null)
            {
                credentials.Key3Password = await _helpers.DecryptString(credentials.Key3Password);
            }

            return View(credentials);
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateProjectClientCredentials(ClientCredentialsModel credentials)
        {
            if (!ModelState.IsValid)
            {
                return View("GetProjectDetails", credentials.ProjectId);
            }

            if (credentials.PanelPassword != null)
            {
                credentials.PanelPassword = await _helpers.EncryptString(credentials.PanelPassword);
            }

            if (credentials.FtpPassword != null)
            {
                credentials.FtpPassword = await _helpers.EncryptString(credentials.FtpPassword);
            }
            if (credentials.DomainOperatorPassword != null)
            {
                credentials.DomainOperatorPassword = await _helpers.EncryptString(credentials.DomainOperatorPassword);
            }
            if (credentials.HostingPassword != null)
            {
                credentials.HostingPassword = await _helpers.EncryptString(credentials.HostingPassword);
            }
            if (credentials.Key1Password != null)
            {
                credentials.Key1Password = await _helpers.EncryptString(credentials.Key1Password);
            }
            if (credentials.Key2Password != null)
            {
                credentials.Key2Password = await _helpers.EncryptString(credentials.Key2Password);
            }
            if (credentials.Key3Password != null)
            {
                credentials.Key3Password = await _helpers.EncryptString(credentials.Key3Password);
            }


            var curUser = await GetCurrentUserAsync();
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            credentials.UpdatedBy = curUser.UserName;
            credentials.UpdatedById = curUser.Id;

            await _userRepository.UpdateProjectClientCredentialsAsync(credentials);


            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
                }
            }

            var project = await _userRepository.GetProjectDetailsAsync(credentials.ProjectId);
            if(project.ClientId == null)
            {
                return RedirectToAction("GetProjectDetails", new { projectId = credentials.ProjectId });
            }
            var client = await _userRepository.GetClientDetailsAsync(project.ClientId);
            if(client.UserId != null)
            { 
                var clientUser = await _userManager.FindByIdAsync(client.UserId);
            
                await _helpers.SendSMTPEmailAsync(clientUser.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
            }
            return RedirectToAction("GetProjectDetails", new { projectId = credentials.ProjectId });
        }


        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadFile(List<IFormFile> uploadFiles, string projectId)
        {
           if(!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", new { projectId });
            }
            FileModel fileModel = new()
            {
                Uploader = "user",
                ProjectId = projectId,
            };
            var tasks = uploadFiles.Select(uploadFile => Task.Run( async () =>
            {
                await _fileService.UploadFileAsync(uploadFile, fileModel);

            })).ToList();
            await Task.WhenAll(tasks);

            var project = await _userRepository.GetProjectDetailsAsync(projectId);
            
            var curUser = await GetCurrentUserAsync();
            
            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został dodany nowy plik przez " + curUser.UserName);
                }
            }

            return RedirectToAction("GetProjectDetails", "User", new { projectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DownloadFile(FileModel file)
        {

            MemoryStream stream = new();
            using FileStream fileStream = new(file.FilePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(stream);
            stream.Position = 0;

         
            return File(stream, MimeTypes.GetMimeType(file.FileName), file.FileName);

        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveFile(FileModel file)
        {
            await Task.Run(() =>
            {
                string dirPath = @"uploadedfiles/deletedfiles";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                string pathToMove = Path.Combine(dirPath, file.FileName);
                System.IO.File.Move(file.FilePath, pathToMove);
            });

            var project = await _userRepository.GetProjectDetailsAsync(file.ProjectId);

            var curUser = await GetCurrentUserAsync();

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Plik usunięty z projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został usunięty plik przez " + curUser.UserName);
                }
            }

            return RedirectToAction("GetProjectDetails", "User", new { file.ProjectId });

        }

        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadClientFile(List<IFormFile> uploadFiles, string ProjectId)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", new { ProjectId });
            }
            FileModel fileModel = new()
            {
                Uploader = "client",
                ProjectId = ProjectId,
            };

            var tasks = uploadFiles.Select(uploadFile => Task.Run( async () =>
            {
                await _fileService.UploadFileAsync(uploadFile, fileModel);

            })).ToList();

            await Task.WhenAll(tasks);

            var project = await _userRepository.GetProjectDetailsAsync(ProjectId);

            var curUser = await GetCurrentUserAsync();

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);
                }
            }

            if(project.ClientId == null)
            {
                return RedirectToAction("GetProjectDetails", "User", new { ProjectId });
            }

            var client = await _userRepository.GetClientDetailsAsync(project.ClientId);
            if (client.UserId != null)
            {
                var clientUser = await _userManager.FindByIdAsync(client.UserId);

                await _helpers.SendSMTPEmailAsync(clientUser.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);
            }

            return RedirectToAction("GetProjectDetails", "User", new { ProjectId });
        }
        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DownloadClientFile(FileModel file)
        {

            MemoryStream stream = new();
            using FileStream fileStream = new(file.FilePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(stream);
            stream.Position = 0;


            return File(stream, MimeTypes.GetMimeType(file.FileName), file.FileName);

        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveClientFile(FileModel file)
        {
            await Task.Run(() =>
            {
                string dirPath = @"uploadedfiles/deletedfiles";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                string pathToMove = Path.Combine(dirPath, file.FileName);
                System.IO.File.Move(file.FilePath, pathToMove);

            });

            var project = await _userRepository.GetProjectDetailsAsync(file.ProjectId);

            var curUser = await GetCurrentUserAsync();

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Plik usunięty z projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został usunięty plik przez " + curUser.UserName);
                }
            }

            return RedirectToAction("GetProjectDetails", "User", new { file.ProjectId });

        }

        [Authorize]
        public async Task<IActionResult> GetCooperators()
        {
            var cooperators = await _userRepository.GetCooperatorsAsync();
            return View(cooperators);
        }

        [Authorize]
        public async Task<IActionResult> AssignCooperatorToProjectForm(string projectId)
        {

            var cooperators = await _userRepository.GetCooperatorsAsync();
            TempData["ProjectId"] = projectId;
            return View(cooperators);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCooperatorToProject(string ProjectId, string cooperatorId)
        {
            await _userRepository.AssignCooperatorToProjectAsync(ProjectId, cooperatorId);
            var users = await _userManager.GetUsersInRoleAsync("user");
            var project = await _userRepository.GetProjectDetailsAsync(ProjectId);
            var cooperator = await _userManager.FindByIdAsync(cooperatorId);
            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Współpracownik przypisany do projektu " + project.ProjectName, "Do projektu " + project.ProjectName + " został przypisany współpracownik: " + cooperator.UserName);
                }
            }
            await _helpers.SendSMTPEmailAsync(cooperator.Email, "Współpracownik przypisany do projektu " + project.ProjectName, "Do projektu " + project.ProjectName + " został przypisany współpracownik: " + cooperator.UserName);

            return RedirectToAction("GetProjectDetails", new { ProjectId });
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCooperatorFromProject(string ProjectId, string cooperatorId)
        {
            await _userRepository.RemoveCooperatorFromProjectAsync(ProjectId, cooperatorId);
            var users = await _userManager.GetUsersInRoleAsync("user");
            var project = await _userRepository.GetProjectDetailsAsync(ProjectId);
            var cooperator = await _userManager.FindByIdAsync(cooperatorId);
            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Współpracownik usunięty z projektu " + project.ProjectName, "Z projektu " + project.ProjectName + " został usunięty współpracownik: " + cooperator.UserName);
                }
            }
            await _helpers.SendSMTPEmailAsync(cooperator.Email, "Współpracownik usunięty z projektu " + project.ProjectName, "Z projektu " + project.ProjectName + " został usunięty współpracownik: " + cooperator.UserName);

            return RedirectToAction("GetProjectDetails", new { ProjectId });
        }

        [Authorize]
        public async Task<IActionResult> GetArchivedProjects()
        {
            var projects = await _userRepository.GetArchivedProjectsAsync();
            return View(projects);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhoneNumber(string userId, string phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            await _userManager.SetPhoneNumberAsync(user, phoneNumber);

            return RedirectToAction("GetCooperators");
        }

        [Authorize]
        public async Task<IActionResult> AddCooperator()
        {
            var usersList = await _userManager.GetUsersInRoleAsync("client");
            return View(usersList);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCooperator(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRoleAsync(user, "cooperator");
            if(await _userManager.IsInRoleAsync(user, "client"))
            {
                await _userManager.RemoveFromRoleAsync(user, "client");

            }
            await _userRepository.RemoveClientByUserId(userId);

            return RedirectToAction("GetCooperators");

        }

    }
}
