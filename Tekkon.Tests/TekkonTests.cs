// (c) 2022 and onwards The vChewing Project (MIT-NTL License).
/*
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

1. The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

2. No trademark license is granted to use the trade names, trademarks, service
marks, or product names of Contributor, except as required to fulfill notice
requirements above.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using NUnit.Framework;

namespace Tekkon.Tests {
public class TekkonTests {
  [Test]
  public void TestInitializingPhonabet() {
    Phonabet thePhonabetNull = new("0");
    Phonabet thePhonabetA = new("ㄉ");
    Phonabet thePhonabetB = new("ㄧ");
    Phonabet thePhonabetC = new("ㄠ");
    Phonabet thePhonabetD = new("ˇ");
    Assert.True(thePhonabetNull.Type == PhoneType.Null &&
                thePhonabetA.Type == PhoneType.Consonant &&
                thePhonabetB.Type == PhoneType.Semivowel &&
                thePhonabetC.Type == PhoneType.Vowel &&
                thePhonabetD.Type == PhoneType.Intonation);
  }

  [Test]
  public void TestIsValidKeyWithKeys() {
    bool result;
    Composer composer = new(arrange: MandarinParser.OfDachen);

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
    Composer composer = new(arrange: MandarinParser.OfDachen);
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
    Assert.AreEqual(Shared.CnvPhonaToTextbookReading("ㄓㄜ˙"), "˙ㄓㄜ");
    Assert.AreEqual(Shared.CnvHanyuPinyinToPhona("bian4-le5-tian1"),
                    "ㄅㄧㄢˋ-ㄌㄜ˙-ㄊㄧㄢ");
  }
}
}
