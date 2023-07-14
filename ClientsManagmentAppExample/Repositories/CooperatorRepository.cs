using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Interfaces;
using ClientsManagmentAppExample.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ClientsManagmentAppExample.Repositories
{
    public class CooperatorRepository : ICooperatorRepository
    {
        private readonly ApplicationDbContext _context;
        public CooperatorRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> IsAssignedTo(string projectId, string userId)
        {
            var project = await _context.Projects.Where(x => x.ProjectId == projectId).FirstOrDefaultAsync();
            if(project == null)
            {
                return false;
            }
            if (project.AssginedTo.Contains(userId))
            {
                return true;
            }
            return false;
        }

        public async Task<List<ProjectModel>> GetAllCooperatorProjectsAsync(string userId)
        {
            List<ProjectModel> projects = await _context.Projects.Where(x => x.AssginedTo.Contains(userId)).Where(x => x.IsVisible == true).ToListAsync();
            return projects;
        }

        public async Task<ProjectModel> GetProjectDetailsAsync(string projectId)
        {
            ProjectModel project = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            return project;
        }
        public async Task AddUpdateAsync(UpdatesModel model)
        {
            model.UpdateId = Guid.NewGuid().ToString();
            model.IsVisible = true;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            await _context.Updates.AddAsync(model);
            await _context.SaveChangesAsync();

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectId.Equals(model.ProjectId));
            project.UpdatedDate = DateTime.Now;
            _context.Projects.Update(project);
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

            var project = await _context.Projects.FirstOrDefaultAsync(x => x.ProjectId.Equals(model.ProjectId));
            project.UpdatedDate = DateTime.Now;
            _context.Projects.Update(project);
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
    }
}
