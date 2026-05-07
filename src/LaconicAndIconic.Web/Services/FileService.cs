using LaconicAndIconic.BLL.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LaconicAndIconic.Web.Models;
using Microsoft.Extensions.Logging;

namespace LaconicAndIconic.Web.Services;


public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly AppSettings _appSettings;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IWebHostEnvironment env,
        IOptions<AppSettings> appSettings,
        ILogger<FileService> logger)
    {
        _env = env;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File is null or empty in SaveFileAsync");
            throw new ArgumentException("File cannot be empty", nameof(file));
        }
        if (string.IsNullOrWhiteSpace(subFolder))
        {
            _logger.LogWarning("Subfolder is null or empty in SaveFileAsync");
            throw new ArgumentException("Subfolder cannot be empty", nameof(subFolder));
        }

        // Приклад використання _appSettings.PageSize (можна використати для обмеження кількості файлів у папці, якщо потрібно)
        var uploadsFolder = Path.Combine(_env.WebRootPath, "images", subFolder);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        _logger.LogInformation("File saved: {FilePath}", filePath);
        return $"/images/{subFolder}/{uniqueFileName}";
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogWarning("DeleteFile called with empty path");
            return;
        }

        var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/').Replace("/", "\\", StringComparison.Ordinal));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {FilePath}", fullPath);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
        }
    }
}
