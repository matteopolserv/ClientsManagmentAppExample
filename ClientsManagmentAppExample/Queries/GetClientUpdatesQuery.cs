using ClientsManagmentAppExample.Models;
using MediatR;

namespace ClientsManagmentAppExample.Queries
{
    public class GetClientUpdatesQuery : IRequest<List<ClientUpdatesModel>>
    {
        public string ProjectId { get; set; }
    }
}
