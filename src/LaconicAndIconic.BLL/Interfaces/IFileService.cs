using Microsoft.AspNetCore.Http;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    void DeleteFile(string filePath);
}
