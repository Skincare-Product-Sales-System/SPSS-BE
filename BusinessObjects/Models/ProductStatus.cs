﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ProductStatus
{
    public Guid Id { get; set; }
    public string StatusName { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}