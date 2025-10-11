// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tekkon {
  /// <summary>
  /// 注音符號型別。本身與字串差不多，但卻只能被設定成一個注音符號字符。
  /// 然後會根據自身的 value 的內容值自動計算自身的 PhoneType 類型（聲介韻調假）。
  /// 如果遇到被設為多個字符、或者字符不對的情況的話，value 會被清空、PhoneType
  /// 會變成 null。 賦值時最好直接重新 init 且一直用 let 來初期化 Phonabet。 其實
  /// value 對外只讀，對內的話另有 valueStorage 代為存儲內容。這樣比較安全一些。
  /// </summary>
  public struct Phonabet : IEquatable<Phonabet> {
    private static readonly Rune NullRune = new Rune('~');
    private static readonly Rune[] AllowedConsonantRunes = BuildRunes("ㄅㄆㄇㄈㄉㄊㄋㄌㄍㄎㄏㄐㄑㄒㄓㄔㄕㄖㄗㄘㄙ");
    private static readonly Rune[] AllowedSemivowelRunes = BuildRunes("ㄧㄨㄩ");
    private static readonly Rune[] AllowedVowelRunes = BuildRunes("ㄚㄛㄜㄝㄞㄟㄠㄡㄢㄣㄤㄥㄦ");
    private static readonly Rune[] AllowedIntonationRunes = BuildRunes(" ˊˇˋ˙");
    private static readonly Rune[] AllowedPhonabetRunes =
        AllowedConsonantRunes.Concat(AllowedSemivowelRunes)
            .Concat(AllowedVowelRunes).Concat(AllowedIntonationRunes).ToArray();

    /// <summary>
    /// 引擎僅接受這些記號作為聲母。
    /// </summary>
    public static IReadOnlyList<Rune> AllowedConsonants => AllowedConsonantRunes;

    /// <summary>
    /// 引擎僅接受這些記號作為介母。
    /// </summary>
    public static IReadOnlyList<Rune> AllowedSemivowels => AllowedSemivowelRunes;

    /// <summary>
    /// 引擎僅接受這些記號作為韻母。
    /// </summary>
    public static IReadOnlyList<Rune> AllowedVowels => AllowedVowelRunes;

    /// <summary>
    /// 引擎僅接受這些記號作為聲調。
    /// </summary>
    public static IReadOnlyList<Rune> AllowedIntonations => AllowedIntonationRunes;

    /// <summary>
    /// 引擎僅接受這些記號作為注音（聲介韻調四個集合加起來）。
    /// </summary>
    public static IReadOnlyList<Rune> AllowedPhonabets => AllowedPhonabetRunes;

    /// <summary>
    /// 聲介韻調種類。
    /// </summary>
    public PhoneType Type { get; private set; }

    /// <summary>
    /// 承載的 Unicode Scalar 內容。
    /// </summary>
    public Rune ScalarValue { get; private set; }

    /// <summary>
    /// 承載的字串內容。
    /// </summary>
    public string Value => IsValid ? ScalarValue.ToString() : string.Empty;

    /// <summary>
    /// 該 Phonabet 注音符號組件內容是否為空。
    /// </summary>
    public bool IsEmpty => Type == PhoneType.Null;

    /// <summary>
    /// 稟明自身是否是合理的聲介韻調。
    /// </summary>
    public bool IsValid => Type != PhoneType.Null;

    /// <summary>
    /// 初期化，會根據傳入的 input 字串參數來自動判定自身的 PhoneType 類型屬性值。
    /// </summary>
    /// <param name="input">傳入的字串參數。</param>
    public Phonabet(string input = "") {
      ScalarValue = NullRune;
      Type = PhoneType.Null;
      if (!string.IsNullOrEmpty(input)) {
        Rune? lastRune = null;
        foreach (Rune current in input.EnumerateRunes()) lastRune = current;
        if (lastRune.HasValue && IsAllowedPhonabet(lastRune.Value)) ScalarValue = lastRune.Value;
      }
      EnsureType();
    }

    /// <summary>
    /// 初期化，會根據傳入的 Unicode Scalar 參數來自動判定自身的 PhoneType 類型屬性值。
    /// </summary>
    /// <param name="input">傳入的 Unicode Scalar 參數。</param>
    public Phonabet(Rune input) {
      ScalarValue = NullRune;
      Type = PhoneType.Null;
      if (IsAllowedPhonabet(input)) ScalarValue = input;
      EnsureType();
    }

    /// <summary>
    /// 自我清空內容。
    /// </summary>
    public void Clear() {
      ScalarValue = NullRune;
      Type = PhoneType.Null;
    }

    /// <summary>
    /// 自我變換資料值。
    /// </summary>
    /// <param name="strOf">要取代的內容。</param>
    /// <param name="strWith">要取代成的內容。</param>
    public void SelfReplace(Rune strOf, Rune? strWith = null) {
      if (!IsValid) return;
      if (ScalarValue.Equals(strOf)) {
        ScalarValue = strWith ?? NullRune;
      }
      EnsureType();
    }

    /// <summary>
    /// 指定新的值並重新審核其 PhoneType。
    /// </summary>
    /// <param name="newValue">新的注音 Unicode Scalar。</param>
    public void SetValue(Rune newValue) {
      ScalarValue = newValue;
      EnsureType();
    }

    /// <summary>
    /// 返回經過字串化的資料實體。
    /// </summary>
    public override string ToString() { return Value; }

    /// <summary>
    /// 返回經過雜湊化的資料實體。
    /// </summary>
    public override int GetHashCode() { return HashCode.Combine(ScalarValue, Type); }

    /// <summary>判定與傳入物件之間的值是否相同。</summary>
    public override bool Equals(object obj) {
      return obj is Phonabet other && Equals(other);
    }

    /// <summary>判定與另一個 <see cref="Phonabet"/> 是否相同。</summary>
    public bool Equals(Phonabet other) {
      return ScalarValue.Equals(other.ScalarValue) && Type == other.Type;
    }

    /// <summary>判定兩個 <see cref="Phonabet"/> 是否相等。</summary>
    public static bool operator ==(Phonabet left, Phonabet right) => left.Equals(right);

    /// <summary>判定兩個 <see cref="Phonabet"/> 是否不相等。</summary>
    public static bool operator !=(Phonabet left, Phonabet right) => !left.Equals(right);

    private void EnsureType() {
      if (Contains(AllowedConsonantRunes, ScalarValue))
        Type = PhoneType.Consonant;
      else if (Contains(AllowedSemivowelRunes, ScalarValue))
        Type = PhoneType.Semivowel;
      else if (Contains(AllowedVowelRunes, ScalarValue))
        Type = PhoneType.Vowel;
      else if (Contains(AllowedIntonationRunes, ScalarValue))
        Type = PhoneType.Intonation;
      else {
        Type = PhoneType.Null;
        ScalarValue = NullRune;
      }
    }

    private static Rune[] BuildRunes(string source) {
      return source.EnumerateRunes().ToArray();
    }

    private static bool Contains(IEnumerable<Rune> source, Rune value) {
      foreach (Rune current in source) {
        if (current.Equals(value)) return true;
      }
      return false;
    }

    private static bool IsAllowedPhonabet(Rune value) {
      return Contains(AllowedPhonabetRunes, value);
    }
  }
}
