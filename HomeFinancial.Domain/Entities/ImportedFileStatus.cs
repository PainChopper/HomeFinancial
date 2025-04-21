namespace HomeFinancial.Domain.Entities
{
    /// <summary>
    /// Статус импортированного файла (отражает этап обработки)
    /// </summary>
    public enum ImportedFileStatus
    {
        /// <summary>
        /// Файл в процессе обработки
        /// </summary>
        Processing = 0,

        /// <summary>
        /// Файл успешно обработан
        /// </summary>
        Processed = 1,

        /// <summary>
        /// При обработке файла возникла ошибка
        /// </summary>
        Error = 2
    }
}
