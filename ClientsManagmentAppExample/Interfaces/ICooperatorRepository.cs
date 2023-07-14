using ClientsManagmentAppExample.Models;

namespace ClientsManagmentAppExample.Interfaces
{
    public interface ICooperatorRepository
    {
        Task<bool> IsAssignedTo(string projectId, string userId);
        Task<List<ProjectModel>> GetAllCooperatorProjectsAsync(string userId);
        Task<ProjectModel> GetProjectDetailsAsync(string projectId);
        Task AddUpdateAsync(UpdatesModel model);
        Task<List<UpdatesModel>> GetUpdatesAsync(string id);
        Task<UpdatesModel> GetUpdateDetails(string id);
        Task UpdateUpdateAsync(UpdatesModel model);
        Task<CredentialsModel> GetProjectCredentialsAsync(string id);
        Task UpdateProjectCredentialsAsync(CredentialsModel model);
        Task<ClientCredentialsModel> GetProjectClientCredentialsAsync(string id);
        Task UpdateProjectClientCredentialsAsync(ClientCredentialsModel model);
    }
}
