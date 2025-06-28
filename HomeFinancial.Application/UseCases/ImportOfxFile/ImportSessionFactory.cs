using HomeFinancial.Application.Common;
using HomeFinancial.Application.Interfaces;
using HomeFinancial.Domain.Repositories;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Сервис, создающий сессии импорта файла
/// </summary>
internal sealed class ImportSessionFactory : IImportSessionFactory
{
    private readonly IFileRepository _fileRepository;
    private readonly ILeaseService _leaseService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Создает новый экземпляр сервиса создания сессий импорта файлов
    /// </summary>
    public ImportSessionFactory(
        IFileRepository fileRepository, 
        ILeaseService leaseService, 
        IDateTimeProvider dateTimeProvider)
    {
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    /// <inheritdoc />
    public Task<ImportSession> StartAsync(string fileName, CancellationToken ct)
    {
        fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        
        return ImportSession.CreateAsync(
            fileName, 
            _fileRepository, 
            _leaseService, 
            _dateTimeProvider, 
            ct);
    }
}
