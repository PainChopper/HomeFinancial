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

        if (!await reader.ReadAsync())
            return null;

        return reader.NodeType == XmlNodeType.Text 
            ? reader.Value 
            : null;
    }
}
