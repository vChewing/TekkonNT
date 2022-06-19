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

namespace Tekkon {
/// <summary>
/// 定義注音符號的種類。
/// </summary>
public enum PhoneType {
  Null = 0,       // 假
  Consonant = 1,  // 聲
  Semivowel = 2,  // 介
  Vowel = 3,      // 韻
  Intonation = 4  // 調
}

/// <summary>
/// 定義注音排列的類型。
/// </summary>
public enum MandarinParser {
  OfDachen = 0,
  OfDachen26 = 1,
  OfETen = 2,
  OfETen26 = 3,
  OfHsu = 4,
  OfIBM = 5,
  OfMiTAC = 6,
  OfSeigyou = 7,
  OfFakeSeigyou = 8,
  OfHanyuPinyin = 100,
  OfSecondaryPinyin = 101,
  OfYalePinyin = 102,
  OfHualuoPinyin = 103,
  OfUniversalPinyin = 104
  // C# 似乎不支援在這裡直接給一個動態 var。
  // 所以，針對每個輸入模式的 token 轉換需要寫在輸入法內。
}
}