using System;
using System.Collections.Generic;
using System.Text;

using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<IEnumerable<User>> GetAllAsync();
}
