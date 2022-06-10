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

public class TekkonTests_Intermediate {
  [Test]
  public void TestPhonabetKeyReceivingAndCompositions_Intermediate() {
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
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true),
                    expected: "diao1");  // 中階測試項目
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true,
                                                    IsTextBookStyle: true),
                    expected: "diāo");  // 中階測試項目
    Assert.AreEqual(
        actual: Composer.GetInlineCompositionForIMK(IsHanyuPinyin: true),
        expected: "diao1");  // 中階測試項目

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄉㄧㄠ˙");
    Assert.AreEqual(actual: Composer.GetComposition(IsTextBookStyle: true),
                    expected: "˙ㄉㄧㄠ");  // 中階測試項目

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

  [Test]
  public void TestHanyuinyinKeyReceivingAndCompositions_Full() {
    Composer Composer = new(Arrange: MandarinParser.OfHanyuPinyin);
    bool ToneMarkerIndicator = true;

    // Test Key Receiving
    Composer.ReceiveKey(100);  // d
    Composer.ReceiveKey("i");
    Composer.ReceiveKey("a");
    Composer.ReceiveKey("o");

    // Testing missing tone markers;
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(!ToneMarkerIndicator);

    Composer.ReceiveKey("3");  // 上聲
    Assert.AreEqual(actual: Composer.Value, expected: "ㄉㄧㄠˇ");
    Composer.DoBackSpace();
    Composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: Composer.Value,
                    expected: "ㄉㄧㄠ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition;
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄉㄧㄠ");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true),
                    expected: "diao1");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true,
                                                    IsTextBookStyle: true),
                    expected: "diāo");
    Assert.AreEqual(
        actual: Composer.GetInlineCompositionForIMK(IsHanyuPinyin: true),
        expected: "diao1");

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄉㄧㄠ˙");
    Assert.AreEqual(actual: Composer.GetComposition(IsTextBookStyle: true),
                    expected: "˙ㄉㄧㄠ");

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

  [Test]
  public void TestSecondaryPinyinKeyReceivingAndCompositions() {
    Composer Composer = new(Arrange: MandarinParser.OfSecondaryPinyin);
    bool ToneMarkerIndicator = true;

    // Test Key Receiving
    Composer.ReceiveKey(99);  // c
    Composer.ReceiveKey("h");
    Composer.ReceiveKey("i");
    Composer.ReceiveKey("u");
    Composer.ReceiveKey("n");
    Composer.ReceiveKey("g");

    // Testing missing tone markers
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(!ToneMarkerIndicator);

    Composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: Composer.Value, expected: "ㄑㄩㄥˊ");
    Composer.DoBackSpace();
    Composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: Composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition;
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true,
                                                    IsTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: Composer.GetInlineCompositionForIMK(IsHanyuPinyin: true),
        expected: "chiung1");

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: Composer.GetComposition(IsTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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

  [Test]
  public void TestYalePinyinKeyReceivingAndCompositions() {
    Composer Composer = new(Arrange: MandarinParser.OfYalePinyin);
    bool ToneMarkerIndicator = true;

    // Test Key Receiving
    Composer.ReceiveKey(99);  // c
    Composer.ReceiveKey("h");
    Composer.ReceiveKey("y");
    Composer.ReceiveKey("u");
    Composer.ReceiveKey("n");
    Composer.ReceiveKey("g");

    // Testing missing tone markers
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(!ToneMarkerIndicator);

    Composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: Composer.Value, expected: "ㄑㄩㄥˊ");
    Composer.DoBackSpace();
    Composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: Composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition;
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true,
                                                    IsTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: Composer.GetInlineCompositionForIMK(IsHanyuPinyin: true),
        expected: "chyung1");

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: Composer.GetComposition(IsTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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

  [Test]
  public void TestHualuoPinyinKeyReceivingAndCompositions() {
    Composer Composer = new(Arrange: MandarinParser.OfHualuoPinyin);
    bool ToneMarkerIndicator = true;

    // Test Key Receiving
    Composer.ReceiveKey(99);  // c
    Composer.ReceiveKey("h");
    Composer.ReceiveKey("y");
    Composer.ReceiveKey("o");
    Composer.ReceiveKey("n");
    Composer.ReceiveKey("g");

    // Testing missing tone markers
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(!ToneMarkerIndicator);

    Composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: Composer.Value, expected: "ㄑㄩㄥˊ");
    Composer.DoBackSpace();
    Composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: Composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true,
                                                    IsTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: Composer.GetInlineCompositionForIMK(IsHanyuPinyin: true),
        expected: "chyong1");

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: Composer.GetComposition(IsTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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

  [Test]
  public void TestUniversalPinyinKeyReceivingAndCompositions() {
    Composer Composer = new(Arrange: MandarinParser.OfUniversalPinyin);
    bool ToneMarkerIndicator = true;

    // Test Key Receiving
    Composer.ReceiveKey(99);  // c
    Composer.ReceiveKey("y");
    Composer.ReceiveKey("o");
    Composer.ReceiveKey("n");
    Composer.ReceiveKey("g");

    // Testing missing tone markers
    ToneMarkerIndicator = Composer.HasToneMarker();
    Assert.True(!ToneMarkerIndicator);

    Composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: Composer.Value, expected: "ㄑㄩㄥˊ");
    Composer.DoBackSpace();
    Composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: Composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: Composer.GetComposition(IsHanyuPinyin: true,
                                                    IsTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: Composer.GetInlineCompositionForIMK(IsHanyuPinyin: true),
        expected: "cyong1");

    // Test Tone 5
    Composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: Composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: Composer.GetComposition(IsTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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
