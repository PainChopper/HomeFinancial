// Mappers.cs
using HomeFinancial.DTOs;
using HomeFinancial.Data.Models;
using Riok.Mapperly.Abstractions;

namespace HomeFinancial.Mapping
{
    [Mapper]
    public partial class AppMapper
    {
        public partial CategoryDto CategoryToDto(Category category);
        public partial Category DtoToCategory(CreateCategoryDto dto);

        public partial BankTransactionDto BankTransactionToDto(BankTransaction transaction);
        public partial BankTransaction DtoToBankTransaction(CreateBankTransactionDto dto, int categoryId);

        public partial ImportedFileDto ImportedFileToDto(ImportedFile file);
        public partial ImportedFile DtoToImportedFile(CreateImportedFileDto dto);
    }
}