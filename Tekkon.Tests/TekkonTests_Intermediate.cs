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
public class TekkonTestsIntermediate {
  [Test]
  public void TestPhonabetKeyReceivingAndCompositions_Intermediate() {
    Composer composer = new(arrange: MandarinParser.OfDachen);

    // Test Key Receiving;
    composer.ReceiveKey(0x0032);  // 2, ㄉ
    composer.ReceiveKey("j");     // ㄨ
    composer.ReceiveKey("u");     // ㄧ
    composer.ReceiveKey("l");     // ㄠ

    // Testing missing tone markers;
    bool toneMarkerIndicator = composer.HasIntonation();
    Assert.True(!toneMarkerIndicator);

    composer.ReceiveKey("3");  // 上聲
    Assert.AreEqual(actual: composer.Value, expected: "ㄉㄧㄠˇ");
    composer.DoBackSpace();
    composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: composer.Value,
                    expected: "ㄉㄧㄠ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                    expected: "diao1");  // 中階測試項目
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                    isTextBookStyle: true),
                    expected: "diāo");  // 中階測試項目
    Assert.AreEqual(
        actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
        expected: "diao1");  // 中階測試項目

    // Test Tone 5
    composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ˙");
    Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                    expected: "˙ㄉㄧㄠ");  // 中階測試項目

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
  }

  [Test]
  public void TestHanyuPinyinKeyReceivingAndCompositions_Full() {
    Composer composer = new(arrange: MandarinParser.OfHanyuPinyin);

    // Test Key Receiving
    composer.ReceiveKey(100);  // d
    composer.ReceiveKey("i");
    composer.ReceiveKey("a");
    composer.ReceiveKey("o");

    // Testing missing tone markers;
    bool toneMarkerIndicator = composer.HasIntonation();
    Assert.True(!toneMarkerIndicator);

    composer.ReceiveKey("3");  // 上聲
    Assert.AreEqual(actual: composer.Value, expected: "ㄉㄧㄠˇ");
    composer.DoBackSpace();
    composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: composer.Value,
                    expected: "ㄉㄧㄠ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition;
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                    expected: "diao1");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                    isTextBookStyle: true),
                    expected: "diāo");
    Assert.AreEqual(
        actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
        expected: "diao1");

    // Test Tone 5
    composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ˙");
    Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                    expected: "˙ㄉㄧㄠ");

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
  }

  [Test]
  public void TestSecondaryPinyinKeyReceivingAndCompositions() {
    Composer composer = new(arrange: MandarinParser.OfSecondaryPinyin);

    // Test Key Receiving
    composer.ReceiveKey(99);  // c
    composer.ReceiveKey("h");
    composer.ReceiveKey("i");
    composer.ReceiveKey("u");
    composer.ReceiveKey("n");
    composer.ReceiveKey("g");

    // Testing missing tone markers
    bool toneMarkerIndicator = composer.HasIntonation();
    Assert.True(!toneMarkerIndicator);

    composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
    composer.DoBackSpace();
    composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition;
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                    isTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
        expected: "chiung1");

    // Test Tone 5
    composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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
  }

  [Test]
  public void TestYalePinyinKeyReceivingAndCompositions() {
    Composer composer = new(arrange: MandarinParser.OfYalePinyin);

    // Test Key Receiving
    composer.ReceiveKey(99);  // c
    composer.ReceiveKey("h");
    composer.ReceiveKey("y");
    composer.ReceiveKey("u");
    composer.ReceiveKey("n");
    composer.ReceiveKey("g");

    // Testing missing tone markers
    bool toneMarkerIndicator = composer.HasIntonation();
    Assert.True(!toneMarkerIndicator);

    composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
    composer.DoBackSpace();
    composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition;
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                    isTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
        expected: "chyung1");

    // Test Tone 5
    composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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
  }

  [Test]
  public void TestHualuoPinyinKeyReceivingAndCompositions() {
    Composer composer = new(arrange: MandarinParser.OfHualuoPinyin);

    // Test Key Receiving
    composer.ReceiveKey(99);  // c
    composer.ReceiveKey("h");
    composer.ReceiveKey("y");
    composer.ReceiveKey("o");
    composer.ReceiveKey("n");
    composer.ReceiveKey("g");

    // Testing missing tone markers
    bool toneMarkerIndicator = composer.HasIntonation();
    Assert.True(!toneMarkerIndicator);

    composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
    composer.DoBackSpace();
    composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                    isTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
        expected: "chyong1");

    // Test Tone 5
    composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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
  }

  [Test]
  public void TestUniversalPinyinKeyReceivingAndCompositions() {
    Composer composer = new(arrange: MandarinParser.OfUniversalPinyin);

    // Test Key Receiving
    composer.ReceiveKey(99);  // c
    composer.ReceiveKey("y");
    composer.ReceiveKey("o");
    composer.ReceiveKey("n");
    composer.ReceiveKey("g");

    // Testing missing tone markers
    bool toneMarkerIndicator = composer.HasIntonation();
    Assert.True(!toneMarkerIndicator);

    composer.ReceiveKey("2");  // 陽平
    Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
    composer.DoBackSpace();
    composer.ReceiveKey(" ");  // 陰平
    Assert.AreEqual(actual: composer.Value,
                    expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

    // Test Getting Displayed Composition
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                    expected: "qiong1");
    Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                    isTextBookStyle: true),
                    expected: "qiōng");
    Assert.AreEqual(
        actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
        expected: "cyong1");

    // Test Tone 5
    composer.ReceiveKey("7");  // 輕聲
    Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
    Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                    expected: "˙ㄑㄩㄥ");

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
  }
}
}
