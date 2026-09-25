using Mal3abi.Core.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Mal3abi.Infra.Data
{
    // Match IdentityUser<Guid> with Guid as the key type
    public class MalaabiDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public MalaabiDbContext(DbContextOptions<MalaabiDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}