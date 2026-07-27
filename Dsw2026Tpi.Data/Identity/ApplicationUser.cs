using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Data.Identity;

public class ApplicationUser : IdentityUser
{
    public long? Dni { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}