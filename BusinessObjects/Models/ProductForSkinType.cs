﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ProductForSkinType
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SkinTypeId { get; set; }
    public virtual Product Product { get; set; }
    public virtual SkinType SkinType { get; set; }
}