// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Tekkon.Tests {
  /// <summary>
  /// <see cref="SyllableIndex" /> 之行為測試，暨漢語拼音單字母條目之解碼契約。
  /// <para>
  /// 斷言分三類：① 資料規模（426／442／16／37 四項可稽核數字）；② 成員資格之正反例；
  /// ③ 與引擎既有表（<see cref="Phonabet.AllowedConsonants" /> 等）及測試素材之交叉比對。
  /// </para>
  /// </summary>
  public class TekkonTestsSyllableIndex {
    /// <summary>由全部讀音逐條展開之全部非空前綴。</summary>
    private static HashSet<string> DerivedPrefixes(IEnumerable<string> readings) {
      HashSet<string> result = new(StringComparer.Ordinal);
      foreach (string reading in readings) {
        for (int length = 1; length <= reading.Length; ++length) {
          result.Add(reading.Substring(0, length));
        }
      }
      return result;
    }

    /// <summary>由單一讀音取首個字元（＝單符號前綴）。</summary>
    private static string FirstChar(string reading) =>
      reading.Length == 0 ? "" : reading.Substring(0, 1);

    /// <summary>語料表之全部無調詞幹（底線＝空格＝陰平，須先還原再剝調）。</summary>
    private static HashSet<string> TestTableStems() {
      HashSet<string> stems = new(StringComparer.Ordinal);
      string[] lines = TekkonTestData.DynamicLayoutTable.Split('\n');
      foreach (string raw in lines) {
        string[] tokens = raw.TrimEnd('\r').Split(' ')
          .Where(token => token.Length > 0).ToArray();
        if (tokens.Length == 0) continue;
        string first = tokens[0].Replace('_', ' ');
        // 表頭（`$READING Dachen26 …`）非測資。
        if (first.StartsWith("$", StringComparison.Ordinal)) continue;
        // 剝去尾端聲調（含以空格表記之陰平）。
        if (first.Length > 0 && " ˊˇˋ˙".IndexOf(first[first.Length - 1]) >= 0) {
          first = first.Substring(0, first.Length - 1);
        }
        stems.Add(first);
      }
      return stems;
    }

    // MARK: 資料規模

    [Test]
    public void TestCanonicalReadingsMatchThePinyinMap() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      Assert.AreEqual(actual: index.Readings.Count, expected: 426);
      Assert.AreEqual(actual: SyllableIndex.AllReadings, expected: index.Readings);
      HashSet<string> expected = new(Tekkon.Shared.MapHanyuPinyin.Values,
                                     StringComparer.Ordinal);
      HashSet<string> actual = new(index.Readings, StringComparer.Ordinal);
      // 注意：NUnit 之 Assert.AreEqual 對集合是「依列舉順序」比對，非集合等價比對，
      // 故集合之比較一律改用 CollectionAssert.AreEquivalent。
      CollectionAssert.AreEquivalent(expected, actual);
      // 升冪、且無重複。
      List<string> sorted = index.Readings.ToList();
      sorted.Sort(StringComparer.Ordinal);
      Assert.AreEqual(actual: index.Readings, expected: sorted);
      Assert.AreEqual(actual: actual.Count, expected: index.Readings.Count);
    }

    [Test]
    public void TestPrefixSetIsExactlyFourHundredFortyTwo() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      HashSet<string> derived = DerivedPrefixes(index.Readings);
      Assert.AreEqual(actual: derived.Count, expected: 442);
      // 每一條推導而得之前綴皆須被索引認可。
      foreach (string prefix in derived) Assert.True(index.IsPrefix(prefix), prefix);
      // 必然為假者。
      foreach (string bogus in new[] { "", "ㄍㄋ", "ㄅㄆ", "ㄚㄅ", "abc", "zhi",
                                       "ㄅㄚˇ", "ㄅㄚˋ", "ㄅㄚ1", " ㄅ" }) {
        Assert.False(index.IsPrefix(bogus), bogus);
      }
      // 逐條讀音之延伸一符號後皆假。
      foreach (string reading in index.Readings) {
        Assert.True(index.IsPrefix(reading));
        Assert.False(index.IsPrefix(reading + "ㄅ"));
      }
    }

    [Test]
    public void TestOneCharacterPrefixesAreThePhonabetTables() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      HashSet<string> derivedOneChar = new(StringComparer.Ordinal);
      foreach (string reading in index.Readings) derivedOneChar.Add(FirstChar(reading));
      HashSet<string> tableUnion = new(StringComparer.Ordinal);
      foreach (Rune scalar in Phonabet.AllowedConsonants) tableUnion.Add(scalar.ToString());
      foreach (Rune scalar in Phonabet.AllowedSemivowels) tableUnion.Add(scalar.ToString());
      foreach (Rune scalar in Phonabet.AllowedVowels) tableUnion.Add(scalar.ToString());
      Assert.AreEqual(actual: derivedOneChar.Count, expected: 37);
      CollectionAssert.AreEquivalent(tableUnion, derivedOneChar);
      foreach (string symbol in tableUnion) Assert.True(index.IsPrefix(symbol));
    }

    [Test]
    public void TestStrictPrefixesAreExactlyTheSixteen() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      HashSet<string> derived = DerivedPrefixes(index.Readings);
      List<string> strict = derived.Where(prefix => !index.IsComplete(prefix)).ToList();
      strict.Sort(StringComparer.Ordinal);
      List<string> expected = new() {
        "ㄅ", "ㄆ", "ㄇ", "ㄈ", "ㄈㄧ", "ㄉ", "ㄊ", "ㄋ",
        "ㄌ", "ㄍ", "ㄎ", "ㄎㄧ", "ㄏ", "ㄐ", "ㄑ", "ㄒ",
      };
      Assert.AreEqual(actual: strict, expected: expected);
      foreach (string symbol in strict) {
        Assert.True(index.IsPrefix(symbol), symbol);
        Assert.False(index.IsComplete(symbol), symbol);
      }
      // 反向：推導集之中，凡不在上述 16 條者皆須為完整讀音。
      HashSet<string> strictSet = new(strict, StringComparer.Ordinal);
      foreach (string prefix in derived) {
        if (strictSet.Contains(prefix)) continue;
        Assert.True(index.IsComplete(prefix), prefix);
      }
      // 兩條 2 字嚴格前綴之來由。
      Assert.AreEqual(actual: index.Completions("ㄈㄧ"),
                      expected: new List<string> { "ㄈㄧㄠ" });
      Assert.AreEqual(actual: index.Completions("ㄎㄧ"),
                      expected: new List<string> { "ㄎㄧㄡ", "ㄎㄧㄤ" });
    }

    [Test]
    public void TestSingleSymbolCompleteSplit() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      HashSet<string> oneChar = new(StringComparer.Ordinal);
      foreach (string reading in index.Readings) oneChar.Add(FirstChar(reading));
      List<string> completeOneChar = oneChar.Where(index.IsComplete).ToList();
      List<string> strictOneChar = oneChar.Where(symbol => !index.IsComplete(symbol)).ToList();
      completeOneChar.Sort(StringComparer.Ordinal);
      strictOneChar.Sort(StringComparer.Ordinal);
      // 37 ＝ 23 完整 ＋ 14 嚴格。
      Assert.AreEqual(actual: completeOneChar.Count, expected: 23);
      Assert.AreEqual(actual: strictOneChar, expected: new List<string> {
        "ㄅ", "ㄆ", "ㄇ", "ㄈ", "ㄉ", "ㄊ", "ㄋ",
        "ㄌ", "ㄍ", "ㄎ", "ㄏ", "ㄐ", "ㄑ", "ㄒ",
      });
      // 預期之完整單符號：7 個可獨立成音節之聲母 ＋ 3 個介母 ＋ 13 個韻母。
      List<string> expectedComplete = new() {
        "ㄓ", "ㄔ", "ㄕ", "ㄖ", "ㄗ", "ㄘ", "ㄙ",
        "ㄚ", "ㄛ", "ㄜ", "ㄝ", "ㄞ", "ㄟ", "ㄠ", "ㄡ", "ㄢ", "ㄣ", "ㄤ", "ㄥ",
        "ㄦ", "ㄧ", "ㄨ", "ㄩ",
      };
      expectedComplete.Sort(StringComparer.Ordinal);
      Assert.AreEqual(actual: completeOneChar, expected: expectedComplete);
      // `ㄑ` 不是獨立音節，只以「ㄑ 一族之嚴格前綴」之身分存在。
      Assert.False(index.IsComplete("ㄑ"));
      Assert.True(index.IsPrefix("ㄑ"));
      Assert.False(index.Completions("ㄑ").Count == 0);
      // 單字母條目僅餘三條真音節。
      Assert.AreEqual(actual: Tekkon.Shared.MapHanyuPinyin["qi"], expected: "ㄑㄧ");
      Assert.False(Tekkon.Shared.MapHanyuPinyin.ContainsKey("q"));
      Assert.False(Tekkon.Shared.MapHanyuPinyin.ContainsKey("b"));
      Assert.False(Tekkon.Shared.MapHanyuPinyin.ContainsKey("p"));
      List<string> singleLetterKeys = Tekkon.Shared.MapHanyuPinyin.Keys
        .Where(key => key.Length == 1).ToList();
      singleLetterKeys.Sort(StringComparer.Ordinal);
      Assert.AreEqual(actual: singleLetterKeys, expected: new List<string> { "a", "e", "o" });
      Assert.AreEqual(actual: Tekkon.Shared.MapHanyuPinyin["a"], expected: "ㄚ");
      Assert.AreEqual(actual: Tekkon.Shared.MapHanyuPinyin["e"], expected: "ㄜ");
      Assert.AreEqual(actual: Tekkon.Shared.MapHanyuPinyin["o"], expected: "ㄛ");
      Assert.AreEqual(actual: Tekkon.Shared.MapHanyuPinyin.Count, expected: 426);
    }

    // MARK: 成員資格

    [Test]
    public void TestCompleteAlwaysImpliesPrefix() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      foreach (string reading in index.Readings) {
        Assert.True(index.IsComplete(reading));
        Assert.True(index.IsPrefix(reading));
      }
      foreach (string reading in SyllableIndex.AllReadings) {
        Assert.True(index.IsComplete(reading));
      }
    }

    [Test]
    public void TestCompletionsAreSortedAndStable() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      // 完整讀音：僅回傳自身。
      Assert.AreEqual(actual: index.Completions("ㄍㄚ"),
                      expected: new List<string> { "ㄍㄚ" });
      // 空字串：全部 426 條。此與 IsPrefix("") == false 並不矛盾——後者是「非空」之定義，
      // 前者是列舉之定義。
      Assert.AreEqual(actual: index.Completions("").Count, expected: 426);
      Assert.False(index.IsPrefix(""));
      // 非前綴：空集。
      Assert.AreEqual(actual: index.Completions("ㄍㄋ").Count, expected: 0);
      Assert.AreEqual(actual: index.Completions("zzz").Count, expected: 0);
      // 升冪、且與兩次呼叫之結果一致。
      foreach (string prefix in new[] { "ㄍ", "ㄓ", "ㄧ", "ㄈㄧ" }) {
        List<string> result = index.Completions(prefix);
        Assert.False(result.Count == 0, prefix);
        List<string> sorted = result.ToList();
        sorted.Sort(StringComparer.Ordinal);
        Assert.AreEqual(actual: result, expected: sorted);
        Assert.AreEqual(actual: result, expected: index.Completions(prefix));
        foreach (string reading in result) {
          Assert.True(reading.StartsWith(prefix, StringComparison.Ordinal));
        }
      }
      Assert.Greater(index.Completions("ㄍ").Count, 1);
    }

    [Test]
    public void TestTableStemsAreAllCompleteReadings() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      HashSet<string> stems = TestTableStems();
      Assert.AreEqual(actual: stems.Count, expected: 422);
      // 422 條之中，421 條為完整讀音；唯一之例外是 `ㄑ`。
      HashSet<string> notComplete = new(
        stems.Where(stem => !index.IsComplete(stem)), StringComparer.Ordinal);
      CollectionAssert.AreEquivalent(new HashSet<string> { "ㄑ" }, notComplete);
      // 反向落差：字典有而素材無者恰為 5 條。
      HashSet<string> readings = new(index.Readings, StringComparer.Ordinal);
      HashSet<string> onlyInMap = new(
        readings.Where(reading => !stems.Contains(reading)), StringComparer.Ordinal);
      CollectionAssert.AreEquivalent(
        new HashSet<string> { "ㄈㄨㄥ", "ㄍㄧ", "ㄍㄨㄜ", "ㄎㄧㄡ", "ㄘㄟ" }, onlyInMap);
    }

    // MARK: 共用快取

    [Test]
    public void TestSharedIndexIsParserNeutral() {
      MandarinParser[] parsers = {
        MandarinParser.OfDachen, MandarinParser.OfDachen26, MandarinParser.OfETen,
        MandarinParser.OfHanyuPinyin, MandarinParser.OfWadeGilesPinyin,
      };
      List<IReadOnlyList<string>> snapshots = parsers
        .Select(parser => SyllableIndex.Shared(parser).Readings).ToList();
      foreach (IReadOnlyList<string> snapshot in snapshots) {
        Assert.AreEqual(actual: snapshot.Count, expected: 426);
        Assert.AreEqual(actual: snapshot, expected: snapshots[0]);
      }
      // 清快取之後仍可重建，且內容不變。
      SyllableIndex.ClearSharedCache();
      SyllableIndex rebuilt = SyllableIndex.Shared(MandarinParser.OfDachen);
      Assert.AreEqual(actual: rebuilt.Readings, expected: snapshots[0]);
      Assert.True(rebuilt.IsPrefix("ㄍ"));
      Assert.False(rebuilt.IsPrefix("ㄍㄋ"));
      Assert.True(rebuilt.IsComplete("ㄍㄚ"));
      Assert.False(rebuilt.IsComplete("ㄍ"));
    }

    // MARK: 單字母條目之解碼契約

    [Test]
    public void TestHanyuPinyinSingleLetterEntries() {
      // `q` 與 `b`／`z` 皆不寫槽（整體查表無此條目）；`a`／`e`／`o` 則寫入各自之韻母。
      foreach (string key in new[] { "q", "b", "z" }) {
        Composer composer = new(arrange: MandarinParser.OfHanyuPinyin);
        composer.ReceiveKey(key);
        Assert.AreEqual(actual: composer.GetComposition(), expected: "");
        Assert.AreEqual(actual: composer.RomajiBuffer, expected: key);
      }
      foreach ((string Key, string Expected) pair in new[] { ("a", "ㄚ"), ("e", "ㄜ"), ("o", "ㄛ") }) {
        Composer composer = new(arrange: MandarinParser.OfHanyuPinyin);
        composer.ReceiveKey(pair.Key);
        Assert.AreEqual(actual: composer.GetComposition(), expected: pair.Expected);
      }

      // 自動切音節：`q`＋`f` 與 `b`／`z`＋`f` 皆不提交。
      foreach (string first in new[] { "q", "b", "z" }) {
        Composer composer = new(arrange: MandarinParser.OfHanyuPinyin) {
          AllowsExtendedRomajiBuffer = true,
        };
        composer.ReceiveKey(first);
        Assert.AreEqual(actual: composer.RomajiBuffer, expected: first);
        Assert.IsNull(composer.GetPinyinAutoChopResult("f"), first);
      }
      // 對照組：`a`＋`f` 仍會提交 ㄚ（`a` 是真音節，此行為由音節事實支持）。
      {
        Composer composer = new(arrange: MandarinParser.OfHanyuPinyin) {
          AllowsExtendedRomajiBuffer = true,
        };
        composer.ReceiveKey("a");
        Assert.AreEqual(actual: composer.RomajiBuffer, expected: "a");
        Composer.PinyinAutoChopResult? chopped = composer.GetPinyinAutoChopResult("f");
        Assert.IsNotNull(chopped);
        Assert.AreEqual(actual: chopped!.Value.CommittedReadings,
                        expected: new[] { "ㄚ" });
      }

      // 拼音片段展開：`q` 與 `b` 皆走前綴擴張（不再是精確命中）。
      PinyinTrie trie = PinyinTrie.Shared(MandarinParser.OfHanyuPinyin);
      List<string> qExpansion = trie.ZhuyinReadings("q");
      Assert.AreEqual(actual: qExpansion.Count(reading => reading == "ㄑ"), expected: 0);
      Assert.Greater(qExpansion.Count(reading => reading == "ㄑㄧ"), 0);
      Assert.Greater(qExpansion.Count, 1);
      Assert.Greater(trie.ZhuyinReadings("b").Count, 1);
      // `a` 仍是精確命中。
      Assert.AreEqual(actual: trie.ZhuyinReadings("a"), expected: new List<string> { "ㄚ" });
    }
  }
}
