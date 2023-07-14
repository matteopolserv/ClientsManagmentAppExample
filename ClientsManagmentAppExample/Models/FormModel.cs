using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsManagmentAppExample.Models
{
    public class FormModel
    {
        [Key]
        public string? Id { get; set; }
        
        [Required(ErrorMessage = "To pole nie może być puste")]
        public string ClientName { get; set; }
        
        public string? ClientID { get; set; }

        [Required(ErrorMessage = "To pole nie może być puste")]
        [EmailAddress(ErrorMessage = "Proszę podać prawidłowy adres email")]
        public string ClientEmail { get; set; }

        [Required(ErrorMessage = "To pole nie może być puste")]
        public string ClientPhone { get; set; }

        [Required(ErrorMessage = "Proszę wybrać usługę")]
        public string Service { get; set; }

        public string? Deadline { get; set; }
        
        public bool? HasDomain { get; set; }
        public string? DomainName { get; set; }
        public string? Notices { get; set; }
        
       
        public bool? HasHosting { get; set; }
        public string? HostingName { get; set; }


        
        public bool? HasWebSite { get; set; }
        public string? WebSiteAddress { get; set; }
        
     
        public string? HasVisualProject { get; set; }
        public string? VisualProjectDesc { get; set; }
        
        
        public string? WebSiteProfile { get; set; }
        
        public string? CMSDesc { get; set; }
        public string? ShopDesc { get; set; }
        public string? WebAppDesc { get; set; }

        public string? Language { get; set; }
        public string? DeskForm { get; set; }
        public string? OperatingSystem { get; set; }
        public string? DeskAppDesc { get; set; }
        public string? MobileAppDesc { get; set; }

        public bool? Consent { get; set; }

        public DateTime? CreationTime { get; set; }
    }
}
