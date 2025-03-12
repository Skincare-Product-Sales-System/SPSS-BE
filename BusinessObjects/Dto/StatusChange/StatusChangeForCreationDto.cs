namespace BusinessObjects.Dto.StatusChange
{
    public class StatusChangeForCreationDto
    {
        public DateTimeOffset Date { get; set; }
        public string Status { get; set; }
        public Guid OrderId { get; set; }
    }
}
