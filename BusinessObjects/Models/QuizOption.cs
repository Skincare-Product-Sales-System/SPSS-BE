﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class QuizOption : BaseEntity
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string Value { get; set; }

    public int Score { get; set; }

    public Guid? QuizQuestionId { get; set; }
    public virtual QuizQuestion QuizQuestion { get; set; }
}