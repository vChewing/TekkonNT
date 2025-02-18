// (c) 2022 and onwards The vChewing Project (MIT-NTL License).
// ====================
// This code is released under the MIT license (SPDX-License-Identifier: MIT)
// ... with NTL restriction stating that:
// No trademark license is granted to use the trade names, trademarks, service
// marks, or product names of Contributor, except as required to fulfill notice
// requirements defined in MIT License.

using NUnit.Framework;

namespace Tekkon.Tests {
  public class TekkonTests {
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
      composer.ReceiveKeyFromPhonabet("ㄓ");
      composer.ReceiveKeyFromPhonabet("ㄧ");
      composer.ReceiveKeyFromPhonabet("ˋ");
      Assert.AreEqual(composer.Value, "ㄓˋ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet("ㄓ");
      composer.ReceiveKeyFromPhonabet("ㄩ");
      composer.ReceiveKeyFromPhonabet("ˋ");
      Assert.AreEqual(composer.Value, "ㄐㄩˋ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet("ㄓ");
      composer.ReceiveKeyFromPhonabet("ㄧ");
      composer.ReceiveKeyFromPhonabet("ㄢ");
      Assert.AreEqual(composer.Value, "ㄓㄢ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet("ㄓ");
      composer.ReceiveKeyFromPhonabet("ㄩ");
      composer.ReceiveKeyFromPhonabet("ㄢ");
      Assert.AreEqual(composer.Value, "ㄐㄩㄢ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet("ㄓ");
      composer.ReceiveKeyFromPhonabet("ㄧ");
      composer.ReceiveKeyFromPhonabet("ㄢ");
      composer.ReceiveKeyFromPhonabet("ˋ");
      Assert.AreEqual(composer.Value, "ㄓㄢˋ");

      composer.Clear();
      composer.ReceiveKeyFromPhonabet("ㄓ");
      composer.ReceiveKeyFromPhonabet("ㄩ");
      composer.ReceiveKeyFromPhonabet("ㄢ");
      composer.ReceiveKeyFromPhonabet("ˋ");
      Assert.AreEqual(composer.Value, "ㄐㄩㄢˋ");
    }
  }
}
