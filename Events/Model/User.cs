using System.ComponentModel.DataAnnotations;
using Events.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Events.Model
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Length(3, 25)]
        public string? FirstName { get; set; }
        [Length(3, 25)]
        public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public object ToJson()
        {
            return new { Id = Id, FirstName = FirstName, LastName = LastName, Email = Email, PhoneNumber = PhoneNumber };
        }
    }
    

}
