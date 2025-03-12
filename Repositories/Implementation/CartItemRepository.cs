using BusinessObjects.Models;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class CartItemRepository : RepositoryBase<CartItem, Guid>, ICartItemRepository
    {
        public CartItemRepository(SPSSContext context) : base(context)
        {
        }
    }
}
