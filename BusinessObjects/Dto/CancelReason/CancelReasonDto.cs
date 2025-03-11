namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonDto
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public decimal RefundRate { get; set; }
        public string CreatedBy { get; set; }

        public string LastUpdatedBy { get; set; }

        public string DeletedBy { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public DateTimeOffset? LastUpdatedTime { get; set; }

        public DateTimeOffset? DeletedTime { get; set; }

        public bool IsDeleted { get; set; }
    }
}
