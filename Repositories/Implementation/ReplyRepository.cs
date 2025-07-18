﻿using BusinessObjects.Models;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class ReplyRepository : RepositoryBase<Reply, Guid>, IReplyRepository
    {
        public ReplyRepository(SPSSContext context) : base(context)
        {
        }
    }
}
