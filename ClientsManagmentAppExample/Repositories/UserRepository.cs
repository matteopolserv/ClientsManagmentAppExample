using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ClientsManagmentAppExample.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<FormModel>> GetAllFormsAsync()
        {
            return await _context.Forms.ToListAsync();
        }

        public async Task<FormModel> GetFormDetailsAsync(string id)
        {
            return await _context.Forms.FindAsync(id);
        }
        public async Task DeleteFormAsync(string id)
        {
            var model = await _context.Forms.FindAsync(id);
            _context.Forms.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClientModel>> GetAllClientsAsync()
        {
            return await _context.Clients.Where(x => x.IsVisible == true).ToListAsync();
        }
        public async Task<ClientModel>GetClientDetailsAsync(string id)
        {
            return await _context.Clients.FindAsync(id);
        }
        public async Task RemoveClientByUserId(string userId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.UserId == userId);
            if(client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            
        }
        public async Task AddNewClientAsync(ClientModel model)
        {
            model.ClientId = Guid.NewGuid().ToString();
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.IsVisible = true;
            await _context.Clients.AddAsync(model);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateClientInfoAsync(ClientModel model)
        {
            model.UpdatedDate = DateTime.Now;
            
            _context.Clients.Update(model);
            await _context.SaveChangesAsync();

        }
      
        public async Task<List<ProjectModel>> GetAllProjectsAsync()
        {
            var list = await _context.Projects.Where(x => x.IsVisible == true).ToListAsync();
            if (list.Count > 0)
            {
                foreach (var project in list)
                {
                    if (project.UpdatedDate.Value.AddDays(30) < DateTime.Now)
                    {
                        project.IsVisible = false;
                        project.UpdatedDate = DateTime.Now;
                        project.UpdatedBy = "System";
                        project.UpdatedById = "System";
                        await UpdateProjectInfoAsync(project);
                        
                        list.Remove(project);
                    }
                }
                list = list.OrderByDescending(x => x.UpdatedDate).ToList();
            }
            return list;
        }

        public async Task<List<ProjectModel>> GetArchivedProjectsAsync()
        {
            var list = await _context.Projects.Where(x => x.IsVisible == false).ToListAsync();
            if (list.Count > 0)
            {
                list = list.OrderByDescending(x => x.UpdatedDate).ToList();
            }
            return list;
        }

        public async Task<ProjectModel> GetProjectDetailsAsync(string id)
        {
            return await _context.Projects.FindAsync(id);
        }
        
        
        public async Task AddNewProjectAsync(ProjectModel model)
        {
            CredentialsModel credentials = new();
            ClientCredentialsModel clientCredentials = new();
            
            string id = Guid.NewGuid().ToString();
            model.ProjectId = id;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.IsVisible = true;
            
            credentials.CreatedDate = DateTime.Now;
            credentials.UpdatedDate = DateTime.Now;
            credentials.IsVisible = true;
            credentials.CreatedBy = model.CreatedBy;
            credentials.CreatedById = model.CreatedById;
            credentials.ProjectId = id;
            credentials.ProjectName = model.ProjectName;

            clientCredentials.CreatedDate = DateTime.Now;
            clientCredentials.UpdatedDate = DateTime.Now;
            clientCredentials.IsVisible = true;
            clientCredentials.CreatedBy = model.CreatedBy;
            clientCredentials.CreatedById = model.CreatedById;
            clientCredentials.ProjectId = id;
            clientCredentials.ProjectName = model.ProjectName;
            
            await _context.Projects.AddAsync(model);
            await _context.Credentials.AddAsync(credentials);
            await _context.ClientCredentials.AddAsync(clientCredentials);
            await _context.SaveChangesAsync();
        }
        
        
        public async Task UpdateProjectInfoAsync(ProjectModel model)
        {
            model.UpdatedDate = DateTime.Now;
            _context.Projects.Update(model);
            await _context.SaveChangesAsync();
        }
        
        
        public async Task AddUpdateAsync(UpdatesModel model)
        {
            model.UpdateId = Guid.NewGuid().ToString();
            model.IsVisible = true;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            await _context.Updates.AddAsync(model);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<UpdatesModel>> GetUpdatesAsync(string id)
        {
            List<UpdatesModel> list = await _context.Updates.Where(x => x.ProjectId == id).Where(x => x.IsVisible == true).ToListAsync();
            if (list.Count() > 0)
            {
                list = list.OrderByDescending(x => x.CreatedDate).ToList();
            }
            return list;
        }
        public async Task<UpdatesModel> GetUpdateDetails(string id)
        {
            return await _context.Updates.FindAsync(id);
        }

        public async Task UpdateUpdateAsync(UpdatesModel model)
        {
            model.UpdatedDate = DateTime.Now;
            _context.Updates.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<CredentialsModel> GetProjectCredentialsAsync(string id)
        {
            return await _context.Credentials.FindAsync(id);
        }

        public async Task UpdateProjectCredentialsAsync(CredentialsModel model)
        {
            model.UpdatedDate = DateTime.Now;
            _context.Credentials.Update(model);
            await _context.SaveChangesAsync();
        }


        public async Task<ClientCredentialsModel> GetProjectClientCredentialsAsync(string id)
        {
            return await _context.ClientCredentials.FindAsync(id);
        }

        public async Task UpdateProjectClientCredentialsAsync(ClientCredentialsModel model)
        {
            model.UpdatedDate = DateTime.Now;
            _context.ClientCredentials.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<IList<IdentityUser>> GetCooperatorsAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync("cooperator");
            return users;
        }

        public async Task AssignCooperatorToProjectAsync(string projectId, string cooperatorId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            project.AssginedTo += cooperatorId + "|";
            await UpdateProjectInfoAsync(project);
        }

        public async Task RemoveCooperatorFromProjectAsync(string projectId, string cooperatorId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            int start = project.AssginedTo.IndexOf(cooperatorId);
            project.AssginedTo = project.AssginedTo.Remove(start, cooperatorId.Length+1);
            await UpdateProjectInfoAsync(project);
        }

        public async Task RemoveCooperatorAsync(string cooperatorId)
        {
            var cooperator = await _userManager.FindByIdAsync(cooperatorId);
            await _userManager.DeleteAsync(cooperator);
            var projects = await _context.Projects.ToListAsync();
            foreach (var project in projects)
            {
                if (project.AssginedTo.Contains(cooperatorId))
                {
                    int start = project.AssginedTo.IndexOf(cooperatorId);
                    project.AssginedTo.Remove(start, cooperatorId.Length + 1);
                }
            }
        }

        public async Task RemoveProject(string projectId)
        {
            var project = await GetProjectDetailsAsync(projectId);
            var clientCredentials = await GetProjectClientCredentialsAsync(projectId);
            var credentials = await GetProjectCredentialsAsync(projectId);
            var updates = await GetUpdatesAsync(projectId);
            var clientUpdates = await GetUpdatesAsync(projectId);
            _context.Credentials.Remove(credentials);
            _context.ClientCredentials.Remove(clientCredentials);
            foreach (var update in updates)
            {
                _context.Updates.Remove(update);
            }
            foreach (var update in clientUpdates)
            {
                _context.Updates.Remove(update);
            }
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

        }

        public async Task<List<IdentityUser>> GetProjectCooperatorsAsync(string projectId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            List<IdentityUser> projectCooperators = new();
            List<string> list = new();
            var users = await _userManager.GetUsersInRoleAsync("cooperator");
            if (project.AssginedTo != null)
            {
                list = project.AssginedTo.Split("|").ToList();
                foreach (var user in users)
                {
                    if (list.Contains(user.Id))
                    {
                        projectCooperators.Add(user);
                    }
                }
                return projectCooperators;
            }
            else
            {
                return null;
            }
        }
    }
}
