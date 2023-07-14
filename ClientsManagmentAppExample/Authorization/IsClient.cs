using ClientsManagmentAppExample.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ClientsManagmentAppExample.Authorization
{
    public class IsClient : AuthorizationHandler<CheckRole>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public IsClient(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _userManager = userManager; 
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CheckRole requirement)
        {
            string curUserId = _userManager.GetUserId(context.User);
            if(_dbContext.Clients.Any(c => c.UserId == curUserId))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
