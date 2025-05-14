using System.Xml;

namespace HomeFinancial.OfxParser;

/// <summary>
/// Класс расширений для XmlReader
/// </summary>
public static class XmlReaderExtensions
{
    /// <summary>
    /// Пытается переместить указатель потока на элемент с указанным тегом на правильном уровне вложенности
    /// и перемещает его на следующий элемент. Не генерирует исключение, если элемент не найден.
    /// </summary>
    /// <param name="reader">XML reader для чтения</param>
    /// <param name="element">Тег искомого элемента</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>True, если элемент найден и указатель перемещен, иначе False</returns>
    public static async Task<bool> TrySkipToElementAsync(
        this XmlReader reader,
        OfxTag element,
        CancellationToken ct = default)
    {
        do
        {
            if (reader.Depth < element.TagDepth)
            {
                return false;
            }
            
            if (reader.Name == element.TagName && 
                reader.NodeType == XmlNodeType.Element && 
                reader.Depth == element.TagDepth)
            {
                ct.ThrowIfCancellationRequested();
                await reader.ReadAsync();
                return true;
            }

            ct.ThrowIfCancellationRequested();
            
        } while (await reader.ReadAsync());

        return false;
    }

    /// <summary>
    /// Перемещает указатель потока на элемент с указанным именем и на правильном уровне вложенности,
    /// после чего перемещает указатель на следующий элемент. Если элемент не найден, генерирует исключение.
    /// </summary>
    /// <param name="reader">XML reader для чтения</param>
    /// <param name="element">Тег искомого элемента</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <exception cref="InvalidDataException">Если элемент не найден или находится на неправильном уровне вложенности</exception>
    public static async Task SkipToElementAsync(
        this XmlReader reader,
        OfxTag element,
        CancellationToken ct = default)
    {
        if(!await TrySkipToElementAsync(reader, element, ct))
        {
            throw new InvalidDataException(
                $"Элемент <{element.TagName}> не найден на уровне {element.TagDepth}.");
        }
    }
    
    /// <summary>
    /// Читает содержимое элемента как строку, проверяя, что текущий элемент соответствует указанному тегу.
    /// </summary>
    /// <param name="reader">XML reader для чтения</param>
    /// <param name="element">Тег искомого элемента</param>
    /// <param name="ct">Токен отмены операции</param>
    /// <returns>Содержимое элемента как строка</returns>
    /// <exception cref="InvalidDataException">Если текущий элемент не соответствует указанному тегу или уровню вложенности</exception>
    public static async Task<string> ReadElementContentAsStringAsync(
        this XmlReader reader,
        OfxTag element,
        CancellationToken ct = default)
    {
        if (reader.Name == element.TagName &&
            reader.NodeType == XmlNodeType.Element &&
            reader.Depth == element.TagDepth)
        {
            ct.ThrowIfCancellationRequested();

            return await reader.ReadElementContentAsStringAsync();
        }
        
        throw new InvalidDataException(
            $"Элемент <{element.TagName}> не найден на уровне {element.TagDepth}.");
    }
}