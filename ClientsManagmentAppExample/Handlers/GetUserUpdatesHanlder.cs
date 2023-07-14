using ClientsManagmentAppExample.Data;
using ClientsManagmentAppExample.Models;
using ClientsManagmentAppExample.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientsManagmentAppExample.Handlers
{
    public class GetUserUpdatesHanlder : IRequestHandler<GetUserUpdatesQuery, List<UpdatesModel>>
    {
        private readonly ApplicationDbContext _ctx;

        public GetUserUpdatesHanlder(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<UpdatesModel>> Handle(GetUserUpdatesQuery request, CancellationToken cancellationToken)
        {
            List<UpdatesModel> updates = await _ctx.Updates.ToListAsync();
            updates = updates.Where(update => update.IsVisible == true).Where(update => update.ProjectId.Equals(request.ProjectId)).ToList();
            return updates;
        }
    }
}
    