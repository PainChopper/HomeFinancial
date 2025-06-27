using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Value-object сессии импорта файла.
/// </summary>
public readonly record struct ImportFileSession(BankFile File, Guid LeaseId);