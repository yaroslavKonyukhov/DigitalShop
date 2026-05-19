using System;
using System.Collections.Generic;
using System.Text;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
}
