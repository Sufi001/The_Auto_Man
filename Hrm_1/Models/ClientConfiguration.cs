using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class ClientConfiguration
    {
        public int ClientId { get; set; }
        [Required]
        public bool SendInvoiceDataToFBR { get; set; }
        public bool SendSMS { get; set; }
        public bool SendWhatsApp { get; set; }
        public bool SendEmail { get; set; }
    }
}