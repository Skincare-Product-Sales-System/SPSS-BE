using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class User
    {
        public int UserId { get; set; }
        public Guid? SkinTypeId { get; set; }
        public Guid? RoleId { get; set; }
        public string UserName { get; set; }
        public string SurName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string Password { get; set; }
        public string AvatarUrl { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedBy { get; set; }

        public string DeletedBy { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public DateTimeOffset? LastUpdatedTime { get; set; }

        public DateTimeOffset? DeletedTime { get; set; }

        public bool IsDeleted { get; set; }


        // Navigation properties
        public virtual SkinType? SkinType { get; set; }
        public virtual Role? Role { get; set; }
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public ICollection<Reply> Replies { get; set; } = new List<Reply>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
