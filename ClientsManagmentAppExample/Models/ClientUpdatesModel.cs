using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsManagmentAppExample.Models
{
    public class ClientUpdatesModel
    {
        [Key]
        public string? UpdateId { get; set; }
        [ForeignKey("Projects")]
        public string? ProjectId { get; set; }
        [Required(ErrorMessage = "Aktualizacja musi zawierać treść")]
        public string? Description { get; set; }
        public string? SeenBy { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsVisible { get; set; }
    }
}
