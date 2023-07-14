using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientsManagmentAppExample.Models
{
    public class ProjectModel
    {
        [Key]
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        
        [ForeignKey("Clients")]
        public string? ClientId { get; set; }
        public string? ClientName { get; set;}
        public DateTime? DeadLine { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedById { get; set; }
        public string? ProjectStatus { get; set;}
        public decimal? Price { get; set; }
        public string? AssginedTo { get; set; }
        public bool? IsVisible { get; set; }

    }
}
