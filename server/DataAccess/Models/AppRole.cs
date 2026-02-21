using Microsoft.AspNetCore.Identity;

namespace DataAccess.Models;


//Adding this in case I want to do multi-user calendars eventually (owner vs user)
public class AppRole : IdentityRole<Guid>
{
}