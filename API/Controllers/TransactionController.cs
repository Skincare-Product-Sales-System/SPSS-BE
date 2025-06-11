using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BusinessObjects.Dto.Transaction;
using API.Extensions;
using BusinessObjects.Dto.Account;
using Services.Dto.Api;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IHubContext<TransactionHub> _transactionHubContext;

        public TransactionController(
            ITransactionService transactionService,
            IHubContext<TransactionHub> transactionHubContext)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _transactionHubContext = transactionHubContext ?? throw new ArgumentNullException(nameof(transactionHubContext));
        }

        [CustomAuthorize("Customer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Transaction details cannot be empty"));
                }

                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                var transaction = await _transactionService.CreateTransactionAsync(dto, userId.Value);
                
                // G?i thông báo t?i admin v? giao d?ch m?i
                await _transactionHubContext.Clients.All.SendAsync("NewTransaction", transaction);
                
                return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, "Transaction created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error creating transaction", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Customer")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving transaction", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Customer")]
        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<TransactionDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetUserTransactions()
        {
            try
            {
                // Get user ID from context
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("User ID is missing or invalid"));
                }

                var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId.Value);
                return Ok(ApiResponse<IEnumerable<TransactionDto>>.SuccessResponse(transactions));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving user transactions", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Manager")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PagedResponse<TransactionDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> GetPagedTransactions(
            [Range(1, int.MaxValue)] int pageNumber = 1,
            [Range(1, 100)] int pageSize = 10,
            string status = null)
        {
            try
            {
                var pagedTransactions = await _transactionService.GetPagedTransactionsAsync(pageNumber, pageSize, status);
                return Ok(ApiResponse<PagedResponse<TransactionDto>>.SuccessResponse(pagedTransactions));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error retrieving transactions", new List<string> { ex.Message }));
            }
        }

        [CustomAuthorize("Manager")]
        [HttpPut("status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> UpdateTransactionStatus([FromBody] UpdateTransactionStatusDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse("Update details cannot be empty"));
                }

                // Get admin ID from context
                Guid? adminId = HttpContext.Items["UserId"] as Guid?;
                if (adminId == null)
                {
                    return BadRequest(ApiResponse<AccountDto>.FailureResponse("Admin ID is missing or invalid"));
                }

                var transaction = await _transactionService.UpdateTransactionStatusAsync(dto, adminId.Value.ToString());
                
                // G?i thông báo v? tr?ng thái giao d?ch ?ã ???c c?p nh?t
                await _transactionHubContext.Clients.All.SendAsync("TransactionUpdated", transaction);
                
                return Ok(ApiResponse<TransactionDto>.SuccessResponse(transaction, 
                    $"Transaction status updated to {dto.Status} successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error updating transaction status", new List<string> { ex.Message }));
            }
        }
        
        /// <summary>
        /// Endpoint ?? ki?m tra tr?ng thái giao d?ch
        /// </summary>
        [CustomAuthorize("Customer")]
        [HttpGet("check-status/{transactionId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> CheckTransactionStatus(Guid transactionId)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(transactionId);
                return Ok(ApiResponse<string>.SuccessResponse(transaction.Status, "Transaction status retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.FailureResponse("Error checking transaction status", new List<string> { ex.Message }));
            }
        }
    }
}