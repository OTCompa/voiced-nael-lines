using System;
using System.Collections.Generic;
using System.Text;

namespace VoicedNaelQuotes;

public partial class QuoteHandler
{
    public enum NaelQuote
    {
        InOut,
        InStack,
        StackOut,
        StackIn,
        SpreadOut,
        SpreadIn,
        SpreadDive,
        DiveStack,
        SpreadInStream,
        InSpreadStream,
        OutStackSpread,
        OutSpreadStack,
        InSpreadStack,
        InOutSpread,
        AddsRP1,
        AddsRP2,
    }

    public enum Sheet
    {
        NpcYell,
        InstanceContentTextData,
    }

    private record struct QuoteInfo(NaelQuote quote, Sheet sheet, uint rowId);

    // NpcYell 6492-6497, 6500-6507
    // InstanceContentTextData, 18100 18101
    private List<QuoteInfo> quoteInitInfo = [
        new QuoteInfo(NaelQuote.InOut, Sheet.NpcYell, 6492),
        new QuoteInfo(NaelQuote.InStack, Sheet.NpcYell, 6493),
        new QuoteInfo(NaelQuote.StackOut, Sheet.NpcYell, 6494),
        new QuoteInfo(NaelQuote.StackIn, Sheet.NpcYell, 6495),
        new QuoteInfo(NaelQuote.SpreadOut, Sheet.NpcYell, 6496),
        new QuoteInfo(NaelQuote.SpreadIn, Sheet.NpcYell, 6497),
        new QuoteInfo(NaelQuote.SpreadDive, Sheet.NpcYell, 6500),
        new QuoteInfo(NaelQuote.DiveStack, Sheet.NpcYell, 6501),
        new QuoteInfo(NaelQuote.SpreadInStream, Sheet.NpcYell, 6502),
        new QuoteInfo(NaelQuote.InSpreadStream, Sheet.NpcYell, 6503),
        new QuoteInfo(NaelQuote.OutStackSpread, Sheet.NpcYell, 6504),
        new QuoteInfo(NaelQuote.OutSpreadStack, Sheet.NpcYell, 6505),
        new QuoteInfo(NaelQuote.InSpreadStack, Sheet.NpcYell, 6506),
        new QuoteInfo(NaelQuote.InOutSpread, Sheet.NpcYell, 6507),
        new QuoteInfo(NaelQuote.AddsRP1, Sheet.InstanceContentTextData, 18100),
        new QuoteInfo(NaelQuote.AddsRP2, Sheet.InstanceContentTextData, 18101),
    ];

    private uint naelNameRowId = 2612;
}
