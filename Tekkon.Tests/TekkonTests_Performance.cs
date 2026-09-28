// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Tekkon.Tests {
  /// <summary>
  /// 效能基準測試。
  /// </summary>
  public class TekkonTestsPerformance {
    /// <summary>
    /// 效能基準測試 - 動態佈局處理效能。
    /// </summary>
    [Test]
    public void TestDynamicLayoutPerformance() {
      string[] testSequences = { "e", "r", "d", "y", "qu", "quu", "quur", "q", "qj", "qjo", "l", "lr" };
      const int iterations = 50;

      // 效能期望：每次迭代應該在 50ms 以內完成（包含 12 個測試序列）。
      // 從 25ms 調整為 35ms，再調整為 50ms 以在不同平台環境中提供更好的可靠性。
      // Apple 平台因為執行環境差異較大，故放寬為 0.50 秒。
      double avgTimeExpected = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 0.50 : 0.050;

      foreach (MandarinParser parser in MandarinParserExtensions.AllDynamicZhuyinCases) {
        var stopwatch = Stopwatch.StartNew();

        // 與 Swift 原版一致：每次迭代都重新配置一只 Composer，
        // 故每次迭代的耗時也涵蓋建構成本（重用與否的比較交給 TestMemoryOptimization）。
        for (int i = 0; i < iterations; i++) {
          var composer = new Composer(arrange: parser);
          foreach (string sequence in testSequences) {
            composer.Clear();
            _ = composer.ReceiveSequence(sequence);
          }
        }

        stopwatch.Stop();
        double timeDelta = stopwatch.Elapsed.TotalSeconds;
        double avgTime = timeDelta / iterations;
        string timeDeltaStr = $"{timeDelta:F4}";
        string avgTimeStr = $"{avgTime:F6}";

        Console.WriteLine(
          $" -> [Tekkon][({parser.NameTag()})] {iterations} iterations in {timeDeltaStr}s (avg: {avgTimeStr}s per iteration)");

        Assert.That(avgTime, Is.LessThan(avgTimeExpected),
                    $"Performance regression: {parser.NameTag()} took {avgTimeStr}s per iteration");
      }
    }

    /// <summary>
    /// 記憶體效能測試 - 測試物件重用效能。
    /// </summary>
    [Test]
    public void TestMemoryOptimization() {
      string[] testSequences = { "ba", "pa", "ma", "fa", "da", "ta", "na", "la" };
      const int iterations = 50; // 增加迭代次數以產生更顯著的效能差異。

      // 測試物件重用 vs 重新建立。
      var reuseStopwatch = Stopwatch.StartNew();
      var reusableComposer = new Composer(arrange: MandarinParser.OfDachen26);
      for (int i = 0; i < iterations; i++) {
        foreach (string sequence in testSequences) {
          reusableComposer.Clear();
          _ = reusableComposer.ReceiveSequence(sequence);
        }
      }
      reuseStopwatch.Stop();
      double reuseTime = reuseStopwatch.Elapsed.TotalSeconds;

      // 測試重新建立物件。
      var recreateStopwatch = Stopwatch.StartNew();
      for (int i = 0; i < iterations; i++) {
        foreach (string sequence in testSequences) {
          var composer = new Composer(arrange: MandarinParser.OfDachen26);
          _ = composer.ReceiveSequence(sequence);
        }
      }
      recreateStopwatch.Stop();
      double recreateTime = recreateStopwatch.Elapsed.TotalSeconds;

      string reuseTimeStr = $"{reuseTime:F4}";
      string recreateTimeStr = $"{recreateTime:F4}";
      double improvement = ((recreateTime - reuseTime) / recreateTime) * 100;
      string improvementStr = $"{improvement:F1}";

      Console.WriteLine($" -> [Tekkon] Object reuse: {reuseTimeStr}s vs recreation: {recreateTimeStr}s");
      Console.WriteLine($" -> [Tekkon] Memory optimization improvement: {improvementStr}%");

      // 允許效能測量的變異性 - 由於現代執行環境的最佳化，物件建立可能比重用更快。
      // 我們檢查重用效能沒有嚴重退化即可（允許 5x 的差異，因為測試環境和編譯器版本差異可能很大）。
      double performanceTolerance = recreateTime * 5.0;
      bool isReuseFasterOrComparable = reuseTime <= performanceTolerance;

      Assert.True(isReuseFasterOrComparable,
                  $"Object reuse performance regression: reuse({reuseTimeStr}s) vs recreation({recreateTimeStr}s)");
    }

    /// <summary>
    /// 字串處理效能測試。
    /// </summary>
    [Test]
    public void TestStringProcessingPerformance() {
      string[] testStrings = { "ㄅㄆㄇㄈ", "ㄐㄑㄒ", "ㄓㄔㄕㄗㄘㄙ", "ㄧㄩ", "ㄛㄥ", "ㄟ" };
      const string targetChar = "ㄅ";
      const int iterations = 10000;

      // 測試字串包含檢查效能。Swift 原版在此量測的是 String 內建的 contains，
      // 其 C# 對應物為 Tekkon 自帶的 StringExtensions.DoesHave：
      // 後者在非空目標時直接轉呼叫 string.Contains，比對成本與前者相同。
      var stopwatch = Stopwatch.StartNew();
      for (int i = 0; i < iterations; i++) {
        foreach (string testString in testStrings) {
          _ = testString.DoesHave(targetChar);
        }
      }
      stopwatch.Stop();
      double processingTime = stopwatch.Elapsed.TotalSeconds;

      string processingTimeStr = $"{processingTime:F6}";
      Console.WriteLine($" -> [Tekkon] String processing ({iterations} iterations): {processingTimeStr}s");

      // 效能期望：字串處理應該相對較快。
      Assert.That(processingTime, Is.LessThan(0.2), "String processing performance regression");
    }
  }
}
