using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public string TransactionType { get; set; } // "SkinAnalysis", "Order", etc.
        
        public decimal Amount { get; set; }
        
        public string Status { get; set; } // "Pending", "Approved", "Rejected"
        
        public string QrImageUrl { get; set; } // URL to the QR code image
        
        public string BankInformation { get; set; } // Bank account information for the transfer
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string CreatedBy { get; set; }
        
        public string LastUpdatedBy { get; set; }
        
        public string? ApprovedBy { get; set; }
        
        public DateTimeOffset CreatedTime { get; set; }
        
        public DateTimeOffset LastUpdatedTime { get; set; }
        
        public DateTimeOffset? ApprovedTime { get; set; }
        
        public bool IsDeleted { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
    }
}