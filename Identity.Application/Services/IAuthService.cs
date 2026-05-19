using System;
using System.Collections.Generic;
using System.Text;

using Identity.Application.DTOs;

namespace Identity.Application.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(UserRegistrationDto dto);
    Task<string> LoginAsync(UserLoginDto dto);
    Task AssignAdminRoleAsync(Guid userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
}