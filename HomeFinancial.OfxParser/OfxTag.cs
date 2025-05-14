using System.Collections.Immutable;
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
    #region Корневые и промежуточные элементы для доступа к выпискам

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

    #endregion

    #region Lookup dictionary

    private static readonly ImmutableDictionary<string, OfxTag> Lookup =
        ImmutableDictionary.CreateRange(new Dictionary<string, OfxTag>
        {
            [Ofx.TagName]          = Ofx,
            [BankMsgsRsv1.TagName] = BankMsgsRsv1,
            [StmtTrnRs.TagName]    = StmtTrnRs,
            [StmTrs.TagName]       = StmTrs,
            [BankAcctFrom.TagName] = BankAcctFrom,
            [BankId.TagName]       = BankId,
            [AcctId.TagName]       = AcctId,
            [AcctType.TagName]     = AcctType,
            [BankTranList.TagName] = BankTranList,
            [DtStart.TagName]      = DtStart,
            [DtEnd.TagName]        = DtEnd,
            [StmtTrn.TagName]      = StmtTrn,
            [TrnType.TagName]      = TrnType,
            [DtPosted.TagName]     = DtPosted,
            [TrnAmt.TagName]       = TrnAmt,
            [FitId.TagName]        = FitId,
            [Name.TagName]         = Name,
            [Memo.TagName]         = Memo
        });

    #endregion

    /// <summary>
    /// Попытаться получить OfxTag по его строковому представлению.
    /// </summary>
    /// <param name="tagName">Строковое имя тега.</param>
    /// <param name="tag">Найденный OfxTag или default.</param>
    /// <returns>True, если тег найден, иначе False.</returns>
    public static bool TryGet(string tagName, out OfxTag tag) =>
        Lookup.TryGetValue(tagName, out tag);
    
    /// <inheritdoc/>
    public override string ToString() => TagName;
}
