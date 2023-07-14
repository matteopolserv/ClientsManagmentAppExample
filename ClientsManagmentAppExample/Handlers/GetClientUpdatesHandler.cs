using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Models;
using ClientsManagmentAppExample.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace ClientsManagmentAppExample.Handlers
{
    public class GetClientUpdatesHandler : IRequestHandler<GetClientUpdatesQuery, List<ClientUpdatesModel>>
    {
        private readonly ApplicationDbContext _ctx;

        public GetClientUpdatesHandler(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<List<ClientUpdatesModel>> Handle(GetClientUpdatesQuery request, CancellationToken cancellationToken)
        {
            List<ClientUpdatesModel> updates = await _ctx.ClientUpdates.ToListAsync();
            updates = updates.Where(update => update.IsVisible == true && update.ProjectId.Equals(request.ProjectId)).ToList();
            return updates;
        }
    }
}
