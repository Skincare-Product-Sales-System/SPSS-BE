namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonForUpdateDto
    {
        public string Description { get; set; } = null!;

        public decimal RefundRate { get; set; }
    }
}
