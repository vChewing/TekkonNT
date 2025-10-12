// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Tekkon.Tests {
  public class TekkonTestsBasic {
    [Test]
    public void TestMandarinParser() {
      // 此測試僅用於填補測試覆蓋率。
      var composer = new Composer(arrange: MandarinParser.OfDachen);
      Assert.True(composer.IsEmpty);
      Assert.AreEqual(0, composer.Count(withIntonation: true));
      Assert.AreEqual(0, composer.Count(withIntonation: false));
      var composer2 = new Composer(arrange: MandarinParser.OfETen);
      composer2.EnsureParser(arrange: MandarinParser.OfDachen);
      Assert.AreEqual(composer.Parser, composer2.Parser);

      foreach (var parser in MandarinParserExtensions.AllCases) {
        Assert.AreNotEqual(parser.IsDynamic().ToString(), parser.NameTag());
        composer.EnsureParser(arrange: parser);
        // Translate 為私有方法，無法直接測試
        composer.Clear();
      }
    }

    [Test]
    public void TestInitializingPhonabet() {
      Phonabet thePhonabetNull = new Phonabet("0");
      Phonabet thePhonabetA = new Phonabet("ㄉ");
      Phonabet thePhonabetB = new Phonabet("ㄧ");
      Phonabet thePhonabetC = new Phonabet("ㄠ");
      Phonabet thePhonabetD = new Phonabet("ˇ");
      Assert.True(thePhonabetNull.Type == PhoneType.Null &&
            thePhonabetA.Type == PhoneType.Consonant &&
            thePhonabetB.Type == PhoneType.Semivowel &&
            thePhonabetC.Type == PhoneType.Vowel &&
            thePhonabetD.Type == PhoneType.Intonation);
      Assert.AreEqual(new Rune('~'), thePhonabetNull.ScalarValue);
      Assert.AreEqual(new Rune('ㄉ'), thePhonabetA.ScalarValue);
      Assert.AreEqual(new Rune('ㄧ'), thePhonabetB.ScalarValue);
      Assert.AreEqual(new Rune('ㄠ'), thePhonabetC.ScalarValue);
      Assert.AreEqual(new Rune('ˇ'), thePhonabetD.ScalarValue);

      Phonabet phonabetFromRune = new Phonabet(new Rune('ㄓ'));
      Assert.AreEqual(PhoneType.Consonant, phonabetFromRune.Type);
      Assert.AreEqual(new Rune('ㄓ'), phonabetFromRune.ScalarValue);
    }

    [Test]
    public void TestIsValidKeyWithKeys() {
      bool result;
      Composer composer = new Composer(arrange: MandarinParser.OfDachen);

      // Testing Failed Key
      result = composer.InputValidityCheck(0x0024);
      Assert.True(result == false);

      // Testing Correct Qwerty Dachen Key
      composer.EnsureParser(arrange: MandarinParser.OfDachen);
      result = composer.InputValidityCheck(0x002F);
      Assert.True(result);

      // Testing Correct ETen26 Key
      composer.EnsureParser(arrange: MandarinParser.OfETen26);
      result = composer.InputValidityCheck(0x0062);
      Assert.True(result);

      // Testing Correct Hanyu-Pinyin Key
      composer.EnsureParser(arrange: MandarinParser.OfHanyuPinyin);
      result = composer.InputValidityCheck(0x0062);
      Assert.True(result);
    }

    // 下面這個測試不完全。完全版本放在 Intermediate 測試當中。
    [Test]
    public void TestPhonabetKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfDachen);
      bool toneMarkerIndicator;

      // Test Key Receiving;
      composer.ReceiveKey(0x0032);  // 2, ㄉ
      composer.ReceiveKey("j");     // ㄨ
      composer.ReceiveKey("u");     // ㄧ
      composer.ReceiveKey("l");     // ㄠ

      // Testing missing tone markers;
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("3");  // 上聲
      Assert.AreEqual(actual: composer.Value, expected: "ㄉㄧㄠˇ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄉㄧㄠ ");  // 這裡回傳的結果的陰平是空格

      // Test Getting Displayed Composition
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ");

      // Test Tone 5
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ˙");

      // Testing having tone markers
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // Testing having not-only tone markers
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // Testing having only tone markers
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);

      // Testing auto phonabet combination fixing process.
      composer.PhonabetCombinationCorrectionEnabled = true;

      // Testing exceptions of handling "ㄅㄨㄛ ㄆㄨㄛ ㄇㄨㄛ ㄈㄨㄛ"
      composer.Clear();
      composer.ReceiveKey("1");
      composer.ReceiveKey("j");
      composer.ReceiveKey("i");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄅㄛ");
      composer.ReceiveKey("q");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄆㄛ");
      composer.ReceiveKey("a");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄇㄛ");
      composer.ReceiveKey("z");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄈㄛ");

      // Testing exceptions of handling "ㄅㄨㄥ ㄆㄨㄥ ㄇㄨㄥ ㄈㄨㄥ"
      composer.Clear();
      composer.ReceiveKey("1");
      composer.ReceiveKey("j");
      composer.ReceiveKey("/");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄅㄥ");
      composer.ReceiveKey("q");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄆㄥ");
      composer.ReceiveKey("a");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄇㄥ");
      composer.ReceiveKey("z");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄈㄥ");

      // Testing exceptions of handling "ㄋㄨㄟ ㄌㄨㄟ"
      composer.Clear();
      composer.ReceiveKey("s");
      composer.ReceiveKey("j");
      composer.ReceiveKey("o");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄋㄟ");
      composer.ReceiveKey("x");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄌㄟ");

      // Testing exceptions of handling "ㄧㄜ ㄩㄜ"
      composer.Clear();
      composer.ReceiveKey("s");
      composer.ReceiveKey("k");
      composer.ReceiveKey("u");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄋㄧㄝ");
      composer.ReceiveKey("s");
      composer.ReceiveKey("m");
      composer.ReceiveKey("k");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄋㄩㄝ");
      composer.ReceiveKey("s");
      composer.ReceiveKey("u");
      composer.ReceiveKey("k");
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄋㄧㄝ");

      // Testing exceptions of handling "ㄨㄜ ㄨㄝ"
      composer.Clear();
      composer.ReceiveKey("j");
      composer.ReceiveKey("k");
      Assert.AreEqual(composer.GetComposition(), "ㄩㄝ");
      composer.Clear();
      composer.ReceiveKey("j");
      composer.ReceiveKey(",");
      Assert.AreEqual(composer.GetComposition(), "ㄩㄝ");
      composer.Clear();
      composer.ReceiveKey(",");
      composer.ReceiveKey("j");
      Assert.AreEqual(composer.GetComposition(), "ㄩㄝ");
      composer.Clear();
      composer.ReceiveKey("k");
      composer.ReceiveKey("j");
      Assert.AreEqual(composer.GetComposition(), "ㄩㄝ");

      // Testing tool functions
      Assert.AreEqual(Shared.RestoreToneOneInPhona("ㄉㄧㄠ"), "ㄉㄧㄠ1");
      Assert.AreEqual(Shared.CnvPhonaToTextbookStyle("ㄓㄜ˙"), "˙ㄓㄜ");
      Assert.AreEqual(Shared.CnvPhonaToHanyuPinyin("ㄍㄢˋ"), "gan4");
      Assert.AreEqual(Shared.CnvHanyuPinyinToTextbookStyle("起(qi3)居(ju1)"),
                      "起(qǐ)居(jū)");
      Assert.AreEqual(Shared.CnvHanyuPinyinToPhona("bian4-le5-tian1"),
                      "ㄅㄧㄢˋ-ㄌㄜ˙-ㄊㄧㄢ");
      // 測試這種情形：「如果傳入的字串不包含任何半形英數內容的話，那麼應該直接將傳入的字串原樣返回」。
      Assert.AreEqual(Shared.CnvHanyuPinyinToPhona("ㄅㄧㄢˋ-˙ㄌㄜ-ㄊㄧㄢ"),
                      "ㄅㄧㄢˋ-˙ㄌㄜ-ㄊㄧㄢ");
    }

    [Test]
    public void TestPhonabetCombinationCorrection() {
      Composer composer =
          new Composer(arrange: MandarinParser.OfDachen, correction: true);
      composer.ReceiveKeyFromPhonabet(new Rune('ㄓ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄧ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ˋ'));
      Assert.AreEqual(composer.Value, "ㄓˋ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet(new Rune('ㄓ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄩ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ˋ'));
      Assert.AreEqual(composer.Value, "ㄐㄩˋ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet(new Rune('ㄓ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄧ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄢ'));
      Assert.AreEqual(composer.Value, "ㄓㄢ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet(new Rune('ㄓ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄩ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄢ'));
      Assert.AreEqual(composer.Value, "ㄐㄩㄢ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet(new Rune('ㄓ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄧ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄢ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ˋ'));
      Assert.AreEqual(composer.Value, "ㄓㄢˋ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet(new Rune('ㄓ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄩ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄢ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ˋ'));
      Assert.AreEqual(composer.Value, "ㄐㄩㄢˋ");
    }

    [Test]
    public void TestSemivowelNormalizationWithEncounteredVowels() {
      // 測試自動將「ㄩ」轉寫為「ㄨ」的情境，避免輸出異常的拼法。
      Composer composer =
          new Composer(arrange: MandarinParser.OfDachen, correction: true);
      composer.ReceiveKeyFromPhonabet("ㄩ");
      composer.ReceiveKeyFromPhonabet("ㄛ");
      Assert.AreEqual("ㄨㄛ", composer.Value);

      // 測試聲母搭配後維持相同邏輯。
      composer.Clear();
      composer.ReceiveKeyFromPhonabet("ㄅ");
      composer.ReceiveKeyFromPhonabet("ㄩ");
      composer.ReceiveKeyFromPhonabet("ㄛ");
      Assert.AreEqual("ㄅㄛ", composer.GetComposition());
    }

    [Test]
    public void TestPronounceableQueryKeyGate() {
      // 測試 pronounceableOnly 旗標對查詢鍵值的篩選行為。
      Composer composer = new Composer(arrange: MandarinParser.OfDachen);
      composer.ReceiveKeyFromPhonabet("ˊ");
      Assert.False(composer.IsPronounceable);
      Assert.IsNull(composer.PhonabetKeyForQuery(pronounceableOnly: true));
      Assert.AreEqual("ˊ", composer.PhonabetKeyForQuery(pronounceableOnly: false));
    }

    [Test]
    public void TestPinyinTrieBranchInsertKeepsExistingBranches() {
      // 測試 PinyinTrie 在同一節點新增多個分支時，既有讀音不會被覆蓋。
      PinyinTrie trie = new PinyinTrie(MandarinParser.OfDachen);
      trie.Insert("li", "ㄌㄧ");
      trie.Insert("lin", "ㄌㄧㄣ");
      trie.Insert("liu", "ㄌㄧㄡ");

      List<string> fetched = trie.Search("li");
      CollectionAssert.Contains(fetched, "ㄌㄧ");
      CollectionAssert.Contains(fetched, "ㄌㄧㄣ");
      CollectionAssert.Contains(fetched, "ㄌㄧㄡ");
    }

    [Test]
    public void TestMandarinParserExtensions() {
      Assert.True(MandarinParser.OfHanyuPinyin.IsPinyin());
      Assert.False(MandarinParser.OfDachen.IsPinyin());

      Assert.True(MandarinParser.OfHsu.IsDynamic());
      Assert.False(MandarinParser.OfIBM.IsDynamic());

      IReadOnlyDictionary<string, string> pinyinMap =
          MandarinParser.OfHanyuPinyin.MapZhuyinPinyin();
      Assert.NotNull(pinyinMap);
      Assert.True(pinyinMap.ContainsKey("zhe"));
      Assert.AreEqual("ㄓㄜ", pinyinMap["zhe"]);

      Assert.IsNull(MandarinParser.OfDachen.MapZhuyinPinyin());

      HashSet<string> hanyuReadings =
          MandarinParser.OfHanyuPinyin.AllPossibleReadings();
      Assert.True(hanyuReadings.Contains("shi"));
      Assert.True(hanyuReadings.Contains("shi4"));

      HashSet<string> zhuyinReadings =
          MandarinParser.OfDachen.AllPossibleReadings();
      Assert.True(zhuyinReadings.Contains("ㄅㄚ"));
      Assert.True(zhuyinReadings.Contains("ㄅㄚˋ"));

      Assert.AreEqual("Dachen", MandarinParser.OfDachen.NameTag());
      Assert.AreEqual("HanyuPinyin", MandarinParser.OfHanyuPinyin.NameTag());
    }

    [Test]
    public void TestChoppingRawComplex() {
      PinyinTrie trie = new PinyinTrie(MandarinParser.OfHanyuPinyin);
      List<string> segments = trie.Chop("shjdaz");
      CollectionAssert.AreEqual(new[] { "sh", "j", "da", "z" }, segments);
    }

    [Test]
    public void TestPinyinTrieConvertingPinyinChopsToZhuyin() {
      PinyinTrie trie = new PinyinTrie(MandarinParser.OfHanyuPinyin);

      // Test search functionality
      List<string> results = trie.Search("shi");
      Assert.IsNotNull(results);
      Assert.IsTrue(results.Any(item => item.Contains("ㄕ")));

      List<string> missing = trie.Search("xyz");
      Assert.IsNotNull(missing);
      Assert.IsEmpty(missing);

      // Test chopped pinyin to zhuyin conversion
      List<string> chopped = new List<string> { "shi", "jie", "da", "zhan" };
      List<string> zhuyin = trie.DeductChoppedPinyinToZhuyin(chopped, initialZhuyinOnly: false);
      Assert.AreEqual(chopped.Count, zhuyin.Count);
      Assert.IsTrue(zhuyin[0].Contains("ㄕ"));
      Assert.IsTrue(zhuyin[1].Contains("ㄐㄧ"));
      Assert.IsTrue(zhuyin[2].Contains("ㄉ"));
      Assert.IsTrue(zhuyin[3].Contains("ㄓ"));
    }
  }
}
