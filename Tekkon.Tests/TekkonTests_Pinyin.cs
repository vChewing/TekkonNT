// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using NUnit.Framework;

namespace Tekkon.Tests {
  public class TekkonTestsPinyin {
    [Test]
    public void TestHanyuPinyinKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfHanyuPinyin);

      // 測試按鍵接收
      composer.ReceiveKey(100);  // d
      composer.ReceiveKey("i");
      composer.ReceiveKey("a");
      composer.ReceiveKey("o");

      // 測試缺少聲調標記
      bool toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("3");  // 上聲
      Assert.AreEqual(actual: composer.Value, expected: "ㄉㄧㄠˇ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄉㄧㄠ ");  // 這裡回傳的結果的陰平是空格

      // 測試取得顯示用組字
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                      expected: "diao1");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                      isTextBookStyle: true),
                      expected: "diāo");
      Assert.AreEqual(
          actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
          expected: "diao1");

      // 測試聲調 5（輕聲）
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄉㄧㄠ˙");
      Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                      expected: "˙ㄉㄧㄠ");

      // 測試是否有聲調標記
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // 測試是否有除聲調外的其他內容
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // 測試是否僅有聲調標記
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);
    }

    [Test]
    public void TestSecondaryPinyinKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfSecondaryPinyin);

      // 測試按鍵接收
      composer.ReceiveKey(99);  // c
      composer.ReceiveKey("h");
      composer.ReceiveKey("i");
      composer.ReceiveKey("u");
      composer.ReceiveKey("n");
      composer.ReceiveKey("g");

      // 測試缺少聲調標記
      bool toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("2");  // 陽平
      Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

      // 測試取得顯示用組字;
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                      expected: "qiong1");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                      isTextBookStyle: true),
                      expected: "qiōng");
      Assert.AreEqual(
          actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
          expected: "chiung1");

      // 測試聲調 5（輕聲）
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
      Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                      expected: "˙ㄑㄩㄥ");

      // 測試是否有聲調標記
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // 測試是否有除聲調外的其他內容
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // 測試是否僅有聲調標記
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);
    }

    [Test]
    public void TestYalePinyinKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfYalePinyin);

      // 測試按鍵接收
      composer.ReceiveKey(99);  // c
      composer.ReceiveKey("h");
      composer.ReceiveKey("y");
      composer.ReceiveKey("u");
      composer.ReceiveKey("n");
      composer.ReceiveKey("g");

      // 測試缺少聲調標記
      bool toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("2");  // 陽平
      Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

      // 測試取得顯示用組字;
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                      expected: "qiong1");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                      isTextBookStyle: true),
                      expected: "qiōng");
      Assert.AreEqual(
          actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
          expected: "chyung1");

      // 測試聲調 5（輕聲）
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
      Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                      expected: "˙ㄑㄩㄥ");

      // 測試是否有聲調標記
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // 測試是否有除聲調外的其他內容
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // 測試是否僅有聲調標記
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);
    }

    [Test]
    public void TestHualuoPinyinKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfHualuoPinyin);

      // 測試按鍵接收
      composer.ReceiveKey(99);  // c
      composer.ReceiveKey("h");
      composer.ReceiveKey("y");
      composer.ReceiveKey("o");
      composer.ReceiveKey("n");
      composer.ReceiveKey("g");

      // 測試缺少聲調標記
      bool toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("2");  // 陽平
      Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

      // 測試取得顯示用組字
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                      expected: "qiong1");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                      isTextBookStyle: true),
                      expected: "qiōng");
      Assert.AreEqual(
          actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
          expected: "chyong1");

      // 測試聲調 5（輕聲）
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
      Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                      expected: "˙ㄑㄩㄥ");

      // 測試是否有聲調標記
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // 測試是否有除聲調外的其他內容
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // 測試是否僅有聲調標記
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);
    }

    [Test]
    public void TestUniversalPinyinKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfUniversalPinyin);

      // 測試按鍵接收
      composer.ReceiveKey(99);  // c
      composer.ReceiveKey("y");
      composer.ReceiveKey("o");
      composer.ReceiveKey("n");
      composer.ReceiveKey("g");

      // 測試缺少聲調標記
      bool toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("2");  // 陽平
      Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

      // 測試取得顯示用組字
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                      expected: "qiong1");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                      isTextBookStyle: true),
                      expected: "qiōng");
      Assert.AreEqual(
          actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
          expected: "cyong1");

      // 測試聲調 5（輕聲）
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
      Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                      expected: "˙ㄑㄩㄥ");

      // 測試是否有聲調標記
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // 測試是否有除聲調外的其他內容
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // 測試是否僅有聲調標記
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);
    }

    [Test]
    public void TestWadeGilesPinyinKeyReceivingAndCompositions() {
      Composer composer = new Composer(arrange: MandarinParser.OfWadeGilesPinyin);

      // 測試按鍵接收
      composer.ReceiveKey(99);  // c
      composer.ReceiveKey("h");
      composer.ReceiveKey("'");  // 韋氏拼音清濁分辨鍵
      composer.ReceiveKey("i");
      composer.ReceiveKey("u");
      composer.ReceiveKey("n");
      composer.ReceiveKey("g");

      // 測試缺少聲調標記
      bool toneMarkerIndicator = composer.HasIntonation();
      Assert.True(!toneMarkerIndicator);

      composer.ReceiveKey("2");  // 陽平
      Assert.AreEqual(actual: composer.Value, expected: "ㄑㄩㄥˊ");
      composer.DoBackSpace();
      composer.ReceiveKey(" ");  // 陰平
      Assert.AreEqual(actual: composer.Value,
                      expected: "ㄑㄩㄥ ");  // 這裡回傳的結果的陰平是空格

      // 測試取得顯示用組字
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true),
                      expected: "qiong1");
      Assert.AreEqual(actual: composer.GetComposition(isHanyuPinyin: true,
                                                      isTextBookStyle: true),
                      expected: "qiōng");
      Assert.AreEqual(
          actual: composer.GetInlineCompositionForDisplay(isHanyuPinyin: true),
          expected: "ch'iung1");

      // 測試聲調 5（輕聲）
      composer.ReceiveKey("7");  // 輕聲
      Assert.AreEqual(actual: composer.GetComposition(), expected: "ㄑㄩㄥ˙");
      Assert.AreEqual(actual: composer.GetComposition(isTextBookStyle: true),
                      expected: "˙ㄑㄩㄥ");

      // 測試是否有聲調標記
      toneMarkerIndicator = composer.HasIntonation();
      Assert.True(toneMarkerIndicator);

      // 測試是否有除聲調外的其他內容
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(!toneMarkerIndicator);

      // 測試是否僅有聲調標記
      composer.Clear();
      composer.ReceiveKey("3");  // 上聲
      toneMarkerIndicator = composer.HasIntonation(withNothingElse: true);
      Assert.True(toneMarkerIndicator);
    }
  }
}
