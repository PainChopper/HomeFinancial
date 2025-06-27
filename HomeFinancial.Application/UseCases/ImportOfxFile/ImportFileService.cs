using HomeFinancial.Application.Common;
using HomeFinancial.Application.Dtos;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Entities;
using HomeFinancial.Domain.Repositories;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Сервис, создающий сеанс импорта файла.
/// </summary>
internal sealed class ImportFileService : IImportFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly ILeaseService _leaseService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ImportFileService(IFileRepository fileRepository, ILeaseService leaseService, IDateTimeProvider dateTimeProvider)
    {
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ImportFileSession> StartAsync(string fileName, CancellationToken ct)
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

        return new ImportFileSession(file, leaseId);
    }

    public async Task ValidateAndExtendAsync(ImportFileSession session) =>
        await _leaseService.ValidateAndExtendLeaseAsync(session.File.FileName, session.LeaseId, TimeSpan.FromMinutes(1));

    public async Task CompleteAsync(ImportFileSession session, CancellationToken ct)
    {
        session.File.Status = BankFileStatus.Completed;
        await _fileRepository.UpdateAsync(session.File, ct);
        await _leaseService.ReleaseLeaseAsync(session.File.FileName, session.LeaseId);
    }

    public async Task ReleaseAsync(ImportFileSession session) =>
        await _leaseService.ReleaseLeaseAsync(session.File.FileName, session.LeaseId);
}