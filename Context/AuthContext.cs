using lu.Models;
using Microsoft.EntityFrameworkCore;

namespace lu.Context
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options)
            : base(options)
        { }
        public DbSet<User> Users { get; set; }
    }
}