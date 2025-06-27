using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Application.Interfaces;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Сессия импорта файла с управлением жизненным циклом
/// </summary>
public sealed class FileImportSession : IAsyncDisposable
{
    private readonly IFileRepository _fileRepository;
    private readonly ILeaseService _leaseService;
    private bool _isCompleted;
    
    /// <summary>
    /// Импортируемый файл
    /// </summary>
    public BankFile File { get; }
    
    /// <summary>
    /// Идентификатор лиза на файл
    /// </summary>
    private Guid LeaseId { get; }

    /// <summary>
    /// Создает новый экземпляр сессии импорта файла
    /// </summary>
    /// <param name="file">Импортируемый файл</param>
    /// <param name="leaseId">Идентификатор лиза</param>
    /// <param name="fileRepository">Репозиторий файлов</param>
    /// <param name="leaseService">Сервис управления лизами</param>
    public FileImportSession(
        BankFile file, 
        Guid leaseId, 
        IFileRepository fileRepository, 
        ILeaseService leaseService)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        LeaseId = leaseId;
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
        _isCompleted = false;
    }

    /// <summary>
    /// Завершает импорт: обновляет статус файла и освобождает лиз
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    public async Task CompleteAsync(CancellationToken ct)
    {
        if (_isCompleted)
        {
            return;
        }
        
        File.Status = BankFileStatus.Completed;
        await _fileRepository.UpdateAsync(File, ct);
        await _leaseService.ReleaseLeaseAsync(File.FileName, LeaseId);
        _isCompleted = true;
    }

    /// <summary>
    /// Освобождает лиз, если сессия не была завершена явно
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_isCompleted)
        {
            await _leaseService.ReleaseLeaseAsync(File.FileName, LeaseId);
        }
    }
}
