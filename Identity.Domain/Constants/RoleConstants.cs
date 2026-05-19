using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Domain.Constants;
public static class RoleConstants
{
    public static readonly Guid AdminRole = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid UserRole = Guid.Parse("00000000-0000-0000-0000-000000000002");
}
