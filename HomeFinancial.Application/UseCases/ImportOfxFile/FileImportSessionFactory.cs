using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Сервис, создающий сессии импорта файла
/// </summary>
internal sealed class FileImportSessionFactory : IFileImportSessionFactory
{
    private readonly IFileRepository _fileRepository;
    private readonly ILeaseService _leaseService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Создает новый экземпляр сервиса создания сессий импорта файлов
    /// </summary>
    public FileImportSessionFactory(
        IFileRepository fileRepository, 
        ILeaseService leaseService, 
        IDateTimeProvider dateTimeProvider)
    {
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    /// <inheritdoc />
    public async Task<FileImportSession> StartAsync(string fileName, CancellationToken ct)
    {
        var leaseId = await _leaseService.AcquireLeaseAsync(fileName, TimeSpan.FromMinutes(1));

        var existing = await _fileRepository.GetByFileNameAsync(fileName);
        if (existing != null)
        {
            if (existing.Status == BankFileStatus.Completed)
            {
                await _leaseService.ReleaseLeaseAsync(existing.FileName, leaseId);
                throw new InvalidOperationException($"Файл '{existing.FileName}' уже импортирован.");
            }

            await _fileRepository.DeleteAsync(existing.Id, ct);
        }

        var file = new BankFile
        {
            FileName = fileName,
            ImportedAt = _dateTimeProvider.UtcNow,
            Status = BankFileStatus.InProgress
        };
        file = await _fileRepository.CreateAsync(file, ct);

        return new FileImportSession(file, leaseId, _fileRepository, _leaseService);
    }
}
