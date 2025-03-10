namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonDto
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public decimal RefundRate { get; set; }
    }
}
