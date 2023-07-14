using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using ClientsManagmentAppExample.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using MimeKit;
using System.Net;

namespace ClientsManagmentAppExample.Controllers
{
    [Authorize(Roles = "cooperator")]
    public class CooperatorController : Controller
    {
        private readonly ICooperatorRepository _cooperatorRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHelpers _helpers;
        private readonly IFileService _fileService;

        public CooperatorController(ICooperatorRepository cooperatorRepository, UserManager<IdentityUser> userManager, IHelpers helpers, IFileService fileService, IUserRepository userRepository)
        {
            _cooperatorRepository = cooperatorRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _helpers = helpers;
            _fileService = fileService;
            
        }

        [Authorize]
        private async Task<IdentityUser> GetCurrentUserAsync()
        {
            string userId = _userManager.GetUserId(HttpContext.User);
            IdentityUser user = await _userManager.FindByIdAsync(userId);

            return user;
        }

        [Authorize]
        public IActionResult CooperatorDashboard()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetAllCooperatorProjects()
        {
            var curUser = await GetCurrentUserAsync();
            
            List<ProjectModel> projects = await _cooperatorRepository.GetAllCooperatorProjectsAsync(curUser.Id);

            if (projects.Count < 1)
            {
                ModelState.AddModelError("", "Brak projektów");
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }

            return View(projects);
        }

        [Authorize]
        public async Task<IActionResult> GetProjectDetails(string projectId)
        {
            var curUser = await GetCurrentUserAsync();
            if(!await _cooperatorRepository.IsAssignedTo(projectId, curUser.Id))
            {
                ModelState.AddModelError("", "Nie masz uprawnień do przeglądania projektu");
                return RedirectToAction("GetAllCooperatorProjects", "Cooperator");
            }
            ProjectModel model = await _cooperatorRepository.GetProjectDetailsAsync(projectId);
            if (model.IsVisible != true || model == null)
            {
                ModelState.AddModelError("", "Nie ma takiego projektu");
                return RedirectToAction("GetAllCooperatorProjects", "Cooperator");
            }
            return View(model);
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

            if(!await _cooperatorRepository.IsAssignedTo(id, curUser.Id))
            {
                return RedirectToAction("Index", "Home");
            }

            if (await _helpers.CheckPassword(description, curUser.UserName))
            {
                return RedirectToAction("FatalError", "Home");
            }


            var project = await _cooperatorRepository.GetProjectDetailsAsync(id);
            if (project == null)
            {
                return RedirectToAction("GetAllCooperatorProjects", "Cooperator");
            }
            UpdatesModel update = new()
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = description,
                CreatedBy = curUser.UserName,
                CreatedById = curUser.Id,
            };

