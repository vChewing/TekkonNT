// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tekkon {
  /// <summary>
  /// 漢語音節之唯讀前綴索引。
  /// <para>
  /// 由 <see cref="Shared.MapHanyuPinyin"/> 之注音詞幹（426 條）派生，含其全部非空前綴
  /// （442 條）。它只回答<strong>一問</strong>：「此串是否還可能延伸成某個合法讀音」
  /// ——即前綴之成員資格。
  /// </para>
  /// <para>
  /// 注意：本型別<strong>不</strong>回答「此串在辭典內有無詞條」。那是辭典之職責，
  /// 兩者互不替代。
  /// </para>
  /// <para>
  /// 注意：本型別<strong>不</strong>收錄單符號讀音（21 個聲母、16 個韻母）。單符號之合法性是
  /// <strong>辭典之事實</strong>、不是<strong>音節表之事實</strong>：其中 14 個聲母
  /// （ㄅ ㄆ ㄇ ㄈ ㄉ ㄊ ㄋ ㄌ ㄍ ㄎ ㄏ ㄐ ㄑ ㄒ）並非獨立音節，只是各自一族之嚴格前綴。
  /// </para>
  /// <para>
  /// 注意：<strong>排列中立</strong>。注音是排列中立之表記，故 426 條詞幹對所有
  /// <see cref="MandarinParser"/> 皆相同，今日全部排列共用同一份索引；<c>parser</c> 參數
  /// 為未來之擴充位。
  /// </para>
  /// <example>
  /// <code>
  /// var index = SyllableIndex.Shared(MandarinParser.OfDachen);
  /// index.IsPrefix("ㄍ");      // true —— 還能延伸成 ㄍㄚ、ㄍㄜ…
  /// index.IsPrefix("ㄍㄋ");    // false —— 不可能再延伸
  /// index.IsComplete("ㄍ");    // false —— ㄍ 非獨立音節
  /// index.IsComplete("ㄍㄚ");  // true
  /// </code>
  /// </example>
  /// </summary>
  public sealed class SyllableIndex {
    // MARK: Shared Cache

    /// <summary>
    /// 取得共用之索引。
    /// <para>
    /// 快取範式與 <see cref="PinyinTrie.Shared"/> 一致，惟<strong>只留一份</strong>：
    /// 注音是排列中立之表記，見型別說明。
    /// </para>
    /// </summary>
    /// <param name="parser">本索引被要求服務之排列。今日不影響內容。</param>
    /// <returns>共用之索引實例。</returns>
    public static SyllableIndex Shared(MandarinParser parser) {
      lock (SharedCacheLock) {
        if (SharedCache != null) return SharedCache;
        SharedCache = new SyllableIndex(parser);
        return SharedCache;
      }
    }

    /// <summary>清除共用快取。供測試使用。</summary>
    public static void ClearSharedCache() {
      lock (SharedCacheLock) { SharedCache = null; }
    }

    /// <summary>
    /// 全部完整讀音（426 條；升冪）。此即<strong>排列中立之正本</strong>，
    /// 亦是 <see cref="Readings"/> 之來源。
    /// </summary>
    public static IReadOnlyList<string> AllReadings => CanonicalReadings.Value;

    /// <summary>本索引持有之完整讀音（升冪）。今日恆等於 <see cref="AllReadings"/>。</summary>
    public IReadOnlyList<string> Readings { get; }

    /// <summary>
    /// 該字串是否為某合法讀音之<strong>完整</strong>形式。
    /// </summary>
    /// <param name="reading">待檢定之字串。</param>
    /// <returns>是否為完整讀音。</returns>
    /// <remarks>
    /// <strong>不得</strong>以本函式當作「當前注拼槽可否提交」之依據。單聲母／單韻母乃原廠
    /// 辭典之合法詞條，若以 <c>IsComplete</c> 為閘則單聲母縮寫打法全滅。
    /// </remarks>
    public bool IsComplete(string reading) => CompleteSet.Contains(reading);

    /// <summary>
    /// 該字串是否為某合法讀音之<strong>非空</strong>前綴（含其本身即完整者）。
    /// 空字串恆為 <c>false</c>。
    /// </summary>
    /// <param name="reading">待檢定之字串。</param>
    /// <returns>是否為非空前綴。</returns>
    public bool IsPrefix(string reading) =>
      !string.IsNullOrEmpty(reading) && PrefixSet.Contains(reading);

    /// <summary>
    /// 以該字串為前綴之全部完整讀音（升冪，內容穩定）。
    /// </summary>
    /// <param name="prefix">待列舉之前綴。</param>
    /// <returns>以該字串為前綴之全部完整讀音。</returns>
    /// <remarks>
    /// 對外暫緩公開（Swift 側為 <c>internal</c>）。目前之生產端消費者（自動切音節判準）
    /// 只用 <see cref="IsPrefix"/>，故不預先承諾此 API 之形狀。
    /// </remarks>
    public List<string> Completions(string prefix) =>
      Readings.Where(reading => reading.StartsWith(prefix, StringComparison.Ordinal))
        .ToList();

    // MARK: Private

    private SyllableIndex(MandarinParser parser) {
      Readings = AllReadings;
      CompleteSet = new HashSet<string>(Readings, StringComparer.Ordinal);
      PrefixSet = new HashSet<string>(StringComparer.Ordinal);
      // 以 UTF-16 字元為界逐段取非空前綴。注音符號（U+3105–U+3129）與聲調符號
      // （U+02C7/02CA/02CB/02D9）皆在 BMP 之內，故一個 char 即一個碼點。
      foreach (string reading in Readings) {
        for (int length = 1; length <= reading.Length; ++length) {
          PrefixSet.Add(reading.Substring(0, length));
        }
      }
    }

    private static readonly Lazy<List<string>> CanonicalReadings = new(() => {
      HashSet<string> stems = new(StringComparer.Ordinal);
      // 全名限定：本類別自身之 Shared(MandarinParser) 會遮蔽 Tekkon.Shared 結構。
      foreach (string stem in Tekkon.Shared.MapHanyuPinyin.Values) stems.Add(stem);
      List<string> sorted = stems.ToList();
      // Swift 之 `.sorted()` 為序數排序；C# 之 List.Sort() 預設為文化相關排序，
      // 故必須顯式指定 StringComparer.Ordinal，否則兩語言版本之「升冪」會不一致。
      sorted.Sort(StringComparer.Ordinal);
      return sorted;
    });

    private readonly HashSet<string> CompleteSet;
    private readonly HashSet<string> PrefixSet;

    private static readonly object SharedCacheLock = new();
    private static SyllableIndex? SharedCache;
  }
}
