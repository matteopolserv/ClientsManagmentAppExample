using ClientsManagmentAppExample.Helpers;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using ClientsManagmentAppExample.Repositories;
using ClientsManagmentAppExample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using MimeKit;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ClientsManagmentAppExample.Controllers
{
    public class ClientController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IHelpers _helpers;
        private readonly IFileService _fileService;
        
        public ClientController(UserManager<IdentityUser> userManager, IClientRepository clientRepository, IHelpers helpers, IFileService service)
        {
            _userManager = userManager;
            _clientRepository = clientRepository;
            _helpers = helpers;
            _fileService = service;
        }

        [Authorize]
        private async Task<IdentityUser> GetCurrentUserAsync()
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            IdentityUser user = await _userManager.FindByIdAsync(userId);

            return user;
        }

        [Authorize]
        public async Task<IActionResult> ClientDashboard()
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> BecomeClient() 
        {
            var curUser = await GetCurrentUserAsync();
            if (await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("ClientDashboard", "Client");
            }
            else
            {
                
                return View();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BecomeClient(ClientModel client) 
        {
            var curUser = await GetCurrentUserAsync();
            if (await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("ClientDashboard", "Client");
            }
            if (!ModelState.IsValid || client.Consent != true)
            {
                return View("BecomeClient", client); ;
            }
            client.UserId = curUser.Id;
            client.CreatedBy = curUser.UserName;
            client.CreatedById = curUser.Id;
            client.ClientEmail = curUser.Email;

            await _clientRepository.AddClientAsync(client);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy klient: " + client.ClientName, "Zarejestrował się nowy klient " + client.ClientName);
                }
            }

            return RedirectToAction("ClientDashboard", "Client");

        }

        [Authorize]
        public async Task<IActionResult> EditClientInfo()
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            else
            {
                ClientModel client = await _clientRepository.GetClientInfo(curUser.Id);
                
                if(client == null)
                {
                    return RedirectToAction("ClientDashboard", "Client");
                }
                
                return View(client);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateClientInfo(ClientModel client)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            if (!ModelState.IsValid)
            {
                return View("EditClientInfo", client); ;
            }
            var clientToUpdate = await _clientRepository.GetClientInfo(curUser.Id);

            clientToUpdate.UserId = curUser.Id;
            clientToUpdate.UpdatedBy = curUser.UserName;
            clientToUpdate.UpdatedById = curUser.Id;
            clientToUpdate.REGON = client.REGON;
            clientToUpdate.NIP = client.NIP;
            clientToUpdate.ClientPhone = client.ClientPhone;
            clientToUpdate.ClientAddress = client.ClientAddress;
            clientToUpdate.ClientEmail = curUser.Email;
            clientToUpdate.ClientName = client.ClientName;


            await _clientRepository.UpdateClientAsync(clientToUpdate);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja danych klienta: " + client.ClientName, "Dane użytkownika " + client.ClientName + " zostały zmienione przez " + client.UpdatedBy);
                }
            }
            return RedirectToAction("ClientDashboard", "Client");

        }

        [Authorize]
        public async Task<IActionResult> AddProject()
        {
            
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProject(ProjectModel project)
        {
            
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            if (!ModelState.IsValid)
            {
                return View(project);
            }
            var curClient = await _clientRepository.GetClientInfo(curUser.Id);
            project.ClientName = curClient.ClientName;
            project.ClientId = curClient.ClientId;
            project.CreatedBy = curUser.UserName;
            project.CreatedById = curUser.Id;
            project.UpdatedBy = curUser.UserName;
            project.UpdatedById = curUser.Id;

            await _clientRepository.AddProjectAsync(project);

            var users = await _userManager.GetUsersInRoleAsync("user");
            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy projekt: " + project.ProjectName, "Nowy projekt " + project.ProjectName + " został dodany przez " + project.CreatedBy);
                }
            }
            return RedirectToAction("ClientProjectsList", "Client");

        }

        public async Task<IActionResult> ClientProjectsList()
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            var curClient = await _clientRepository.GetClientInfo(curUser.Id);

            List<ProjectModel> projectsList = await _clientRepository.GetClientProjectsListAsync(curClient.ClientId);
            if(projectsList.Count() < 1)
            {
                
                ModelState.AddModelError("", "Brak projektów");
                return RedirectToAction("AddProject", "Client");
            }
            return View(projectsList);
        }

        [Authorize]
        public async Task<IActionResult> GetProjectDetails(string projectId)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            var curClient = await _clientRepository.GetClientInfo(curUser.Id);

            var curProject = await _clientRepository.GetProjectDetailsAsync(projectId);

            if(curProject.ClientId == null)
            {
                ModelState.AddModelError("", "Nie ma takiego projektu");
                return RedirectToAction("ClientProjectsList", "Client");
            }
            
            if(!curProject.ClientId.Equals(curClient.ClientId) || curProject.IsVisible != true) 
            {
                ModelState.AddModelError("", "Nie ma takiego projektu");
                return RedirectToAction("ClientProjectsList", "Client");
            }

            return View(curProject);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveProjectConfirmation(string id)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            var curClient = await _clientRepository.GetClientInfo(curUser.Id);

            var project = await _clientRepository.GetProjectDetailsAsync(id);

            if (!project.ClientId.Equals(curClient.ClientId))
            {
                return RedirectToAction("ClientDashboard", "Client");
            }

            return View(project);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveProject(string projectId, string projectName)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var project = await _clientRepository.GetProjectDetailsAsync(projectId);
            if (!projectName.Equals(project.ProjectName))
            {
                ModelState.AddModelError("", "Brak zgodności");
                return RedirectToAction("ClientProjectsList", "Client");
            }
            var curClient = await _clientRepository.GetClientInfo(curUser.Id);

            if(!project.ClientId.Equals(curClient.ClientId))
            {
                ModelState.AddModelError("", "Brak uprawnień");
                return RedirectToAction("ClientProjectsList", "Client");
            }

            var credentials = await _clientRepository.GetProjectCredentialsAsync(projectId);
            project.UpdatedBy = curUser.UserName;
            project.UpdatedById = curUser.Id;
            project.IsVisible = false;
            credentials.IsVisible = false;
            await _clientRepository.RemoveProjectAsync(project.ProjectId);
            await _clientRepository.RemoveProjectCredentialsAsync(credentials.ProjectId);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Projekt " + project.ProjectName, "Projekt " + project.ProjectName + " zostały usunięty przez " + project.UpdatedBy);
                }
            }

            return RedirectToAction("ClientProjectsList", "Client");


        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddUpdate(string id, string description)
        {
           
            var curUser = await GetCurrentUserAsync();
            
            if (!await _clientRepository.IsClient(curUser.Id) && !await _userManager.IsInRoleAsync(curUser, "user") && !await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("BecomeClient", "Client");
            }

            if (await _helpers.CheckPassword(description, curUser.UserName))
            {
                return RedirectToAction("FatalError", "Home");
            }

            var project = await _clientRepository.GetProjectDetailsAsync(id);
            if (project == null)
            {
                return RedirectToAction("GetAllProjects", "User");
            }
            ClientUpdatesModel update = new()
            {
                ProjectId = project.ProjectId,
                Description = description,
                CreatedBy = curUser.UserName,
                CreatedById = curUser.Id,
            };

            await _clientRepository.AddUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");


                      
                foreach (var user in users)
                {
                    if (user.Email != null)
                    {
                        await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);
                    }
                }

            if(project.ClientId != null)
            {
                var client = await _clientRepository.GetClientInfoByCLientId(project.ClientId);
                if (client.UserId != null)
                {
                    var clientUser = await _userManager.FindByIdAsync(client.UserId);
                    await _helpers.SendSMTPEmailAsync(clientUser.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);

                }
            }
            
            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);

                        }
                    }

                }
            }

            if (await _userManager.IsInRoleAsync(curUser, "user"))
            {
                return RedirectToAction("GetProjectDetails", "User", new { projectId = update.ProjectId }); ;
            }
            if (await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("GetProjectDetails", "Cooperator", new { projectId = update.ProjectId });
            }
            return RedirectToAction("GetProjectDetails", "Client", new { projectId = project.ProjectId});
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditUpdate(string id)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id) && !await _userManager.IsInRoleAsync(curUser, "user") && !await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("BecomeClient", "Client");
            }

            var update = await _clientRepository.GetUpdateDetails(id);
            if (update == null)
            {
                return RedirectToAction("ClientDashboard", "Client");
            }


            if (update.CreatedById != curUser.Id)
            {
                return RedirectToAction("ClientDashboard", "Client");
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
                ModelState.AddModelError("", "Aktualizacja musi zawierać treść");
                return RedirectToAction("ClientDashboard", "Client");
            }
            
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id) && !await _userManager.IsInRoleAsync(curUser, "user") && !await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            if (await _helpers.CheckPassword(description, curUser.UserName))
            {
                return RedirectToAction("FatalError", "Home");
            }
            ClientUpdatesModel update = await _clientRepository.GetUpdateDetails(id);
            
            if(!update.CreatedById.Equals(curUser.Id))
            {
                ModelState.AddModelError("", "Błąd");
                return RedirectToAction("ClientDashboard", "Client");
            }
            if (update.CreatedDate.Value.AddDays(1) < DateTime.Now)
            {
                return RedirectToAction("GetProjectDetails", "Client", new { projectId = update.ProjectId });
            }
            update.Description = description;
            update.UpdatedBy = curUser.UserName;
            update.UpdatedById = curUser.Id;


            await _clientRepository.UpdateUpdateAsync(update);

            var project = await _clientRepository.GetProjectDetailsAsync(update.ProjectId);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);
                }
            }

            if (project.ClientId != null)
            {
                var client = await _clientRepository.GetClientInfoByCLientId(project.ClientId);
                if (client != null)
                {
                    var clientUser = await _userManager.FindByIdAsync(client.UserId);
                    await _helpers.SendSMTPEmailAsync(clientUser.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);

                }
            }
            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy +  "<br>" + update.Description);

                        }
                    }

                }
            }
            if (await _userManager.IsInRoleAsync(curUser, "user"))
            {
                return RedirectToAction("GetProjectDetails", "User", new {projectId = update.ProjectId });
            }
            if (await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("GetProjectDetails", "Cooperator", new {projectId = update.ProjectId });
            }

            return RedirectToAction("GetProjectDetails", "Client", new { projectId = update.ProjectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveUpdate(string updateId)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id) && !await _userManager.IsInRoleAsync(curUser, "user") && !await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("BecomeClient", "Client");
            }
            
            ClientUpdatesModel update = await _clientRepository.GetUpdateDetails(updateId);
            
            var project = await _clientRepository.GetProjectDetailsAsync(update.ProjectId);
            if (project == null || project.IsVisible != true)
            {
                return RedirectToAction("ClientDashboard", "Client");
            }

            if (!update.CreatedById.Equals(curUser.Id))
            {
                return RedirectToAction("ClientDashboard", "Client");
            }

            update.IsVisible = false;
            update.UpdatedBy = curUser.UserName;
            update.UpdatedById = curUser.Id;

            await _clientRepository.UpdateUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Usunięcie aktualizacji projektu: " + project.ProjectName, "Aktualizacja w projekcie " + project.ProjectName + " została usunięta przez " + update.UpdatedBy);
                }
            }
            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja usunięta" + project.ProjectName, "Z projektu " + project.ProjectName + " została usunięta aktualizacja przez " + update.CreatedBy);

                        }
                    }

                }
            }
            if (await _userManager.IsInRoleAsync(curUser, "user"))
            {
                return RedirectToAction("GetProjectDetails", "User", new { projectId = update.ProjectId });
            }
            if (await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                return RedirectToAction("GetProjectDetails", "Cooperator", new { projectId = update.ProjectId });
            }
            return RedirectToAction("GetProjectDetails", "Client", new { projectId = update.ProjectId });
        }

        
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetProjectCredentials(string id)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }

            var curClient = await _clientRepository.GetClientInfo(curUser.Id);

            var project = await _clientRepository.GetProjectDetailsAsync(id);

            var credentials = await _clientRepository.GetProjectCredentialsAsync(id);
                     
            if (curClient.ClientId.Equals(project.ClientId))
            {

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
            else
            {
                await _userManager.SetLockoutEnabledAsync(curUser, true);
                return RedirectToAction("Index", "Home");
            }
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateProjectCredentials(ClientCredentialsModel credentials)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", "Client", new { projectId = credentials.ProjectId });
            }
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("BecomeClient", "Client");
            }

            var curClient = await _clientRepository.GetClientInfo(curUser.Id);

            var project = await _clientRepository.GetProjectDetailsAsync(credentials.ProjectId);


            if (!curClient.ClientId.Equals(project.ClientId))
            {
                await _userManager.SetLockoutEnabledAsync(curUser, true);
                return RedirectToAction("Index", "Home");
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
                                    
            if (curUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            credentials.UpdatedBy = curUser.UserName;
            credentials.UpdatedById = curUser.Id;

            await _clientRepository.UpdateProjectCredentialsAsync(credentials);


            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
                }
            }
            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + credentials.UpdatedBy);

                        }
                    }

                }
            }
            return RedirectToAction("GetProjectDetails", new { projectId = credentials.ProjectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadFile(List<IFormFile> uploadFiles, string projectId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", new { projectId });
            }
            FileModel fileModel = new()
            {
                Uploader = "client",
                ProjectId = projectId,
            };

            var tasks = uploadFiles.Select(uploadFile => Task.Run(async () =>
            {
                await _fileService.UploadFileAsync(uploadFile, fileModel);

            })).ToList();

            await Task.WhenAll(tasks);

            var project = await _clientRepository.GetProjectDetailsAsync(projectId);

            var users = await _userManager.GetUsersInRoleAsync("user");

            var curUser = await GetCurrentUserAsync();

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);
                }
            }

            if (project.ClientId == null)
            {
                return RedirectToAction("GetProjectDetails", "Client", new { projectId });
            }

            var client = await _clientRepository.GetClientInfo(project.ClientId);
            if (client != null)
            {
                var clientUser = await _userManager.FindByIdAsync(client.UserId);

                await _helpers.SendSMTPEmailAsync(clientUser.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);
            }

            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + project.ProjectName, "Do projektu " + project.ProjectName + " została dodana aktualizacja przez " + curUser.UserName);

                        }
                    }

                }
            }
            return RedirectToAction("GetProjectDetails", "Client", new { projectId });
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

            var project = await _clientRepository.GetProjectDetailsAsync(file.ProjectId);

            var curUser = await GetCurrentUserAsync();

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Plik usunięty projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został usunięty plik przez " + curUser.UserName);
                }
            }
            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Plik usunięty projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został usunięty plik przez " + curUser.UserName);

                        }
                    }

                }
            }

            return RedirectToAction("GetProjectDetails", "Client", new { file.ProjectId });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangeProjectStatus(string projectId, string projectStatus)
        {
            if(projectId == null || projectStatus == null)
            {
                return RedirectToAction("ClientDashboard", "Client");
            }
            var curUser = await GetCurrentUserAsync();
            if (!await _clientRepository.IsClient(curUser.Id))
            {
                return RedirectToAction("ClientDashboard", "Client");
            }
            var project = await _clientRepository.GetProjectDetailsAsync(projectId);
            var curClient = await _clientRepository.GetClientInfo(curUser.Id);
            if (project == null || !project.ClientId.Equals(curClient.ClientId))
            {
                return RedirectToAction("ClientDashboard", "Client");
            }

            project.ProjectStatus = projectStatus;
            project.UpdatedById = curUser.Id;
            project.UpdatedBy = curUser.UserName;
            await _clientRepository.UpdateProjectAsync(project);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Zmiana statusu projektu: " + project.ProjectName, "Status projektu " + project.ProjectName + " został zmieniony przez klienta " + curUser.UserName);
                }
            }
            if (project.AssginedTo != null)
            {
                List<string> cooperators = project.AssginedTo.Split('|').ToList();
                if (cooperators.Count() > 0)
                {
                    foreach (var cooperator in cooperators)
                    {
                        var user = await _userManager.FindByIdAsync(cooperator);
                        if (user != null)
                        {
                            await _helpers.SendSMTPEmailAsync(user.Email, "Zmiana statusu projektu: " + project.ProjectName, "Status projektu " + project.ProjectName + " został zmieniony przez klienta " + curUser.UserName);

                        }
                    }

                }
            }

            return RedirectToAction("GetProjectDetails", "Client", new { projectId }); ;
        }
    }
}
