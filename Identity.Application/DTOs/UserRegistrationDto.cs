using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.DTOs
{
    public class UserRegistrationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
