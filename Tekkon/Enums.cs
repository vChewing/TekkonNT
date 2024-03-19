// (c) 2022 and onwards The vChewing Project (MIT-NTL License).
// ====================
// This code is released under the MIT license (SPDX-License-Identifier: MIT)
// ... with NTL restriction stating that:
// No trademark license is granted to use the trade names, trademarks, service
// marks, or product names of Contributor, except as required to fulfill notice
// requirements defined in MIT License.

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
  /// <summary>注音：星光動態排列。</summary>
  OfStarlight = 9,
  /// <summary>注音：劉氏動態排列。</summary>
  OfAlvinLiu = 10,
  /// <summary>拼音：漢語拼音排列。</summary>
  OfHanyuPinyin = 100,
  /// <summary>拼音：國音二式排列。</summary>
  OfSecondaryPinyin = 101,
  /// <summary>拼音：耶魯拼音排列。</summary>
  OfYalePinyin = 102,
  /// <summary>拼音：華羅拼音排列。</summary>
  OfHualuoPinyin = 103,
  /// <summary>拼音：通用拼音排列。</summary>
  OfUniversalPinyin = 104,
  /// <summary>拼音：韋氏拼音排列。</summary>
  OfWadeGilesPinyin = 105
  // C# 似乎不支援在這裡直接給一個動態 var。
  // 所以，針對每個輸入模式的 token 轉換需要寫在輸入法內。
}
}