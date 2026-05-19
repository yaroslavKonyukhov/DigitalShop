using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.DTOs;

public record CategoryDto(Guid Id, string Name);

public record CreateCategoryDto(string Name);
