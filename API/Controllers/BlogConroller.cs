using BusinessObjects.Dto.Blog;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Response;
using System.ComponentModel.DataAnnotations;
using Services.Dto.Api;

namespace API.Controllers;

[ApiController]
[Route("api/blogs")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var blog = await _blogService.GetByIdAsync(id);
            return Ok(ApiResponse<BlogDto>.SuccessResponse(blog));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BlogDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([Range(1, int.MaxValue)] int pageNumber = 1, [Range(1, 100)] int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PagedResponse<BlogDto>>.FailureResponse("Invalid pagination parameters", errors));
        }

        var pagedBlogs = await _blogService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResponse<BlogDto>>.SuccessResponse(pagedBlogs));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] BlogForCreationDto blogDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<BlogDto>.FailureResponse("Invalid blog data", errors));
        }

        try
        {
            var createdBlog = await _blogService.CreateAsync(blogDto);
            return CreatedAtAction(nameof(GetById), new { id = createdBlog.Id }, ApiResponse<BlogDto>.SuccessResponse(createdBlog));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BlogDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] BlogForUpdateDto blogDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<BlogDto>.FailureResponse("Invalid blog data", errors));
        }

        try
        {
            await _blogService.UpdateAsync(id, blogDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BlogDto>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BlogDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _blogService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<BlogDto>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BlogDto>.FailureResponse(ex.Message));
        }
    }
}
