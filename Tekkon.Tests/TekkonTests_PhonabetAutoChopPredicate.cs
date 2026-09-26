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
  /// 注音狂打自動切音節判準：<b>生產實作</b>之回歸靶。
  /// <para>
  /// 對應 Swift 版 Tests/TekkonTests/TekkonTests_PhonabetAutoChopPredicate.swift 之四支測項。
  /// </para>
  /// <para>
  /// 本檔<b>不</b>自帶任何「測試端參考實作」——判準之正本只有一處
  /// （<see cref="Composer.ShouldAutoChopPhonabets" />），本檔直接驅動它，
  /// 杜絕「兩份各自演化之判準」。
  /// </para>
  /// <para>
  /// 四項地面真相（與 P251 之結論逐項對應）：<br />
  /// ① 合法單音節編碼之<b>每一個中途前綴</b>皆不得觸發切音節；<br />
  /// ② 音節交界處<b>必須</b>切（殘餘漏切率 &lt; 5%）；<br />
  /// ③ 單聲母縮寫（<c>ess</c>＝ㄍㄋㄋ）須得三顆鍵；<br />
  /// ④ 動態排列之逐槽覆寫（大千26 <c>qquu</c>＝ㄅㄚ）之四拍不得被切斷。
  /// </para>
  /// <para>
  /// 語料（1485 列 × 5 動態排列）<b>不另抄一份</b>，而是自
  /// <see cref="TekkonTestData.DynamicLayoutTable" /> 就地解析——故語料仍為單一正本。
  /// </para>
  /// <para>
  /// 靶之<b>輸入域</b>（P261 之 CI 跟進）：候選鍵與語料單元格皆須落在鍵面字元域內
  /// （僅 ASCII 字母與數字）。素材檔內之反引號（<c>`NULL</c>）與尾端空格（源自 <c>__</c>）
  /// 皆為「本排列無此鍵」之標記，<b>非按鍵</b>；先前之版本把兩者一併當成候選鍵，
  /// 遂使反推鍵表把反引號登記成某注音符號之按鍵、由合法讀音之前綴生成出<b>不可鍵入</b>
  /// 之鍵序——而判準對非注音按鍵之反應隨平台而異（CI 實錄：Linux 誤切 7366 次、
  /// Windows 語料整批讀不到）。<b>此為靶之缺陷，非判準之缺陷</b>；判準本身未動。
  /// </para>
  /// <para>
  /// 語料之<b>載入</b>（P261 之 CI 跟進 2，Swift 側 <c>aec2f32</c>）：本倉之語料是編譯期常數
  /// （<see cref="TekkonTestData.DynamicLayoutTable" />），故 Swift 側該次所加之「候選路徑清單」
  /// 與「讀不到時附上嘗試紀錄」在本倉<b>結構上無對位</b>——檔案根本不會開不成。本倉取該次
  /// 之兩項可移植者：行尾正規化（Windows checkout 之 CRLF），以及把「原始列數／過濾後列數」
  /// 之診斷附於一切依賴語料之斷言（<see cref="AutoChopCorpus.Diagnostic" />）。
  /// </para>
  /// </summary>
  public class TekkonTestsPhonabetAutoChopPredicate {
    /// <summary>動態排列語料之行（欄序與素材檔同）。</summary>
    private sealed class CorpusRow {
      public string Reading { get; set; } = "";

      public List<string> Cells { get; set; } = new();
    }

    /// <summary>解析後之語料：過濾後之資料列 ＋ 素材檔之原始資料列數（<b>未</b>過濾）。</summary>
    private sealed class AutoChopCorpus {
      public List<CorpusRow> Rows { get; } = new();

      public int RawRowCount { get; set; }

      /// <summary>診斷訊息（附於一切依賴語料之斷言）。</summary>
      /// <remarks>
      /// Swift 側之同項尚須列出嘗試過的檔案路徑；本倉之語料是<b>編譯期常數</b>，讀不到係
      /// 結構上不可能，故本項只報來源與兩個列數——「沒讀到」與「讀到了但濾掉幾列」在本倉
      /// 只能是後者，而兩者之數值仍須一眼可辨（P261 之 CI 通則 (b)／(d)）。
      /// </remarks>
      public string Diagnostic =>
        "語料來源：TekkonTestData.DynamicLayoutTable（編譯期常數，讀不到係結構上不可能）。" +
        $"已解析：raw={RawRowCount}、kept={Rows.Count}。";
    }

    /// <summary>語料表之五個動態排列（欄序與素材檔同）。</summary>
    private static readonly (string Name, MandarinParser Parser)[] DynamicLayouts = {
      ("Dachen26", MandarinParser.OfDachen26),
      ("ETen26", MandarinParser.OfETen26),
      ("Hsu", MandarinParser.OfHsu),
      ("Starlight", MandarinParser.OfStarlight),
      ("AlvinLiu", MandarinParser.OfAlvinLiu),
    };

    /// <summary>六個靜態排列（一鍵一注音，鍵表可由公開 API 反推）。</summary>
    private static readonly (string Name, MandarinParser Parser)[] StaticLayouts = {
      ("Dachen", MandarinParser.OfDachen),
      ("ETen", MandarinParser.OfETen),
      ("IBM", MandarinParser.OfIBM),
      ("MiTAC", MandarinParser.OfMiTAC),
      ("Seigyou", MandarinParser.OfSeigyou),
      ("FakeSeigyou", MandarinParser.OfFakeSeigyou),
    };

    /// <summary>
    /// 單一按鍵之候選集（靜態注音排列之鍵面字元：數字 ＋ 小寫字母）。
    /// <para>
    /// <b>不得</b>再收反引號與空格：兩者非任何出貨排列之按鍵，見型別說明。
    /// </para>
    /// </summary>
    private static readonly List<Rune> CandidateKeys =
      "0123456789abcdefghijklmnopqrstuvwxyz".EnumerateRunes()
        .Where(IsKeyCharacter).ToList();

    /// <summary>自素材檔就地解析之語料（僅解析一次）。</summary>
    private static readonly AutoChopCorpus Corpus = ParseCorpus();

    // MARK: ① 判準不得在單一音節內誤切

    /// <summary>
    /// 合法單音節編碼之每一個中途前綴皆不得觸發切音節。
    /// <para>
    /// 覆蓋 <b>11 個排列</b>：5 個動態排列用語料之 1485 列編碼；
    /// 6 個靜態排列用其鍵表反推全部前綴。
    /// </para>
    /// </summary>
    [Test]
    public void TestPhonabetAutoChopPredicateNeverFiresWithinASyllable() {
      Assert.That(Corpus.Rows, Is.Not.Empty,
                  $"語料解析失敗——素材檔格式已變：\n{Corpus.Diagnostic}");
      var offenders = new List<string>();
      long steps = 0;

      for (int layoutIndex = 0; layoutIndex < DynamicLayouts.Length; ++layoutIndex) {
        (string name, MandarinParser parser) = DynamicLayouts[layoutIndex];
        foreach (CorpusRow row in Corpus.Rows) {
          string cell = row.Cells[layoutIndex];
          if (!IsAllKeyCharacters(cell)) continue;
          var composer = new Composer(arrange: parser);
          int step = 0;
          foreach (Rune key in cell.EnumerateRunes()) {
            ++steps;
            if (composer.ShouldAutoChopPhonabets(key)) {
              offenders.Add($"{name}/{row.Reading}/拍{step}/鍵`{ShownKey(key)}`");
            }
            composer.ReceiveKey(key);
            ++step;
          }
        }
      }

      // 前綴集由 `Readings` 就地推導——索引本身刻意不暴露 `allPrefixes`（P252 之裁定：
      // 只答「是否為前綴」一問），故本靶自行展開、再逐條以 `IsPrefix` 交叉驗證。
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      var allPrefixes = new SortedSet<string>(StringComparer.Ordinal);
      foreach (string reading in index.Readings) {
        for (int length = 1; length <= reading.Length; ++length) {
          allPrefixes.Add(reading.Substring(0, length));
        }
      }
      foreach ((string name, MandarinParser parser) in StaticLayouts) {
        Dictionary<string, Rune> keyMap = StaticKeyMap(parser);
        // 靶之輸入域不變式：反推所得之按鍵一律須為鍵面字元。**此行即迴歸釘**——先前之
        // 候選鍵含反引號與空格，反推遂把它們登記成某注音符號之按鍵，而由合法讀音之前綴
        // 生成出**不可鍵入**之鍵序（CI 實錄：Linux 誤切 7366 次，全數為該等鍵）。
        foreach (KeyValuePair<string, Rune> pair in keyMap) {
          Assert.True(IsKeyCharacter(pair.Value),
                      $"{name} 之反推鍵表含非鍵面字元：{ShownKey(pair.Value)}");
        }
        foreach (string reading in allPrefixes) {
          // 就地推導之前綴集須與索引自身之判準一致（索引為排列中立）。
          Assert.True(index.IsPrefix(reading), reading);
          var keys = new List<Rune>();
          bool keysComplete = true;
          foreach (Rune codepoint in reading.EnumerateRunes()) {
            if (!keyMap.TryGetValue(codepoint.ToString(), out Rune key)) {
              keysComplete = false;
              break;
            }
            keys.Add(key);
          }
          if (!keysComplete) continue;
          var composer = new Composer(arrange: parser);
          int step = 0;
          foreach (Rune key in keys) {
            ++steps;
            if (composer.ShouldAutoChopPhonabets(key)) {
              offenders.Add($"{name}/{reading}/拍{step}/鍵`{ShownKey(key)}`");
            }
            composer.ReceiveKey(key);
            ++step;
          }
        }
      }

      // 下界改以**結構**表達，不再釘死魔數：語料須幾近全數解析成功、且受檢步數須成規模。
      Assert.That(Corpus.RawRowCount, Is.GreaterThan(1000),
                  $"素材檔之資料列僅 {Corpus.RawRowCount}：\n{Corpus.Diagnostic}");
      // 實測剔除率 38/1485 ＝ 2.56%（全為上揭兩種「不適用」標記）；門檻取 95% 以為餘裕，
      // 意在攔住「整批讀不到」與「大規模解析失敗」，而非逐列計較。
      Assert.That(Corpus.Rows.Count * 100, Is.GreaterThanOrEqualTo(Corpus.RawRowCount * 95),
                  $"語料解析損失過大：{Corpus.Rows.Count} / {Corpus.RawRowCount}");
      Assert.That(steps, Is.GreaterThan(20000L),
                  $"受檢步數僅 {steps}：\n{Corpus.Diagnostic}");
      Assert.That(offenders, Is.Empty,
                  $"誤切 {offenders.Count} 次：{JoinedSample(offenders)}");
    }

    // MARK: ② 判準須在音節交界處切分

    /// <summary>
    /// 音節交界處必須切。地面真相：A ＝ 一完整合法讀音；本鍵若<b>不能</b>
    /// 把 A 延伸成更長之合法前綴，則 A 須先固化 ⇒ 必切。
    /// </summary>
    [Test]
    public void TestPhonabetAutoChopPredicateFiresAtJunctions() {
      SyllableIndex index = SyllableIndex.Shared(MandarinParser.OfDachen);
      long checkedCount = 0;
      long missed = 0;
      var sample = new List<string>();

      for (int layoutIndex = 0; layoutIndex < DynamicLayouts.Length; ++layoutIndex) {
        (string name, MandarinParser parser) = DynamicLayouts[layoutIndex];
        var keys = new SortedSet<string>(StringComparer.Ordinal);
        var states = new List<Composer>();
        foreach (CorpusRow row in Corpus.Rows) {
          string cell = row.Cells[layoutIndex];
          if (!IsAllKeyCharacters(cell)) continue;
          List<Rune> codepoints = cell.EnumerateRunes().ToList();
          foreach (Rune key in codepoints) keys.Add(key.ToString());
          var composer = new Composer(arrange: parser);
          foreach (Rune key in codepoints) composer.ReceiveKey(key);
          states.Add(composer);
        }
        foreach (Composer state in states) {
          string preContent = state.GetComposition();
          foreach (string keyString in keys) {
            Rune key = Rune.GetRuneAt(keyString, 0);
            Composer probe = state;
            probe.ReceiveKey(key);
            string postContent = probe.GetComposition();
            // 於<b>當前狀態</b>下寫入聲調槽者（聲調鍵／空格）由既有管線固化，
            // 不屬本案（照 P251 之守衛）。
            if (probe.Intonation.Value != state.Intonation.Value) continue;
            // 注音內容概為 BMP 字元，故 String.Length 即碼點數（與 Swift 側 `.count` 同義）。
            bool greedy = index.IsPrefix(postContent) &&
                          postContent.Length > preContent.Length;
            if (greedy) continue;
            ++checkedCount;
            if (!state.ShouldAutoChopPhonabets(key)) {
              ++missed;
              if (sample.Count < 5) sample.Add($"{name}/{preContent}+`{ShownKey(key)}`");
            }
          }
        }
      }

      // 同上：交界數之下界為結構量（0 即語料未載入）；「不得漏切」由 `missed` 承擔。
      Assert.That(checkedCount, Is.GreaterThan(0L),
                  $"受檢交界僅 {checkedCount}：\n{Corpus.Diagnostic}");
      // P251 之實測為 2.19%；此處以 5% 為上限——殘餘之成因（與 `qquu`
      // 之逐槽覆寫在局部可觀測量上同構）已證不可由局部判準分離，屬<b>已知界線</b>。
      double rate = missed * 100.0 / Math.Max(checkedCount, 1);
      Assert.That(rate, Is.LessThan(5.0),
                  $"漏切率 {rate}%（{missed}/{checkedCount}）；樣本：{JoinedSample(sample, 5)}");
    }

    // MARK: ③ 單聲母縮寫

    /// <summary>單聲母縮寫：<c>ess</c>（大千：ㄍ＝<c>e</c>、ㄋ＝<c>s</c>）須得三顆鍵。</summary>
    [Test]
    public void TestPhonabetAutoChopPredicateSinglePhonabetAbbreviationIsCommittedAsThreeKeys() {
      CollectionAssert.AreEqual(
        new[] { "ㄍ", "ㄋ", "ㄋ" },
        TypeWithProductionPredicate("ess", MandarinParser.OfDachen));
      CollectionAssert.AreEqual(
        new[] { "ㄋ", "ㄋ" },
        TypeWithProductionPredicate("ss", MandarinParser.OfDachen));
      // 反向護欄：完整讀音不得被切碎。
      CollectionAssert.AreEqual(
        new[] { "ㄅㄧㄢ" },
        TypeWithProductionPredicate("1u0", MandarinParser.OfDachen));
    }

    // MARK: ④ 動態排列之逐槽覆寫

    /// <summary>動態排列之逐槽覆寫：大千26 <c>qquu</c>＝ㄅㄚ 之四拍不得被切斷。</summary>
    [Test]
    public void TestPhonabetAutoChopPredicateDachen26SlotOverwriteSurvivesThePredicate() {
      var composer = new Composer(arrange: MandarinParser.OfDachen26);
      var verdicts = new List<bool>();
      foreach (Rune key in "qquu".EnumerateRunes()) {
        verdicts.Add(composer.ShouldAutoChopPhonabets(key));
        composer.ReceiveKey(key);
      }
      string verdictsText = string.Concat(verdicts.Select(verdict => verdict ? "1" : "0"));
      CollectionAssert.AreEqual(new[] { false, false, false, false }, verdicts,
                                $"實得：{verdictsText}");
      Assert.AreEqual(expected: "ㄅㄚ", actual: composer.GetComposition());
    }

    // MARK: - 私有工具

    /// <summary>鍵面字元之地面真值（靜態注音排列之按鍵域）：僅 ASCII 字母與數字。</summary>
    /// <remarks>
    /// 實查自素材檔之 1485 列 × 5 動態排列：其鍵面字元僅 <c>0-9</c> 與 <c>a-z</c>。
    /// 反引號與空格<b>不在其列</b>——兩者在素材檔內只作「無此鍵」之標記。
    /// 此函式即靶之輸入域不變式。
    /// </remarks>
    private static bool IsKeyCharacter(Rune rune) {
      int value = rune.Value;
      if (value > 0x7F) return false;
      char ch = (char)value;
      if (ch >= '0' && ch <= '9') return true;
      if (ch >= 'a' && ch <= 'z') return true;
      return ch >= 'A' && ch <= 'Z';
    }

    /// <summary>整格是否皆為鍵面字元。</summary>
    private static bool IsAllKeyCharacters(string cell) =>
      cell.EnumerateRunes().All(IsKeyCharacter);

    /// <summary>供診斷輸出之鍵面表現：非鍵面字元一律以 <c>U+XXXX</c> 呈現。</summary>
    private static string ShownKey(Rune rune) =>
      IsKeyCharacter(rune) ? rune.ToString() : $"U+{rune.Value:X4}";

    /// <summary>將底線還原為空格（語料表以底線代表空白＝陰平鍵）。</summary>
    private static string ReplaceUnderscores(string str) => str.Replace('_', ' ');

    /// <summary>行尾正規化：CRLF／CR 一律化為 LF。</summary>
    /// <remarks>
    /// 本倉之素材是原始字串常數（<see cref="TekkonTestData.DynamicLayoutTable" />）：版控內為
    /// LF，但 Windows 之 checkout 可能改寫為 CRLF。此處顯式正規化，免日後之解析器倚賴隱性性質
    /// ——Swift 側之同一步係 <c>aec2f32</c> 所加。
    /// </remarks>
    private static string NormalizeLineEndings(string text) =>
      text.Replace("\r\n", "\n").Replace('\r', '\n');

    /// <summary>
    /// 自素材檔就地解析 <see cref="TekkonTestData.DynamicLayoutTable" /> 之內容。
    /// <para>
    /// 僅解析一次，<c>Rows</c> 與 <c>RawRowCount</c> 共用——<b>語料讀不到時必須大聲失敗</b>
    /// （P261 之 CI 實錄：Windows 之語料整批讀不到，而當時之靶只在兩處下界斷言上失手）。
    /// </para>
    /// </summary>
    private static AutoChopCorpus ParseCorpus() {
      var corpus = new AutoChopCorpus();
      string[] lines = NormalizeLineEndings(TekkonTestData.DynamicLayoutTable).Split('\n');
      foreach (string raw in lines) {
        string[] tokens = raw.Split(' ')
          .Where(token => token.Length > 0)
          .Select(ReplaceUnderscores)
          .ToArray();
        // 表頭（`$READING Dachen26 …`）與空行非測資。
        if (tokens.Length == 0) continue;
        if (tokens[0].StartsWith("$", StringComparison.Ordinal)) continue;
        ++corpus.RawRowCount;
        if (tokens.Length != 6) continue;
        // 校驗閘：任何單元格若含鍵面字元以外之字元即整列剔除。實查素材檔之此類單元格只有
        // 兩種：① 以反引號起始者（`NULL、`vezf…，標記「本排列無此鍵」）；② 尾端帶一空格者
        // （m 、too …，源自素材檔之 `__` ⇒ 空 cell）。**兩者皆為「不適用」之標記，非按鍵。**
        if (!tokens.Skip(1).All(IsAllKeyCharacters)) continue;
        corpus.Rows.Add(new CorpusRow {
          Reading = tokens[0],
          Cells = tokens.Skip(1).ToList(),
        });
      }
      return corpus;
    }

    /// <summary>
    /// 由靜態排列之鍵表反推「注音符號 → 按鍵」。
    /// <para>
    /// <b>不</b>取用引擎之鍵表——改以純公開 API 逐鍵探測：把每個候選鍵餵進一枚空
    /// <see cref="Composer" />，看它填入哪個槽。靜態排列是一鍵一注音，
    /// 故此探測即其鍵表之逆。同一符號多鍵時取候選序中最早者。
    /// </para>
    /// </summary>
    private static Dictionary<string, Rune> StaticKeyMap(MandarinParser parser) {
      var result = new Dictionary<string, Rune>(StringComparer.Ordinal);
      foreach (Rune key in CandidateKeys) {
        var composer = new Composer(arrange: parser);
        composer.ReceiveKey(key);
        string[] slots = PhonabetAutoChopSlots(composer);
        string phonabet = "";
        int filledCount = 0;
        foreach (string slot in slots) {
          if (slot.Length == 0) continue;
          ++filledCount;
          phonabet = slot;
        }
        if (filledCount != 1) continue;
        if (!result.ContainsKey(phonabet)) result[phonabet] = key;
      }
      return result;
    }

    /// <summary>四槽內容（聲／介／韻／調）。</summary>
    private static string[] PhonabetAutoChopSlots(Composer composer) => new[] {
      composer.Consonant.Value, composer.Semivowel.Value,
      composer.Vowel.Value, composer.Intonation.Value,
    };

    /// <summary>將前若干筆診斷字串接成一行（避免測項輸出被數千筆誤切灌爆）。</summary>
    private static string JoinedSample(IEnumerable<string> sample, int limit = 10) =>
      string.Join(" ", sample.Take(limit));

    /// <summary>以生產判準實際驅動一次輸入，回傳送入組字器之音節序列。</summary>
    private static List<string> TypeWithProductionPredicate(string keys, MandarinParser parser) {
      var composer = new Composer(arrange: parser);
      var committed = new List<string>();
      foreach (Rune key in keys.EnumerateRunes()) {
        if (composer.ShouldAutoChopPhonabets(key)) {
          string? reading = composer.PhonabetKeyForQuery(true);
          if (!string.IsNullOrEmpty(reading)) {
            committed.Add(reading);
            composer.Clear();
          }
        }
        composer.ReceiveKey(key);
      }
      string? last = composer.PhonabetKeyForQuery(true);
      if (!string.IsNullOrEmpty(last)) committed.Add(last);
      return committed;
    }
  }
}
