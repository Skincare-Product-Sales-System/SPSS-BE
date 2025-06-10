using System;

namespace BusinessObjects.Dto.Transaction
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public string UserName { get; set; }
        
        public string TransactionType { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Status { get; set; }
        
        public string QrImageUrl { get; set; }
        
        public string BankInformation { get; set; }
        
        public string Description { get; set; }
        
        public DateTimeOffset CreatedTime { get; set; }
        
        public DateTimeOffset LastUpdatedTime { get; set; }
        
        public DateTimeOffset? ApprovedTime { get; set; }
    }

    public class CreateTransactionDto
    {
        public string TransactionType { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Description { get; set; }
    }

    public class UpdateTransactionStatusDto
    {
        public Guid TransactionId { get; set; }
        
        public string Status { get; set; } // "Approved" or "Rejected"
    }
}