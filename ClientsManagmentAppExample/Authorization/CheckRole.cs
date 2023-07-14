using Microsoft.AspNetCore.Authorization;

namespace ClientsManagmentAppExample.Authorization
{
    public class CheckRole : IAuthorizationRequirement
    {
        public string RoleName { get; set;}

        public CheckRole(string role)
        {
            RoleName = role;
        }
    }
}
