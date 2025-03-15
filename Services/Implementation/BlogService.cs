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
            .Include(bi => bi.BlogImages)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {id} not found.");

        // Map thủ công từ Blog entity sang BlogWithDetailDto
        var blogDto = new BlogWithDetailDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Thumbnail = blog.Thumbnail,
            UserId = blog.UserId,
            BlogImages = blog.BlogImages?.Select(bi => bi.ImageUrl).ToList() ?? new List<string>()
        };

        return blogDto;
    }
    public async Task<PagedResponse<BlogDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (blogs, totalCount) = await _unitOfWork.Blogs.GetPagedAsync(
            pageNumber,
            pageSize,
            b => !b.IsDeleted // Only active blogs
        );

        var blogDtos = _mapper.Map<IEnumerable<BlogDto>>(blogs);

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
                CreatedTime = DateTimeOffset.UtcNow,
                LastUpdatedTime = DateTimeOffset.UtcNow
            });
        }

        _unitOfWork.Blogs.Add(blog);
        await _unitOfWork.SaveChangesAsync();

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Description = blog.Description,
            Thumbnail = blog.Thumbnail,
            Sections = blog.BlogSections.Select(s => new BlogSectionDto
            {
                Subtitle = s.Subtitle,
                ContentType = s.ContentType,
                Content = s.Content,
                Order = s.Order
            }).ToList()
        };
    }

    public async Task<BlogDto> UpdateAsync(Guid blogId, BlogForUpdateDto blogForUpdateDto, Guid userId)
    {
        if (blogForUpdateDto == null)
            throw new ArgumentNullException(nameof(blogForUpdateDto), "Blog data cannot be null.");

        var blog = await _unitOfWork.Blogs.GetByIdAsync(blogId);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {blogId} not found.");

        blog.LastUpdatedTime = DateTimeOffset.UtcNow;
        blog.LastUpdatedBy = "System"; // Optionally replace with current user

        _mapper.Map(blogForUpdateDto, blog);

        _unitOfWork.Blogs.Update(blog);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BlogDto>(blog);
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
