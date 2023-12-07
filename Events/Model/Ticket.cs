using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Events.Model
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        [JsonIgnore]
        public Event? Event { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        public int NumberOfTicket { get; set;}
    }
}
