using AutoMapper;
using BusinessObjects.Dto.Blog;
using BusinessObjects.Models;
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

    public async Task<BlogDto> GetByIdAsync(Guid id)
    {
        var blog = await _unitOfWork.Blogs.GetByIdAsync(id);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {id} not found.");

        return _mapper.Map<BlogDto>(blog);
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

    public async Task<BlogDto> CreateAsync(BlogForCreationDto? blogForCreationDto)
    {
        if (blogForCreationDto == null)
            throw new ArgumentNullException(nameof(blogForCreationDto), "Blog data cannot be null.");

        var blog = _mapper.Map<Blog>(blogForCreationDto);

        blog.CreatedTime = DateTimeOffset.UtcNow;
        blog.CreatedBy = "System"; // Optionally replace with current user
        blog.IsDeleted = false;

        _unitOfWork.Blogs.Add(blog);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BlogDto>(blog);
    }

    public async Task<BlogDto> UpdateAsync(Guid blogId, BlogForUpdateDto blogForUpdateDto)
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

    public async Task DeleteAsync(Guid id)
    {
        var blog = await _unitOfWork.Blogs.GetByIdAsync(id);

        if (blog == null || blog.IsDeleted)
            throw new KeyNotFoundException($"Blog with ID {id} not found.");

        blog.IsDeleted = true;
        blog.DeletedTime = DateTimeOffset.UtcNow;
        blog.DeletedBy = "System"; // Optionally replace with current user

        _unitOfWork.Blogs.Update(blog);
        await _unitOfWork.SaveChangesAsync();
    }
}
