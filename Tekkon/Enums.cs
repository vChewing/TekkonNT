// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;

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

  /// <summary>
  /// 提供與 <see cref="MandarinParser"/> 相關的輔助工具，協助維持與 Swift 版本的行為一致。
  /// </summary>
  public static class MandarinParserExtensions {
    /// <summary>判定指定排列是否為拼音模式。</summary>
    public static bool IsPinyin(this MandarinParser parser) => (int)parser >= 100;

    /// <summary>判定指定排列是否為動態注音排列。</summary>
    public static bool IsDynamic(this MandarinParser parser) => parser switch {
      MandarinParser.OfDachen26 => true,
      MandarinParser.OfETen26 => true,
      MandarinParser.OfHsu => true,
      MandarinParser.OfStarlight => true,
      MandarinParser.OfAlvinLiu => true,
      _ => false
    };

    /// <summary>列出所有可用的鍵盤排列。</summary>
    public static IEnumerable<MandarinParser> AllCases => Enum.GetValues<MandarinParser>();

    /// <summary>列出所有拼音排列。</summary>
    public static IEnumerable<MandarinParser> AllPinyinCases => AllCases.Where(self => self.IsPinyin());

    /// <summary>列出所有動態注音排列。</summary>
    public static IEnumerable<MandarinParser> AllDynamicZhuyinCases =>
        AllCases.Where(self => self.IsDynamic());

    /// <summary>列出所有靜態注音排列。</summary>
    public static IEnumerable<MandarinParser> AllStaticZhuyinCases =>
        AllCases.Where(self => !self.IsDynamic() && !self.IsPinyin());

    /// <summary>取得與拼音排列對應的注音查表。</summary>
    public static IReadOnlyDictionary<string, string>? MapZhuyinPinyin(this MandarinParser parser) =>
        parser switch {
          MandarinParser.OfHanyuPinyin => Shared.MapHanyuPinyin,
          MandarinParser.OfSecondaryPinyin => Shared.MapSecondaryPinyin,
          MandarinParser.OfYalePinyin => Shared.MapYalePinyin,
          MandarinParser.OfHualuoPinyin => Shared.MapHualuoPinyin,
          MandarinParser.OfUniversalPinyin => Shared.MapUniversalPinyin,
          MandarinParser.OfWadeGilesPinyin => Shared.MapWadeGilesPinyin,
          _ => null
        };

    /// <summary>提供排列的識別名稱，方便序列化或記錄。</summary>
    public static string NameTag(this MandarinParser parser) => parser switch {
      MandarinParser.OfDachen => "Dachen",
      MandarinParser.OfDachen26 => "Dachen26",
      MandarinParser.OfETen => "ETen",
      MandarinParser.OfETen26 => "ETen26",
      MandarinParser.OfHsu => "Hsu",
      MandarinParser.OfIBM => "IBM",
      MandarinParser.OfMiTAC => "MiTAC",
      MandarinParser.OfSeigyou => "Seigyou",
      MandarinParser.OfFakeSeigyou => "FakeSeigyou",
      MandarinParser.OfStarlight => "Starlight",
      MandarinParser.OfAlvinLiu => "AlvinLiu",
      MandarinParser.OfHanyuPinyin => "HanyuPinyin",
      MandarinParser.OfSecondaryPinyin => "SecondaryPinyin",
      MandarinParser.OfYalePinyin => "YalePinyin",
      MandarinParser.OfHualuoPinyin => "HualuoPinyin",
      MandarinParser.OfUniversalPinyin => "UniversalPinyin",
      MandarinParser.OfWadeGilesPinyin => "WadeGilesPinyin",
      _ => parser.ToString()
    };

    /// <summary>取得當前排列可能產生的所有讀音組合。</summary>
    public static HashSet<string> AllPossibleReadings(this MandarinParser parser) {
      string intonations = parser.IsPinyin() ? " 12345" : " ˊˇˋ˙";
      List<string> baseReadingStems = parser switch {
        MandarinParser.OfHanyuPinyin => Shared.MapHanyuPinyin.Keys.ToList(),
        MandarinParser.OfSecondaryPinyin => Shared.MapSecondaryPinyin.Keys.ToList(),
        MandarinParser.OfYalePinyin => Shared.MapYalePinyin.Keys.ToList(),
        MandarinParser.OfHualuoPinyin => Shared.MapHualuoPinyin.Keys.ToList(),
        MandarinParser.OfUniversalPinyin => Shared.MapUniversalPinyin.Keys.ToList(),
        MandarinParser.OfWadeGilesPinyin => Shared.MapWadeGilesPinyin.Keys.ToList(),
        _ => Shared.MapHanyuPinyin.Values.ToList()
      };

      HashSet<string> result = new HashSet<string>(baseReadingStems);
      foreach (char currentIntonation in intonations) {
        foreach (string stem in baseReadingStems) {
          result.Add(stem + currentIntonation);
        }
      }
      return result;
    }
  }
}
