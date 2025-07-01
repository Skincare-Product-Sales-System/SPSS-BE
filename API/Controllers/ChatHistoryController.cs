using Services.Dto.Api;
using BusinessObjects.Dto.ChatHistory;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Extensions;

namespace API.Controllers
{
    [Route("api/chat-history")]
    [ApiController]
    public class ChatHistoryController : ControllerBase
    {
        private readonly IChatHistoryService _chatHistoryService;

        public ChatHistoryController(IChatHistoryService chatHistoryService)
        {
            _chatHistoryService = chatHistoryService;
        }

        [HttpGet("user/{userId}")]
        [CustomAuthorize("Customer,Manager")]
        public async Task<IActionResult> GetChatHistoryByUserId(Guid userId, [FromQuery] int limit = 100)
        {
            try
            {
                var history = await _chatHistoryService.GetChatHistoryByUserIdAsync(userId, limit);
                return Ok(ApiResponse<IEnumerable<ChatHistoryDto>>.SuccessResponse(history));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        [HttpGet("session/{sessionId}")]
        [CustomAuthorize("Customer,Manager")]
        public async Task<IActionResult> GetChatSessionById(string sessionId)
        {
            try
            {
                var session = await _chatHistoryService.GetChatSessionAsync(sessionId);
                return Ok(ApiResponse<IEnumerable<ChatHistoryDto>>.SuccessResponse(session));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        [HttpGet("sessions/{userId}")]
        [CustomAuthorize("Customer,Manager")]
        public async Task<IActionResult> GetRecentSessionsForUser(Guid userId, [FromQuery] int maxSessions = 10)
        {
            try
            {
                var sessionIds = await _chatHistoryService.GetRecentSessionsIdsAsync(userId, maxSessions);
                return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(sessionIds));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message));
            }
        }

        [HttpPost]
        [CustomAuthorize("Customer,Manager")]
        public async Task<IActionResult> SaveChatMessage([FromBody] ChatHistoryForCreationDto chatMessage)
        {
            try
            {
                var savedMessage = await _chatHistoryService.SaveChatMessageAsync(chatMessage);
                return Ok(ApiResponse<ChatHistoryDto>.SuccessResponse(savedMessage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(ex.Message));
            }
        }
    }
}