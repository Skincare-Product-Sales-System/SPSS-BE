using AutoMapper;
using BusinessObjects.Dto.Blog;
using BusinessObjects.Dto.BlogImage;
using BusinessObjects.Dto.BlogSection;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation;

public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BlogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BlogWithDetailDto> GetByIdAsync(Guid id)
    {
        var blogQuery = await _unitOfWork.Blogs.GetQueryableAsync();
        var blog = await blogQuery
            .Include(b => b.User) // Include User để lấy thông tin tác giả
            .Include(bi => bi.BlogSections)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {id} not found.");

        // Map thủ công từ Blog entity sang BlogWithDetailDto
        var blogDto = new BlogWithDetailDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Thumbnail = blog.Thumbnail,
            Description = blog.Description, // Nếu blog có nội dung mô tả chung
            Author = blog.User?.UserName, // Nếu có quan hệ với User để lấy tên tác giả
            LastUpdatedAt = blog.LastUpdatedTime,
            Sections = blog.BlogSections
                .OrderBy(bs => bs.Order) // Đảm bảo sắp xếp các section theo thứ tự
                .Select(bs => new BlogSectionDto
                {
                    ContentType = bs.ContentType,
                    Subtitle = bs.Subtitle,
                    Content = bs.Content,
                    Order = bs.Order
                })
                .ToList()
        };

        return blogDto;
    }

    public async Task<PagedResponse<BlogDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        // Tính tổng số blog chưa bị xóa
        var totalCount = await _unitOfWork.Blogs.Entities
            .Where(b => !b.IsDeleted)
            .CountAsync();

        // Lấy danh sách blog theo phân trang, bao gồm thông tin User
        var blogs = await _unitOfWork.Blogs.Entities
            .Include(b => b.User) // Bao gồm thông tin User
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.LastUpdatedTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map thủ công từng đối tượng Blog sang BlogDto
        var blogDtos = blogs.Select(b => new BlogDto
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            Thumbnail = b.Thumbnail,
            LastUpdatedTime = b.LastUpdatedTime,
            AuthorName = $"{b.User?.SurName} {b.User?.LastName}"
        }).ToList();

        // Trả về kết quả phân trang
        return new PagedResponse<BlogDto>
        {
            Items = blogDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<BlogDto> CreateBlogAsync(BlogForCreationDto blogDto, Guid userId)
    {
        if (blogDto == null)
            throw new ArgumentNullException(nameof(blogDto));

        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = blogDto.Title,
            Description = blogDto.Description,
            Thumbnail = blogDto.Thumbnail,
            UserId = userId,
            CreatedTime = DateTimeOffset.UtcNow,
            LastUpdatedTime = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastUpdatedBy = userId.ToString(),
            IsDeleted = false,
        };

        foreach (var sectionDto in blogDto.Sections.OrderBy(s => s.Order))
        {
            blog.BlogSections.Add(new BlogSection
            {
                Id = Guid.NewGuid(),
                ContentType = sectionDto.ContentType,
                Subtitle = sectionDto.Subtitle,
                Content = sectionDto.Content,
                Order = sectionDto.Order,
                BlogId = blog.Id
            });
        }

        _unitOfWork.Blogs.Add(blog);
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi chi tiết
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Description = blog.Description,
            Thumbnail = blog.Thumbnail,
            LastUpdatedTime = blog.LastUpdatedTime
        };
    }

    public async Task<BlogDto> UpdateBlogAsync(Guid blogId, BlogForUpdateDto blogDto, Guid userId)
    {
        if (blogDto == null)
            throw new ArgumentNullException(nameof(blogDto));

        var blogQuery = await _unitOfWork.Blogs.GetQueryableAsync();
        var blog = await blogQuery
            .Include(b => b.BlogSections)
            .FirstOrDefaultAsync(b => b.Id == blogId);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {blogId} not found.");

        // Cập nhật thông tin chính của blog
        blog.Title = blogDto.Title;
        blog.Description = blogDto.Description;
        blog.Thumbnail = blogDto.Thumbnail;
        blog.LastUpdatedTime = DateTimeOffset.UtcNow;
        blog.LastUpdatedBy = userId.ToString();

        // Cập nhật BlogSections một cách tường minh
        var existingSections = blog.BlogSections.ToList();
        var sectionIdsInDto = blogDto.Sections.Select(s => s.Id).ToHashSet();

        // Xóa các mục không còn trong DTO
        foreach (var section in existingSections)
        {
            if (!sectionIdsInDto.Contains(section.Id))
            {
                _unitOfWork.BlogSections.Delete(section);
            }
        }

        // Thêm hoặc cập nhật các mục từ DTO
        foreach (var sectionDto in blogDto.Sections)
        {
            var existingSection = existingSections.FirstOrDefault(s => s.Id == sectionDto.Id);
            if (existingSection != null)
            {
                existingSection.ContentType = sectionDto.ContentType;
                existingSection.Subtitle = sectionDto.Subtitle;
                existingSection.Content = sectionDto.Content;
                existingSection.Order = sectionDto.Order;
            }
            else
            {
                var newSection = new BlogSection
                {
                    Id = Guid.NewGuid(),
                    BlogId = blog.Id,
                    ContentType = sectionDto.ContentType,
                    Subtitle = sectionDto.Subtitle,
                    Content = sectionDto.Content,
                    Order = sectionDto.Order
                };
                _unitOfWork.BlogSections.Add(newSection);
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Concurrency conflict detected while updating the blog.", ex);
        }

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Description = blog.Description,
            Thumbnail = blog.Thumbnail,
            LastUpdatedTime = blog.LastUpdatedTime
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var blog = await _unitOfWork.Blogs.GetByIdAsync(id);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {id} not found.");

        blog.IsDeleted = true;
        blog.DeletedTime = DateTimeOffset.UtcNow;
        blog.DeletedBy = userId.ToString();

        _unitOfWork.Blogs.Update(blog);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
