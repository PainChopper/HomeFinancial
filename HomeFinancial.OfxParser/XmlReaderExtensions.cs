using System.Xml;

namespace HomeFinancial.OfxParser;

public static class XmlReaderExtensions
{
    /// <summary>
    /// Читает текстовое содержимое текущего элемента, оставляя ридер на закрывающем теге.
    /// </summary>
    public static async Task<string?> ReadElementTextAndStayOnEndTagAsync(this XmlReader reader)
    {
        if (reader.NodeType != XmlNodeType.Element)
            throw new InvalidOperationException("Reader должен быть на элементе!");

        // Переходим на текстовое содержимое
        if (!await reader.ReadAsync())
            return null;

        string? value = null;
        if (reader.NodeType == XmlNodeType.Text)
        {
            value = reader.Value;
            // reader теперь на тексте, следующий ReadAsync() будет на закрывающем теге
        }
        // Если нет текста (например, элемент пустой), value останется null
        return value;
    }
}