            await _cooperatorRepository.AddUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowa aktualizacja " + update.ProjectName, "Do projektu " + update.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);
                }
            }
            if(await _userManager.IsInRoleAsync(curUser, "cooperator"))
            {
                await _helpers.SendSMTPEmailAsync(curUser.Email, "Nowa aktualizacja " + update.ProjectName, "Do projektu " + update.ProjectName + " została dodana aktualizacja przez " + update.CreatedBy + "<br>" + update.Description);

            }
            return RedirectToAction("GetProjectDetails", "Cooperator", new { projectId = project.ProjectId });
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
           
            var update = await _cooperatorRepository.GetUpdateDetails(id);
            if (update == null)
            {
                return RedirectToAction("GetAllCooperatorProjects", "Cooperator");
            }


            if (update.CreatedById != curUser.Id)
            {
                return RedirectToAction("GetAllCooperatorProjects", "Cooperator");
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
            if (description == null)
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
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

            UpdatesModel update = await _cooperatorRepository.GetUpdateDetails(id);

            if (update.CreatedDate.Value.AddDays(1) < DateTime.Now)
            {
                return RedirectToAction("GetProjectDetails", "User", new { projectId = update.ProjectId });
            }

            update.Description = description;
            update.UpdatedBy = curUser.UserName;
            update.UpdatedById = curUser.Id;


            await _cooperatorRepository.UpdateUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Edycja aktualizacji projektu: " + update.ProjectName, "Aktualizacja w projekcie " + update.ProjectName + " została edytowana przez " + update.UpdatedBy + "\n\r<br/>" + update.Description);
                }
            }
            
            await _helpers.SendSMTPEmailAsync(curUser.Email, "Edycja aktualizacji projektu: " + update.ProjectName, "Aktualizacja w projekcie " + update.ProjectName + " została edytowana przez " + update.UpdatedBy + "\n\r<br/>" + update.Description);

            return RedirectToAction("GetProjectDetails", "Cooperator", new { projectId = update.ProjectId });
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

            //var project = await _cooperatorRepository.GetProjectDetailsAsync(id);
            //if (project == null || project.IsVisible != true)
            //{
            //    return RedirectToAction("GetAllCooperatorProjects", "Cooperator");
            //}
            UpdatesModel update = await _cooperatorRepository.GetUpdateDetails(id);
            update.IsVisible = false;
            update.UpdatedBy = curUser.UserName;
            update.UpdatedById = curUser.Id;

            await _cooperatorRepository.UpdateUpdateAsync(update);

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Usunięcie aktualizacji projektu: " + update.ProjectName, "Aktualizacja w projekcie " + update.ProjectName + " została usunięta przez " + update.UpdatedBy);
                }
            }

            return RedirectToAction("GetProjectDetails", "Cooperator", new { projectId = update.ProjectId });
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetProjectCredentials(string projectId)
        {

            var curUser = await GetCurrentUserAsync();
            if(!await _cooperatorRepository.IsAssignedTo(projectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
            var credentials = await _cooperatorRepository.GetProjectCredentialsAsync(projectId);

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
        public async Task<IActionResult> UpdateProjectCredentials(CredentialsModel credentials)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", "Cooperator", new { projectId = credentials.ProjectId });
            }
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(credentials.ProjectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
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

            await _cooperatorRepository.UpdateProjectCredentialsAsync(credentials);


            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
                }
            }
            
            await _helpers.SendSMTPEmailAsync(curUser.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
            
            return RedirectToAction("GetProjectDetails", new { projectId = credentials.ProjectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GetProjectClientCredentials(string projectId)
        {

            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(projectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
            var credentials = await _cooperatorRepository.GetProjectClientCredentialsAsync(projectId);

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
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(credentials.ProjectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
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

            await _cooperatorRepository.UpdateProjectClientCredentialsAsync(credentials);


            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Aktualizacja poświadczeń projektu: " + credentials.ProjectName, "Poświadczenia w projekcie " + credentials.ProjectName + " zostały zmienione przez " + credentials.UpdatedBy);
                }
            }

            var project = await _cooperatorRepository.GetProjectDetailsAsync(credentials.ProjectId);
            if (project.ClientId == null)
            {
                return RedirectToAction("GetProjectDetails", new { projectId = credentials.ProjectId });
            }
            var client = await _userRepository.GetClientDetailsAsync(project.ClientId);
            if (client.UserId != null)
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
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetProjectDetails", new { projectId });
            }
            FileModel fileModel = new()
            {
                Uploader = "user",
                ProjectId = projectId,
            };

            var tasks = uploadFiles.Select(uploadFile => Task.Run(async () =>
            {
                await _fileService.UploadFileAsync(uploadFile, fileModel);

            })).ToList();
            await Task.WhenAll(tasks);

            var project = await _cooperatorRepository.GetProjectDetailsAsync(projectId);

            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(projectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został dodany nowy plik przez " + curUser.UserName);
                }
            }

            await _helpers.SendSMTPEmailAsync(curUser.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został dodany nowy plik przez " + curUser.UserName);

            return RedirectToAction("GetProjectDetails", "User", new { projectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DownloadFile(FileModel file)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(file.ProjectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
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
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(file.ProjectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
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

            var project = await _cooperatorRepository.GetProjectDetailsAsync(file.ProjectId);

           

            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Plik usunięty z projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został usunięty plik przez " + curUser.UserName);
                }
            }
            await _helpers.SendSMTPEmailAsync(curUser.Email, "Plik usunięty z projektu: " + project.ProjectName, "W projekcie " + project.ProjectName + " został usunięty plik przez " + curUser.UserName);

            return RedirectToAction("GetProjectDetails", "User", new { file.ProjectId });

        }

        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadClientFile(List<IFormFile> uploadFiles, string projectId)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(projectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
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

            var project = await _cooperatorRepository.GetProjectDetailsAsync(projectId);


            var users = await _userManager.GetUsersInRoleAsync("user");

            foreach (var user in users)
            {
                if (user.Email != null)
                {
                    await _helpers.SendSMTPEmailAsync(user.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);
                }
            }
            await _helpers.SendSMTPEmailAsync(curUser.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);

            if (project.ClientId == null)
            {
                return RedirectToAction("GetProjectDetails", "User", new { projectId });
            }

            var client = await _userRepository.GetClientDetailsAsync(project.ClientId);
            if (client.UserId != null)
            {
                var clientUser = await _userManager.FindByIdAsync(client.UserId);

                await _helpers.SendSMTPEmailAsync(clientUser.Email, "Nowy plik dodany do projektu: " + project.ProjectName, "Nowy plik w projekcie " + project.ProjectName + " został dodany przez przez " + curUser.UserName);
            }

            return RedirectToAction("GetProjectDetails", "User", new { projectId });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DownloadClientFile(FileModel file)
        {
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(file.ProjectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
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
            var curUser = await GetCurrentUserAsync();
            if (!await _cooperatorRepository.IsAssignedTo(file.ProjectId, curUser.Id))
            {
                return RedirectToAction("CooperatorDashboard", "Cooperator");
            }
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

            var project = await _cooperatorRepository.GetProjectDetailsAsync(file.ProjectId);

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
    }
}
