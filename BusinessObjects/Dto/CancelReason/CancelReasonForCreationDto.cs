namespace BusinessObjects.Dto.CancelReason
{
    public class CancelReasonForCreationDto
    {
        public string Description { get; set; } = null!;

        public decimal RefundRate { get; set; }
    }
}
