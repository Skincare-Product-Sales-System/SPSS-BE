namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonForUpdateDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }

        public decimal RefundRate { get; set; }
    }
}
