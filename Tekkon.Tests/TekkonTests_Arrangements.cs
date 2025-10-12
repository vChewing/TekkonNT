// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tekkon.Tests {
  /// <summary>
  /// 資料驅動測試案例的輔助類別。
  /// </summary>
  public class SubTestCase {
    public MandarinParser Parser { get; }
    public string Typing { get; }
    public string Expected { get; }

    public SubTestCase(MandarinParser parser, string typing, string expected) {
      // 略過以反引號標記的測試案例
      if (typing.StartsWith("`")) {
        Parser = parser;
        Typing = "";
        Expected = "";
        return;
      }

      Parser = parser;
      Typing = typing.Replace("_", " ");
      Expected = expected.Replace("_", " ");
    }

    public bool Verify() {
      if (string.IsNullOrEmpty(Typing)) return true;

      var composer = new Composer(arrange: Parser);
      string strResult = composer.CnvSequence(Typing);
      if (strResult == Expected) return true;

      string parserTag = Parser.NameTag();
      string strError = $"MISMATCH ({parserTag}): \"{Typing}\" -> \"{strResult}\" != \"{Expected}\"";
      Console.WriteLine(strError);
      return false;
    }
  }

  /// <summary>
  /// 靜態鍵盤排列測試（例如：大千排列）。
  /// </summary>
  public class TekkonTestsKeyboardArrangementsStatic {
    private void CheckEq(ref int counter, ref Composer composer, string strGivenSeq, string strExpected) {
      string strResult = composer.CnvSequence(strGivenSeq);
      if (strResult == strExpected) return;

      string parserTag = composer.Parser.NameTag();
      string strError = $"MISMATCH ({parserTag}): \"{strGivenSeq}\" -> \"{strResult}\" != \"{strExpected}\"";
      Console.WriteLine(strError);
      counter++;
    }

    [Test]
    public void TestQwertyDachenKeys() {
      // 測試大千傳統排列（QWERTY）
      var c = new Composer(arrange: MandarinParser.OfDachen);
      int counter = 0;
      CheckEq(ref counter, ref c, " ", " ");
      CheckEq(ref counter, ref c, "18 ", "ㄅㄚ ");
      CheckEq(ref counter, ref c, "m,4", "ㄩㄝˋ");
      CheckEq(ref counter, ref c, "5j/ ", "ㄓㄨㄥ ");
      CheckEq(ref counter, ref c, "fu.", "ㄑㄧㄡ");
      CheckEq(ref counter, ref c, "g0 ", "ㄕㄢ ");
      CheckEq(ref counter, ref c, "xup6", "ㄌㄧㄣˊ");
      CheckEq(ref counter, ref c, "xu;6", "ㄌㄧㄤˊ");
      CheckEq(ref counter, ref c, "z/", "ㄈㄥ");
      CheckEq(ref counter, ref c, "tjo ", "ㄔㄨㄟ ");
      CheckEq(ref counter, ref c, "284", "ㄉㄚˋ");
      CheckEq(ref counter, ref c, "2u4", "ㄉㄧˋ");
      CheckEq(ref counter, ref c, "hl3", "ㄘㄠˇ");
      CheckEq(ref counter, ref c, "5 ", "ㄓ ");
      CheckEq(ref counter, ref c, "193", "ㄅㄞˇ");
      Assert.AreEqual(0, counter);
    }
  }

  /// <summary>
  /// 動態鍵盤排列測試（大千26鍵、倚天26鍵、許氏鍵盤、星光鍵盤、劉氏鍵盤）。
  /// </summary>
  public class TekkonTestsKeyboardArrangementsDynamic {
    [Test]
    public void TestDynamicKeyLayouts() {
      // 取得所有動態排列
      var dynamicParsers = MandarinParserExtensions.AllDynamicZhuyinCases.ToList();

      foreach (var (parser, idxRaw) in dynamicParsers.Select((p, i) => (p, i))) {
        var cases = new List<SubTestCase>();
        Console.WriteLine($" -> [Tekkon] 準備動態鍵盤處理測試...");

        // 解析測試資料
        var lines = TekkonTestData.DynamicLayoutTable.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        bool isTitleLine = true;

        foreach (var line in lines) {
          if (isTitleLine) {
            isTitleLine = false;
            continue;
          }

          var cells = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
          if (cells.Length < 2) continue;

          string expected = cells[0];
          int idx = idxRaw + 1;
          if (idx >= cells.Length) continue;

          string typing = cells[idx];
          var testCase = new SubTestCase(parser, typing, expected);
          cases.Add(testCase);
        }

        var startTime = DateTime.Now;
        Console.WriteLine($" -> [Tekkon][({parser.NameTag()})] 開始動態鍵盤處理測試...");

        int failures = cases.Select(testCase => testCase.Verify() ? 0 : 1).Sum();

        Assert.AreEqual(0, failures,
                        $"[失敗] {parser.NameTag()} 處理失敗，共 {failures} 個錯誤結果。");

        var elapsed = DateTime.Now - startTime;
        Console.WriteLine($" -> [Tekkon][({parser.NameTag()})] 測試完成，耗時 {elapsed.TotalSeconds:F4} 秒。");
      }
    }
  }
}
