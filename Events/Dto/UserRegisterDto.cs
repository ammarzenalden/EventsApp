using System.ComponentModel.DataAnnotations;

namespace Events.Dto
{
    public class UserRegisterDto
    {
        [Length(3, 25)]
        public string? FirstName { get; set; }
        [Length(3, 25)]
        public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    }
}
