using DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DataAccess;

public class DbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    
}