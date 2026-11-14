using System.ComponentModel.DataAnnotations;

namespace HelpDeskWeb.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad və soyad daxil edilməlidir")]
        [StringLength(80)]
        public string ReporterName { get; set; } = "";

        [Required(ErrorMessage = "E-poçt daxil edilməlidir")]
        [EmailAddress(ErrorMessage = "E-poçt formatı düzgün deyil")]
        public string ReporterEmail { get; set; } = "";

        [Required(ErrorMessage = "Başlıq daxil edilməlidir")]
        [StringLength(120)]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Təsvir daxil edilməlidir")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Kateqoriya seçilməlidir")]
        public string Category { get; set; } = "";

        public string Priority { get; set; } = "Normal";

        public string Status { get; set; } = "New";

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}
