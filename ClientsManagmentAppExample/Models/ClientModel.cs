using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsManagmentAppExample.Models
{
    public class ClientModel
    {
        [Key]
        public string? ClientId { get; set; }
        
        [Required(ErrorMessage = "Żeby dodanie klienta miało jakiś sens, to go trzeba jakoś nazwać. Resztę można pominąć")]
        public string? ClientName { get; set; }
        [EmailAddress(ErrorMessage = "Proszę podać poprawny adres email")]
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientAddress { get; set; }
        
        public string? UserId { get; set; }
        public string? NIP { get; set; }
        public string? REGON { get; set; }
        public string? Notices { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? AssginedTo { get; set; }
        public bool? IsVisible { get; set; }
        [NotMapped]
        public bool? Consent { get; set; }
    }
}
