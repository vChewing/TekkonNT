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

using System;
using System.Linq;

namespace Tekkon {
/// 注音符號型別。本身與字串差不多，但卻只能被設定成一個注音符號字符。
/// 然後會根據自身的 value 的內容值自動計算自身的 PhoneType 類型（聲介韻調假）。
/// 如果遇到被設為多個字符、或者字符不對的情況的話，value 會被清空、PhoneType
/// 會變成 null。 賦值時最好直接重新 init 且一直用 let 來初期化 Phonabet。 其實
/// value 對外只讀，對內的話另有 valueStorage 代為存儲內容。這樣比較安全一些。
public struct Phonabet {
  // =======================
  // MARK: Basic Static Constants.
  // -----------------------

  /// 引擎僅接受這些記號作為聲母
  public static string AllowedConsonants =>
      "ㄅㄆㄇㄈㄉㄊㄋㄌㄍㄎㄏㄐㄑㄒㄓㄔㄕㄖㄗㄘㄙ";

  /// 引擎僅接受這些記號作為介母
  public static string AllowedSemivowels => "ㄧㄨㄩ";

  /// 引擎僅接受這些記號作為韻母
  public static string AllowedVowels => "ㄚㄛㄜㄝㄞㄟㄠㄡㄢㄣㄤㄥㄦ";

  /// 引擎僅接受這些記號作為聲調
  public static string AllowedIntonations => " ˊˇˋ˙";

  /// 引擎僅接受這些記號作為注音（聲介韻調四個集合加起來）
  public static string AllowedPhonabets => AllowedConsonants +
                                           AllowedSemivowels + AllowedVowels +
                                           AllowedIntonations;

  // =======================
  // MARK: Phonabet Structure.
  // -----------------------
  public PhoneType Type = PhoneType.Null;
  private string ValueStorage = "";
  public string Value => ValueStorage;
  public bool IsEmpty => (ValueStorage == null) || (ValueStorage.Length == 0);

  /// 初期化，會根據傳入的 input 字串參數來自動判定自身的 PhoneType 類型屬性值。
  public Phonabet(string Input = "") {
    if (Input.Length > 0) {
      if (AllowedPhonabets.Contains(Input.LastOrDefault())) {
        ValueStorage = Input.LastOrDefault().ToString();
        EnsureType();
      }
    }
  }

  /// 自我清空內容。
  public void Clear() {
    ValueStorage = "";
    Type = PhoneType.Null;
  }

  /// 自我變換資料值。
  /// - Parameters:
  ///   - strOf: 要取代的內容。
  ///   - strWith: 要取代成的內容。
  public void SelfReplace(string StrOf, string StrWith) {
    ValueStorage = ValueStorage.Replace(StrOf, StrWith);
    EnsureType();
  }

  /// 用來自動更新自身的屬性值的函數。
  public void EnsureType() {
    if (AllowedConsonants.Contains(Value)) {
      Type = PhoneType.Consonant;
    } else if (AllowedSemivowels.Contains(Value)) {
      Type = PhoneType.Semivowel;
    } else if (AllowedVowels.Contains(Value)) {
      Type = PhoneType.Vowel;
    } else if (AllowedIntonations.Contains(Value)) {
      Type = PhoneType.Intonation;
    } else {
      Type = PhoneType.Null;
      ValueStorage = "";
    }
  }

  // =======================
  // MARK: Misc Definitions.
  // -----------------------
  public override int GetHashCode() { return HashCode.Combine(Value); }
  public override string ToString() => $"{Value}";
}
}