using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ClientsManagmentAppExample.Repositories
{
    

    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsClient(string id)
        {
            var clients = _context.Clients.ToList();
            if(clients.Where(x => x.UserId == id).Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task AddClientAsync(ClientModel client)
        {
            client.ClientId = Guid.NewGuid().ToString();
            client.CreatedDate = DateTime.Now;
            client.IsVisible = true;
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
        } 
       
        public async Task<ClientModel> GetClientInfo(string id)
        {
            ClientModel client = _context.Clients.Where(x => x.UserId == id).FirstOrDefault();

            return client;
        } 
        
        public async Task<ClientModel> GetClientInfoByCLientId(string id)
        {
            ClientModel client = _context.Clients.Where(x => x.ClientId == id).FirstOrDefault();

            return client;
        }

        public async Task UpdateClientAsync(ClientModel client)
        {
           
            client.UpdatedDate = DateTime.Now;
            
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task AddProjectAsync(ProjectModel project)
        {
            project.CreatedDate = DateTime.Now;
            project.UpdatedDate = DateTime.Now;
            project.IsVisible = true;

            ClientCredentialsModel clientCredentials = new()
            {
                IsVisible = true,
                ProjectId = project.ProjectId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                CreatedBy = project.CreatedBy,
                UpdatedBy = project.UpdatedBy,
                CreatedById = project.CreatedById,
                UpdatedById = project.UpdatedById,
            };
            CredentialsModel credentials = new()
            {
                IsVisible = true,
                ProjectId = project.ProjectId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                CreatedBy = project.CreatedBy,
                UpdatedBy = project.UpdatedBy,
                CreatedById = project.CreatedById,
                UpdatedById = project.UpdatedById,
            };

            await _context.Projects.AddAsync(project);
            await _context.Credentials.AddAsync(credentials);
            await _context.ClientCredentials.AddAsync(clientCredentials);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProjectModel>>GetClientProjectsListAsync(string id)
        {
           return await _context.Projects.Where(x => x.IsVisible == true).Where(x => x.ClientId == id).ToListAsync();
        }

        public async Task<ProjectModel> GetProjectDetailsAsync(string id) 
        {
            ProjectModel project = await _context.Projects.FindAsync(id);
            return project;
        }

        public async Task UpdateProjectAsync(ProjectModel project)
        {
            project.UpdatedDate = DateTime.Now;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClientUpdatesModel>> GetClientUpdatesAsync(string id)
        {
            List<ClientUpdatesModel> list = await _context.ClientUpdates.Where(x => x.ProjectId == id).Where(x => x.IsVisible == true).ToListAsync();

            if (list.Count() > 0)
            {
                list = list.OrderByDescending(x => x.CreatedDate).ToList();
            }
            return list;
        }

        

        public async Task RemoveProjectAsync(string id)
        {
            var project = await _context.Projects.FindAsync(id);
            project.IsVisible = false;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveProjectCredentialsAsync(string id)
        {
            var credentials = await _context.ClientCredentials.FindAsync(id);
            credentials.IsVisible = false;
            var credentials2 = await _context.Credentials.FindAsync(id);
            credentials2.IsVisible = false;
            
            _context.ClientCredentials.Update(credentials);
            _context.Credentials.Update(credentials2);
            await _context.SaveChangesAsync();
        }

        public async Task AddUpdateAsync(ClientUpdatesModel model)
        {
            model.UpdateId = Guid.NewGuid().ToString();
            model.IsVisible = true;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;

            await _context.ClientUpdates.AddAsync(model);
            await _context.SaveChangesAsync();

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectId.Equals(model.ProjectId));
            project.UpdatedDate = DateTime.Now;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task<ClientUpdatesModel> GetUpdateDetails(string id)
        {
            return await _context.ClientUpdates.FindAsync(id);
        }

        public async Task UpdateUpdateAsync(ClientUpdatesModel model)
        {
            model.UpdatedDate = DateTime.Now;
            _context.ClientUpdates.Update(model);
            await _context.SaveChangesAsync();

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectId.Equals(model.ProjectId));
            project.UpdatedDate = DateTime.Now;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task<ClientCredentialsModel> GetProjectCredentialsAsync(string id)
        {
            return await _context.ClientCredentials.FindAsync(id);
        }

        public async Task UpdateProjectCredentialsAsync(ClientCredentialsModel model)
        {
            model.UpdatedDate = DateTime.Now;
            _context.ClientCredentials.Update(model);
            await _context.SaveChangesAsync();
        }
    }
}
