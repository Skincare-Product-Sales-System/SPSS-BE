namespace BusinessObjects.Dto.StatusChange
{
    public class StatusChangeForCreationDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Status { get; set; }
        public Guid OrderId { get; set; }
    }
}
