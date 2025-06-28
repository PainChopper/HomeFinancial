using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Application.Common;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Сессия импорта файла с управлением жизненным циклом
/// </summary>
public sealed class ImportSession : IAsyncDisposable
{
    private readonly IFileRepository _fileRepository;
    private readonly ILeaseService _leaseService;
    private bool _isCompleted;
    private bool _isDisposed;
    
    /// <summary>
    /// Импортируемый файл
    /// </summary>
    public BankFile File { get; }
    
    /// <summary>
    /// Идентификатор лиза на файл
    /// </summary>
    private Guid LeaseId { get; }

    /// <summary>
    /// Создает новый экземпляр сессии импорта файла с проверкой и созданием BankFile
    /// </summary>
    /// <param name="fileName">Имя файла для импорта</param>
    /// <param name="fileRepository">Репозиторий файлов</param>
    /// <param name="leaseService">Сервис управления лизами</param>
    /// <param name="dateTimeProvider">Провайдер текущего времени</param>
    /// <param name="ct">Токен отмены</param>
    public static async Task<ImportSession> CreateAsync(
        string fileName,
        IFileRepository fileRepository,
        ILeaseService leaseService,
        IDateTimeProvider dateTimeProvider,
        CancellationToken ct)
    {
        fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
        dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        
        var leaseId = await leaseService.AcquireLeaseAsync(fileName, TimeSpan.FromMinutes(1));

        try
        {
            var existing = await fileRepository.GetByFileNameAsync(fileName);
            if (existing != null)
            {
                if (existing.Status == BankFileStatus.Completed)
                {
                    await leaseService.ReleaseLeaseAsync(existing.FileName, leaseId);
                    throw new InvalidOperationException($"Файл '{existing.FileName}' уже импортирован.");
                }

                await fileRepository.DeleteAsync(existing.Id, ct);
            }

            var file = new BankFile
            {
                FileName = fileName,
                ImportedAt = dateTimeProvider.UtcNow,
                Status = BankFileStatus.InProgress
            };
            file = await fileRepository.CreateAsync(file, ct);

            return new ImportSession(file, leaseId, fileRepository, leaseService);
        }
        catch
        {
            // При возникновении исключения освобождаем лиз
            await leaseService.ReleaseLeaseAsync(fileName, leaseId);
            throw;
        }
    }

    /// <summary>
    /// Создает новый экземпляр сессии импорта файла
    /// </summary>
    /// <param name="file">Импортируемый файл</param>
    /// <param name="leaseId">Идентификатор лиза</param>
    /// <param name="fileRepository">Репозиторий файлов</param>
    /// <param name="leaseService">Сервис управления лизами</param>
    private ImportSession(
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
        _isDisposed = false;
    }

    /// <summary>
    /// Завершает импорт: обновляет статус файла и освобождает лиз
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="InvalidOperationException">Если сессия уже была освобождена через DisposeAsync</exception>
    public async Task CompleteAsync(CancellationToken ct)
    {
        if (_isCompleted)
        {
            return;
        }
        
        if (_isDisposed)
        {
            throw new InvalidOperationException("Невозможно завершить сессию импорта, которая уже была освобождена.");
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
        if (!_isCompleted && !_isDisposed)
        {
            await _leaseService.ReleaseLeaseAsync(File.FileName, LeaseId);
        }
        
        _isDisposed = true;
    }
}
