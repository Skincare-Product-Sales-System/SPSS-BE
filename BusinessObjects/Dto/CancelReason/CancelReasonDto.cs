namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonDto
    {
        public Guid Id { get; set; }

        public string Description { get; set; } = null!;

        public decimal RefundRate { get; set; }
    }
}
