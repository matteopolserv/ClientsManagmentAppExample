using ClientsManagmentAppExample.Models;
using Microsoft.AspNetCore.Identity;

namespace ClientsManagmentAppExample.Interfaces
{
    public interface IUserRepository
    {
        Task<List<FormModel>> GetAllFormsAsync();
        Task<FormModel> GetFormDetailsAsync(string id);
        
        Task<List<ClientModel>> GetAllClientsAsync();
        Task<ClientModel> GetClientDetailsAsync(string id);
        Task AddNewClientAsync(ClientModel model);
        Task UpdateClientInfoAsync(ClientModel model);
        Task RemoveClientByUserId(string userId);

        Task<List<ProjectModel>> GetAllProjectsAsync();
        Task<List<ProjectModel>> GetArchivedProjectsAsync();
        Task<ProjectModel> GetProjectDetailsAsync(string id);
        Task AddNewProjectAsync(ProjectModel model);
        Task UpdateProjectInfoAsync(ProjectModel model);
        Task AddUpdateAsync(UpdatesModel model);
        Task<List<UpdatesModel>> GetUpdatesAsync(string id);
        Task<UpdatesModel> GetUpdateDetails(string id);
        Task UpdateUpdateAsync(UpdatesModel model);
        Task<CredentialsModel> GetProjectCredentialsAsync(string id);
        Task UpdateProjectCredentialsAsync(CredentialsModel model);
        Task RemoveProject(string projectId);

        Task<ClientCredentialsModel> GetProjectClientCredentialsAsync(string id);
        Task UpdateProjectClientCredentialsAsync(ClientCredentialsModel model);

        Task<IList<IdentityUser>> GetCooperatorsAsync();
        Task<List<IdentityUser>> GetProjectCooperatorsAsync(string projectId);
        Task AssignCooperatorToProjectAsync(string projectId, string cooperatorId);
        Task RemoveCooperatorFromProjectAsync(string projectId, string cooperatorId);
        Task RemoveCooperatorAsync(string cooperatorId);

    }
}
