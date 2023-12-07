using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Events.Model
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public string? EventName { get; set; }
        public string? Description {  get; set; }
        public string? Location { get; set;}
        public DateTime? EventDate { get; set; }
        public int Available {  get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
