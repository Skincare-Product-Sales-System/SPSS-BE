﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class SkinType
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ICollection<SkinTypeRoutineStep> SkinTypeRoutineSteps { get; set; } = new List<SkinTypeRoutineStep>();

    public virtual ICollection<User> AspNetUsers { get; set; } = new List<User>();

    public virtual ICollection<ProductForSkinType> ProductForSkinTypes { get; set; } = new List<ProductForSkinType>();

    public virtual ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
}