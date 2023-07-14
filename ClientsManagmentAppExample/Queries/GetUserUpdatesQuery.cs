using ClientsManagmentAppExample.Models;
using MediatR;

namespace ClientsManagmentAppExample.Queries
{
    public class GetUserUpdatesQuery : IRequest<List<UpdatesModel>>
    {
        public string ProjectId { get; set; }
    }
}
