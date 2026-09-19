// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tekkon.Tests {
  /// <summary>
  /// 逐鍵順序檢定 API（<see cref="Composer.IsSequentiallyTypedRawKeyOrder" />）之測試。
  /// </summary>
  public class TekkonTestsSequentialValidation {
    [Test]
    public void TestRawKeyOrderStaticLayouts() {
      // 靜態注音排列：逐鍵即為逐個注音，依序鍵入者恆應合格。首六筆皆為「ㄅㄚˇ」。
      var cases = new List<(MandarinParser Parser, string Input, bool Expected)> {
        (MandarinParser.OfDachen, "183", true),
        (MandarinParser.OfETen, "ba3", true),
        (MandarinParser.OfIBM, "1f,", true),
        (MandarinParser.OfMiTAC, "ba3", true),
        (MandarinParser.OfSeigyou, "28a", true),
        (MandarinParser.OfFakeSeigyou, "28a", true),
        (MandarinParser.OfDachen, "cl", true), // ㄏㄠ
        (MandarinParser.OfDachen, "1u,", true), // ㄅㄧㄝ
        (MandarinParser.OfDachen, "5j/ ", true), // ㄓㄨㄥ
        (MandarinParser.OfDachen, "xup6", true), // ㄌㄧㄣˊ
        (MandarinParser.OfDachen, "hl3", true), // ㄘㄠˇ
        (MandarinParser.OfDachen, "ll", true), // 同值之重複寫入不留痕跡、不構成違序
        (MandarinParser.OfDachen, "lc", false), // 先韻後聲：倒序
        (MandarinParser.OfDachen, "us", false), // 先介後聲：倒序
        (MandarinParser.OfDachen, "3u", false), // 聲調前置
        (MandarinParser.OfDachen, "44", false), // 僅聲調、不可唸
        (MandarinParser.OfDachen, "C", false), // 非該排列之按鍵
        (MandarinParser.OfDachen, "幹", false), // 非按鍵
        (MandarinParser.OfDachen, "", false), // 空序列
        (MandarinParser.OfDachen, " ", false), // 僅陰平
      };
      CheckSequentialReadings(cases);
    }

    /// <summary>
    /// 覆寫修正（以另一鍵改寫既有槽值）於預設模式下判不合格；suffixOnly 模式下放寬之。
    /// </summary>
    [Test]
    public void TestRawKeyOrderOverwriteCorrectionAndSuffixOnly() {
      var cases = new List<(MandarinParser Parser, string Input, bool ExpectedStrict, bool ExpectedSuffixOnly)> {
        (MandarinParser.OfDachen, "cl", true, true),
        (MandarinParser.OfDachen, "dcl", false, true), // ㄎ→ㄏ 以另一鍵覆寫修正
        (MandarinParser.OfDachen, "qn", false, true), // ㄆ→ㄙ 以另一鍵覆寫修正
        (MandarinParser.OfDachen, "cl34", false, true), // 聲調 ˇ→ˋ 以另一鍵覆寫
        (MandarinParser.OfDachen, "ll", true, true), // 同值重寫：無可觀測變化
        (MandarinParser.OfDachen, "lc", false, false), // 倒序：兩模式皆不合格
        (MandarinParser.OfDachen26, "qquu", true, true), // 同鍵重寫：排列自身編碼所必需
        (MandarinParser.OfDachen26, "uuu", true, true),
        (MandarinParser.OfDachen26, "mm", true, true),
        (MandarinParser.OfETen26, "ge", true, true), // ㄓ→ㄐ：引擎自身之跨鍵糾正（條件五不在其限）
        (MandarinParser.OfHanyuPinyin, "su3", true, true),
        (MandarinParser.OfHanyuPinyin, "suan", true, true),
        (MandarinParser.OfHanyuPinyin, "3su", false, false),
      };
      foreach (var (parser, input, expectedStrict, expectedSuffixOnly) in cases) {
        var composer = new Composer(arrange: parser);
        Assert.AreEqual(
          actual: composer.IsSequentiallyTypedRawKeyOrder(input),
          expected: expectedStrict,
          message: $"{parser.NameTag()} \"{input}\" 於預設模式應為 {expectedStrict}");
        Assert.AreEqual(
          actual: composer.IsSequentiallyTypedRawKeyOrder(input, true),
          expected: expectedSuffixOnly,
          message: $"{parser.NameTag()} \"{input}\" 於 suffixOnly 模式應為 {expectedSuffixOnly}");
      }
    }

    [Test]
    public void TestRawKeyOrderDynamicLayoutsTypical() {
      var cases = new List<(MandarinParser Parser, string Input, bool Expected)> {
        (MandarinParser.OfDachen26, "qquu", true), // ㄅㄚ（首擊為ㄆ、次擊覆寫為ㄅ）
        (MandarinParser.OfDachen26, "qquur", true), // ㄅㄚˇ
        (MandarinParser.OfDachen26, "uuu", true), // ㄧㄚ（末擊補回介母）
        (MandarinParser.OfDachen26, "mm", true), // ㄩ（首擊為ㄡ、次擊覆寫為ㄩ）
        (MandarinParser.OfDachen26, "uuqq", false), // 亂序
        (MandarinParser.OfDachen26, "qqruu", false), // 聲調前置
        (MandarinParser.OfETen26, "baj", true), // ㄅㄚˇ
        (MandarinParser.OfHsu, "byf", true), // ㄅㄚˇ
        (MandarinParser.OfStarlight, "ba8", true), // ㄅㄚˇ
        (MandarinParser.OfAlvinLiu, "baj", true), // ㄅㄚˇ
      };
      CheckSequentialReadings(cases);
    }

    /// <summary>
    /// 動態注音排列之合法編碼散見於測試素材；逐筆檢證其皆應被本 API 接受。
    /// </summary>
    [Test]
    public void TestRawKeyOrderDynamicLayoutsCorpus() {
      MandarinParser[] parserOrder = {
        MandarinParser.OfDachen26,
        MandarinParser.OfETen26,
        MandarinParser.OfHsu,
        MandarinParser.OfStarlight,
        MandarinParser.OfAlvinLiu,
      };
      var typings = new List<string>[parserOrder.Length];
      for (int index = 0; index < typings.Length; index++) {
        typings[index] = new List<string>();
      }

      string[] lines = TekkonTestData.DynamicLayoutTable.Split(
        '\n', StringSplitOptions.RemoveEmptyEntries);
      bool isTitleLine = true;
      foreach (string line in lines) {
        if (isTitleLine) {
          isTitleLine = false;
          continue;
        }

        string[] cells = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (cells.Length <= parserOrder.Length) continue;
        for (int index = 0; index < parserOrder.Length; index++) {
          string typing = cells[index + 1].Replace("_", " ");
          if (typing.StartsWith("`")) continue;
          typings[index].Add(typing);
        }
      }

      for (int index = 0; index < parserOrder.Length; index++) {
        MandarinParser parser = parserOrder[index];
        var rejected = new List<string>();
        foreach (string typing in typings[index]) {
          if (!new Composer(arrange: parser).IsSequentiallyTypedRawKeyOrder(typing, true)) {
            rejected.Add(typing);
          }
        }

        Assert.IsEmpty(
          rejected,
          $"{parser.NameTag()} 有 {rejected.Count} 筆合法編碼未被接受：" +
          string.Join(", ", rejected.Take(8)));
      }
    }

    [Test]
    public void TestRawKeyOrderPinyinLayouts() {
      var cases = new List<(MandarinParser Parser, string Input, bool Expected)> {
        (MandarinParser.OfHanyuPinyin, "su3", true),
        (MandarinParser.OfHanyuPinyin, "suan", true),
        (MandarinParser.OfHanyuPinyin, "suan3", true),
        (MandarinParser.OfHanyuPinyin, "su", true),
        (MandarinParser.OfHanyuPinyin, "su ", true),
        (MandarinParser.OfHanyuPinyin, "zhong", true),
        (MandarinParser.OfHanyuPinyin, "shi", true),
        (MandarinParser.OfHanyuPinyin, "nv", true),
        (MandarinParser.OfHanyuPinyin, "3su", false), // 聲調前置、為引擎所丟棄
        (MandarinParser.OfHanyuPinyin, "s u", false), // 聲調夾於字中、同遭丟棄
        (MandarinParser.OfHanyuPinyin, "sh", false), // 未成音節
        (MandarinParser.OfHanyuPinyin, "S", false), // 大寫非該排列之按鍵
        (MandarinParser.OfHanyuPinyin, "hello", false),
        (MandarinParser.OfHanyuPinyin, "us", false),
        (MandarinParser.OfSecondaryPinyin, "chiung2", true),
        (MandarinParser.OfSecondaryPinyin, "zhong", false),
        (MandarinParser.OfYalePinyin, "jung", true),
        (MandarinParser.OfYalePinyin, "suan", false),
        (MandarinParser.OfHualuoPinyin, "suan", true),
        (MandarinParser.OfUniversalPinyin, "suan", true),
        (MandarinParser.OfWadeGilesPinyin, "jung", true),
      };
      CheckSequentialReadings(cases);
    }

    [Test]
    public void TestRawKeyOrderDoesNotMutateComposer() {
      var composer = new Composer(arrange: MandarinParser.OfDachen);
      _ = composer.ReceiveSequence("hl3");
      Composer snapshotOfComposer = composer;
      Assert.IsTrue(composer.IsSequentiallyTypedRawKeyOrder("1u,"));
      Assert.AreEqual(expected: snapshotOfComposer, actual: composer);
      // 呼叫端之 CSVT 順序強制設定不應影響本 API 之判定（槽序由本 API 自行觀測）。
      composer.EnforceCSVTOrdering = true;
      snapshotOfComposer.EnforceCSVTOrdering = true;
      Assert.IsTrue(composer.IsSequentiallyTypedRawKeyOrder("dcl", true));
      Assert.AreEqual(expected: snapshotOfComposer, actual: composer);
    }

    /// <summary>
    /// 依 Swift 版測項之形式逐筆檢證，並於失敗時給出可讀訊息。
    /// </summary>
    /// <param name="cases">各筆測例：注音排列、按鍵序列、期望值。</param>
    private static void CheckSequentialReadings(
      List<(MandarinParser Parser, string Input, bool Expected)> cases) {
      foreach (var (parser, input, expected) in cases) {
        var composer = new Composer(arrange: parser);
        Assert.AreEqual(
          actual: composer.IsSequentiallyTypedRawKeyOrder(input),
          expected: expected,
          message: $"{parser.NameTag()} \"{input}\" 應為 {expected}");
      }
    }
  }
}
