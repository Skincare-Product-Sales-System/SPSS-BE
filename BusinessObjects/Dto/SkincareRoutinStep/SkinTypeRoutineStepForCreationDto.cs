namespace BusinessObjects.Dto.SkincareRoutinStep
{
    public class SkinTypeRoutineStepForCreationDto
    {
        public string StepName { get; set; } // Tên bước
        public string Instruction { get; set; } // Hướng dẫn
        public Guid CategoryId { get; set; } // ID danh mục
        public int Order { get; set; } // Thứ tự
    }
}
