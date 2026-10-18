using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskedin.Domain.Entities;

// Join entity linking users to their assigned roles (many-to-many).
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
