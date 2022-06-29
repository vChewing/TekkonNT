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
  /// <summary>
  /// 假。
  /// </summary>
  Null = 0,
  /// <summary>
  /// 聲母。
  /// </summary>
  Consonant = 1,
  /// <summary>
  /// 介母。
  /// </summary>
  Semivowel = 2,
  /// <summary>
  /// 韻母。
  /// </summary>
  Vowel = 3,
  /// <summary>
  /// 聲調。
  /// </summary>
  Intonation = 4
}

/// <summary>
/// 定義注音排列的類型。
/// </summary>
public enum MandarinParser {
  /// <summary>注音：大千傳統排列。</summary>
  OfDachen = 0,
  /// <summary>注音：酷音大千二十六鍵動態排列。</summary>
  OfDachen26 = 1,
  /// <summary>注音：倚天傳統排列。</summary>
  OfETen = 2,
  /// <summary>注音：倚天二十六鍵動態排列。</summary>
  OfETen26 = 3,
  /// <summary>注音：許氏動態排列。</summary>
  OfHsu = 4,
  /// <summary>注音：IBM 排列。</summary>
  OfIBM = 5,
  /// <summary>注音：神通排列。</summary>
  OfMiTAC = 6,
  /// <summary>注音：精業排列。</summary>
  OfSeigyou = 7,
  /// <summary>注音：偽精業排列。</summary>
  OfFakeSeigyou = 8,
  /// <summary>拼音：漢語拼音排列。</summary>
  OfHanyuPinyin = 100,
  /// <summary>拼音：國音二式排列。</summary>
  OfSecondaryPinyin = 101,
  /// <summary>拼音：耶魯拼音排列。</summary>
  OfYalePinyin = 102,
  /// <summary>拼音：華羅拼音排列。</summary>
  OfHualuoPinyin = 103,
  /// <summary>拼音：通用拼音排列。</summary>
  OfUniversalPinyin = 104
  // C# 似乎不支援在這裡直接給一個動態 var。
  // 所以，針對每個輸入模式的 token 轉換需要寫在輸入法內。
}
}