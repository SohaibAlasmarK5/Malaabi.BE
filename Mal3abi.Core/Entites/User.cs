using Microsoft.AspNetCore.Identity;
using System;

namespace Mal3abi.Core.Entites
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
    }
}