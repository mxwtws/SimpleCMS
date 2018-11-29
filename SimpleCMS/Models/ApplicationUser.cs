using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;

namespace SimpleCMS.Models
{
    public class ApplicationUser: IdentityUser
    {
        public bool IsApprove { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<UserProfile> UserProfiles { get; set; }
    }
}