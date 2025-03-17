namespace BusinessObjects.Models
{
    public partial class SkinTypeRoutineStep
    {
        public Guid Id { get; set; }
        public Guid SkinTypeId { get; set; } // Gắn với loại da
        public Guid CategoryId { get; set; } // Gắn với danh mục sản phẩm

        public string StepName { get; set; } // Tên bước (ví dụ: "Cleanser", "Moisturizer")
        public string Instruction { get; set; } // Hướng dẫn cụ thể cho bước này
        public int Order { get; set; } // Thứ tự bước
        public virtual SkinType SkinType { get; set; }
        public virtual ProductCategory Category { get; set; } // Quan hệ với danh mục
    }

}
