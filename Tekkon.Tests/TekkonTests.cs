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

namespace Tekkon.Tests;

public class TekkonTests {
  [Test]
  public void TestInitializingPhonabet() {
    Phonabet ThePhonabetNull = new("0");
    Phonabet ThePhonabetA = new("ㄉ");
    Phonabet ThePhonabetB = new("ㄧ");
    Phonabet ThePhonabetC = new("ㄠ");
    Phonabet ThePhonabetD = new("ˇ");
    Assert.True(ThePhonabetNull.Type == PhoneType.Null &&
                ThePhonabetA.Type == PhoneType.Consonant &&
                ThePhonabetB.Type == PhoneType.Semivowel &&
                ThePhonabetC.Type == PhoneType.Vowel &&
                ThePhonabetD.Type == PhoneType.Intonation);
  }

  [Test]
  public void TestIsValidKeyWithKeys() {
    bool Result = true;
    Composer Composer = new(Arrange: MandarinParser.OfDachen);

    /// Testing Failed Key
    Result = Composer.InputValidityCheck(0x0024);
    Assert.True(Result == false);

    // Testing Correct Qwerty Dachen Key
    Composer.EnsureParser(Arrange: MandarinParser.OfDachen);
    Result = Composer.InputValidityCheck(0x002F);
    Assert.True(Result == true);

    // Testing Correct ETen26 Key
    Composer.EnsureParser(Arrange: MandarinParser.OfETen26);
    Result = Composer.InputValidityCheck(0x0062);
    Assert.True(Result == true);

    // Testing Correct Hanyu-Pinyin Key
    Composer.EnsureParser(Arrange: MandarinParser.OfHanyuPinyin);
    Result = Composer.InputValidityCheck(0x0062);
    Assert.True(Result == true);
  }

  // 下面這個測試不完全。完全版本放在 Intermediate 測試當中。
  [Test]
  public void TestPhonabetKeyReceivingAndCompositions() {
    Composer Composer = new(Arrange: MandarinParser.OfDachen);
    bool ToneMarkerIndicator = true;

    // Test Key Receiving;
    Composer.ReceiveKey(0x0032);  // 2, ㄉ
    Composer.ReceiveKey("j");     // ㄨ
    Composer.ReceiveKey("u");     // ㄧ
    Composer.ReceiveKey("l");     // ㄠ

    // Testing missing tone markers;
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(!ToneMarkerIndicator);

    Composer.ReceiveKey("3");  // 上聲
    Assert.AreEqual(actual: Composer.Value, expected: "ㄉㄧㄠˇ");
    Composer.DoBackSpace();
    Composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: Composer.Value,
                    expected: "ㄉㄧㄠ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄉㄧㄠ");

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄉㄧㄠ˙");

    // Testing having tone markers
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(ToneMarkerIndicator);

    // Testing having not-only tone markers
    ToneMarkerIndicator = Composer.HasToneMarker(WithNothingElse: true);
    Assert.True(!ToneMarkerIndicator);

    // Testing having only tone markers
    Composer.Clear();
    Composer.ReceiveKey("3");  // 上聲
    ToneMarkerIndicator = Composer.HasToneMarker(WithNothingElse: true);
    Assert.True(ToneMarkerIndicator);
  }
}
