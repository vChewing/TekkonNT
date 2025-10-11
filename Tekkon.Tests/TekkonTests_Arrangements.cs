// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Tekkon.Tests {
  /// <summary>
  /// Helper class for data-driven test cases.
  /// </summary>
  public class SubTestCase {
    public MandarinParser Parser { get; }
    public string Typing { get; }
    public string Expected { get; }

    public SubTestCase(MandarinParser parser, string typing, string expected) {
      // Skip test cases marked with backtick
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
  /// Tests for static keyboard arrangements (e.g., Dachen).
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
      // Testing Dachen Traditional Mapping (QWERTY)
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
  /// Tests for dynamic keyboard arrangements (Dachen26, ETen26, Hsu, Starlight, AlvinLiu).
  /// </summary>
  public class TekkonTestsKeyboardArrangementsDynamic {
    [Test]
    public void TestDynamicKeyLayouts() {
      // Get all dynamic parsers
      var dynamicParsers = MandarinParserExtensions.AllDynamicZhuyinCases.ToList();
      
      foreach (var (parser, idxRaw) in dynamicParsers.Select((p, i) => (p, i))) {
        var cases = new List<SubTestCase>();
        Console.WriteLine($" -> [Tekkon] Preparing tests for dynamic keyboard handling...");
        
        // Parse test data
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
        Console.WriteLine($" -> [Tekkon][({parser.NameTag()})] Starting dynamic keyboard handling test ...");
        
        int failures = cases.Select(testCase => testCase.Verify() ? 0 : 1).Sum();
        
        Assert.AreEqual(0, failures, 
          $"[Failure] {parser.NameTag()} failed from being handled correctly with {failures} bad results.");
        
        var elapsed = DateTime.Now - startTime;
        Console.WriteLine($" -> [Tekkon][({parser.NameTag()})] Finished within {elapsed.TotalSeconds:F4} seconds.");
      }
    }
  }
}
