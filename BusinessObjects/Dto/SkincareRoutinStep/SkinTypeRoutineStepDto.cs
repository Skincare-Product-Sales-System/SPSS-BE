using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Dto.SkinType;

namespace BusinessObjects.Dto.SkincareRoutinStep
{
    public class SkinTypeRoutineStepDto
    {
        public string StepName { get; set; } // Tên bước
        public ProductCategoryOverviewDto Category { get; set; } // Tên danh mục
        public string Instruction { get; set; } // Hướng dẫn
        public int Order { get; set; } // Thứ tự bước
        public List<ProductForQuizResultDto> Products { get; set; } // Danh sách sản phẩm
    }
}
