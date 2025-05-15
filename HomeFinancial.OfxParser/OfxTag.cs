using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace HomeFinancial.OfxParser;

/// <summary>
/// Представление тега OFX с его именем и уровнем вложенности.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[DebuggerDisplay("{TagName} (Depth={TagDepth})")]
public readonly record struct OfxTag(string TagName, int TagDepth)
{
    // Корневые и промежуточные элементы для доступа к выпискам

    public static readonly OfxTag Ofx          = new("OFX",           0);
    public static readonly OfxTag BankMsgsRsv1 = new("BANKMSGSRSV1",  1);
    public static readonly OfxTag StmtTrnRs    = new("STMTTRNRS",     2);
    public static readonly OfxTag StmTrs       = new("STMTRS",        3);

    // Элементы банковской выписки
    public static readonly OfxTag BankAcctFrom = new("BANKACCTFROM",  4);
    public static readonly OfxTag BankId       = new("BANKID",        5);
    public static readonly OfxTag AcctId       = new("ACCTID",        5);
    public static readonly OfxTag AcctType     = new("ACCTTYPE",      5);

    // Элементы списка транзакций
    public static readonly OfxTag BankTranList = new("BANKTRANLIST",  4);
    public static readonly OfxTag DtStart      = new("DTSTART",       5);
    public static readonly OfxTag DtEnd        = new("DTEND",         5);

    // Элементы транзакции
    public static readonly OfxTag StmtTrn      = new("STMTTRN",       5);
    public static readonly OfxTag TrnType      = new("TRNTYPE",       6);
    public static readonly OfxTag DtPosted     = new("DTPOSTED",      6);
    public static readonly OfxTag TrnAmt       = new("TRNAMT",        6);
    public static readonly OfxTag FitId        = new("FITID",         6);
    public static readonly OfxTag Name         = new("NAME",          6);
    public static readonly OfxTag Memo         = new("MEMO",          6);

    public override string ToString() => TagName;
}
