using System.ComponentModel.DataAnnotations;

namespace ClientsManagmentAppExample.Models
{
    public class ClientCredentialsModel
    {
        [Key]
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? FtpHost { get; set; }
        public string? FtpUserName { get; set; }
        public string? FtpPassword { get; set; }
        public string? PanelAddress { get; set; }
        public string? PanelUserName { get; set; }
        public string? PanelPassword { get; set;}
        public string? DomainOperatorAddress { get; set; }
        public string? DomainOperatorUserName { get; set; }
        public string? DomainOperatorPassword { get; set; }
        public string? HostingAddress { get; set; }
        public string? HostingUserName { get; set; }
        public string? HostingPassword { get; set; }
        public string? Key1Name { get; set; }
        public string? Key2Name { get; set; }
        public string? Key3Name { get; set; }
        public string? Key1Public { get; set; }
        public string? Key2Public { get; set; }
        public string? Key3Public { get; set; }
        public string? Key1Password { get; set; }
        public string? Key2Password { get; set; }
        public string? Key3Password { get; set; }
        public string? UpdatedById { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsVisible { get; set; }
    }
    
}
