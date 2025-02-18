// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System.Linq;

namespace Tekkon {
  /// <summary>
  /// 注音符號型別。本身與字串差不多，但卻只能被設定成一個注音符號字符。
  /// 然後會根據自身的 value 的內容值自動計算自身的 PhoneType 類型（聲介韻調假）。
  /// 如果遇到被設為多個字符、或者字符不對的情況的話，value 會被清空、PhoneType
  /// 會變成 null。 賦值時最好直接重新 init 且一直用 let 來初期化 Phonabet。 其實
  /// value 對外只讀，對內的話另有 valueStorage 代為存儲內容。這樣比較安全一些。
  /// </summary>
  public struct Phonabet {
    // =======================
    // MARK: Basic Static Constants.
    // -----------------------

    /// <summary>
    /// 引擎僅接受這些記號作為聲母
    /// </summary>
    public static string AllowedConsonants =>
        "ㄅㄆㄇㄈㄉㄊㄋㄌㄍㄎㄏㄐㄑㄒㄓㄔㄕㄖㄗㄘㄙ";

    /// <summary>
    /// 引擎僅接受這些記號作為介母
    /// </summary>
    public static string AllowedSemivowels => "ㄧㄨㄩ";

    /// <summary>
    /// 引擎僅接受這些記號作為韻母
    /// </summary>
    public static string AllowedVowels => "ㄚㄛㄜㄝㄞㄟㄠㄡㄢㄣㄤㄥㄦ";

    /// <summary>
    /// 引擎僅接受這些記號作為聲調
    /// </summary>
    public static string AllowedIntonations => " ˊˇˋ˙";

    /// <summary>
    /// 引擎僅接受這些記號作為注音（聲介韻調四個集合加起來）
    /// </summary>
    public static string AllowedPhonabets => AllowedConsonants +
                                             AllowedSemivowels + AllowedVowels +
                                             AllowedIntonations;

    // =======================
    // MARK: Phonabet Structure.
    // -----------------------
    /// <summary>
    /// 聲介韻調種類。
    /// </summary>
    public PhoneType Type;
    /// <summary>
    /// 承載的字元內容。
    /// </summary>
    /// <value>要承載的字元內容值。</value>
    public string Value { get; private set; }
    /// <summary>
    /// 該 Phonabet 注音符號組件內容是否為空。
    /// </summary>
    /// <returns>若是空的話，則返回 true。</returns>
    public bool IsEmpty => string.IsNullOrEmpty(Value);
    /// <summary>
    /// 稟明自身是否是合理的聲介韻調。
    /// </summary>
    public bool IsValid => Type != PhoneType.Null;

    /// <summary>
    /// 初期化，會根據傳入的 input 字串參數來自動判定自身的 PhoneType 類型屬性值。
    /// </summary>
    /// <param name="input">傳入的字串參數</param>
    public Phonabet(string input = "") {
      Value = "";
      Type = PhoneType.Null;
      if (string.IsNullOrEmpty(input)) return;
      if (!AllowedPhonabets.Contains(input.LastOrDefault())) return;
      Value = input.LastOrDefault().ToString();
      EnsureType();
    }

    /// <summary>
    /// Unity 2017.4 用的 C# 6.0 的 String 是可以 null
    /// 的，影響了鐵恨引擎的一些內部運算。這裡用了這麼一個應對變數。
    /// </summary>
    public char Char => string.IsNullOrEmpty(Value) ? '？' : Value.First();

    /// <summary>
    /// 自我清空內容。
    /// </summary>
    public void Clear() {
      if (Value != null) Value = "";
      Type = PhoneType.Null;
    }

    /// <summary>
    /// 自我變換資料值。
    /// </summary>
    /// <param name="strOf">要取代的內容。</param>
    /// <param name="strWith">要取代成的內容。</param>
    public void SelfReplace(string strOf, string strWith) {
      Value = Value.Replace(strOf, strWith);
      EnsureType();
    }

    /// <summary>
    /// 用來自動更新自身的屬性值的函式。
    /// </summary>
    public void EnsureType() {
      if (AllowedConsonants.Contains(Value))
        Type = PhoneType.Consonant;
      else if (AllowedSemivowels.Contains(Value))
        Type = PhoneType.Semivowel;
      else if (AllowedVowels.Contains(Value))
        Type = PhoneType.Vowel;
      else if (AllowedIntonations.Contains(Value))
        Type = PhoneType.Intonation;
      else {
        Type = PhoneType.Null;
        Value = "";
      }
    }

    // =======================
    // MARK: Misc Definitions.
    // -----------------------
    /// <summary>
    /// 返回經過雜湊化的資料實體。
    /// </summary>
    /// <returns>經過雜湊化的資料實體。</returns>
    public override int GetHashCode() { return Value.GetHashCode(); }
    /// <summary>
    /// 返回經過字串化的資料實體。
    /// </summary>
    /// <returns>經過字串化的資料實體。</returns>
    public override string ToString() => $"{Value}";
  }
}
