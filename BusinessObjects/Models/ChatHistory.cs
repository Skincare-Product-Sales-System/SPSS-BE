using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ChatHistory
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string MessageContent { get; set; }
    
    public string SenderType { get; set; } // "User" or "AI"
    
    public DateTimeOffset Timestamp { get; set; }
    
    public string SessionId { get; set; } // To group messages in a conversation
    
    public string CreatedBy { get; set; }

    public string LastUpdatedBy { get; set; }

    public string? DeletedBy { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }

    public DateTimeOffset? LastUpdatedTime { get; set; }

    public DateTimeOffset? DeletedTime { get; set; }

    public bool IsDeleted { get; set; }
    
    // Navigation property
    public virtual User User { get; set; }
}