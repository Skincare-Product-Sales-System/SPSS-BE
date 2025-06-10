using BusinessObjects.Dto.Transaction;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCoder;
using System.Drawing;
using System.IO;

namespace Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionService> _logger;
        private readonly ManageFirebaseImage.ManageFirebaseImageService _firebaseImageService;

        // Banking information read from configuration
        private readonly string _bankInformation;

        public TransactionService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _firebaseImageService = new ManageFirebaseImage.ManageFirebaseImageService();
            
            // Load banking information from configuration
            var bankName = _configuration["Banking:BankName"] ?? "MBBANK";
            var accountNumber = _configuration["Banking:AccountNumber"] ?? "0358696560";
            var accountName = _configuration["Banking:AccountName"] ?? "NGUYEN NGOC SON";
            var branch = _configuration["Banking:Branch"] ?? "";
            
            _bankInformation = $"Ngân hàng: {bankName}\nSố tài khoản: {accountNumber}\nChủ tài khoản: {accountName}";
            if (!string.IsNullOrEmpty(branch))
            {
                _bankInformation += $"\nChi nhánh: {branch}";
            }
        }

        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating transaction for user {UserId}", userId);
                
                // Generate QR code for payment
                string qrCodeUrl = await GenerateQrCodeAsync(dto.Amount, dto.Description);
                
                // Create new transaction
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TransactionType = dto.TransactionType,
                    Amount = dto.Amount,
                    Status = "Pending",
                    QrImageUrl = qrCodeUrl,
                    BankInformation = _bankInformation,
                    Description = dto.Description,
                    CreatedBy = userId.ToString(),
                    LastUpdatedBy = userId.ToString(),
                    CreatedTime = DateTimeOffset.UtcNow,
                    LastUpdatedTime = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };
                
                _unitOfWork.Transactions.Add(transaction);
                await _unitOfWork.SaveChangesAsync();
                
                // Get user information
                var user = await _unitOfWork.Users.Entities
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }
                
                return new TransactionDto
                {
                    Id = transaction.Id,
                    UserId = transaction.UserId,
                    UserName = user.UserName,
                    TransactionType = transaction.TransactionType,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    QrImageUrl = transaction.QrImageUrl,
                    BankInformation = transaction.BankInformation,
                    Description = transaction.Description,
                    CreatedTime = transaction.CreatedTime,
                    LastUpdatedTime = transaction.LastUpdatedTime,
                    ApprovedTime = transaction.ApprovedTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<string> GenerateQrCodeAsync(decimal amount, string description)
        {
            try
            {
                // Lấy thông tin ngân hàng từ cấu hình
                var bankName = _configuration["Banking:BankName"] ?? "MBBANK";
                var accountNumber = _configuration["Banking:AccountNumber"] ?? "0358696560";
                var accountName = _configuration["Banking:AccountName"] ?? "NGUYEN NGOC SON";
                var bankId = _configuration["Banking:BankId"] ?? "970422";

                // Format số tiền (VietQR yêu cầu số tiền không có thập phân và không có dấu phân cách)
                string formattedAmount = ((long)amount).ToString();

                // Xử lý nội dung chuyển khoản - loại bỏ dấu tiếng Việt
                string normalizedDescription = RemoveVietnameseAccents(description);

                // Tạo nội dung QR code theo chuẩn VietQR mới
                string qrContent = "00020101021138";

                // Thông tin người thụ hưởng
                qrContent += "540010A000000727012400";
                qrContent += $"06{bankId}"; // Thêm mã ngân hàng với độ dài
                qrContent += $"01{accountNumber.Length:D2}{accountNumber}";

                // Thêm trường QRIBFTTA
                qrContent += "0208QRIBFTTA";

                // Thêm loại tiền tệ (704 = VND)
                qrContent += "5303704";

                // Thêm số tiền
                qrContent += $"54{formattedAmount.Length:D2}{formattedAmount}";

                // Thêm quốc gia
                qrContent += "5802VN";

                // Thêm nội dung chuyển khoản (chỉ khi có nội dung)
                if (!string.IsNullOrEmpty(normalizedDescription))
                {
                    qrContent += $"62{(normalizedDescription.Length + 4):D2}01{normalizedDescription.Length:D2}{normalizedDescription}";
                }

                // Thêm trường checksum
                qrContent += "6304";

                // Tính toán checksum theo chuẩn CRC-16
                string checksum = CalculateChecksum(qrContent);
                qrContent = qrContent.Substring(0, qrContent.Length - 4) + "6304" + checksum;

                // Generate QR code using QRCoder
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new BitmapByteQRCode(qrCodeData))
                    {
                        // Tạo QR code với kích thước lớn hơn để dễ quét
                        var qrCodeBytes = qrCode.GetGraphic(20);

                        // Chuyển sang stream để tải lên
                        using var stream = new MemoryStream(qrCodeBytes);

                        // Tải lên Firebase
                        string fileName = $"qr-codes/payment-{Guid.NewGuid()}.png";
                        string imageUrl = await _firebaseImageService.UploadFileAsync(stream, fileName);

                        return imageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code: {ErrorMessage}", ex.Message);
                throw new Exception("Failed to generate QR code for payment");
            }
        }

        /// <summary>
        /// Tính toán checksum CRC-16 cho chuỗi QR VietQR
        /// </summary>
        private string CalculateChecksum(string qrContent)
        {
            // Thực hiện CRC-16 theo chuẩn CCITT
            ushort crc = 0xFFFF;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(qrContent);

            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }

            return crc.ToString("X4").ToLower();
        }

        /// <summary>
        /// Loại bỏ dấu tiếng Việt và các ký tự đặc biệt không hợp lệ
        /// </summary>
        private string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string[] vietnameseChars = new string[]
            {
                "áàảãạâấầẩẫậăắằẳẵặ", "ÁÀẢÃẠÂẤẦẨẪẬĂẮẰẲẴẶ",
                "éèẻẽẹêếềểễệ", "ÉÈẺẼẸÊẾỀỂỄỆ",
                "íìỉĩị", "ÍÌỈĨỊ",
                "óòỏõọôốồổỗộơớờởỡợ", "ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ",
                "úùủũụưứừửữự", "ÚÙỦŨỤƯỨỪỬỮỰ",
                "ýỳỷỹỵ", "ÝỲỶỸỴ",
                "đ", "Đ"
            };

            string[] replaceChars = new string[]
            {
                "a", "A",
                "e", "E",
                "i", "I",
                "o", "O",
                "u", "U",
                "y", "Y",
                "d", "D"
            };

            string result = text;

            // Thay thế từng ký tự có dấu
            for (int i = 0; i < vietnameseChars.Length; i++)
            {
                foreach (char c in vietnameseChars[i])
                {
                    result = result.Replace(c.ToString(), replaceChars[i]);
                }
            }

            // Loại bỏ các ký tự đặc biệt không hợp lệ (chỉ giữ lại chữ cái, số và khoảng trắng)
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-zA-Z0-9\s]", "");

            return result;
        }

        public async Task<TransactionDto> GetTransactionByIdAsync(Guid id)
        {
            try
            {
                var transaction = await _unitOfWork.Transactions.GetTransactionByIdAsync(id);
                
                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {id} not found");
                }
                
                return new TransactionDto
                {
                    Id = transaction.Id,
                    UserId = transaction.UserId,
                    UserName = transaction.User.UserName,
                    TransactionType = transaction.TransactionType,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    QrImageUrl = transaction.QrImageUrl,
                    BankInformation = transaction.BankInformation,
                    Description = transaction.Description,
                    CreatedTime = transaction.CreatedTime,
                    LastUpdatedTime = transaction.LastUpdatedTime,
                    ApprovedTime = transaction.ApprovedTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(Guid userId)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetTransactionsByUserIdAsync(userId);
                
                var user = await _unitOfWork.Users.Entities
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }
                
                return transactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = user.UserName,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Status = t.Status,
                    QrImageUrl = t.QrImageUrl,
                    BankInformation = t.BankInformation,
                    Description = t.Description,
                    CreatedTime = t.CreatedTime,
                    LastUpdatedTime = t.LastUpdatedTime,
                    ApprovedTime = t.ApprovedTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user transactions: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResponse<TransactionDto>> GetPagedTransactionsAsync(int pageNumber, int pageSize, string status = null)
        {
            try
            {
                // Get all transactions or filter by status
                IEnumerable<Transaction> transactions;
                if (!string.IsNullOrEmpty(status))
                {
                    transactions = await _unitOfWork.Transactions.GetTransactionsByStatusAsync(status);
                }
                else
                {
                    transactions = await _unitOfWork.Transactions.Entities
                        .Where(t => !t.IsDeleted)
                        .Include(t => t.User)
                        .OrderByDescending(t => t.CreatedTime)
                        .ToListAsync();
                }
                
                // Calculate total count
                int totalCount = transactions.Count();
                
                // Apply pagination
                var pagedTransactions = transactions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                // Map to DTOs
                var transactionDtos = pagedTransactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = t.User?.UserName ?? "Unknown",
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Status = t.Status,
                    QrImageUrl = t.QrImageUrl,
                    BankInformation = t.BankInformation,
                    Description = t.Description,
                    CreatedTime = t.CreatedTime,
                    LastUpdatedTime = t.LastUpdatedTime,
                    ApprovedTime = t.ApprovedTime
                }).ToList();
                
                // Return paged response
                return new PagedResponse<TransactionDto>
                {
                    Items = transactionDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged transactions: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<TransactionDto> UpdateTransactionStatusAsync(UpdateTransactionStatusDto dto, string adminId)
        {
            try
            {
                // Validate status
                if (dto.Status != "Approved" && dto.Status != "Rejected")
                {
                    throw new ArgumentException("Status must be 'Approved' or 'Rejected'");
                }
                
                // Get transaction
                var transaction = await _unitOfWork.Transactions.GetTransactionByIdAsync(dto.TransactionId);
                
                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {dto.TransactionId} not found");
                }
                
                // Update transaction status
                transaction.Status = dto.Status;
                transaction.LastUpdatedBy = adminId;
                transaction.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                // Set approval information if approved
                if (dto.Status == "Approved")
                {
                    transaction.ApprovedBy = adminId;
                    transaction.ApprovedTime = DateTimeOffset.UtcNow;
                }
                
                _unitOfWork.Transactions.Update(transaction);
                await _unitOfWork.SaveChangesAsync();
                
                // Return updated transaction
                return new TransactionDto
                {
                    Id = transaction.Id,
                    UserId = transaction.UserId,
                    UserName = transaction.User?.UserName ?? "Unknown",
                    TransactionType = transaction.TransactionType,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    QrImageUrl = transaction.QrImageUrl,
                    BankInformation = transaction.BankInformation,
                    Description = transaction.Description,
                    CreatedTime = transaction.CreatedTime,
                    LastUpdatedTime = transaction.LastUpdatedTime,
                    ApprovedTime = transaction.ApprovedTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction status: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}