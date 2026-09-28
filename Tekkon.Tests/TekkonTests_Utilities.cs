// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tekkon.Tests {
  public class TekkonTestsUtilities {
    [Test]
    public void TestHasStringEdgeCases() {
      Assert.True("ㄅㄧㄢˋ".DoesHave("ㄧㄢ"));
      Assert.True(!"ㄅㄧㄢˋ".DoesHave("ㄧㄥ"));
      Assert.True("aaa".DoesHave("aa"));
      Assert.True(!"x".DoesHave("xyz"));
      // 空目標的既有語義：僅當自身為空時為 true。
      Assert.True("".DoesHave(""));
      Assert.True(!"a".DoesHave(""));
      Assert.True(!"".DoesHave("a"));
    }

    [Test]
    public void TestRestoreToneOneEdgeCases() {
      // 空字串防呆。
      Assert.AreEqual(actual: Shared.RestoreToneOneInPhona(""), expected: "");
      Assert.AreEqual(actual: Shared.RestoreToneOneInPhona("ㄉㄧㄠ"), expected: "ㄉㄧㄠ1");
      Assert.AreEqual(actual: Shared.RestoreToneOneInPhona("ㄉㄧㄠˋ"), expected: "ㄉㄧㄠˋ");
      Assert.AreEqual(actual: Shared.RestoreToneOneInPhona("ㄉㄧㄠ˙"), expected: "ㄉㄧㄠ˙");
    }

    [Test]
    public void TestPhonaToPinyinFullTableSweep() {
      // 對照表全表掃描：bucket 化之後每筆條目仍須精確命中。
      foreach (string[] pair in Shared.ArrPhonaToHanyuPinyin!)
        Assert.AreEqual(actual: Shared.CnvPhonaToHanyuPinyin(pair[0]), expected: pair[1]);
    }

    [Test]
    public void TestPhonaToPinyinLongestMatch() {
      // 最長比對優先：三字組合不得被拆成「聲母＋韻母」。
      Assert.AreEqual(actual: Shared.CnvPhonaToHanyuPinyin("ㄅㄧㄥ"), expected: "bing");
      Assert.AreEqual(actual: Shared.CnvPhonaToHanyuPinyin("ㄅㄧㄥˋ"), expected: "bing4");
      // 未命中條目的字元原樣保留。
      Assert.AreEqual(actual: Shared.CnvPhonaToHanyuPinyin("幹"), expected: "幹");
    }

    [Test]
    public void TestPinyinToPhonaCompound() {
      Assert.AreEqual(actual: Shared.CnvHanyuPinyinToPhona("shang4"), expected: "ㄕㄤˋ");
      Assert.AreEqual(actual: Shared.CnvHanyuPinyinToPhona("zhang1"), expected: "ㄓㄤ");
      Assert.AreEqual(actual: Shared.CnvHanyuPinyinToPhona("zhang1", " "), expected: "ㄓㄤ ");
      // 含不允許字元（非半形英數）時放棄轉換、原樣回傳。
      Assert.AreEqual(actual: Shared.CnvHanyuPinyinToPhona("nǐ"), expected: "nǐ");
    }

    [Test]
    public void TestMakeToneInsensitiveVariantsBasic() {
      // 無調讀音展開為同音節聲調候選桶：陰平以空字串表示，
      // 順序依 Phonabet.AllowedIntonations（" ", "ˊ", "ˇ", "ˋ", "˙"）。
      List<string> result = Shared.MakeToneInsensitiveVariants("ㄕ");
      Assert.AreEqual(actual: result,
                      expected: new[] { "ㄕ", "ㄕˊ", "ㄕˇ", "ㄕˋ", "ㄕ˙" });
      // 去重守衛：回傳內容不得重複。
      Assert.AreEqual(actual: result.Count, expected: Phonabet.AllowedIntonations.Count);
      Assert.AreEqual(actual: result.Distinct().Count(), expected: result.Count);
    }

    [Test]
    public void TestMakeToneInsensitiveVariantsEmptyReading() {
      Assert.AreEqual(actual: Shared.MakeToneInsensitiveVariants(""),
                      expected: new[] { "", "ˊ", "ˇ", "ˋ", "˙" });
    }
  }
}
