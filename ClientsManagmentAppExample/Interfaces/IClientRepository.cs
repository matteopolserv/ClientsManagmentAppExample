using ClientsManagmentAppExample.Models;

namespace ClientsManagmentAppExample.Interfaces
{
    public interface IClientRepository
    {
        Task<bool> IsClient(string id);

        Task AddClientAsync(ClientModel client);
        Task<ClientModel> GetClientInfo(string id);
        Task<ClientModel> GetClientInfoByCLientId(string id);
        Task UpdateClientAsync(ClientModel client);

        Task AddProjectAsync(ProjectModel project);
        Task<List<ProjectModel>> GetClientProjectsListAsync(string id);
        Task<ProjectModel> GetProjectDetailsAsync(string id);
        Task UpdateProjectAsync(ProjectModel project);
        Task<List<ClientUpdatesModel>> GetClientUpdatesAsync(string id);
        Task RemoveProjectAsync(string id);
        Task RemoveProjectCredentialsAsync(string id);
        
        Task AddUpdateAsync(ClientUpdatesModel model);
        Task<ClientUpdatesModel> GetUpdateDetails(string id);
        Task UpdateUpdateAsync(ClientUpdatesModel model);

        Task<ClientCredentialsModel> GetProjectCredentialsAsync(string id);
        Task UpdateProjectCredentialsAsync(ClientCredentialsModel model);
    }
}
