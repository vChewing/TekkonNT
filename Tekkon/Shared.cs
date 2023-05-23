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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tekkon {
/// <summary>
/// 該結構用來存放鐵恨引擎的一些公用資料。
/// </summary>
public struct Shared {
  // MARK: - Phonabet to Hanyu-Pinyin Conversion Processing

  /// <summary>
  /// 注音轉拼音，要求陰平必須是空格。
  /// </summary>
  /// <param name="targetJoined">傳入的 String 對象物件。</param>
  /// <returns>漢語拼音字串。</returns>
  public static string CnvPhonaToHanyuPinyin(string targetJoined) {
    return ArrPhonaToHanyuPinyin.Aggregate(
        targetJoined, (current, key) => current.Replace(key.Item1, key.Item2));
  }

  /// <summary>
  /// 漢語拼音數字標調式轉漢語拼音教科書格式，要求陰平必須是數字 1。
  /// </summary>
  /// <param name="targetJoined">傳入的 String 對象物件。</param>
  /// <returns>符合教科書排版規範的漢語拼音字串。</returns>
  public static string CnvHanyuPinyinToTextbookStyle(string targetJoined) {
    return ArrHanyuPinyinTextbookStyleConversionTable.Aggregate(
        targetJoined, (current, key) => current.Replace(key.Item1, key.Item2));
  }

  /// <summary>
  /// 該函式負責將注音轉為教科書印刷的方式（先寫輕聲）。
  /// </summary>
  /// <param
  /// name="target">要拿來做轉換處理的讀音。</param>
  /// <returns>經過轉換處理的讀音。</returns>
  public static string CnvPhonaToTextbookReading(string target) {
    if (target.Length == 0) return target;
    string newString = target;
    if (newString[^1] == '˙')
      newString = "˙" + newString.Substring(0, newString.Length - 1);
    return newString;
  }

  /// <summary>
  /// 該函式用來恢復注音當中的陰平聲調，恢復之後會以「1」表示陰平。
  /// </summary>
  /// <param
  /// name="target">要拿來做轉換處理的讀音。</param>
  /// <returns>經過轉換處理的讀音。</returns>
  public static string RestoreToneOneInPhona(string target) {
    if (target.Length == 0) return target;
    string newString = target;
    if (!newString.Contains('ˊ') && !newString.Contains('ˇ') &&
        !newString.Contains('ˋ') && !newString.Contains('˙'))
      newString += "1";
    return newString;
  }

  /// <summary>
  /// 該函式用來將漢語拼音轉為注音。
  /// </summary>
  /// <param name="targetJoined">要轉換的漢語拼音內容，要求必須帶有 12345
  /// 數字標調。</param>
  /// <param
  /// name="newToneOne">對陰平指定新的標記。預設情況下該標記為空字串。</param>
  /// <returns>轉換結果。</returns>
  public static string CnvHanyuPinyinToPhona(string targetJoined,
                                             string newToneOne = "") {
    if (targetJoined.Contains('_') ||
        !Regex.IsMatch(targetJoined, @".*[^A-Za-z0-9].*"))
      return targetJoined;
    foreach (string key in MapHanyuPinyin.Keys.OrderBy(x => x.Length)
                 .Reverse()) {
      if (targetJoined.Contains(key))
        targetJoined = targetJoined.Replace(key, MapHanyuPinyin[key]);
    }
    foreach (string key in MapArayuruPinyinIntonation.Keys
                 .OrderBy(x => x.Length)
                 .Reverse()) {
      if (targetJoined.Contains(key))
        targetJoined = targetJoined.Replace(
            key, key == "1" ? newToneOne : MapArayuruPinyinIntonation[key]);
    }
    return targetJoined;
  }

  /// <summary>
  /// 原始轉換對照表資料貯存專用佇列（數字標調格式）。<br />
  /// 排序很重要。先處理最長的，再處理短的。不然會出亂子。
  /// </summary>
  public readonly static (string, string)[] ArrPhonaToHanyuPinyin = {
    (" ", "1"),           ("ˊ", "2"),           ("ˇ", "3"),
    ("ˋ", "4"),           ("˙", "5"),           ("ㄅㄧㄝ", "bie"),
    ("ㄅㄧㄠ", "biao"),   ("ㄅㄧㄢ", "bian"),   ("ㄅㄧㄣ", "bin"),
    ("ㄅㄧㄥ", "bing"),   ("ㄆㄧㄚ", "pia"),    ("ㄆㄧㄝ", "pie"),
    ("ㄆㄧㄠ", "piao"),   ("ㄆㄧㄢ", "pian"),   ("ㄆㄧㄣ", "pin"),
    ("ㄆㄧㄥ", "ping"),   ("ㄇㄧㄝ", "mie"),    ("ㄇㄧㄠ", "miao"),
    ("ㄇㄧㄡ", "miu"),    ("ㄇㄧㄢ", "mian"),   ("ㄇㄧㄣ", "min"),
    ("ㄇㄧㄥ", "ming"),   ("ㄈㄧㄠ", "fiao"),   ("ㄈㄨㄥ", "fong"),
    ("ㄉㄧㄚ", "dia"),    ("ㄉㄧㄝ", "die"),    ("ㄉㄧㄠ", "diao"),
    ("ㄉㄧㄡ", "diu"),    ("ㄉㄧㄢ", "dian"),   ("ㄉㄧㄥ", "ding"),
    ("ㄉㄨㄛ", "duo"),    ("ㄉㄨㄟ", "dui"),    ("ㄉㄨㄢ", "duan"),
    ("ㄉㄨㄣ", "dun"),    ("ㄉㄨㄥ", "dong"),   ("ㄊㄧㄝ", "tie"),
    ("ㄊㄧㄠ", "tiao"),   ("ㄊㄧㄢ", "tian"),   ("ㄊㄧㄥ", "ting"),
    ("ㄊㄨㄛ", "tuo"),    ("ㄊㄨㄟ", "tui"),    ("ㄊㄨㄢ", "tuan"),
    ("ㄊㄨㄣ", "tun"),    ("ㄊㄨㄥ", "tong"),   ("ㄋㄧㄝ", "nie"),
    ("ㄋㄧㄠ", "niao"),   ("ㄋㄧㄡ", "niu"),    ("ㄋㄧㄢ", "nian"),
    ("ㄋㄧㄣ", "nin"),    ("ㄋㄧㄤ", "niang"),  ("ㄋㄧㄥ", "ning"),
    ("ㄋㄨㄛ", "nuo"),    ("ㄋㄨㄟ", "nui"),    ("ㄋㄨㄢ", "nuan"),
    ("ㄋㄨㄣ", "nun"),    ("ㄋㄨㄥ", "nong"),   ("ㄋㄩㄝ", "nve"),
    ("ㄌㄧㄚ", "lia"),    ("ㄌㄧㄝ", "lie"),    ("ㄌㄧㄠ", "liao"),
    ("ㄌㄧㄡ", "liu"),    ("ㄌㄧㄢ", "lian"),   ("ㄌㄧㄣ", "lin"),
    ("ㄌㄧㄤ", "liang"),  ("ㄌㄧㄥ", "ling"),   ("ㄌㄨㄛ", "luo"),
    ("ㄌㄨㄢ", "luan"),   ("ㄌㄨㄣ", "lun"),    ("ㄌㄨㄥ", "long"),
    ("ㄌㄩㄝ", "lve"),    ("ㄌㄩㄢ", "lvan"),   ("ㄍㄧㄠ", "giao"),
    ("ㄍㄧㄣ", "gin"),    ("ㄍㄨㄚ", "gua"),    ("ㄍㄨㄛ", "guo"),
    ("ㄍㄨㄜ", "gue"),    ("ㄍㄨㄞ", "guai"),   ("ㄍㄨㄟ", "gui"),
    ("ㄍㄨㄢ", "guan"),   ("ㄍㄨㄣ", "gun"),    ("ㄍㄨㄤ", "guang"),
    ("ㄍㄨㄥ", "gong"),   ("ㄎㄧㄡ", "kiu"),    ("ㄎㄧㄤ", "kiang"),
    ("ㄎㄨㄚ", "kua"),    ("ㄎㄨㄛ", "kuo"),    ("ㄎㄨㄞ", "kuai"),
    ("ㄎㄨㄟ", "kui"),    ("ㄎㄨㄢ", "kuan"),   ("ㄎㄨㄣ", "kun"),
    ("ㄎㄨㄤ", "kuang"),  ("ㄎㄨㄥ", "kong"),   ("ㄏㄨㄚ", "hua"),
    ("ㄏㄨㄛ", "huo"),    ("ㄏㄨㄞ", "huai"),   ("ㄏㄨㄟ", "hui"),
    ("ㄏㄨㄢ", "huan"),   ("ㄏㄨㄣ", "hun"),    ("ㄏㄨㄤ", "huang"),
    ("ㄏㄨㄥ", "hong"),   ("ㄐㄧㄚ", "jia"),    ("ㄐㄧㄝ", "jie"),
    ("ㄐㄧㄠ", "jiao"),   ("ㄐㄧㄡ", "jiu"),    ("ㄐㄧㄢ", "jian"),
    ("ㄐㄧㄣ", "jin"),    ("ㄐㄧㄤ", "jiang"),  ("ㄐㄧㄥ", "jing"),
    ("ㄐㄩㄝ", "jue"),    ("ㄐㄩㄢ", "juan"),   ("ㄐㄩㄣ", "jun"),
    ("ㄐㄩㄥ", "jiong"),  ("ㄑㄧㄚ", "qia"),    ("ㄑㄧㄝ", "qie"),
    ("ㄑㄧㄠ", "qiao"),   ("ㄑㄧㄡ", "qiu"),    ("ㄑㄧㄢ", "qian"),
    ("ㄑㄧㄣ", "qin"),    ("ㄑㄧㄤ", "qiang"),  ("ㄑㄧㄥ", "qing"),
    ("ㄑㄩㄝ", "que"),    ("ㄑㄩㄢ", "quan"),   ("ㄑㄩㄣ", "qun"),
    ("ㄑㄩㄥ", "qiong"),  ("ㄒㄧㄚ", "xia"),    ("ㄒㄧㄝ", "xie"),
    ("ㄒㄧㄠ", "xiao"),   ("ㄒㄧㄡ", "xiu"),    ("ㄒㄧㄢ", "xian"),
    ("ㄒㄧㄣ", "xin"),    ("ㄒㄧㄤ", "xiang"),  ("ㄒㄧㄥ", "xing"),
    ("ㄒㄩㄝ", "xue"),    ("ㄒㄩㄢ", "xuan"),   ("ㄒㄩㄣ", "xun"),
    ("ㄒㄩㄥ", "xiong"),  ("ㄓㄨㄚ", "zhua"),   ("ㄓㄨㄛ", "zhuo"),
    ("ㄓㄨㄞ", "zhuai"),  ("ㄓㄨㄟ", "zhui"),   ("ㄓㄨㄢ", "zhuan"),
    ("ㄓㄨㄣ", "zhun"),   ("ㄓㄨㄤ", "zhuang"), ("ㄓㄨㄥ", "zhong"),
    ("ㄔㄨㄚ", "chua"),   ("ㄔㄨㄛ", "chuo"),   ("ㄔㄨㄞ", "chuai"),
    ("ㄔㄨㄟ", "chui"),   ("ㄔㄨㄢ", "chuan"),  ("ㄔㄨㄣ", "chun"),
    ("ㄔㄨㄤ", "chuang"), ("ㄔㄨㄥ", "chong"),  ("ㄕㄨㄚ", "shua"),
    ("ㄕㄨㄛ", "shuo"),   ("ㄕㄨㄞ", "shuai"),  ("ㄕㄨㄟ", "shui"),
    ("ㄕㄨㄢ", "shuan"),  ("ㄕㄨㄣ", "shun"),   ("ㄕㄨㄤ", "shuang"),
    ("ㄖㄨㄛ", "ruo"),    ("ㄖㄨㄟ", "rui"),    ("ㄖㄨㄢ", "ruan"),
    ("ㄖㄨㄣ", "run"),    ("ㄖㄨㄥ", "rong"),   ("ㄗㄨㄛ", "zuo"),
    ("ㄗㄨㄟ", "zui"),    ("ㄗㄨㄢ", "zuan"),   ("ㄗㄨㄣ", "zun"),
    ("ㄗㄨㄥ", "zong"),   ("ㄘㄨㄛ", "cuo"),    ("ㄘㄨㄟ", "cui"),
    ("ㄘㄨㄢ", "cuan"),   ("ㄘㄨㄣ", "cun"),    ("ㄘㄨㄥ", "cong"),
    ("ㄙㄨㄛ", "suo"),    ("ㄙㄨㄟ", "sui"),    ("ㄙㄨㄢ", "suan"),
    ("ㄙㄨㄣ", "sun"),    ("ㄙㄨㄥ", "song"),   ("ㄅㄧㄤ", "biang"),
    ("ㄉㄨㄤ", "duang"),  ("ㄅㄚ", "ba"),       ("ㄅㄛ", "bo"),
    ("ㄅㄞ", "bai"),      ("ㄅㄟ", "bei"),      ("ㄅㄠ", "bao"),
    ("ㄅㄢ", "ban"),      ("ㄅㄣ", "ben"),      ("ㄅㄤ", "bang"),
    ("ㄅㄥ", "beng"),     ("ㄅㄧ", "bi"),       ("ㄅㄨ", "bu"),
    ("ㄆㄚ", "pa"),       ("ㄆㄛ", "po"),       ("ㄆㄞ", "pai"),
    ("ㄆㄟ", "pei"),      ("ㄆㄠ", "pao"),      ("ㄆㄡ", "pou"),
    ("ㄆㄢ", "pan"),      ("ㄆㄣ", "pen"),      ("ㄆㄤ", "pang"),
    ("ㄆㄥ", "peng"),     ("ㄆㄧ", "pi"),       ("ㄆㄨ", "pu"),
    ("ㄇㄚ", "ma"),       ("ㄇㄛ", "mo"),       ("ㄇㄜ", "me"),
    ("ㄇㄞ", "mai"),      ("ㄇㄟ", "mei"),      ("ㄇㄠ", "mao"),
    ("ㄇㄡ", "mou"),      ("ㄇㄢ", "man"),      ("ㄇㄣ", "men"),
    ("ㄇㄤ", "mang"),     ("ㄇㄥ", "meng"),     ("ㄇㄧ", "mi"),
    ("ㄇㄨ", "mu"),       ("ㄈㄚ", "fa"),       ("ㄈㄛ", "fo"),
    ("ㄈㄟ", "fei"),      ("ㄈㄡ", "fou"),      ("ㄈㄢ", "fan"),
    ("ㄈㄣ", "fen"),      ("ㄈㄤ", "fang"),     ("ㄈㄥ", "feng"),
    ("ㄈㄨ", "fu"),       ("ㄉㄚ", "da"),       ("ㄉㄜ", "de"),
    ("ㄉㄞ", "dai"),      ("ㄉㄟ", "dei"),      ("ㄉㄠ", "dao"),
    ("ㄉㄡ", "dou"),      ("ㄉㄢ", "dan"),      ("ㄉㄣ", "den"),
    ("ㄉㄤ", "dang"),     ("ㄉㄥ", "deng"),     ("ㄉㄧ", "di"),
    ("ㄉㄨ", "du"),       ("ㄊㄚ", "ta"),       ("ㄊㄜ", "te"),
    ("ㄊㄞ", "tai"),      ("ㄊㄠ", "tao"),      ("ㄊㄡ", "tou"),
    ("ㄊㄢ", "tan"),      ("ㄊㄤ", "tang"),     ("ㄊㄥ", "teng"),
    ("ㄊㄧ", "ti"),       ("ㄊㄨ", "tu"),       ("ㄋㄚ", "na"),
    ("ㄋㄜ", "ne"),       ("ㄋㄞ", "nai"),      ("ㄋㄟ", "nei"),
    ("ㄋㄠ", "nao"),      ("ㄋㄡ", "nou"),      ("ㄋㄢ", "nan"),
    ("ㄋㄣ", "nen"),      ("ㄋㄤ", "nang"),     ("ㄋㄥ", "neng"),
    ("ㄋㄧ", "ni"),       ("ㄋㄨ", "nu"),       ("ㄋㄩ", "nv"),
    ("ㄌㄚ", "la"),       ("ㄌㄛ", "lo"),       ("ㄌㄜ", "le"),
    ("ㄌㄞ", "lai"),      ("ㄌㄟ", "lei"),      ("ㄌㄠ", "lao"),
    ("ㄌㄡ", "lou"),      ("ㄌㄢ", "lan"),      ("ㄌㄤ", "lang"),
    ("ㄌㄥ", "leng"),     ("ㄌㄧ", "li"),       ("ㄌㄨ", "lu"),
    ("ㄌㄩ", "lv"),       ("ㄍㄚ", "ga"),       ("ㄍㄜ", "ge"),
    ("ㄍㄞ", "gai"),      ("ㄍㄟ", "gei"),      ("ㄍㄠ", "gao"),
    ("ㄍㄡ", "gou"),      ("ㄍㄢ", "gan"),      ("ㄍㄣ", "gen"),
    ("ㄍㄤ", "gang"),     ("ㄍㄥ", "geng"),     ("ㄍㄧ", "gi"),
    ("ㄍㄨ", "gu"),       ("ㄎㄚ", "ka"),       ("ㄎㄜ", "ke"),
    ("ㄎㄞ", "kai"),      ("ㄎㄠ", "kao"),      ("ㄎㄡ", "kou"),
    ("ㄎㄢ", "kan"),      ("ㄎㄣ", "ken"),      ("ㄎㄤ", "kang"),
    ("ㄎㄥ", "keng"),     ("ㄎㄨ", "ku"),       ("ㄏㄚ", "ha"),
    ("ㄏㄜ", "he"),       ("ㄏㄞ", "hai"),      ("ㄏㄟ", "hei"),
    ("ㄏㄠ", "hao"),      ("ㄏㄡ", "hou"),      ("ㄏㄢ", "han"),
    ("ㄏㄣ", "hen"),      ("ㄏㄤ", "hang"),     ("ㄏㄥ", "heng"),
    ("ㄏㄨ", "hu"),       ("ㄐㄧ", "ji"),       ("ㄐㄩ", "ju"),
    ("ㄑㄧ", "qi"),       ("ㄑㄩ", "qu"),       ("ㄒㄧ", "xi"),
    ("ㄒㄩ", "xu"),       ("ㄓㄚ", "zha"),      ("ㄓㄜ", "zhe"),
    ("ㄓㄞ", "zhai"),     ("ㄓㄟ", "zhei"),     ("ㄓㄠ", "zhao"),
    ("ㄓㄡ", "zhou"),     ("ㄓㄢ", "zhan"),     ("ㄓㄣ", "zhen"),
    ("ㄓㄤ", "zhang"),    ("ㄓㄥ", "zheng"),    ("ㄓㄨ", "zhu"),
    ("ㄔㄚ", "cha"),      ("ㄔㄜ", "che"),      ("ㄔㄞ", "chai"),
    ("ㄔㄠ", "chao"),     ("ㄔㄡ", "chou"),     ("ㄔㄢ", "chan"),
    ("ㄔㄣ", "chen"),     ("ㄔㄤ", "chang"),    ("ㄔㄥ", "cheng"),
    ("ㄔㄨ", "chu"),      ("ㄕㄚ", "sha"),      ("ㄕㄜ", "she"),
    ("ㄕㄞ", "shai"),     ("ㄕㄟ", "shei"),     ("ㄕㄠ", "shao"),
    ("ㄕㄡ", "shou"),     ("ㄕㄢ", "shan"),     ("ㄕㄣ", "shen"),
    ("ㄕㄤ", "shang"),    ("ㄕㄥ", "sheng"),    ("ㄕㄨ", "shu"),
    ("ㄖㄜ", "re"),       ("ㄖㄠ", "rao"),      ("ㄖㄡ", "rou"),
    ("ㄖㄢ", "ran"),      ("ㄖㄣ", "ren"),      ("ㄖㄤ", "rang"),
    ("ㄖㄥ", "reng"),     ("ㄖㄨ", "ru"),       ("ㄗㄚ", "za"),
    ("ㄗㄜ", "ze"),       ("ㄗㄞ", "zai"),      ("ㄗㄟ", "zei"),
    ("ㄗㄠ", "zao"),      ("ㄗㄡ", "zou"),      ("ㄗㄢ", "zan"),
    ("ㄗㄣ", "zen"),      ("ㄗㄤ", "zang"),     ("ㄗㄥ", "zeng"),
    ("ㄗㄨ", "zu"),       ("ㄘㄚ", "ca"),       ("ㄘㄜ", "ce"),
    ("ㄘㄞ", "cai"),      ("ㄘㄟ", "cei"),      ("ㄘㄠ", "cao"),
    ("ㄘㄡ", "cou"),      ("ㄘㄢ", "can"),      ("ㄘㄣ", "cen"),
    ("ㄘㄤ", "cang"),     ("ㄘㄥ", "ceng"),     ("ㄘㄨ", "cu"),
    ("ㄙㄚ", "sa"),       ("ㄙㄜ", "se"),       ("ㄙㄞ", "sai"),
    ("ㄙㄟ", "sei"),      ("ㄙㄠ", "sao"),      ("ㄙㄡ", "sou"),
    ("ㄙㄢ", "san"),      ("ㄙㄣ", "sen"),      ("ㄙㄤ", "sang"),
    ("ㄙㄥ", "seng"),     ("ㄙㄨ", "su"),       ("ㄧㄚ", "ya"),
    ("ㄧㄛ", "yo"),       ("ㄧㄝ", "ye"),       ("ㄧㄞ", "yai"),
    ("ㄧㄠ", "yao"),      ("ㄧㄡ", "you"),      ("ㄧㄢ", "yan"),
    ("ㄧㄣ", "yin"),      ("ㄧㄤ", "yang"),     ("ㄧㄥ", "ying"),
    ("ㄨㄚ", "wa"),       ("ㄨㄛ", "wo"),       ("ㄨㄞ", "wai"),
    ("ㄨㄟ", "wei"),      ("ㄨㄢ", "wan"),      ("ㄨㄣ", "wen"),
    ("ㄨㄤ", "wang"),     ("ㄨㄥ", "weng"),     ("ㄩㄝ", "yue"),
    ("ㄩㄢ", "yuan"),     ("ㄩㄣ", "yun"),      ("ㄩㄥ", "yong"),
    ("ㄅ", "b"),          ("ㄆ", "p"),          ("ㄇ", "m"),
    ("ㄈ", "f"),          ("ㄉ", "d"),          ("ㄊ", "t"),
    ("ㄋ", "n"),          ("ㄌ", "l"),          ("ㄍ", "g"),
    ("ㄎ", "k"),          ("ㄏ", "h"),          ("ㄐ", "j"),
    ("ㄑ", "q"),          ("ㄒ", "x"),          ("ㄓ", "zhi"),
    ("ㄔ", "chi"),        ("ㄕ", "shi"),        ("ㄖ", "ri"),
    ("ㄗ", "zi"),         ("ㄘ", "ci"),         ("ㄙ", "si"),
    ("ㄚ", "a"),          ("ㄛ", "o"),          ("ㄜ", "e"),
    ("ㄝ", "eh"),         ("ㄞ", "ai"),         ("ㄟ", "ei"),
    ("ㄠ", "ao"),         ("ㄡ", "ou"),         ("ㄢ", "an"),
    ("ㄣ", "en"),         ("ㄤ", "ang"),        ("ㄥ", "eng"),
    ("ㄦ", "er"),         ("ㄧ", "yi"),         ("ㄨ", "wu"),
    ("ㄩ", "yu")
  };

  /// <summary>
  /// 漢語拼音韻母轉換對照表資料貯存專用佇列。<br />
  /// 排序很重要。先處理最長的，再處理短的。不然會出亂子。
  /// </summary>
  public readonly static (
      string, string)[] ArrHanyuPinyinTextbookStyleConversionTable = {
    ("iang1", "iāng"), ("iang2", "iáng"), ("iang3", "iǎng"), ("iang4", "iàng"),
    ("iong1", "iōng"), ("iong2", "ióng"), ("iong3", "iǒng"), ("iong4", "iòng"),
    ("uang1", "uāng"), ("uang2", "uáng"), ("uang3", "uǎng"), ("uang4", "uàng"),
    ("uang5", "uang"), ("ang1", "āng"),   ("ang2", "áng"),   ("ang3", "ǎng"),
    ("ang4", "àng"),   ("ang5", "ang"),   ("eng1", "ēng"),   ("eng2", "éng"),
    ("eng3", "ěng"),   ("eng4", "èng"),   ("ian1", "iān"),   ("ian2", "ián"),
    ("ian3", "iǎn"),   ("ian4", "iàn"),   ("iao1", "iāo"),   ("iao2", "iáo"),
    ("iao3", "iǎo"),   ("iao4", "iào"),   ("ing1", "īng"),   ("ing2", "íng"),
    ("ing3", "ǐng"),   ("ing4", "ìng"),   ("ong1", "ōng"),   ("ong2", "óng"),
    ("ong3", "ǒng"),   ("ong4", "òng"),   ("uai1", "uāi"),   ("uai2", "uái"),
    ("uai3", "uǎi"),   ("uai4", "uài"),   ("uan1", "uān"),   ("uan2", "uán"),
    ("uan3", "uǎn"),   ("uan4", "uàn"),   ("van2", "üán"),   ("van3", "üǎn"),
    ("ai1", "āi"),     ("ai2", "ái"),     ("ai3", "ǎi"),     ("ai4", "ài"),
    ("ai5", "ai"),     ("an1", "ān"),     ("an2", "án"),     ("an3", "ǎn"),
    ("an4", "àn"),     ("ao1", "āo"),     ("ao2", "áo"),     ("ao3", "ǎo"),
    ("ao4", "ào"),     ("ao5", "ao"),     ("eh2", "ế"),      ("eh3", "êˇ"),
    ("eh4", "ề"),      ("eh5", "ê"),      ("ei1", "ēi"),     ("ei2", "éi"),
    ("ei3", "ěi"),     ("ei4", "èi"),     ("ei5", "ei"),     ("en1", "ēn"),
    ("en2", "én"),     ("en3", "ěn"),     ("en4", "èn"),     ("en5", "en"),
    ("er1", "ēr"),     ("er2", "ér"),     ("er3", "ěr"),     ("er4", "èr"),
    ("er5", "er"),     ("ia1", "iā"),     ("ia2", "iá"),     ("ia3", "iǎ"),
    ("ia4", "ià"),     ("ie1", "iē"),     ("ie2", "ié"),     ("ie3", "iě"),
    ("ie4", "iè"),     ("ie5", "ie"),     ("in1", "īn"),     ("in2", "ín"),
    ("in3", "ǐn"),     ("in4", "ìn"),     ("iu1", "iū"),     ("iu2", "iú"),
    ("iu3", "iǔ"),     ("iu4", "iù"),     ("ou1", "ōu"),     ("ou2", "óu"),
    ("ou3", "ǒu"),     ("ou4", "òu"),     ("ou5", "ou"),     ("ua1", "uā"),
    ("ua2", "uá"),     ("ua3", "uǎ"),     ("ua4", "uà"),     ("ue1", "uē"),
    ("ue2", "ué"),     ("ue3", "uě"),     ("ue4", "uè"),     ("ui1", "uī"),
    ("ui2", "uí"),     ("ui3", "uǐ"),     ("ui4", "uì"),     ("un1", "ūn"),
    ("un2", "ún"),     ("un3", "ǔn"),     ("un4", "ùn"),     ("uo1", "uō"),
    ("uo2", "uó"),     ("uo3", "uǒ"),     ("uo4", "uò"),     ("uo5", "uo"),
    ("ve1", "üē"),     ("ve3", "üě"),     ("ve4", "üè"),     ("a1", "ā"),
    ("a2", "á"),       ("a3", "ǎ"),       ("a4", "à"),       ("a5", "a"),
    ("e1", "ē"),       ("e2", "é"),       ("e3", "ě"),       ("e4", "è"),
    ("e5", "e"),       ("i1", "ī"),       ("i2", "í"),       ("i3", "ǐ"),
    ("i4", "ì"),       ("i5", "i"),       ("o1", "ō"),       ("o2", "ó"),
    ("o3", "ǒ"),       ("o4", "ò"),       ("o5", "o"),       ("u1", "ū"),
    ("u2", "ú"),       ("u3", "ǔ"),       ("u4", "ù"),       ("v1", "ǖ"),
    ("v2", "ǘ"),       ("v3", "ǚ"),       ("v4", "ǜ")
  };

  // MARK: - Maps for Keyboard-to-Pinyin parsers

  /// <summary>
  /// 任何形式的拼音排列都會用到的陣列（韋氏拼音與趙元任國語羅馬字除外），
  /// 用 Strings 反而省事一些。<br />
  /// 這裡同時兼容大千注音的調號數字，所以也將 6、7 號數字鍵放在允許範圍內。
  /// </summary>
  public const string MapArayuruPinyin = "abcdefghijklmnopqrstuvwxyz1234567 ";

  /// <summary>
  /// 任何形式的拼音排列都會用到的陣列（韋氏拼音與趙元任國語羅馬字除外），
  /// 用 Strings 反而省事一些。<br />
  /// 這裡同時兼容大千注音的調號數字，所以也將 6、7 號數字鍵放在允許範圍內。
  /// </summary>
  public const string MapWadeGilesPinyinKeys = MapArayuruPinyin + "'";

  /// <summary>
  /// 任何拼音都會用到的聲調鍵陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapArayuruPinyinIntonation =
      new() { ["1"] = " ", ["2"] = "ˊ", ["3"] = "ˇ", ["4"] = "ˋ",
              ["5"] = "˙", ["6"] = "ˊ", ["7"] = "˙", [" "] = " " };

  /// <summary>
  /// 漢語拼音排列專用處理陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapHanyuPinyin = new() {
    ["chuang"] = "ㄔㄨㄤ", ["shuang"] = "ㄕㄨㄤ", ["zhuang"] = "ㄓㄨㄤ",
    ["chang"] = "ㄔㄤ",    ["cheng"] = "ㄔㄥ",    ["chong"] = "ㄔㄨㄥ",
    ["chuai"] = "ㄔㄨㄞ",  ["chuan"] = "ㄔㄨㄢ",  ["guang"] = "ㄍㄨㄤ",
    ["huang"] = "ㄏㄨㄤ",  ["jiang"] = "ㄐㄧㄤ",  ["jiong"] = "ㄐㄩㄥ",
    ["kiang"] = "ㄎㄧㄤ",  ["kuang"] = "ㄎㄨㄤ",  ["biang"] = "ㄅㄧㄤ",
    ["duang"] = "ㄉㄨㄤ",  ["liang"] = "ㄌㄧㄤ",  ["niang"] = "ㄋㄧㄤ",
    ["qiang"] = "ㄑㄧㄤ",  ["qiong"] = "ㄑㄩㄥ",  ["shang"] = "ㄕㄤ",
    ["sheng"] = "ㄕㄥ",    ["shuai"] = "ㄕㄨㄞ",  ["shuan"] = "ㄕㄨㄢ",
    ["xiang"] = "ㄒㄧㄤ",  ["xiong"] = "ㄒㄩㄥ",  ["zhang"] = "ㄓㄤ",
    ["zheng"] = "ㄓㄥ",    ["zhong"] = "ㄓㄨㄥ",  ["zhuai"] = "ㄓㄨㄞ",
    ["zhuan"] = "ㄓㄨㄢ",  ["bang"] = "ㄅㄤ",     ["beng"] = "ㄅㄥ",
    ["bian"] = "ㄅㄧㄢ",   ["biao"] = "ㄅㄧㄠ",   ["bing"] = "ㄅㄧㄥ",
    ["cang"] = "ㄘㄤ",     ["ceng"] = "ㄘㄥ",     ["chai"] = "ㄔㄞ",
    ["chan"] = "ㄔㄢ",     ["chao"] = "ㄔㄠ",     ["chen"] = "ㄔㄣ",
    ["chou"] = "ㄔㄡ",     ["chua"] = "ㄔㄨㄚ",   ["chui"] = "ㄔㄨㄟ",
    ["chun"] = "ㄔㄨㄣ",   ["chuo"] = "ㄔㄨㄛ",   ["cong"] = "ㄘㄨㄥ",
    ["cuan"] = "ㄘㄨㄢ",   ["dang"] = "ㄉㄤ",     ["deng"] = "ㄉㄥ",
    ["dian"] = "ㄉㄧㄢ",   ["diao"] = "ㄉㄧㄠ",   ["ding"] = "ㄉㄧㄥ",
    ["dong"] = "ㄉㄨㄥ",   ["duan"] = "ㄉㄨㄢ",   ["fang"] = "ㄈㄤ",
    ["feng"] = "ㄈㄥ",     ["fiao"] = "ㄈㄧㄠ",   ["fong"] = "ㄈㄨㄥ",
    ["gang"] = "ㄍㄤ",     ["geng"] = "ㄍㄥ",     ["giao"] = "ㄍㄧㄠ",
    ["gong"] = "ㄍㄨㄥ",   ["guai"] = "ㄍㄨㄞ",   ["guan"] = "ㄍㄨㄢ",
    ["hang"] = "ㄏㄤ",     ["heng"] = "ㄏㄥ",     ["hong"] = "ㄏㄨㄥ",
    ["huai"] = "ㄏㄨㄞ",   ["huan"] = "ㄏㄨㄢ",   ["jian"] = "ㄐㄧㄢ",
    ["jiao"] = "ㄐㄧㄠ",   ["jing"] = "ㄐㄧㄥ",   ["juan"] = "ㄐㄩㄢ",
    ["kang"] = "ㄎㄤ",     ["keng"] = "ㄎㄥ",     ["kong"] = "ㄎㄨㄥ",
    ["kuai"] = "ㄎㄨㄞ",   ["kuan"] = "ㄎㄨㄢ",   ["lang"] = "ㄌㄤ",
    ["leng"] = "ㄌㄥ",     ["lian"] = "ㄌㄧㄢ",   ["liao"] = "ㄌㄧㄠ",
    ["ling"] = "ㄌㄧㄥ",   ["long"] = "ㄌㄨㄥ",   ["luan"] = "ㄌㄨㄢ",
    ["lvan"] = "ㄌㄩㄢ",   ["mang"] = "ㄇㄤ",     ["meng"] = "ㄇㄥ",
    ["mian"] = "ㄇㄧㄢ",   ["miao"] = "ㄇㄧㄠ",   ["ming"] = "ㄇㄧㄥ",
    ["nang"] = "ㄋㄤ",     ["neng"] = "ㄋㄥ",     ["nian"] = "ㄋㄧㄢ",
    ["niao"] = "ㄋㄧㄠ",   ["ning"] = "ㄋㄧㄥ",   ["nong"] = "ㄋㄨㄥ",
    ["nuan"] = "ㄋㄨㄢ",   ["pang"] = "ㄆㄤ",     ["peng"] = "ㄆㄥ",
    ["pian"] = "ㄆㄧㄢ",   ["piao"] = "ㄆㄧㄠ",   ["ping"] = "ㄆㄧㄥ",
    ["qian"] = "ㄑㄧㄢ",   ["qiao"] = "ㄑㄧㄠ",   ["qing"] = "ㄑㄧㄥ",
    ["quan"] = "ㄑㄩㄢ",   ["rang"] = "ㄖㄤ",     ["reng"] = "ㄖㄥ",
    ["rong"] = "ㄖㄨㄥ",   ["ruan"] = "ㄖㄨㄢ",   ["sang"] = "ㄙㄤ",
    ["seng"] = "ㄙㄥ",     ["shai"] = "ㄕㄞ",     ["shan"] = "ㄕㄢ",
    ["shao"] = "ㄕㄠ",     ["shei"] = "ㄕㄟ",     ["shen"] = "ㄕㄣ",
    ["shou"] = "ㄕㄡ",     ["shua"] = "ㄕㄨㄚ",   ["shui"] = "ㄕㄨㄟ",
    ["shun"] = "ㄕㄨㄣ",   ["shuo"] = "ㄕㄨㄛ",   ["song"] = "ㄙㄨㄥ",
    ["suan"] = "ㄙㄨㄢ",   ["tang"] = "ㄊㄤ",     ["teng"] = "ㄊㄥ",
    ["tian"] = "ㄊㄧㄢ",   ["tiao"] = "ㄊㄧㄠ",   ["ting"] = "ㄊㄧㄥ",
    ["tong"] = "ㄊㄨㄥ",   ["tuan"] = "ㄊㄨㄢ",   ["wang"] = "ㄨㄤ",
    ["weng"] = "ㄨㄥ",     ["xian"] = "ㄒㄧㄢ",   ["xiao"] = "ㄒㄧㄠ",
    ["xing"] = "ㄒㄧㄥ",   ["xuan"] = "ㄒㄩㄢ",   ["yang"] = "ㄧㄤ",
    ["ying"] = "ㄧㄥ",     ["yong"] = "ㄩㄥ",     ["yuan"] = "ㄩㄢ",
    ["zang"] = "ㄗㄤ",     ["zeng"] = "ㄗㄥ",     ["zhai"] = "ㄓㄞ",
    ["zhan"] = "ㄓㄢ",     ["zhao"] = "ㄓㄠ",     ["zhei"] = "ㄓㄟ",
    ["zhen"] = "ㄓㄣ",     ["zhou"] = "ㄓㄡ",     ["zhua"] = "ㄓㄨㄚ",
    ["zhui"] = "ㄓㄨㄟ",   ["zhun"] = "ㄓㄨㄣ",   ["zhuo"] = "ㄓㄨㄛ",
    ["zong"] = "ㄗㄨㄥ",   ["zuan"] = "ㄗㄨㄢ",   ["jun"] = "ㄐㄩㄣ",
    ["ang"] = "ㄤ",        ["bai"] = "ㄅㄞ",      ["ban"] = "ㄅㄢ",
    ["bao"] = "ㄅㄠ",      ["bei"] = "ㄅㄟ",      ["ben"] = "ㄅㄣ",
    ["bie"] = "ㄅㄧㄝ",    ["bin"] = "ㄅㄧㄣ",    ["cai"] = "ㄘㄞ",
    ["can"] = "ㄘㄢ",      ["cao"] = "ㄘㄠ",      ["cei"] = "ㄘㄟ",
    ["cen"] = "ㄘㄣ",      ["cha"] = "ㄔㄚ",      ["che"] = "ㄔㄜ",
    ["chi"] = "ㄔ",        ["chu"] = "ㄔㄨ",      ["cou"] = "ㄘㄡ",
    ["cui"] = "ㄘㄨㄟ",    ["cun"] = "ㄘㄨㄣ",    ["cuo"] = "ㄘㄨㄛ",
    ["dai"] = "ㄉㄞ",      ["dan"] = "ㄉㄢ",      ["dao"] = "ㄉㄠ",
    ["dei"] = "ㄉㄟ",      ["den"] = "ㄉㄣ",      ["dia"] = "ㄉㄧㄚ",
    ["die"] = "ㄉㄧㄝ",    ["diu"] = "ㄉㄧㄡ",    ["dou"] = "ㄉㄡ",
    ["dui"] = "ㄉㄨㄟ",    ["dun"] = "ㄉㄨㄣ",    ["duo"] = "ㄉㄨㄛ",
    ["eng"] = "ㄥ",        ["fan"] = "ㄈㄢ",      ["fei"] = "ㄈㄟ",
    ["fen"] = "ㄈㄣ",      ["fou"] = "ㄈㄡ",      ["gai"] = "ㄍㄞ",
    ["gan"] = "ㄍㄢ",      ["gao"] = "ㄍㄠ",      ["gei"] = "ㄍㄟ",
    ["gin"] = "ㄍㄧㄣ",    ["gen"] = "ㄍㄣ",      ["gou"] = "ㄍㄡ",
    ["gua"] = "ㄍㄨㄚ",    ["gue"] = "ㄍㄨㄜ",    ["gui"] = "ㄍㄨㄟ",
    ["gun"] = "ㄍㄨㄣ",    ["guo"] = "ㄍㄨㄛ",    ["hai"] = "ㄏㄞ",
    ["han"] = "ㄏㄢ",      ["hao"] = "ㄏㄠ",      ["hei"] = "ㄏㄟ",
    ["hen"] = "ㄏㄣ",      ["hou"] = "ㄏㄡ",      ["hua"] = "ㄏㄨㄚ",
    ["hui"] = "ㄏㄨㄟ",    ["hun"] = "ㄏㄨㄣ",    ["huo"] = "ㄏㄨㄛ",
    ["jia"] = "ㄐㄧㄚ",    ["jie"] = "ㄐㄧㄝ",    ["jin"] = "ㄐㄧㄣ",
    ["jiu"] = "ㄐㄧㄡ",    ["jue"] = "ㄐㄩㄝ",    ["kai"] = "ㄎㄞ",
    ["kan"] = "ㄎㄢ",      ["kao"] = "ㄎㄠ",      ["ken"] = "ㄎㄣ",
    ["kiu"] = "ㄎㄧㄡ",    ["kou"] = "ㄎㄡ",      ["kua"] = "ㄎㄨㄚ",
    ["kui"] = "ㄎㄨㄟ",    ["kun"] = "ㄎㄨㄣ",    ["kuo"] = "ㄎㄨㄛ",
    ["lai"] = "ㄌㄞ",      ["lan"] = "ㄌㄢ",      ["lao"] = "ㄌㄠ",
    ["lei"] = "ㄌㄟ",      ["lia"] = "ㄌㄧㄚ",    ["lie"] = "ㄌㄧㄝ",
    ["lin"] = "ㄌㄧㄣ",    ["liu"] = "ㄌㄧㄡ",    ["lou"] = "ㄌㄡ",
    ["lun"] = "ㄌㄨㄣ",    ["luo"] = "ㄌㄨㄛ",    ["lve"] = "ㄌㄩㄝ",
    ["mai"] = "ㄇㄞ",      ["man"] = "ㄇㄢ",      ["mao"] = "ㄇㄠ",
    ["mei"] = "ㄇㄟ",      ["men"] = "ㄇㄣ",      ["mie"] = "ㄇㄧㄝ",
    ["min"] = "ㄇㄧㄣ",    ["miu"] = "ㄇㄧㄡ",    ["mou"] = "ㄇㄡ",
    ["nai"] = "ㄋㄞ",      ["nan"] = "ㄋㄢ",      ["nao"] = "ㄋㄠ",
    ["nei"] = "ㄋㄟ",      ["nen"] = "ㄋㄣ",      ["nie"] = "ㄋㄧㄝ",
    ["nin"] = "ㄋㄧㄣ",    ["niu"] = "ㄋㄧㄡ",    ["nou"] = "ㄋㄡ",
    ["nui"] = "ㄋㄨㄟ",    ["nun"] = "ㄋㄨㄣ",    ["nuo"] = "ㄋㄨㄛ",
    ["nve"] = "ㄋㄩㄝ",    ["pai"] = "ㄆㄞ",      ["pan"] = "ㄆㄢ",
    ["pao"] = "ㄆㄠ",      ["pei"] = "ㄆㄟ",      ["pen"] = "ㄆㄣ",
    ["pia"] = "ㄆㄧㄚ",    ["pie"] = "ㄆㄧㄝ",    ["pin"] = "ㄆㄧㄣ",
    ["pou"] = "ㄆㄡ",      ["qia"] = "ㄑㄧㄚ",    ["qie"] = "ㄑㄧㄝ",
    ["qin"] = "ㄑㄧㄣ",    ["qiu"] = "ㄑㄧㄡ",    ["que"] = "ㄑㄩㄝ",
    ["qun"] = "ㄑㄩㄣ",    ["ran"] = "ㄖㄢ",      ["rao"] = "ㄖㄠ",
    ["ren"] = "ㄖㄣ",      ["rou"] = "ㄖㄡ",      ["rui"] = "ㄖㄨㄟ",
    ["run"] = "ㄖㄨㄣ",    ["ruo"] = "ㄖㄨㄛ",    ["sai"] = "ㄙㄞ",
    ["san"] = "ㄙㄢ",      ["sao"] = "ㄙㄠ",      ["sei"] = "ㄙㄟ",
    ["sen"] = "ㄙㄣ",      ["sha"] = "ㄕㄚ",      ["she"] = "ㄕㄜ",
    ["shi"] = "ㄕ",        ["shu"] = "ㄕㄨ",      ["sou"] = "ㄙㄡ",
    ["sui"] = "ㄙㄨㄟ",    ["sun"] = "ㄙㄨㄣ",    ["suo"] = "ㄙㄨㄛ",
    ["tai"] = "ㄊㄞ",      ["tan"] = "ㄊㄢ",      ["tao"] = "ㄊㄠ",
    ["tie"] = "ㄊㄧㄝ",    ["tou"] = "ㄊㄡ",      ["tui"] = "ㄊㄨㄟ",
    ["tun"] = "ㄊㄨㄣ",    ["tuo"] = "ㄊㄨㄛ",    ["wai"] = "ㄨㄞ",
    ["wan"] = "ㄨㄢ",      ["wei"] = "ㄨㄟ",      ["wen"] = "ㄨㄣ",
    ["xia"] = "ㄒㄧㄚ",    ["xie"] = "ㄒㄧㄝ",    ["xin"] = "ㄒㄧㄣ",
    ["xiu"] = "ㄒㄧㄡ",    ["xue"] = "ㄒㄩㄝ",    ["xun"] = "ㄒㄩㄣ",
    ["yai"] = "ㄧㄞ",      ["yan"] = "ㄧㄢ",      ["yao"] = "ㄧㄠ",
    ["yin"] = "ㄧㄣ",      ["you"] = "ㄧㄡ",      ["yue"] = "ㄩㄝ",
    ["yun"] = "ㄩㄣ",      ["zai"] = "ㄗㄞ",      ["zan"] = "ㄗㄢ",
    ["zao"] = "ㄗㄠ",      ["zei"] = "ㄗㄟ",      ["zen"] = "ㄗㄣ",
    ["zha"] = "ㄓㄚ",      ["zhe"] = "ㄓㄜ",      ["zhi"] = "ㄓ",
    ["zhu"] = "ㄓㄨ",      ["zou"] = "ㄗㄡ",      ["zui"] = "ㄗㄨㄟ",
    ["zun"] = "ㄗㄨㄣ",    ["zuo"] = "ㄗㄨㄛ",    ["ai"] = "ㄞ",
    ["an"] = "ㄢ",         ["ao"] = "ㄠ",         ["ba"] = "ㄅㄚ",
    ["bi"] = "ㄅㄧ",       ["bo"] = "ㄅㄛ",       ["bu"] = "ㄅㄨ",
    ["ca"] = "ㄘㄚ",       ["ce"] = "ㄘㄜ",       ["ci"] = "ㄘ",
    ["cu"] = "ㄘㄨ",       ["da"] = "ㄉㄚ",       ["de"] = "ㄉㄜ",
    ["di"] = "ㄉㄧ",       ["du"] = "ㄉㄨ",       ["eh"] = "ㄝ",
    ["ei"] = "ㄟ",         ["en"] = "ㄣ",         ["er"] = "ㄦ",
    ["fa"] = "ㄈㄚ",       ["fo"] = "ㄈㄛ",       ["fu"] = "ㄈㄨ",
    ["ga"] = "ㄍㄚ",       ["ge"] = "ㄍㄜ",       ["gi"] = "ㄍㄧ",
    ["gu"] = "ㄍㄨ",       ["ha"] = "ㄏㄚ",       ["he"] = "ㄏㄜ",
    ["hu"] = "ㄏㄨ",       ["ji"] = "ㄐㄧ",       ["ju"] = "ㄐㄩ",
    ["ka"] = "ㄎㄚ",       ["ke"] = "ㄎㄜ",       ["ku"] = "ㄎㄨ",
    ["la"] = "ㄌㄚ",       ["le"] = "ㄌㄜ",       ["li"] = "ㄌㄧ",
    ["lo"] = "ㄌㄛ",       ["lu"] = "ㄌㄨ",       ["lv"] = "ㄌㄩ",
    ["ma"] = "ㄇㄚ",       ["me"] = "ㄇㄜ",       ["mi"] = "ㄇㄧ",
    ["mo"] = "ㄇㄛ",       ["mu"] = "ㄇㄨ",       ["na"] = "ㄋㄚ",
    ["ne"] = "ㄋㄜ",       ["ni"] = "ㄋㄧ",       ["nu"] = "ㄋㄨ",
    ["nv"] = "ㄋㄩ",       ["ou"] = "ㄡ",         ["pa"] = "ㄆㄚ",
    ["pi"] = "ㄆㄧ",       ["po"] = "ㄆㄛ",       ["pu"] = "ㄆㄨ",
    ["qi"] = "ㄑㄧ",       ["qu"] = "ㄑㄩ",       ["re"] = "ㄖㄜ",
    ["ri"] = "ㄖ",         ["ru"] = "ㄖㄨ",       ["sa"] = "ㄙㄚ",
    ["se"] = "ㄙㄜ",       ["si"] = "ㄙ",         ["su"] = "ㄙㄨ",
    ["ta"] = "ㄊㄚ",       ["te"] = "ㄊㄜ",       ["ti"] = "ㄊㄧ",
    ["tu"] = "ㄊㄨ",       ["wa"] = "ㄨㄚ",       ["wo"] = "ㄨㄛ",
    ["wu"] = "ㄨ",         ["xi"] = "ㄒㄧ",       ["xu"] = "ㄒㄩ",
    ["ya"] = "ㄧㄚ",       ["ye"] = "ㄧㄝ",       ["yi"] = "ㄧ",
    ["yo"] = "ㄧㄛ",       ["yu"] = "ㄩ",         ["za"] = "ㄗㄚ",
    ["ze"] = "ㄗㄜ",       ["zi"] = "ㄗ",         ["zu"] = "ㄗㄨ",
    ["a"] = "ㄚ",          ["e"] = "ㄜ",          ["o"] = "ㄛ",
    ["q"] = "ㄑ"
  };

  /// <summary>
  /// 國音二式排列專用處理陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapSecondaryPinyin = new() {
    ["chuang"] = "ㄔㄨㄤ", ["shuang"] = "ㄕㄨㄤ", ["chiang"] = "ㄑㄧㄤ",
    ["chiung"] = "ㄑㄩㄥ", ["chiuan"] = "ㄑㄩㄢ", ["shiang"] = "ㄒㄧㄤ",
    ["shiung"] = "ㄒㄩㄥ", ["shiuan"] = "ㄒㄩㄢ", ["biang"] = "ㄅㄧㄤ",
    ["duang"] = "ㄉㄨㄤ",  ["juang"] = "ㄓㄨㄤ",  ["jiang"] = "ㄐㄧㄤ",
    ["jiung"] = "ㄐㄩㄥ",  ["niang"] = "ㄋㄧㄤ",  ["liang"] = "ㄌㄧㄤ",
    ["guang"] = "ㄍㄨㄤ",  ["kuang"] = "ㄎㄨㄤ",  ["huang"] = "ㄏㄨㄤ",
    ["chang"] = "ㄔㄤ",    ["cheng"] = "ㄔㄥ",    ["chuai"] = "ㄔㄨㄞ",
    ["chuan"] = "ㄔㄨㄢ",  ["chung"] = "ㄔㄨㄥ",  ["shang"] = "ㄕㄤ",
    ["sheng"] = "ㄕㄥ",    ["shuai"] = "ㄕㄨㄞ",  ["shuan"] = "ㄕㄨㄢ",
    ["jiuan"] = "ㄐㄩㄢ",  ["chiau"] = "ㄑㄧㄠ",  ["chian"] = "ㄑㄧㄢ",
    ["ching"] = "ㄑㄧㄥ",  ["shing"] = "ㄒㄧㄥ",  ["tzang"] = "ㄗㄤ",
    ["tzeng"] = "ㄗㄥ",    ["tzuan"] = "ㄗㄨㄢ",  ["tzung"] = "ㄗㄨㄥ",
    ["tsang"] = "ㄘㄤ",    ["tseng"] = "ㄘㄥ",    ["tsuan"] = "ㄘㄨㄢ",
    ["tsung"] = "ㄘㄨㄥ",  ["chiue"] = "ㄑㄩㄝ",  ["liuan"] = "ㄌㄩㄢ",
    ["chuei"] = "ㄔㄨㄟ",  ["chuen"] = "ㄔㄨㄣ",  ["shuei"] = "ㄕㄨㄟ",
    ["shuen"] = "ㄕㄨㄣ",  ["chiou"] = "ㄑㄧㄡ",  ["chiun"] = "ㄑㄩㄣ",
    ["tzuei"] = "ㄗㄨㄟ",  ["tzuen"] = "ㄗㄨㄣ",  ["tsuei"] = "ㄘㄨㄟ",
    ["tsuen"] = "ㄘㄨㄣ",  ["kiang"] = "ㄎㄧㄤ",  ["shiau"] = "ㄒㄧㄠ",
    ["shian"] = "ㄒㄧㄢ",  ["shiue"] = "ㄒㄩㄝ",  ["shiou"] = "ㄒㄧㄡ",
    ["shiun"] = "ㄒㄩㄣ",  ["jang"] = "ㄓㄤ",     ["jeng"] = "ㄓㄥ",
    ["juai"] = "ㄓㄨㄞ",   ["juan"] = "ㄓㄨㄢ",   ["jung"] = "ㄓㄨㄥ",
    ["jiau"] = "ㄐㄧㄠ",   ["jian"] = "ㄐㄧㄢ",   ["jing"] = "ㄐㄧㄥ",
    ["jiue"] = "ㄐㄩㄝ",   ["chie"] = "ㄑㄧㄝ",   ["bang"] = "ㄅㄤ",
    ["beng"] = "ㄅㄥ",     ["biau"] = "ㄅㄧㄠ",   ["bian"] = "ㄅㄧㄢ",
    ["bing"] = "ㄅㄧㄥ",   ["pang"] = "ㄆㄤ",     ["peng"] = "ㄆㄥ",
    ["piau"] = "ㄆㄧㄠ",   ["pian"] = "ㄆㄧㄢ",   ["ping"] = "ㄆㄧㄥ",
    ["mang"] = "ㄇㄤ",     ["meng"] = "ㄇㄥ",     ["miau"] = "ㄇㄧㄠ",
    ["mian"] = "ㄇㄧㄢ",   ["ming"] = "ㄇㄧㄥ",   ["fang"] = "ㄈㄤ",
    ["feng"] = "ㄈㄥ",     ["fiau"] = "ㄈㄧㄠ",   ["dang"] = "ㄉㄤ",
    ["deng"] = "ㄉㄥ",     ["diau"] = "ㄉㄧㄠ",   ["dian"] = "ㄉㄧㄢ",
    ["ding"] = "ㄉㄧㄥ",   ["duan"] = "ㄉㄨㄢ",   ["dung"] = "ㄉㄨㄥ",
    ["tang"] = "ㄊㄤ",     ["teng"] = "ㄊㄥ",     ["tiau"] = "ㄊㄧㄠ",
    ["tian"] = "ㄊㄧㄢ",   ["ting"] = "ㄊㄧㄥ",   ["tuan"] = "ㄊㄨㄢ",
    ["tung"] = "ㄊㄨㄥ",   ["nang"] = "ㄋㄤ",     ["neng"] = "ㄋㄥ",
    ["niau"] = "ㄋㄧㄠ",   ["nian"] = "ㄋㄧㄢ",   ["ning"] = "ㄋㄧㄥ",
    ["nuan"] = "ㄋㄨㄢ",   ["nung"] = "ㄋㄨㄥ",   ["lang"] = "ㄌㄤ",
    ["leng"] = "ㄌㄥ",     ["liau"] = "ㄌㄧㄠ",   ["lian"] = "ㄌㄧㄢ",
    ["ling"] = "ㄌㄧㄥ",   ["luan"] = "ㄌㄨㄢ",   ["lung"] = "ㄌㄨㄥ",
    ["gang"] = "ㄍㄤ",     ["geng"] = "ㄍㄥ",     ["guai"] = "ㄍㄨㄞ",
    ["guan"] = "ㄍㄨㄢ",   ["gung"] = "ㄍㄨㄥ",   ["kang"] = "ㄎㄤ",
    ["keng"] = "ㄎㄥ",     ["kuai"] = "ㄎㄨㄞ",   ["kuan"] = "ㄎㄨㄢ",
    ["kung"] = "ㄎㄨㄥ",   ["hang"] = "ㄏㄤ",     ["heng"] = "ㄏㄥ",
    ["huai"] = "ㄏㄨㄞ",   ["huan"] = "ㄏㄨㄢ",   ["hung"] = "ㄏㄨㄥ",
    ["juei"] = "ㄓㄨㄟ",   ["juen"] = "ㄓㄨㄣ",   ["chai"] = "ㄔㄞ",
    ["chau"] = "ㄔㄠ",     ["chou"] = "ㄔㄡ",     ["chan"] = "ㄔㄢ",
    ["chen"] = "ㄔㄣ",     ["chua"] = "ㄔㄨㄚ",   ["shai"] = "ㄕㄞ",
    ["shei"] = "ㄕㄟ",     ["shau"] = "ㄕㄠ",     ["shou"] = "ㄕㄡ",
    ["shan"] = "ㄕㄢ",     ["shen"] = "ㄕㄣ",     ["shua"] = "ㄕㄨㄚ",
    ["shuo"] = "ㄕㄨㄛ",   ["rang"] = "ㄖㄤ",     ["reng"] = "ㄖㄥ",
    ["ruan"] = "ㄖㄨㄢ",   ["rung"] = "ㄖㄨㄥ",   ["sang"] = "ㄙㄤ",
    ["seng"] = "ㄙㄥ",     ["suan"] = "ㄙㄨㄢ",   ["sung"] = "ㄙㄨㄥ",
    ["yang"] = "ㄧㄤ",     ["ying"] = "ㄧㄥ",     ["wang"] = "ㄨㄤ",
    ["weng"] = "ㄨㄥ",     ["yuan"] = "ㄩㄢ",     ["yung"] = "ㄩㄥ",
    ["niue"] = "ㄋㄩㄝ",   ["liue"] = "ㄌㄩㄝ",   ["guei"] = "ㄍㄨㄟ",
    ["kuei"] = "ㄎㄨㄟ",   ["jiou"] = "ㄐㄧㄡ",   ["jiun"] = "ㄐㄩㄣ",
    ["chia"] = "ㄑㄧㄚ",   ["chin"] = "ㄑㄧㄣ",   ["shin"] = "ㄒㄧㄣ",
    ["tzai"] = "ㄗㄞ",     ["tzei"] = "ㄗㄟ",     ["tzau"] = "ㄗㄠ",
    ["tzou"] = "ㄗㄡ",     ["tzan"] = "ㄗㄢ",     ["tzen"] = "ㄗㄣ",
    ["tsai"] = "ㄘㄞ",     ["tsau"] = "ㄘㄠ",     ["tsou"] = "ㄘㄡ",
    ["tsan"] = "ㄘㄢ",     ["tsen"] = "ㄘㄣ",     ["chuo"] = "ㄔㄨㄛ",
    ["miou"] = "ㄇㄧㄡ",   ["diou"] = "ㄉㄧㄡ",   ["duei"] = "ㄉㄨㄟ",
    ["duen"] = "ㄉㄨㄣ",   ["tuei"] = "ㄊㄨㄟ",   ["tuen"] = "ㄊㄨㄣ",
    ["niou"] = "ㄋㄧㄡ",   ["nuei"] = "ㄋㄨㄟ",   ["nuen"] = "ㄋㄨㄣ",
    ["liou"] = "ㄌㄧㄡ",   ["luen"] = "ㄌㄨㄣ",   ["guen"] = "ㄍㄨㄣ",
    ["kuen"] = "ㄎㄨㄣ",   ["huei"] = "ㄏㄨㄟ",   ["huen"] = "ㄏㄨㄣ",
    ["ruei"] = "ㄖㄨㄟ",   ["ruen"] = "ㄖㄨㄣ",   ["tzuo"] = "ㄗㄨㄛ",
    ["tsuo"] = "ㄘㄨㄛ",   ["suei"] = "ㄙㄨㄟ",   ["suen"] = "ㄙㄨㄣ",
    ["chiu"] = "ㄑㄩ",     ["giau"] = "ㄍㄧㄠ",   ["shie"] = "ㄒㄧㄝ",
    ["shia"] = "ㄒㄧㄚ",   ["shiu"] = "ㄒㄩ",     ["jie"] = "ㄐㄧㄝ",
    ["jai"] = "ㄓㄞ",      ["jei"] = "ㄓㄟ",      ["jau"] = "ㄓㄠ",
    ["jou"] = "ㄓㄡ",      ["jan"] = "ㄓㄢ",      ["jen"] = "ㄓㄣ",
    ["jua"] = "ㄓㄨㄚ",    ["bie"] = "ㄅㄧㄝ",    ["pie"] = "ㄆㄧㄝ",
    ["mie"] = "ㄇㄧㄝ",    ["die"] = "ㄉㄧㄝ",    ["tie"] = "ㄊㄧㄝ",
    ["nie"] = "ㄋㄧㄝ",    ["lie"] = "ㄌㄧㄝ",    ["jia"] = "ㄐㄧㄚ",
    ["jin"] = "ㄐㄧㄣ",    ["chr"] = "ㄔ",        ["shr"] = "ㄕ",
    ["yue"] = "ㄩㄝ",      ["juo"] = "ㄓㄨㄛ",    ["bai"] = "ㄅㄞ",
    ["bei"] = "ㄅㄟ",      ["bau"] = "ㄅㄠ",      ["ban"] = "ㄅㄢ",
    ["ben"] = "ㄅㄣ",      ["bin"] = "ㄅㄧㄣ",    ["pai"] = "ㄆㄞ",
    ["pei"] = "ㄆㄟ",      ["pau"] = "ㄆㄠ",      ["pou"] = "ㄆㄡ",
    ["pan"] = "ㄆㄢ",      ["pen"] = "ㄆㄣ",      ["pia"] = "ㄆㄧㄚ",
    ["pin"] = "ㄆㄧㄣ",    ["mai"] = "ㄇㄞ",      ["mei"] = "ㄇㄟ",
    ["mau"] = "ㄇㄠ",      ["mou"] = "ㄇㄡ",      ["man"] = "ㄇㄢ",
    ["men"] = "ㄇㄣ",      ["min"] = "ㄇㄧㄣ",    ["fei"] = "ㄈㄟ",
    ["fou"] = "ㄈㄡ",      ["fan"] = "ㄈㄢ",      ["fen"] = "ㄈㄣ",
    ["dai"] = "ㄉㄞ",      ["dei"] = "ㄉㄟ",      ["dau"] = "ㄉㄠ",
    ["dou"] = "ㄉㄡ",      ["dan"] = "ㄉㄢ",      ["den"] = "ㄉㄣ",
    ["dia"] = "ㄉㄧㄚ",    ["tai"] = "ㄊㄞ",      ["tau"] = "ㄊㄠ",
    ["tou"] = "ㄊㄡ",      ["tan"] = "ㄊㄢ",      ["nai"] = "ㄋㄞ",
    ["nei"] = "ㄋㄟ",      ["nau"] = "ㄋㄠ",      ["nou"] = "ㄋㄡ",
    ["nan"] = "ㄋㄢ",      ["nen"] = "ㄋㄣ",      ["nin"] = "ㄋㄧㄣ",
    ["lai"] = "ㄌㄞ",      ["lei"] = "ㄌㄟ",      ["lau"] = "ㄌㄠ",
    ["lou"] = "ㄌㄡ",      ["lan"] = "ㄌㄢ",      ["lia"] = "ㄌㄧㄚ",
    ["lin"] = "ㄌㄧㄣ",    ["gai"] = "ㄍㄞ",      ["gei"] = "ㄍㄟ",
    ["gau"] = "ㄍㄠ",      ["gou"] = "ㄍㄡ",      ["gan"] = "ㄍㄢ",
    ["gen"] = "ㄍㄣ",      ["gua"] = "ㄍㄨㄚ",    ["guo"] = "ㄍㄨㄛ",
    ["gue"] = "ㄍㄨㄜ",    ["kai"] = "ㄎㄞ",      ["kau"] = "ㄎㄠ",
    ["kou"] = "ㄎㄡ",      ["kan"] = "ㄎㄢ",      ["ken"] = "ㄎㄣ",
    ["kua"] = "ㄎㄨㄚ",    ["kuo"] = "ㄎㄨㄛ",    ["hai"] = "ㄏㄞ",
    ["hei"] = "ㄏㄟ",      ["hau"] = "ㄏㄠ",      ["hou"] = "ㄏㄡ",
    ["han"] = "ㄏㄢ",      ["hen"] = "ㄏㄣ",      ["hua"] = "ㄏㄨㄚ",
    ["huo"] = "ㄏㄨㄛ",    ["cha"] = "ㄔㄚ",      ["che"] = "ㄔㄜ",
    ["chu"] = "ㄔㄨ",      ["sha"] = "ㄕㄚ",      ["she"] = "ㄕㄜ",
    ["shu"] = "ㄕㄨ",      ["rau"] = "ㄖㄠ",      ["rou"] = "ㄖㄡ",
    ["ran"] = "ㄖㄢ",      ["ren"] = "ㄖㄣ",      ["sai"] = "ㄙㄞ",
    ["sei"] = "ㄙㄟ",      ["sau"] = "ㄙㄠ",      ["sou"] = "ㄙㄡ",
    ["san"] = "ㄙㄢ",      ["sen"] = "ㄙㄣ",      ["ang"] = "ㄤ",
    ["eng"] = "ㄥ",        ["yai"] = "ㄧㄞ",      ["yau"] = "ㄧㄠ",
    ["yan"] = "ㄧㄢ",      ["yin"] = "ㄧㄣ",      ["wai"] = "ㄨㄞ",
    ["wei"] = "ㄨㄟ",      ["wan"] = "ㄨㄢ",      ["wen"] = "ㄨㄣ",
    ["yun"] = "ㄩㄣ",      ["jiu"] = "ㄐㄩ",      ["chi"] = "ㄑㄧ",
    ["shi"] = "ㄒㄧ",      ["tza"] = "ㄗㄚ",      ["tze"] = "ㄗㄜ",
    ["tzu"] = "ㄗㄨ",      ["tsz"] = "ㄘ",        ["tsa"] = "ㄘㄚ",
    ["tse"] = "ㄘㄜ",      ["tsu"] = "ㄘㄨ",      ["duo"] = "ㄉㄨㄛ",
    ["tuo"] = "ㄊㄨㄛ",    ["nuo"] = "ㄋㄨㄛ",    ["luo"] = "ㄌㄨㄛ",
    ["ruo"] = "ㄖㄨㄛ",    ["suo"] = "ㄙㄨㄛ",    ["you"] = "ㄧㄡ",
    ["niu"] = "ㄋㄩ",      ["liu"] = "ㄌㄩ",      ["gin"] = "ㄍㄧㄣ",
    ["bo"] = "ㄅㄛ",       ["po"] = "ㄆㄛ",       ["mo"] = "ㄇㄛ",
    ["fo"] = "ㄈㄛ",       ["jr"] = "ㄓ",         ["ja"] = "ㄓㄚ",
    ["je"] = "ㄓㄜ",       ["ju"] = "ㄓㄨ",       ["ji"] = "ㄐㄧ",
    ["tz"] = "ㄗ",         ["sz"] = "ㄙ",         ["er"] = "ㄦ",
    ["ye"] = "ㄧㄝ",       ["ba"] = "ㄅㄚ",       ["bi"] = "ㄅㄧ",
    ["bu"] = "ㄅㄨ",       ["pa"] = "ㄆㄚ",       ["pi"] = "ㄆㄧ",
    ["pu"] = "ㄆㄨ",       ["ma"] = "ㄇㄚ",       ["me"] = "ㄇㄜ",
    ["mi"] = "ㄇㄧ",       ["mu"] = "ㄇㄨ",       ["fa"] = "ㄈㄚ",
    ["fu"] = "ㄈㄨ",       ["da"] = "ㄉㄚ",       ["de"] = "ㄉㄜ",
    ["di"] = "ㄉㄧ",       ["du"] = "ㄉㄨ",       ["ta"] = "ㄊㄚ",
    ["te"] = "ㄊㄜ",       ["ti"] = "ㄊㄧ",       ["tu"] = "ㄊㄨ",
    ["na"] = "ㄋㄚ",       ["ne"] = "ㄋㄜ",       ["ni"] = "ㄋㄧ",
    ["nu"] = "ㄋㄨ",       ["la"] = "ㄌㄚ",       ["lo"] = "ㄌㄛ",
    ["le"] = "ㄌㄜ",       ["li"] = "ㄌㄧ",       ["lu"] = "ㄌㄨ",
    ["ga"] = "ㄍㄚ",       ["ge"] = "ㄍㄜ",       ["gu"] = "ㄍㄨ",
    ["ka"] = "ㄎㄚ",       ["ke"] = "ㄎㄜ",       ["ku"] = "ㄎㄨ",
    ["ha"] = "ㄏㄚ",       ["he"] = "ㄏㄜ",       ["hu"] = "ㄏㄨ",
    ["re"] = "ㄖㄜ",       ["ru"] = "ㄖㄨ",       ["sa"] = "ㄙㄚ",
    ["se"] = "ㄙㄜ",       ["su"] = "ㄙㄨ",       ["eh"] = "ㄝ",
    ["ai"] = "ㄞ",         ["ei"] = "ㄟ",         ["au"] = "ㄠ",
    ["ou"] = "ㄡ",         ["an"] = "ㄢ",         ["en"] = "ㄣ",
    ["ya"] = "ㄧㄚ",       ["yo"] = "ㄧㄛ",       ["wu"] = "ㄨ",
    ["wa"] = "ㄨㄚ",       ["wo"] = "ㄨㄛ",       ["yu"] = "ㄩ",
    ["ch"] = "ㄑ",         ["yi"] = "ㄧ",         ["r"] = "ㄖ",
    ["a"] = "ㄚ",          ["o"] = "ㄛ",          ["e"] = "ㄜ"
  };

  /// <summary>
  /// 耶魯拼音排列專用處理陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapYalePinyin = new() {
    ["chwang"] = "ㄔㄨㄤ", ["shwang"] = "ㄕㄨㄤ", ["chyang"] = "ㄑㄧㄤ",
    ["chyung"] = "ㄑㄩㄥ", ["chywan"] = "ㄑㄩㄢ", ["byang"] = "ㄅㄧㄤ",
    ["dwang"] = "ㄉㄨㄤ",  ["jwang"] = "ㄓㄨㄤ",  ["syang"] = "ㄒㄧㄤ",
    ["syung"] = "ㄒㄩㄥ",  ["jyang"] = "ㄐㄧㄤ",  ["jyung"] = "ㄐㄩㄥ",
    ["nyang"] = "ㄋㄧㄤ",  ["lyang"] = "ㄌㄧㄤ",  ["gwang"] = "ㄍㄨㄤ",
    ["kwang"] = "ㄎㄨㄤ",  ["hwang"] = "ㄏㄨㄤ",  ["chang"] = "ㄔㄤ",
    ["cheng"] = "ㄔㄥ",    ["chwai"] = "ㄔㄨㄞ",  ["chwan"] = "ㄔㄨㄢ",
    ["chung"] = "ㄔㄨㄥ",  ["shang"] = "ㄕㄤ",    ["sheng"] = "ㄕㄥ",
    ["shwai"] = "ㄕㄨㄞ",  ["shwan"] = "ㄕㄨㄢ",  ["sywan"] = "ㄒㄩㄢ",
    ["jywan"] = "ㄐㄩㄢ",  ["chyau"] = "ㄑㄧㄠ",  ["chyan"] = "ㄑㄧㄢ",
    ["ching"] = "ㄑㄧㄥ",  ["sying"] = "ㄒㄧㄥ",  ["dzang"] = "ㄗㄤ",
    ["dzeng"] = "ㄗㄥ",    ["dzwan"] = "ㄗㄨㄢ",  ["dzung"] = "ㄗㄨㄥ",
    ["tsang"] = "ㄘㄤ",    ["tseng"] = "ㄘㄥ",    ["tswan"] = "ㄘㄨㄢ",
    ["tsung"] = "ㄘㄨㄥ",  ["chywe"] = "ㄑㄩㄝ",  ["lywan"] = "ㄌㄩㄢ",
    ["chwei"] = "ㄔㄨㄟ",  ["chwun"] = "ㄔㄨㄣ",  ["shwei"] = "ㄕㄨㄟ",
    ["shwun"] = "ㄕㄨㄣ",  ["chyou"] = "ㄑㄧㄡ",  ["chyun"] = "ㄑㄩㄣ",
    ["dzwei"] = "ㄗㄨㄟ",  ["dzwun"] = "ㄗㄨㄣ",  ["tswei"] = "ㄘㄨㄟ",
    ["tswun"] = "ㄘㄨㄣ",  ["kyang"] = "ㄎㄧㄤ",  ["jang"] = "ㄓㄤ",
    ["jeng"] = "ㄓㄥ",     ["jwai"] = "ㄓㄨㄞ",   ["jwan"] = "ㄓㄨㄢ",
    ["jung"] = "ㄓㄨㄥ",   ["syau"] = "ㄒㄧㄠ",   ["syan"] = "ㄒㄧㄢ",
    ["jyau"] = "ㄐㄧㄠ",   ["jyan"] = "ㄐㄧㄢ",   ["jing"] = "ㄐㄧㄥ",
    ["sywe"] = "ㄒㄩㄝ",   ["jywe"] = "ㄐㄩㄝ",   ["chye"] = "ㄑㄧㄝ",
    ["bang"] = "ㄅㄤ",     ["beng"] = "ㄅㄥ",     ["byau"] = "ㄅㄧㄠ",
    ["byan"] = "ㄅㄧㄢ",   ["bing"] = "ㄅㄧㄥ",   ["pang"] = "ㄆㄤ",
    ["peng"] = "ㄆㄥ",     ["pyau"] = "ㄆㄧㄠ",   ["pyan"] = "ㄆㄧㄢ",
    ["ping"] = "ㄆㄧㄥ",   ["mang"] = "ㄇㄤ",     ["meng"] = "ㄇㄥ",
    ["myau"] = "ㄇㄧㄠ",   ["myan"] = "ㄇㄧㄢ",   ["ming"] = "ㄇㄧㄥ",
    ["fang"] = "ㄈㄤ",     ["feng"] = "ㄈㄥ",     ["fyau"] = "ㄈㄧㄠ",
    ["dang"] = "ㄉㄤ",     ["deng"] = "ㄉㄥ",     ["dyau"] = "ㄉㄧㄠ",
    ["dyan"] = "ㄉㄧㄢ",   ["ding"] = "ㄉㄧㄥ",   ["dwan"] = "ㄉㄨㄢ",
    ["dung"] = "ㄉㄨㄥ",   ["tang"] = "ㄊㄤ",     ["teng"] = "ㄊㄥ",
    ["tyau"] = "ㄊㄧㄠ",   ["tyan"] = "ㄊㄧㄢ",   ["ting"] = "ㄊㄧㄥ",
    ["twan"] = "ㄊㄨㄢ",   ["tung"] = "ㄊㄨㄥ",   ["nang"] = "ㄋㄤ",
    ["neng"] = "ㄋㄥ",     ["nyau"] = "ㄋㄧㄠ",   ["nyan"] = "ㄋㄧㄢ",
    ["ning"] = "ㄋㄧㄥ",   ["nwan"] = "ㄋㄨㄢ",   ["nung"] = "ㄋㄨㄥ",
    ["lang"] = "ㄌㄤ",     ["leng"] = "ㄌㄥ",     ["lyau"] = "ㄌㄧㄠ",
    ["lyan"] = "ㄌㄧㄢ",   ["ling"] = "ㄌㄧㄥ",   ["lwan"] = "ㄌㄨㄢ",
    ["lung"] = "ㄌㄨㄥ",   ["gang"] = "ㄍㄤ",     ["geng"] = "ㄍㄥ",
    ["gwai"] = "ㄍㄨㄞ",   ["gwan"] = "ㄍㄨㄢ",   ["gung"] = "ㄍㄨㄥ",
    ["kang"] = "ㄎㄤ",     ["keng"] = "ㄎㄥ",     ["kwai"] = "ㄎㄨㄞ",
    ["kwan"] = "ㄎㄨㄢ",   ["kung"] = "ㄎㄨㄥ",   ["hang"] = "ㄏㄤ",
    ["heng"] = "ㄏㄥ",     ["hwai"] = "ㄏㄨㄞ",   ["hwan"] = "ㄏㄨㄢ",
    ["hung"] = "ㄏㄨㄥ",   ["jwei"] = "ㄓㄨㄟ",   ["jwun"] = "ㄓㄨㄣ",
    ["chai"] = "ㄔㄞ",     ["chau"] = "ㄔㄠ",     ["chou"] = "ㄔㄡ",
    ["chan"] = "ㄔㄢ",     ["chen"] = "ㄔㄣ",     ["chwa"] = "ㄔㄨㄚ",
    ["shai"] = "ㄕㄞ",     ["shei"] = "ㄕㄟ",     ["shau"] = "ㄕㄠ",
    ["shou"] = "ㄕㄡ",     ["shan"] = "ㄕㄢ",     ["shen"] = "ㄕㄣ",
    ["shwa"] = "ㄕㄨㄚ",   ["shwo"] = "ㄕㄨㄛ",   ["rang"] = "ㄖㄤ",
    ["reng"] = "ㄖㄥ",     ["rwan"] = "ㄖㄨㄢ",   ["rung"] = "ㄖㄨㄥ",
    ["sang"] = "ㄙㄤ",     ["seng"] = "ㄙㄥ",     ["swan"] = "ㄙㄨㄢ",
    ["sung"] = "ㄙㄨㄥ",   ["yang"] = "ㄧㄤ",     ["ying"] = "ㄧㄥ",
    ["wang"] = "ㄨㄤ",     ["weng"] = "ㄨㄥ",     ["ywan"] = "ㄩㄢ",
    ["yung"] = "ㄩㄥ",     ["syou"] = "ㄒㄧㄡ",   ["syun"] = "ㄒㄩㄣ",
    ["nywe"] = "ㄋㄩㄝ",   ["lywe"] = "ㄌㄩㄝ",   ["gwei"] = "ㄍㄨㄟ",
    ["kwei"] = "ㄎㄨㄟ",   ["jyou"] = "ㄐㄧㄡ",   ["jyun"] = "ㄐㄩㄣ",
    ["chya"] = "ㄑㄧㄚ",   ["chin"] = "ㄑㄧㄣ",   ["syin"] = "ㄒㄧㄣ",
    ["dzai"] = "ㄗㄞ",     ["dzei"] = "ㄗㄟ",     ["dzau"] = "ㄗㄠ",
    ["dzou"] = "ㄗㄡ",     ["dzan"] = "ㄗㄢ",     ["dzen"] = "ㄗㄣ",
    ["tsai"] = "ㄘㄞ",     ["tsau"] = "ㄘㄠ",     ["tsou"] = "ㄘㄡ",
    ["tsan"] = "ㄘㄢ",     ["tsen"] = "ㄘㄣ",     ["chwo"] = "ㄔㄨㄛ",
    ["myou"] = "ㄇㄧㄡ",   ["dyou"] = "ㄉㄧㄡ",   ["dwei"] = "ㄉㄨㄟ",
    ["dwun"] = "ㄉㄨㄣ",   ["twei"] = "ㄊㄨㄟ",   ["twun"] = "ㄊㄨㄣ",
    ["nyou"] = "ㄋㄧㄡ",   ["nwei"] = "ㄋㄨㄟ",   ["nwun"] = "ㄋㄨㄣ",
    ["lyou"] = "ㄌㄧㄡ",   ["lwun"] = "ㄌㄨㄣ",   ["gwun"] = "ㄍㄨㄣ",
    ["kwun"] = "ㄎㄨㄣ",   ["hwei"] = "ㄏㄨㄟ",   ["hwun"] = "ㄏㄨㄣ",
    ["rwei"] = "ㄖㄨㄟ",   ["rwun"] = "ㄖㄨㄣ",   ["dzwo"] = "ㄗㄨㄛ",
    ["tswo"] = "ㄘㄨㄛ",   ["swei"] = "ㄙㄨㄟ",   ["swun"] = "ㄙㄨㄣ",
    ["chyu"] = "ㄑㄩ",     ["giau"] = "ㄍㄧㄠ",   ["sye"] = "ㄒㄧㄝ",
    ["jye"] = "ㄐㄧㄝ",    ["jai"] = "ㄓㄞ",      ["jei"] = "ㄓㄟ",
    ["jau"] = "ㄓㄠ",      ["jou"] = "ㄓㄡ",      ["jan"] = "ㄓㄢ",
    ["jen"] = "ㄓㄣ",      ["jwa"] = "ㄓㄨㄚ",    ["sya"] = "ㄒㄧㄚ",
    ["bye"] = "ㄅㄧㄝ",    ["pye"] = "ㄆㄧㄝ",    ["mye"] = "ㄇㄧㄝ",
    ["dye"] = "ㄉㄧㄝ",    ["tye"] = "ㄊㄧㄝ",    ["nye"] = "ㄋㄧㄝ",
    ["lye"] = "ㄌㄧㄝ",    ["jya"] = "ㄐㄧㄚ",    ["jin"] = "ㄐㄧㄣ",
    ["chr"] = "ㄔ",        ["shr"] = "ㄕ",        ["ywe"] = "ㄩㄝ",
    ["jwo"] = "ㄓㄨㄛ",    ["bai"] = "ㄅㄞ",      ["bei"] = "ㄅㄟ",
    ["bau"] = "ㄅㄠ",      ["ban"] = "ㄅㄢ",      ["ben"] = "ㄅㄣ",
    ["bin"] = "ㄅㄧㄣ",    ["pai"] = "ㄆㄞ",      ["pei"] = "ㄆㄟ",
    ["pau"] = "ㄆㄠ",      ["pou"] = "ㄆㄡ",      ["pan"] = "ㄆㄢ",
    ["pen"] = "ㄆㄣ",      ["pya"] = "ㄆㄧㄚ",    ["pin"] = "ㄆㄧㄣ",
    ["mai"] = "ㄇㄞ",      ["mei"] = "ㄇㄟ",      ["mau"] = "ㄇㄠ",
    ["mou"] = "ㄇㄡ",      ["man"] = "ㄇㄢ",      ["men"] = "ㄇㄣ",
    ["min"] = "ㄇㄧㄣ",    ["fei"] = "ㄈㄟ",      ["fou"] = "ㄈㄡ",
    ["fan"] = "ㄈㄢ",      ["fen"] = "ㄈㄣ",      ["dai"] = "ㄉㄞ",
    ["dei"] = "ㄉㄟ",      ["dau"] = "ㄉㄠ",      ["dou"] = "ㄉㄡ",
    ["dan"] = "ㄉㄢ",      ["den"] = "ㄉㄣ",      ["dya"] = "ㄉㄧㄚ",
    ["tai"] = "ㄊㄞ",      ["tau"] = "ㄊㄠ",      ["tou"] = "ㄊㄡ",
    ["tan"] = "ㄊㄢ",      ["nai"] = "ㄋㄞ",      ["nei"] = "ㄋㄟ",
    ["nau"] = "ㄋㄠ",      ["nou"] = "ㄋㄡ",      ["nan"] = "ㄋㄢ",
    ["nen"] = "ㄋㄣ",      ["nin"] = "ㄋㄧㄣ",    ["lai"] = "ㄌㄞ",
    ["lei"] = "ㄌㄟ",      ["lau"] = "ㄌㄠ",      ["lou"] = "ㄌㄡ",
    ["lan"] = "ㄌㄢ",      ["lya"] = "ㄌㄧㄚ",    ["lin"] = "ㄌㄧㄣ",
    ["gai"] = "ㄍㄞ",      ["gei"] = "ㄍㄟ",      ["gau"] = "ㄍㄠ",
    ["gou"] = "ㄍㄡ",      ["gan"] = "ㄍㄢ",      ["gen"] = "ㄍㄣ",
    ["gwa"] = "ㄍㄨㄚ",    ["gwo"] = "ㄍㄨㄛ",    ["gue"] = "ㄍㄨㄜ",
    ["kai"] = "ㄎㄞ",      ["kau"] = "ㄎㄠ",      ["kou"] = "ㄎㄡ",
    ["kan"] = "ㄎㄢ",      ["ken"] = "ㄎㄣ",      ["kwa"] = "ㄎㄨㄚ",
    ["kwo"] = "ㄎㄨㄛ",    ["hai"] = "ㄏㄞ",      ["hei"] = "ㄏㄟ",
    ["hau"] = "ㄏㄠ",      ["hou"] = "ㄏㄡ",      ["han"] = "ㄏㄢ",
    ["hen"] = "ㄏㄣ",      ["hwa"] = "ㄏㄨㄚ",    ["hwo"] = "ㄏㄨㄛ",
    ["cha"] = "ㄔㄚ",      ["che"] = "ㄔㄜ",      ["chu"] = "ㄔㄨ",
    ["sha"] = "ㄕㄚ",      ["she"] = "ㄕㄜ",      ["shu"] = "ㄕㄨ",
    ["rau"] = "ㄖㄠ",      ["rou"] = "ㄖㄡ",      ["ran"] = "ㄖㄢ",
    ["ren"] = "ㄖㄣ",      ["sai"] = "ㄙㄞ",      ["sei"] = "ㄙㄟ",
    ["sau"] = "ㄙㄠ",      ["sou"] = "ㄙㄡ",      ["san"] = "ㄙㄢ",
    ["sen"] = "ㄙㄣ",      ["ang"] = "ㄤ",        ["eng"] = "ㄥ",
    ["yai"] = "ㄧㄞ",      ["yau"] = "ㄧㄠ",      ["yan"] = "ㄧㄢ",
    ["yin"] = "ㄧㄣ",      ["wai"] = "ㄨㄞ",      ["wei"] = "ㄨㄟ",
    ["wan"] = "ㄨㄢ",      ["wen"] = "ㄨㄣ",      ["yun"] = "ㄩㄣ",
    ["syu"] = "ㄒㄩ",      ["jyu"] = "ㄐㄩ",      ["chi"] = "ㄑㄧ",
    ["syi"] = "ㄒㄧ",      ["dza"] = "ㄗㄚ",      ["dze"] = "ㄗㄜ",
    ["dzu"] = "ㄗㄨ",      ["tsz"] = "ㄘ",        ["tsa"] = "ㄘㄚ",
    ["tse"] = "ㄘㄜ",      ["tsu"] = "ㄘㄨ",      ["dwo"] = "ㄉㄨㄛ",
    ["two"] = "ㄊㄨㄛ",    ["nwo"] = "ㄋㄨㄛ",    ["lwo"] = "ㄌㄨㄛ",
    ["rwo"] = "ㄖㄨㄛ",    ["swo"] = "ㄙㄨㄛ",    ["you"] = "ㄧㄡ",
    ["nyu"] = "ㄋㄩ",      ["lyu"] = "ㄌㄩ",      ["bwo"] = "ㄅㄛ",
    ["pwo"] = "ㄆㄛ",      ["mwo"] = "ㄇㄛ",      ["fwo"] = "ㄈㄛ",
    ["gin"] = "ㄍㄧㄣ",    ["jr"] = "ㄓ",         ["ja"] = "ㄓㄚ",
    ["je"] = "ㄓㄜ",       ["ju"] = "ㄓㄨ",       ["ji"] = "ㄐㄧ",
    ["dz"] = "ㄗ",         ["sz"] = "ㄙ",         ["er"] = "ㄦ",
    ["ye"] = "ㄧㄝ",       ["ba"] = "ㄅㄚ",       ["bi"] = "ㄅㄧ",
    ["bu"] = "ㄅㄨ",       ["pa"] = "ㄆㄚ",       ["pi"] = "ㄆㄧ",
    ["pu"] = "ㄆㄨ",       ["ma"] = "ㄇㄚ",       ["me"] = "ㄇㄜ",
    ["mi"] = "ㄇㄧ",       ["mu"] = "ㄇㄨ",       ["fa"] = "ㄈㄚ",
    ["fu"] = "ㄈㄨ",       ["da"] = "ㄉㄚ",       ["de"] = "ㄉㄜ",
    ["di"] = "ㄉㄧ",       ["du"] = "ㄉㄨ",       ["ta"] = "ㄊㄚ",
    ["te"] = "ㄊㄜ",       ["ti"] = "ㄊㄧ",       ["tu"] = "ㄊㄨ",
    ["na"] = "ㄋㄚ",       ["ne"] = "ㄋㄜ",       ["ni"] = "ㄋㄧ",
    ["nu"] = "ㄋㄨ",       ["la"] = "ㄌㄚ",       ["lo"] = "ㄌㄛ",
    ["le"] = "ㄌㄜ",       ["li"] = "ㄌㄧ",       ["lu"] = "ㄌㄨ",
    ["ga"] = "ㄍㄚ",       ["ge"] = "ㄍㄜ",       ["gu"] = "ㄍㄨ",
    ["ka"] = "ㄎㄚ",       ["ke"] = "ㄎㄜ",       ["ku"] = "ㄎㄨ",
    ["ha"] = "ㄏㄚ",       ["he"] = "ㄏㄜ",       ["hu"] = "ㄏㄨ",
    ["re"] = "ㄖㄜ",       ["ru"] = "ㄖㄨ",       ["sa"] = "ㄙㄚ",
    ["se"] = "ㄙㄜ",       ["su"] = "ㄙㄨ",       ["eh"] = "ㄝ",
    ["ai"] = "ㄞ",         ["ei"] = "ㄟ",         ["au"] = "ㄠ",
    ["ou"] = "ㄡ",         ["an"] = "ㄢ",         ["en"] = "ㄣ",
    ["ya"] = "ㄧㄚ",       ["yo"] = "ㄧㄛ",       ["wu"] = "ㄨ",
    ["wa"] = "ㄨㄚ",       ["wo"] = "ㄨㄛ",       ["yu"] = "ㄩ",
    ["ch"] = "ㄑ",         ["yi"] = "ㄧ",         ["r"] = "ㄖ",
    ["a"] = "ㄚ",          ["o"] = "ㄛ",          ["e"] = "ㄜ"
  };

  /// <summary>
  /// 華羅拼音排列專用處理陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapHualuoPinyin = new() {
    ["shuang"] = "ㄕㄨㄤ", ["jhuang"] = "ㄓㄨㄤ", ["chyueh"] = "ㄑㄩㄝ",
    ["chyuan"] = "ㄑㄩㄢ", ["chyong"] = "ㄑㄩㄥ", ["chiang"] = "ㄑㄧㄤ",
    ["chuang"] = "ㄔㄨㄤ", ["biang"] = "ㄅㄧㄤ",  ["duang"] = "ㄉㄨㄤ",
    ["kyang"] = "ㄎㄧㄤ",  ["syueh"] = "ㄒㄩㄝ",  ["syuan"] = "ㄒㄩㄢ",
    ["syong"] = "ㄒㄩㄥ",  ["sihei"] = "ㄙㄟ",    ["siang"] = "ㄒㄧㄤ",
    ["shuei"] = "ㄕㄨㄟ",  ["shuan"] = "ㄕㄨㄢ",  ["shuai"] = "ㄕㄨㄞ",
    ["sheng"] = "ㄕㄥ",    ["shang"] = "ㄕㄤ",    ["nyueh"] = "ㄋㄩㄝ",
    ["niang"] = "ㄋㄧㄤ",  ["lyueh"] = "ㄌㄩㄝ",  ["lyuan"] = "ㄌㄩㄢ",
    ["liang"] = "ㄌㄧㄤ",  ["kuang"] = "ㄎㄨㄤ",  ["jyueh"] = "ㄐㄩㄝ",
    ["jyuan"] = "ㄐㄩㄢ",  ["jyong"] = "ㄐㄩㄥ",  ["jiang"] = "ㄐㄧㄤ",
    ["jhuei"] = "ㄓㄨㄟ",  ["jhuan"] = "ㄓㄨㄢ",  ["jhuai"] = "ㄓㄨㄞ",
    ["jhong"] = "ㄓㄨㄥ",  ["jheng"] = "ㄓㄥ",    ["jhang"] = "ㄓㄤ",
    ["huang"] = "ㄏㄨㄤ",  ["guang"] = "ㄍㄨㄤ",  ["chyun"] = "ㄑㄩㄣ",
    ["tsuei"] = "ㄘㄨㄟ",  ["tsuan"] = "ㄘㄨㄢ",  ["tsong"] = "ㄘㄨㄥ",
    ["chiou"] = "ㄑㄧㄡ",  ["ching"] = "ㄑㄧㄥ",  ["chieh"] = "ㄑㄧㄝ",
    ["chiao"] = "ㄑㄧㄠ",  ["chian"] = "ㄑㄧㄢ",  ["chuei"] = "ㄔㄨㄟ",
    ["chuan"] = "ㄔㄨㄢ",  ["chuai"] = "ㄔㄨㄞ",  ["chong"] = "ㄔㄨㄥ",
    ["cheng"] = "ㄔㄥ",    ["chang"] = "ㄔㄤ",    ["tseng"] = "ㄘㄥ",
    ["tsang"] = "ㄘㄤ",    ["gyao"] = "ㄍㄧㄠ",   ["fiao"] = "ㄈㄧㄠ",
    ["zuei"] = "ㄗㄨㄟ",   ["zuan"] = "ㄗㄨㄢ",   ["zong"] = "ㄗㄨㄥ",
    ["zeng"] = "ㄗㄥ",     ["zang"] = "ㄗㄤ",     ["yueh"] = "ㄩㄝ",
    ["yuan"] = "ㄩㄢ",     ["yong"] = "ㄩㄥ",     ["ying"] = "ㄧㄥ",
    ["yang"] = "ㄧㄤ",     ["wong"] = "ㄨㄥ",     ["wang"] = "ㄨㄤ",
    ["tuei"] = "ㄊㄨㄟ",   ["tuan"] = "ㄊㄨㄢ",   ["tong"] = "ㄊㄨㄥ",
    ["ting"] = "ㄊㄧㄥ",   ["tieh"] = "ㄊㄧㄝ",   ["tiao"] = "ㄊㄧㄠ",
    ["tian"] = "ㄊㄧㄢ",   ["teng"] = "ㄊㄥ",     ["tang"] = "ㄊㄤ",
    ["syun"] = "ㄒㄩㄣ",   ["suei"] = "ㄙㄨㄟ",   ["suan"] = "ㄙㄨㄢ",
    ["song"] = "ㄙㄨㄥ",   ["siou"] = "ㄒㄧㄡ",   ["sing"] = "ㄒㄧㄥ",
    ["sieh"] = "ㄒㄧㄝ",   ["siao"] = "ㄒㄧㄠ",   ["sian"] = "ㄒㄧㄢ",
    ["shuo"] = "ㄕㄨㄛ",   ["shun"] = "ㄕㄨㄣ",   ["shua"] = "ㄕㄨㄚ",
    ["shou"] = "ㄕㄡ",     ["shih"] = "ㄕ",       ["shen"] = "ㄕㄣ",
    ["shei"] = "ㄕㄟ",     ["shao"] = "ㄕㄠ",     ["shan"] = "ㄕㄢ",
    ["shai"] = "ㄕㄞ",     ["seng"] = "ㄙㄥ",     ["sang"] = "ㄙㄤ",
    ["ruei"] = "ㄖㄨㄟ",   ["ruan"] = "ㄖㄨㄢ",   ["rong"] = "ㄖㄨㄥ",
    ["reng"] = "ㄖㄥ",     ["rang"] = "ㄖㄤ",     ["ping"] = "ㄆㄧㄥ",
    ["pieh"] = "ㄆㄧㄝ",   ["piao"] = "ㄆㄧㄠ",   ["pian"] = "ㄆㄧㄢ",
    ["peng"] = "ㄆㄥ",     ["pang"] = "ㄆㄤ",     ["nuei"] = "ㄋㄨㄟ",
    ["nuan"] = "ㄋㄨㄢ",   ["nong"] = "ㄋㄨㄥ",   ["niou"] = "ㄋㄧㄡ",
    ["ning"] = "ㄋㄧㄥ",   ["nieh"] = "ㄋㄧㄝ",   ["niao"] = "ㄋㄧㄠ",
    ["nian"] = "ㄋㄧㄢ",   ["neng"] = "ㄋㄥ",     ["nang"] = "ㄋㄤ",
    ["miou"] = "ㄇㄧㄡ",   ["ming"] = "ㄇㄧㄥ",   ["mieh"] = "ㄇㄧㄝ",
    ["miao"] = "ㄇㄧㄠ",   ["mian"] = "ㄇㄧㄢ",   ["meng"] = "ㄇㄥ",
    ["mang"] = "ㄇㄤ",     ["luan"] = "ㄌㄨㄢ",   ["long"] = "ㄌㄨㄥ",
    ["liou"] = "ㄌㄧㄡ",   ["ling"] = "ㄌㄧㄥ",   ["lieh"] = "ㄌㄧㄝ",
    ["liao"] = "ㄌㄧㄠ",   ["lian"] = "ㄌㄧㄢ",   ["leng"] = "ㄌㄥ",
    ["lang"] = "ㄌㄤ",     ["kuei"] = "ㄎㄨㄟ",   ["kuan"] = "ㄎㄨㄢ",
    ["kuai"] = "ㄎㄨㄞ",   ["kong"] = "ㄎㄨㄥ",   ["keng"] = "ㄎㄥ",
    ["kang"] = "ㄎㄤ",     ["jyun"] = "ㄐㄩㄣ",   ["jiou"] = "ㄐㄧㄡ",
    ["jing"] = "ㄐㄧㄥ",   ["jieh"] = "ㄐㄧㄝ",   ["jiao"] = "ㄐㄧㄠ",
    ["jian"] = "ㄐㄧㄢ",   ["jhuo"] = "ㄓㄨㄛ",   ["jhun"] = "ㄓㄨㄣ",
    ["jhua"] = "ㄓㄨㄚ",   ["jhou"] = "ㄓㄡ",     ["jhih"] = "ㄓ",
    ["jhen"] = "ㄓㄣ",     ["jhei"] = "ㄓㄟ",     ["jhao"] = "ㄓㄠ",
    ["jhan"] = "ㄓㄢ",     ["jhai"] = "ㄓㄞ",     ["huei"] = "ㄏㄨㄟ",
    ["huan"] = "ㄏㄨㄢ",   ["huai"] = "ㄏㄨㄞ",   ["hong"] = "ㄏㄨㄥ",
    ["heng"] = "ㄏㄥ",     ["hang"] = "ㄏㄤ",     ["guei"] = "ㄍㄨㄟ",
    ["guan"] = "ㄍㄨㄢ",   ["guai"] = "ㄍㄨㄞ",   ["gong"] = "ㄍㄨㄥ",
    ["geng"] = "ㄍㄥ",     ["gang"] = "ㄍㄤ",     ["feng"] = "ㄈㄥ",
    ["fang"] = "ㄈㄤ",     ["duei"] = "ㄉㄨㄟ",   ["duan"] = "ㄉㄨㄢ",
    ["dong"] = "ㄉㄨㄥ",   ["diou"] = "ㄉㄧㄡ",   ["ding"] = "ㄉㄧㄥ",
    ["dieh"] = "ㄉㄧㄝ",   ["diao"] = "ㄉㄧㄠ",   ["dian"] = "ㄉㄧㄢ",
    ["deng"] = "ㄉㄥ",     ["dang"] = "ㄉㄤ",     ["chyu"] = "ㄑㄩ",
    ["tsuo"] = "ㄘㄨㄛ",   ["tsun"] = "ㄘㄨㄣ",   ["tsou"] = "ㄘㄡ",
    ["chin"] = "ㄑㄧㄣ",   ["tsih"] = "ㄘ",       ["chia"] = "ㄑㄧㄚ",
    ["chuo"] = "ㄔㄨㄛ",   ["chun"] = "ㄔㄨㄣ",   ["chua"] = "ㄔㄨㄚ",
    ["chou"] = "ㄔㄡ",     ["chih"] = "ㄔ",       ["chen"] = "ㄔㄣ",
    ["chao"] = "ㄔㄠ",     ["chan"] = "ㄔㄢ",     ["chai"] = "ㄔㄞ",
    ["tsen"] = "ㄘㄣ",     ["tsao"] = "ㄘㄠ",     ["tsan"] = "ㄘㄢ",
    ["tsai"] = "ㄘㄞ",     ["bing"] = "ㄅㄧㄥ",   ["bieh"] = "ㄅㄧㄝ",
    ["biao"] = "ㄅㄧㄠ",   ["bian"] = "ㄅㄧㄢ",   ["beng"] = "ㄅㄥ",
    ["bang"] = "ㄅㄤ",     ["gin"] = "ㄍㄧㄣ",    ["den"] = "ㄉㄣ",
    ["zuo"] = "ㄗㄨㄛ",    ["zun"] = "ㄗㄨㄣ",    ["zou"] = "ㄗㄡ",
    ["zih"] = "ㄗ",        ["zen"] = "ㄗㄣ",      ["zei"] = "ㄗㄟ",
    ["zao"] = "ㄗㄠ",      ["zan"] = "ㄗㄢ",      ["zai"] = "ㄗㄞ",
    ["yun"] = "ㄩㄣ",      ["you"] = "ㄧㄡ",      ["yin"] = "ㄧㄣ",
    ["yeh"] = "ㄧㄝ",      ["yao"] = "ㄧㄠ",      ["yan"] = "ㄧㄢ",
    ["yai"] = "ㄧㄞ",      ["wun"] = "ㄨㄣ",      ["wei"] = "ㄨㄟ",
    ["wan"] = "ㄨㄢ",      ["wai"] = "ㄨㄞ",      ["tuo"] = "ㄊㄨㄛ",
    ["tun"] = "ㄊㄨㄣ",    ["tou"] = "ㄊㄡ",      ["tao"] = "ㄊㄠ",
    ["tan"] = "ㄊㄢ",      ["tai"] = "ㄊㄞ",      ["syu"] = "ㄒㄩ",
    ["suo"] = "ㄙㄨㄛ",    ["sun"] = "ㄙㄨㄣ",    ["sou"] = "ㄙㄡ",
    ["sin"] = "ㄒㄧㄣ",    ["sih"] = "ㄙ",        ["sia"] = "ㄒㄧㄚ",
    ["shu"] = "ㄕㄨ",      ["she"] = "ㄕㄜ",      ["sha"] = "ㄕㄚ",
    ["sen"] = "ㄙㄣ",      ["sao"] = "ㄙㄠ",      ["san"] = "ㄙㄢ",
    ["sai"] = "ㄙㄞ",      ["ruo"] = "ㄖㄨㄛ",    ["run"] = "ㄖㄨㄣ",
    ["rou"] = "ㄖㄡ",      ["rih"] = "ㄖ",        ["ren"] = "ㄖㄣ",
    ["rao"] = "ㄖㄠ",      ["ran"] = "ㄖㄢ",      ["pou"] = "ㄆㄡ",
    ["pin"] = "ㄆㄧㄣ",    ["pia"] = "ㄆㄧㄚ",    ["pen"] = "ㄆㄣ",
    ["pei"] = "ㄆㄟ",      ["pao"] = "ㄆㄠ",      ["pan"] = "ㄆㄢ",
    ["pai"] = "ㄆㄞ",      ["nyu"] = "ㄋㄩ",      ["nuo"] = "ㄋㄨㄛ",
    ["nun"] = "ㄋㄨㄣ",    ["nou"] = "ㄋㄡ",      ["nin"] = "ㄋㄧㄣ",
    ["nen"] = "ㄋㄣ",      ["nei"] = "ㄋㄟ",      ["nao"] = "ㄋㄠ",
    ["nan"] = "ㄋㄢ",      ["nai"] = "ㄋㄞ",      ["mou"] = "ㄇㄡ",
    ["min"] = "ㄇㄧㄣ",    ["men"] = "ㄇㄣ",      ["mei"] = "ㄇㄟ",
    ["mao"] = "ㄇㄠ",      ["man"] = "ㄇㄢ",      ["mai"] = "ㄇㄞ",
    ["lyu"] = "ㄌㄩ",      ["luo"] = "ㄌㄨㄛ",    ["lun"] = "ㄌㄨㄣ",
    ["lou"] = "ㄌㄡ",      ["lin"] = "ㄌㄧㄣ",    ["lia"] = "ㄌㄧㄚ",
    ["lei"] = "ㄌㄟ",      ["lao"] = "ㄌㄠ",      ["lan"] = "ㄌㄢ",
    ["lai"] = "ㄌㄞ",      ["kuo"] = "ㄎㄨㄛ",    ["kun"] = "ㄎㄨㄣ",
    ["kua"] = "ㄎㄨㄚ",    ["kou"] = "ㄎㄡ",      ["ken"] = "ㄎㄣ",
    ["kao"] = "ㄎㄠ",      ["kan"] = "ㄎㄢ",      ["kai"] = "ㄎㄞ",
    ["jyu"] = "ㄐㄩ",      ["jin"] = "ㄐㄧㄣ",    ["jia"] = "ㄐㄧㄚ",
    ["jhu"] = "ㄓㄨ",      ["jhe"] = "ㄓㄜ",      ["jha"] = "ㄓㄚ",
    ["huo"] = "ㄏㄨㄛ",    ["hun"] = "ㄏㄨㄣ",    ["hua"] = "ㄏㄨㄚ",
    ["hou"] = "ㄏㄡ",      ["hen"] = "ㄏㄣ",      ["hei"] = "ㄏㄟ",
    ["hao"] = "ㄏㄠ",      ["han"] = "ㄏㄢ",      ["hai"] = "ㄏㄞ",
    ["guo"] = "ㄍㄨㄛ",    ["gun"] = "ㄍㄨㄣ",    ["gue"] = "ㄍㄨㄜ",
    ["gua"] = "ㄍㄨㄚ",    ["gou"] = "ㄍㄡ",      ["gen"] = "ㄍㄣ",
    ["gei"] = "ㄍㄟ",      ["gao"] = "ㄍㄠ",      ["gan"] = "ㄍㄢ",
    ["gai"] = "ㄍㄞ",      ["fou"] = "ㄈㄡ",      ["fen"] = "ㄈㄣ",
    ["fei"] = "ㄈㄟ",      ["fan"] = "ㄈㄢ",      ["eng"] = "ㄥ",
    ["duo"] = "ㄉㄨㄛ",    ["dun"] = "ㄉㄨㄣ",    ["dou"] = "ㄉㄡ",
    ["dia"] = "ㄉㄧㄚ",    ["dei"] = "ㄉㄟ",      ["dao"] = "ㄉㄠ",
    ["dan"] = "ㄉㄢ",      ["dai"] = "ㄉㄞ",      ["tsu"] = "ㄘㄨ",
    ["chi"] = "ㄑㄧ",      ["chu"] = "ㄔㄨ",      ["che"] = "ㄔㄜ",
    ["cha"] = "ㄔㄚ",      ["tse"] = "ㄘㄜ",      ["tsa"] = "ㄘㄚ",
    ["bin"] = "ㄅㄧㄣ",    ["ben"] = "ㄅㄣ",      ["bei"] = "ㄅㄟ",
    ["bao"] = "ㄅㄠ",      ["ban"] = "ㄅㄢ",      ["bai"] = "ㄅㄞ",
    ["ang"] = "ㄤ",        ["ch"] = "ㄑ",         ["zu"] = "ㄗㄨ",
    ["ze"] = "ㄗㄜ",       ["za"] = "ㄗㄚ",       ["yu"] = "ㄩ",
    ["yo"] = "ㄧㄛ",       ["ya"] = "ㄧㄚ",       ["yi"] = "ㄧ",
    ["wu"] = "ㄨ",         ["wo"] = "ㄨㄛ",       ["wa"] = "ㄨㄚ",
    ["tu"] = "ㄊㄨ",       ["ti"] = "ㄊㄧ",       ["te"] = "ㄊㄜ",
    ["ta"] = "ㄊㄚ",       ["su"] = "ㄙㄨ",       ["si"] = "ㄒㄧ",
    ["se"] = "ㄙㄜ",       ["sa"] = "ㄙㄚ",       ["ru"] = "ㄖㄨ",
    ["re"] = "ㄖㄜ",       ["pu"] = "ㄆㄨ",       ["po"] = "ㄆㄛ",
    ["pi"] = "ㄆㄧ",       ["pa"] = "ㄆㄚ",       ["ou"] = "ㄡ",
    ["nu"] = "ㄋㄨ",       ["ni"] = "ㄋㄧ",       ["ne"] = "ㄋㄜ",
    ["na"] = "ㄋㄚ",       ["mu"] = "ㄇㄨ",       ["mo"] = "ㄇㄛ",
    ["mi"] = "ㄇㄧ",       ["me"] = "ㄇㄜ",       ["ma"] = "ㄇㄚ",
    ["lu"] = "ㄌㄨ",       ["lo"] = "ㄌㄛ",       ["li"] = "ㄌㄧ",
    ["le"] = "ㄌㄜ",       ["la"] = "ㄌㄚ",       ["ku"] = "ㄎㄨ",
    ["ke"] = "ㄎㄜ",       ["ka"] = "ㄎㄚ",       ["ji"] = "ㄐㄧ",
    ["hu"] = "ㄏㄨ",       ["he"] = "ㄏㄜ",       ["ha"] = "ㄏㄚ",
    ["gu"] = "ㄍㄨ",       ["ge"] = "ㄍㄜ",       ["ga"] = "ㄍㄚ",
    ["fu"] = "ㄈㄨ",       ["fo"] = "ㄈㄛ",       ["fa"] = "ㄈㄚ",
    ["er"] = "ㄦ",         ["en"] = "ㄣ",         ["ei"] = "ㄟ",
    ["eh"] = "ㄝ",         ["du"] = "ㄉㄨ",       ["di"] = "ㄉㄧ",
    ["de"] = "ㄉㄜ",       ["da"] = "ㄉㄚ",       ["bu"] = "ㄅㄨ",
    ["bo"] = "ㄅㄛ",       ["bi"] = "ㄅㄧ",       ["ba"] = "ㄅㄚ",
    ["ao"] = "ㄠ",         ["an"] = "ㄢ",         ["ai"] = "ㄞ",
    ["o"] = "ㄛ",          ["e"] = "ㄜ",          ["a"] = "ㄚ"
  };

  /// <summary>
  /// 通用拼音排列專用處理陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapUniversalPinyin = new() {
    ["shuang"] = "ㄕㄨㄤ", ["jhuang"] = "ㄓㄨㄤ", ["chuang"] = "ㄔㄨㄤ",
    ["biang"] = "ㄅㄧㄤ",  ["duang"] = "ㄉㄨㄤ",  ["cyuan"] = "ㄑㄩㄢ",
    ["cyong"] = "ㄑㄩㄥ",  ["ciang"] = "ㄑㄧㄤ",  ["kyang"] = "ㄎㄧㄤ",
    ["syuan"] = "ㄒㄩㄢ",  ["syong"] = "ㄒㄩㄥ",  ["sihei"] = "ㄙㄟ",
    ["siang"] = "ㄒㄧㄤ",  ["shuei"] = "ㄕㄨㄟ",  ["shuan"] = "ㄕㄨㄢ",
    ["shuai"] = "ㄕㄨㄞ",  ["sheng"] = "ㄕㄥ",    ["shang"] = "ㄕㄤ",
    ["niang"] = "ㄋㄧㄤ",  ["lyuan"] = "ㄌㄩㄢ",  ["liang"] = "ㄌㄧㄤ",
    ["kuang"] = "ㄎㄨㄤ",  ["jyuan"] = "ㄐㄩㄢ",  ["jyong"] = "ㄐㄩㄥ",
    ["jiang"] = "ㄐㄧㄤ",  ["jhuei"] = "ㄓㄨㄟ",  ["jhuan"] = "ㄓㄨㄢ",
    ["jhuai"] = "ㄓㄨㄞ",  ["jhong"] = "ㄓㄨㄥ",  ["jheng"] = "ㄓㄥ",
    ["jhang"] = "ㄓㄤ",    ["huang"] = "ㄏㄨㄤ",  ["guang"] = "ㄍㄨㄤ",
    ["chuei"] = "ㄔㄨㄟ",  ["chuan"] = "ㄔㄨㄢ",  ["chuai"] = "ㄔㄨㄞ",
    ["chong"] = "ㄔㄨㄥ",  ["cheng"] = "ㄔㄥ",    ["chang"] = "ㄔㄤ",
    ["cyue"] = "ㄑㄩㄝ",   ["syue"] = "ㄒㄩㄝ",   ["nyue"] = "ㄋㄩㄝ",
    ["lyue"] = "ㄌㄩㄝ",   ["jyue"] = "ㄐㄩㄝ",   ["cyun"] = "ㄑㄩㄣ",
    ["cuei"] = "ㄘㄨㄟ",   ["cuan"] = "ㄘㄨㄢ",   ["cong"] = "ㄘㄨㄥ",
    ["ciou"] = "ㄑㄧㄡ",   ["cing"] = "ㄑㄧㄥ",   ["ciao"] = "ㄑㄧㄠ",
    ["cian"] = "ㄑㄧㄢ",   ["ceng"] = "ㄘㄥ",     ["cang"] = "ㄘㄤ",
    ["gyao"] = "ㄍㄧㄠ",   ["fiao"] = "ㄈㄧㄠ",   ["zuei"] = "ㄗㄨㄟ",
    ["zuan"] = "ㄗㄨㄢ",   ["zong"] = "ㄗㄨㄥ",   ["zeng"] = "ㄗㄥ",
    ["zang"] = "ㄗㄤ",     ["yuan"] = "ㄩㄢ",     ["yong"] = "ㄩㄥ",
    ["ying"] = "ㄧㄥ",     ["yang"] = "ㄧㄤ",     ["wong"] = "ㄨㄥ",
    ["wang"] = "ㄨㄤ",     ["tuei"] = "ㄊㄨㄟ",   ["tuan"] = "ㄊㄨㄢ",
    ["tong"] = "ㄊㄨㄥ",   ["ting"] = "ㄊㄧㄥ",   ["tiao"] = "ㄊㄧㄠ",
    ["tian"] = "ㄊㄧㄢ",   ["teng"] = "ㄊㄥ",     ["tang"] = "ㄊㄤ",
    ["syun"] = "ㄒㄩㄣ",   ["suei"] = "ㄙㄨㄟ",   ["suan"] = "ㄙㄨㄢ",
    ["song"] = "ㄙㄨㄥ",   ["siou"] = "ㄒㄧㄡ",   ["sing"] = "ㄒㄧㄥ",
    ["siao"] = "ㄒㄧㄠ",   ["sian"] = "ㄒㄧㄢ",   ["shuo"] = "ㄕㄨㄛ",
    ["shun"] = "ㄕㄨㄣ",   ["shua"] = "ㄕㄨㄚ",   ["shou"] = "ㄕㄡ",
    ["shih"] = "ㄕ",       ["shen"] = "ㄕㄣ",     ["shei"] = "ㄕㄟ",
    ["shao"] = "ㄕㄠ",     ["shan"] = "ㄕㄢ",     ["shai"] = "ㄕㄞ",
    ["seng"] = "ㄙㄥ",     ["sang"] = "ㄙㄤ",     ["ruei"] = "ㄖㄨㄟ",
    ["ruan"] = "ㄖㄨㄢ",   ["rong"] = "ㄖㄨㄥ",   ["reng"] = "ㄖㄥ",
    ["rang"] = "ㄖㄤ",     ["ping"] = "ㄆㄧㄥ",   ["piao"] = "ㄆㄧㄠ",
    ["pian"] = "ㄆㄧㄢ",   ["peng"] = "ㄆㄥ",     ["pang"] = "ㄆㄤ",
    ["nuei"] = "ㄋㄨㄟ",   ["nuan"] = "ㄋㄨㄢ",   ["nong"] = "ㄋㄨㄥ",
    ["niou"] = "ㄋㄧㄡ",   ["ning"] = "ㄋㄧㄥ",   ["niao"] = "ㄋㄧㄠ",
    ["nian"] = "ㄋㄧㄢ",   ["neng"] = "ㄋㄥ",     ["nang"] = "ㄋㄤ",
    ["miou"] = "ㄇㄧㄡ",   ["ming"] = "ㄇㄧㄥ",   ["miao"] = "ㄇㄧㄠ",
    ["mian"] = "ㄇㄧㄢ",   ["meng"] = "ㄇㄥ",     ["mang"] = "ㄇㄤ",
    ["luan"] = "ㄌㄨㄢ",   ["long"] = "ㄌㄨㄥ",   ["liou"] = "ㄌㄧㄡ",
    ["ling"] = "ㄌㄧㄥ",   ["liao"] = "ㄌㄧㄠ",   ["lian"] = "ㄌㄧㄢ",
    ["leng"] = "ㄌㄥ",     ["lang"] = "ㄌㄤ",     ["kuei"] = "ㄎㄨㄟ",
    ["kuan"] = "ㄎㄨㄢ",   ["kuai"] = "ㄎㄨㄞ",   ["kong"] = "ㄎㄨㄥ",
    ["keng"] = "ㄎㄥ",     ["kang"] = "ㄎㄤ",     ["jyun"] = "ㄐㄩㄣ",
    ["jiou"] = "ㄐㄧㄡ",   ["jing"] = "ㄐㄧㄥ",   ["jiao"] = "ㄐㄧㄠ",
    ["jian"] = "ㄐㄧㄢ",   ["jhuo"] = "ㄓㄨㄛ",   ["jhun"] = "ㄓㄨㄣ",
    ["jhua"] = "ㄓㄨㄚ",   ["jhou"] = "ㄓㄡ",     ["jhih"] = "ㄓ",
    ["jhen"] = "ㄓㄣ",     ["jhei"] = "ㄓㄟ",     ["jhao"] = "ㄓㄠ",
    ["jhan"] = "ㄓㄢ",     ["jhai"] = "ㄓㄞ",     ["huei"] = "ㄏㄨㄟ",
    ["huan"] = "ㄏㄨㄢ",   ["huai"] = "ㄏㄨㄞ",   ["hong"] = "ㄏㄨㄥ",
    ["heng"] = "ㄏㄥ",     ["hang"] = "ㄏㄤ",     ["guei"] = "ㄍㄨㄟ",
    ["guan"] = "ㄍㄨㄢ",   ["guai"] = "ㄍㄨㄞ",   ["gong"] = "ㄍㄨㄥ",
    ["geng"] = "ㄍㄥ",     ["gang"] = "ㄍㄤ",     ["fong"] = "ㄈㄥ",
    ["fang"] = "ㄈㄤ",     ["duei"] = "ㄉㄨㄟ",   ["duan"] = "ㄉㄨㄢ",
    ["dong"] = "ㄉㄨㄥ",   ["diou"] = "ㄉㄧㄡ",   ["ding"] = "ㄉㄧㄥ",
    ["diao"] = "ㄉㄧㄠ",   ["dian"] = "ㄉㄧㄢ",   ["deng"] = "ㄉㄥ",
    ["dang"] = "ㄉㄤ",     ["chuo"] = "ㄔㄨㄛ",   ["chun"] = "ㄔㄨㄣ",
    ["chua"] = "ㄔㄨㄚ",   ["chou"] = "ㄔㄡ",     ["chih"] = "ㄔ",
    ["chen"] = "ㄔㄣ",     ["chao"] = "ㄔㄠ",     ["chan"] = "ㄔㄢ",
    ["chai"] = "ㄔㄞ",     ["bing"] = "ㄅㄧㄥ",   ["biao"] = "ㄅㄧㄠ",
    ["bian"] = "ㄅㄧㄢ",   ["beng"] = "ㄅㄥ",     ["bang"] = "ㄅㄤ",
    ["cie"] = "ㄑㄧㄝ",    ["yue"] = "ㄩㄝ",      ["tie"] = "ㄊㄧㄝ",
    ["sie"] = "ㄒㄧㄝ",    ["pie"] = "ㄆㄧㄝ",    ["nie"] = "ㄋㄧㄝ",
    ["mie"] = "ㄇㄧㄝ",    ["lie"] = "ㄌㄧㄝ",    ["jie"] = "ㄐㄧㄝ",
    ["die"] = "ㄉㄧㄝ",    ["cyu"] = "ㄑㄩ",      ["cuo"] = "ㄘㄨㄛ",
    ["cun"] = "ㄘㄨㄣ",    ["cou"] = "ㄘㄡ",      ["cin"] = "ㄑㄧㄣ",
    ["cih"] = "ㄘ",        ["cia"] = "ㄑㄧㄚ",    ["cen"] = "ㄘㄣ",
    ["cao"] = "ㄘㄠ",      ["can"] = "ㄘㄢ",      ["cai"] = "ㄘㄞ",
    ["bie"] = "ㄅㄧㄝ",    ["gin"] = "ㄍㄧㄣ",    ["den"] = "ㄉㄣ",
    ["zuo"] = "ㄗㄨㄛ",    ["zun"] = "ㄗㄨㄣ",    ["zou"] = "ㄗㄡ",
    ["zih"] = "ㄗ",        ["zen"] = "ㄗㄣ",      ["zei"] = "ㄗㄟ",
    ["zao"] = "ㄗㄠ",      ["zan"] = "ㄗㄢ",      ["zai"] = "ㄗㄞ",
    ["yun"] = "ㄩㄣ",      ["you"] = "ㄧㄡ",      ["yin"] = "ㄧㄣ",
    ["yao"] = "ㄧㄠ",      ["yan"] = "ㄧㄢ",      ["yai"] = "ㄧㄞ",
    ["wun"] = "ㄨㄣ",      ["wei"] = "ㄨㄟ",      ["wan"] = "ㄨㄢ",
    ["wai"] = "ㄨㄞ",      ["tuo"] = "ㄊㄨㄛ",    ["tun"] = "ㄊㄨㄣ",
    ["tou"] = "ㄊㄡ",      ["tao"] = "ㄊㄠ",      ["tan"] = "ㄊㄢ",
    ["tai"] = "ㄊㄞ",      ["syu"] = "ㄒㄩ",      ["suo"] = "ㄙㄨㄛ",
    ["sun"] = "ㄙㄨㄣ",    ["sou"] = "ㄙㄡ",      ["sin"] = "ㄒㄧㄣ",
    ["sih"] = "ㄙ",        ["sia"] = "ㄒㄧㄚ",    ["shu"] = "ㄕㄨ",
    ["she"] = "ㄕㄜ",      ["sha"] = "ㄕㄚ",      ["sen"] = "ㄙㄣ",
    ["sao"] = "ㄙㄠ",      ["san"] = "ㄙㄢ",      ["sai"] = "ㄙㄞ",
    ["ruo"] = "ㄖㄨㄛ",    ["run"] = "ㄖㄨㄣ",    ["rou"] = "ㄖㄡ",
    ["rih"] = "ㄖ",        ["ren"] = "ㄖㄣ",      ["rao"] = "ㄖㄠ",
    ["ran"] = "ㄖㄢ",      ["pou"] = "ㄆㄡ",      ["pin"] = "ㄆㄧㄣ",
    ["pia"] = "ㄆㄧㄚ",    ["pen"] = "ㄆㄣ",      ["pei"] = "ㄆㄟ",
    ["pao"] = "ㄆㄠ",      ["pan"] = "ㄆㄢ",      ["pai"] = "ㄆㄞ",
    ["nyu"] = "ㄋㄩ",      ["nuo"] = "ㄋㄨㄛ",    ["nun"] = "ㄋㄨㄣ",
    ["nou"] = "ㄋㄡ",      ["nin"] = "ㄋㄧㄣ",    ["nen"] = "ㄋㄣ",
    ["nei"] = "ㄋㄟ",      ["nao"] = "ㄋㄠ",      ["nan"] = "ㄋㄢ",
    ["nai"] = "ㄋㄞ",      ["mou"] = "ㄇㄡ",      ["min"] = "ㄇㄧㄣ",
    ["men"] = "ㄇㄣ",      ["mei"] = "ㄇㄟ",      ["mao"] = "ㄇㄠ",
    ["man"] = "ㄇㄢ",      ["mai"] = "ㄇㄞ",      ["lyu"] = "ㄌㄩ",
    ["luo"] = "ㄌㄨㄛ",    ["lun"] = "ㄌㄨㄣ",    ["lou"] = "ㄌㄡ",
    ["lin"] = "ㄌㄧㄣ",    ["lia"] = "ㄌㄧㄚ",    ["lei"] = "ㄌㄟ",
    ["lao"] = "ㄌㄠ",      ["lan"] = "ㄌㄢ",      ["lai"] = "ㄌㄞ",
    ["kuo"] = "ㄎㄨㄛ",    ["kun"] = "ㄎㄨㄣ",    ["kua"] = "ㄎㄨㄚ",
    ["kou"] = "ㄎㄡ",      ["ken"] = "ㄎㄣ",      ["kao"] = "ㄎㄠ",
    ["kan"] = "ㄎㄢ",      ["kai"] = "ㄎㄞ",      ["jyu"] = "ㄐㄩ",
    ["jin"] = "ㄐㄧㄣ",    ["jia"] = "ㄐㄧㄚ",    ["jhu"] = "ㄓㄨ",
    ["jhe"] = "ㄓㄜ",      ["jha"] = "ㄓㄚ",      ["huo"] = "ㄏㄨㄛ",
    ["hun"] = "ㄏㄨㄣ",    ["hua"] = "ㄏㄨㄚ",    ["hou"] = "ㄏㄡ",
    ["hen"] = "ㄏㄣ",      ["hei"] = "ㄏㄟ",      ["hao"] = "ㄏㄠ",
    ["han"] = "ㄏㄢ",      ["hai"] = "ㄏㄞ",      ["guo"] = "ㄍㄨㄛ",
    ["gun"] = "ㄍㄨㄣ",    ["gue"] = "ㄍㄨㄜ",    ["gua"] = "ㄍㄨㄚ",
    ["gou"] = "ㄍㄡ",      ["gen"] = "ㄍㄣ",      ["gei"] = "ㄍㄟ",
    ["gao"] = "ㄍㄠ",      ["gan"] = "ㄍㄢ",      ["gai"] = "ㄍㄞ",
    ["fou"] = "ㄈㄡ",      ["fen"] = "ㄈㄣ",      ["fei"] = "ㄈㄟ",
    ["fan"] = "ㄈㄢ",      ["eng"] = "ㄥ",        ["duo"] = "ㄉㄨㄛ",
    ["dun"] = "ㄉㄨㄣ",    ["dou"] = "ㄉㄡ",      ["dia"] = "ㄉㄧㄚ",
    ["dei"] = "ㄉㄟ",      ["dao"] = "ㄉㄠ",      ["dan"] = "ㄉㄢ",
    ["dai"] = "ㄉㄞ",      ["chu"] = "ㄔㄨ",      ["che"] = "ㄔㄜ",
    ["cha"] = "ㄔㄚ",      ["bin"] = "ㄅㄧㄣ",    ["ben"] = "ㄅㄣ",
    ["bei"] = "ㄅㄟ",      ["bao"] = "ㄅㄠ",      ["ban"] = "ㄅㄢ",
    ["bai"] = "ㄅㄞ",      ["ang"] = "ㄤ",        ["yia"] = "ㄧㄚ",
    ["ye"] = "ㄧㄝ",       ["cu"] = "ㄘㄨ",       ["ci"] = "ㄑㄧ",
    ["ce"] = "ㄘㄜ",       ["ca"] = "ㄘㄚ",       ["zu"] = "ㄗㄨ",
    ["ze"] = "ㄗㄜ",       ["za"] = "ㄗㄚ",       ["yu"] = "ㄩ",
    ["yo"] = "ㄧㄛ",       ["yi"] = "ㄧ",         ["wu"] = "ㄨ",
    ["wo"] = "ㄨㄛ",       ["wa"] = "ㄨㄚ",       ["tu"] = "ㄊㄨ",
    ["ti"] = "ㄊㄧ",       ["te"] = "ㄊㄜ",       ["ta"] = "ㄊㄚ",
    ["su"] = "ㄙㄨ",       ["si"] = "ㄒㄧ",       ["se"] = "ㄙㄜ",
    ["sa"] = "ㄙㄚ",       ["ru"] = "ㄖㄨ",       ["re"] = "ㄖㄜ",
    ["pu"] = "ㄆㄨ",       ["po"] = "ㄆㄛ",       ["pi"] = "ㄆㄧ",
    ["pa"] = "ㄆㄚ",       ["ou"] = "ㄡ",         ["nu"] = "ㄋㄨ",
    ["ni"] = "ㄋㄧ",       ["ne"] = "ㄋㄜ",       ["na"] = "ㄋㄚ",
    ["mu"] = "ㄇㄨ",       ["mo"] = "ㄇㄛ",       ["mi"] = "ㄇㄧ",
    ["me"] = "ㄇㄜ",       ["ma"] = "ㄇㄚ",       ["lu"] = "ㄌㄨ",
    ["lo"] = "ㄌㄛ",       ["li"] = "ㄌㄧ",       ["le"] = "ㄌㄜ",
    ["la"] = "ㄌㄚ",       ["ku"] = "ㄎㄨ",       ["ke"] = "ㄎㄜ",
    ["ka"] = "ㄎㄚ",       ["ji"] = "ㄐㄧ",       ["hu"] = "ㄏㄨ",
    ["he"] = "ㄏㄜ",       ["ha"] = "ㄏㄚ",       ["gu"] = "ㄍㄨ",
    ["ge"] = "ㄍㄜ",       ["ga"] = "ㄍㄚ",       ["fu"] = "ㄈㄨ",
    ["fo"] = "ㄈㄛ",       ["fa"] = "ㄈㄚ",       ["er"] = "ㄦ",
    ["en"] = "ㄣ",         ["ei"] = "ㄟ",         ["eh"] = "ㄝ",
    ["du"] = "ㄉㄨ",       ["di"] = "ㄉㄧ",       ["de"] = "ㄉㄜ",
    ["da"] = "ㄉㄚ",       ["bu"] = "ㄅㄨ",       ["bo"] = "ㄅㄛ",
    ["bi"] = "ㄅㄧ",       ["ba"] = "ㄅㄚ",       ["ao"] = "ㄠ",
    ["an"] = "ㄢ",         ["ai"] = "ㄞ",         ["c"] = "ㄑ",
    ["o"] = "ㄛ",          ["e"] = "ㄜ",          ["a"] = "ㄚ"
  };

  /// <summary>
  /// 韋氏拼音排列專用處理陣列
  /// </summary>
  public readonly static Dictionary<string, string> MapWadeGilesPinyin = new() {
    ["a"] = "ㄚ",           ["ai"] = "ㄞ",         ["an"] = "ㄢ",
    ["ang"] = "ㄤ",         ["ao"] = "ㄠ",         ["cha"] = "ㄓㄚ",
    ["chai"] = "ㄓㄞ",      ["chan"] = "ㄓㄢ",     ["chang"] = "ㄓㄤ",
    ["chao"] = "ㄓㄠ",      ["che"] = "ㄓㄜ",      ["chei"] = "ㄓㄟ",
    ["chen"] = "ㄓㄣ",      ["cheng"] = "ㄓㄥ",    ["chi"] = "ㄐㄧ",
    ["chia"] = "ㄐㄧㄚ",    ["chiang"] = "ㄐㄧㄤ", ["chiao"] = "ㄐㄧㄠ",
    ["chieh"] = "ㄐㄧㄝ",   ["chien"] = "ㄐㄧㄢ",  ["chih"] = "ㄓ",
    ["chin"] = "ㄐㄧㄣ",    ["ching"] = "ㄐㄧㄥ",  ["chiu"] = "ㄐㄧㄡ",
    ["chiung"] = "ㄐㄩㄥ",  ["cho"] = "ㄓㄨㄛ",    ["chou"] = "ㄓㄡ",
    ["chu"] = "ㄓㄨ",       ["chua"] = "ㄓㄨㄚ",   ["chuai"] = "ㄓㄨㄞ",
    ["chuan"] = "ㄓㄨㄢ",   ["chuang"] = "ㄓㄨㄤ", ["chui"] = "ㄓㄨㄟ",
    ["chun"] = "ㄓㄨㄣ",    ["chung"] = "ㄓㄨㄥ",  ["ch'a"] = "ㄔㄚ",
    ["ch'ai"] = "ㄔㄞ",     ["ch'an"] = "ㄔㄢ",    ["ch'ang"] = "ㄔㄤ",
    ["ch'ao"] = "ㄔㄠ",     ["ch'e"] = "ㄔㄜ",     ["ch'en"] = "ㄔㄣ",
    ["ch'eng"] = "ㄔㄥ",    ["ch'i"] = "ㄑㄧ",     ["ch'ia"] = "ㄑㄧㄚ",
    ["ch'iang"] = "ㄑㄧㄤ", ["ch'iao"] = "ㄑㄧㄠ", ["ch'ieh"] = "ㄑㄧㄝ",
    ["ch'ien"] = "ㄑㄧㄢ",  ["ch'ih"] = "ㄔ",      ["ch'in"] = "ㄑㄧㄣ",
    ["ch'ing"] = "ㄑㄧㄥ",  ["ch'iu"] = "ㄑㄧㄡ",  ["ch'iung"] = "ㄑㄩㄥ",
    ["ch'o"] = "ㄔㄨㄛ",    ["ch'ou"] = "ㄔㄡ",    ["ch'u"] = "ㄔㄨ",
    ["ch'ua"] = "ㄔㄨㄚ",   ["ch'uai"] = "ㄔㄨㄞ", ["ch'uan"] = "ㄔㄨㄢ",
    ["ch'uang"] = "ㄔㄨㄤ", ["ch'ui"] = "ㄔㄨㄟ",  ["ch'un"] = "ㄔㄨㄣ",
    ["ch'ung"] = "ㄔㄨㄥ",  ["ch'v"] = "ㄑㄩ",     ["ch'van"] = "ㄑㄩㄢ",
    ["ch'veh"] = "ㄑㄩㄝ",  ["ch'vn"] = "ㄑㄩㄣ",  ["chv"] = "ㄐㄩ",
    ["chvan"] = "ㄐㄩㄢ",   ["chveh"] = "ㄐㄩㄝ",  ["chvn"] = "ㄐㄩㄣ",
    ["e"] = "ㄜ",           ["ei"] = "ㄟ",         ["en"] = "ㄣ",
    ["erh"] = "ㄦ",         ["fa"] = "ㄈㄚ",       ["fan"] = "ㄈㄢ",
    ["fang"] = "ㄈㄤ",      ["fei"] = "ㄈㄟ",      ["fen"] = "ㄈㄣ",
    ["feng"] = "ㄈㄥ",      ["fo"] = "ㄈㄛ",       ["fou"] = "ㄈㄡ",
    ["fu"] = "ㄈㄨ",        ["ha"] = "ㄏㄚ",       ["hai"] = "ㄏㄞ",
    ["han"] = "ㄏㄢ",       ["hang"] = "ㄏㄤ",     ["hao"] = "ㄏㄠ",
    ["hei"] = "ㄏㄟ",       ["hen"] = "ㄏㄣ",      ["heng"] = "ㄏㄥ",
    ["ho"] = "ㄏㄜ",        ["hou"] = "ㄏㄡ",      ["hsi"] = "ㄒㄧ",
    ["hsia"] = "ㄒㄧㄚ",    ["hsiang"] = "ㄒㄧㄤ", ["hsiao"] = "ㄒㄧㄠ",
    ["hsieh"] = "ㄒㄧㄝ",   ["hsien"] = "ㄒㄧㄢ",  ["hsin"] = "ㄒㄧㄣ",
    ["hsing"] = "ㄒㄧㄥ",   ["hsiu"] = "ㄒㄧㄡ",   ["hsiung"] = "ㄒㄩㄥ",
    ["hsv"] = "ㄒㄩ",       ["hsvan"] = "ㄒㄩㄢ",  ["hsveh"] = "ㄒㄩㄝ",
    ["hsvn"] = "ㄒㄩㄣ",    ["hu"] = "ㄏㄨ",       ["hua"] = "ㄏㄨㄚ",
    ["huai"] = "ㄏㄨㄞ",    ["huan"] = "ㄏㄨㄢ",   ["huang"] = "ㄏㄨㄤ",
    ["hui"] = "ㄏㄨㄟ",     ["hun"] = "ㄏㄨㄣ",    ["hung"] = "ㄏㄨㄥ",
    ["huo"] = "ㄏㄨㄛ",     ["i"] = "ㄧ",          ["jan"] = "ㄖㄢ",
    ["jang"] = "ㄖㄤ",      ["jao"] = "ㄖㄠ",      ["je"] = "ㄖㄜ",
    ["jen"] = "ㄖㄣ",       ["jeng"] = "ㄖㄥ",     ["jih"] = "ㄖ",
    ["jo"] = "ㄖㄨㄛ",      ["jou"] = "ㄖㄡ",      ["ju"] = "ㄖㄨ",
    ["juan"] = "ㄖㄨㄢ",    ["jui"] = "ㄖㄨㄟ",    ["jun"] = "ㄖㄨㄣ",
    ["jung"] = "ㄖㄨㄥ",    ["ka"] = "ㄍㄚ",       ["kai"] = "ㄍㄞ",
    ["kan"] = "ㄍㄢ",       ["kang"] = "ㄍㄤ",     ["kao"] = "ㄍㄠ",
    ["kei"] = "ㄍㄟ",       ["ken"] = "ㄍㄣ",      ["keng"] = "ㄍㄥ",
    ["ko"] = "ㄍㄜ",        ["kou"] = "ㄍㄡ",      ["ku"] = "ㄍㄨ",
    ["kua"] = "ㄍㄨㄚ",     ["kuai"] = "ㄍㄨㄞ",   ["kuan"] = "ㄍㄨㄢ",
    ["kuang"] = "ㄍㄨㄤ",   ["kuei"] = "ㄍㄨㄟ",   ["kun"] = "ㄍㄨㄣ",
    ["kung"] = "ㄍㄨㄥ",    ["kuo"] = "ㄍㄨㄛ",    ["k'a"] = "ㄎㄚ",
    ["k'ai"] = "ㄎㄞ",      ["k'an"] = "ㄎㄢ",     ["k'ang"] = "ㄎㄤ",
    ["k'ao"] = "ㄎㄠ",      ["k'en"] = "ㄎㄣ",     ["k'eng"] = "ㄎㄥ",
    ["k'o"] = "ㄎㄜ",       ["k'ou"] = "ㄎㄡ",     ["k'u"] = "ㄎㄨ",
    ["k'ua"] = "ㄎㄨㄚ",    ["k'uai"] = "ㄎㄨㄞ",  ["k'uan"] = "ㄎㄨㄢ",
    ["k'uang"] = "ㄎㄨㄤ",  ["k'uei"] = "ㄎㄨㄟ",  ["k'un"] = "ㄎㄨㄣ",
    ["k'ung"] = "ㄎㄨㄥ",   ["k'uo"] = "ㄎㄨㄛ",   ["la"] = "ㄌㄚ",
    ["lai"] = "ㄌㄞ",       ["lan"] = "ㄌㄢ",      ["lang"] = "ㄌㄤ",
    ["lao"] = "ㄌㄠ",       ["le"] = "ㄌㄜ",       ["lei"] = "ㄌㄟ",
    ["leng"] = "ㄌㄥ",      ["li"] = "ㄌㄧ",       ["lia"] = "ㄌㄧㄚ",
    ["liang"] = "ㄌㄧㄤ",   ["liao"] = "ㄌㄧㄠ",   ["lieh"] = "ㄌㄧㄝ",
    ["lien"] = "ㄌㄧㄢ",    ["lin"] = "ㄌㄧㄣ",    ["ling"] = "ㄌㄧㄥ",
    ["liu"] = "ㄌㄧㄡ",     ["lo"] = "ㄌㄨㄛ",     ["lou"] = "ㄌㄡ",
    ["lu"] = "ㄌㄨ",        ["luan"] = "ㄌㄨㄢ",   ["lun"] = "ㄌㄨㄣ",
    ["lung"] = "ㄌㄨㄥ",    ["lv"] = "ㄌㄩ",       ["lveh"] = "ㄌㄩㄝ",
    ["lvn"] = "ㄌㄩㄣ",     ["ma"] = "ㄇㄚ",       ["mai"] = "ㄇㄞ",
    ["man"] = "ㄇㄢ",       ["mang"] = "ㄇㄤ",     ["mao"] = "ㄇㄠ",
    ["me"] = "ㄇㄜ",        ["mei"] = "ㄇㄟ",      ["men"] = "ㄇㄣ",
    ["meng"] = "ㄇㄥ",      ["mi"] = "ㄇㄧ",       ["miao"] = "ㄇㄧㄠ",
    ["mieh"] = "ㄇㄧㄝ",    ["mien"] = "ㄇㄧㄢ",   ["min"] = "ㄇㄧㄣ",
    ["ming"] = "ㄇㄧㄥ",    ["miu"] = "ㄇㄧㄡ",    ["mo"] = "ㄇㄛ",
    ["mou"] = "ㄇㄡ",       ["mu"] = "ㄇㄨ",       ["na"] = "ㄋㄚ",
    ["nai"] = "ㄋㄞ",       ["nan"] = "ㄋㄢ",      ["nang"] = "ㄋㄤ",
    ["nao"] = "ㄋㄠ",       ["ne"] = "ㄋㄜ",       ["nei"] = "ㄋㄟ",
    ["nen"] = "ㄋㄣ",       ["neng"] = "ㄋㄥ",     ["ni"] = "ㄋㄧ",
    ["nia"] = "ㄋㄧㄚ",     ["niang"] = "ㄋㄧㄤ",  ["niao"] = "ㄋㄧㄠ",
    ["nieh"] = "ㄋㄧㄝ",    ["nien"] = "ㄋㄧㄢ",   ["nin"] = "ㄋㄧㄣ",
    ["ning"] = "ㄋㄧㄥ",    ["niu"] = "ㄋㄧㄡ",    ["no"] = "ㄋㄨㄛ",
    ["nou"] = "ㄋㄡ",       ["nu"] = "ㄋㄨ",       ["nuan"] = "ㄋㄨㄢ",
    ["nun"] = "ㄋㄨㄣ",     ["nung"] = "ㄋㄨㄥ",   ["nv"] = "ㄋㄩ",
    ["nveh"] = "ㄋㄩㄝ",    ["ou"] = "ㄡ",         ["pa"] = "ㄅㄚ",
    ["pai"] = "ㄅㄞ",       ["pan"] = "ㄅㄢ",      ["pang"] = "ㄅㄤ",
    ["pao"] = "ㄅㄠ",       ["pei"] = "ㄅㄟ",      ["pen"] = "ㄅㄣ",
    ["peng"] = "ㄅㄥ",      ["pi"] = "ㄅㄧ",       ["piao"] = "ㄅㄧㄠ",
    ["pieh"] = "ㄅㄧㄝ",    ["pien"] = "ㄅㄧㄢ",   ["pin"] = "ㄅㄧㄣ",
    ["ping"] = "ㄅㄧㄥ",    ["po"] = "ㄅㄛ",       ["pu"] = "ㄅㄨ",
    ["p'a"] = "ㄆㄚ",       ["p'ai"] = "ㄆㄞ",     ["p'an"] = "ㄆㄢ",
    ["p'ang"] = "ㄆㄤ",     ["p'ao"] = "ㄆㄠ",     ["p'ei"] = "ㄆㄟ",
    ["p'en"] = "ㄆㄣ",      ["p'eng"] = "ㄆㄥ",    ["p'i"] = "ㄆㄧ",
    ["p'iao"] = "ㄆㄧㄠ",   ["p'ieh"] = "ㄆㄧㄝ",  ["p'ien"] = "ㄆㄧㄢ",
    ["p'in"] = "ㄆㄧㄣ",    ["p'ing"] = "ㄆㄧㄥ",  ["p'o"] = "ㄆㄛ",
    ["p'ou"] = "ㄆㄡ",      ["p'u"] = "ㄆㄨ",      ["sa"] = "ㄙㄚ",
    ["sai"] = "ㄙㄞ",       ["san"] = "ㄙㄢ",      ["sang"] = "ㄙㄤ",
    ["sao"] = "ㄙㄠ",       ["se"] = "ㄙㄜ",       ["sei"] = "ㄙㄟ",
    ["sen"] = "ㄙㄣ",       ["seng"] = "ㄙㄥ",     ["sha"] = "ㄕㄚ",
    ["shai"] = "ㄕㄞ",      ["shan"] = "ㄕㄢ",     ["shang"] = "ㄕㄤ",
    ["shao"] = "ㄕㄠ",      ["she"] = "ㄕㄜ",      ["shei"] = "ㄕㄟ",
    ["shen"] = "ㄕㄣ",      ["sheng"] = "ㄕㄥ",    ["shih"] = "ㄕ",
    ["shou"] = "ㄕㄡ",      ["shu"] = "ㄕㄨ",      ["shua"] = "ㄕㄨㄚ",
    ["shuai"] = "ㄕㄨㄞ",   ["shuan"] = "ㄕㄨㄢ",  ["shuang"] = "ㄕㄨㄤ",
    ["shui"] = "ㄕㄨㄟ",    ["shun"] = "ㄕㄨㄣ",   ["shung"] = "ㄕㄨㄥ",
    ["shuo"] = "ㄕㄨㄛ",    ["so"] = "ㄙㄨㄛ",     ["sou"] = "ㄙㄡ",
    ["ssu"] = "ㄙ",         ["su"] = "ㄙㄨ",       ["suan"] = "ㄙㄨㄢ",
    ["sui"] = "ㄙㄨㄟ",     ["sun"] = "ㄙㄨㄣ",    ["sung"] = "ㄙㄨㄥ",
    ["ta"] = "ㄉㄚ",        ["tai"] = "ㄉㄞ",      ["tan"] = "ㄉㄢ",
    ["tang"] = "ㄉㄤ",      ["tao"] = "ㄉㄠ",      ["te"] = "ㄉㄜ",
    ["tei"] = "ㄉㄟ",       ["ten"] = "ㄉㄣ",      ["teng"] = "ㄉㄥ",
    ["ti"] = "ㄉㄧ",        ["tiang"] = "ㄉㄧㄤ",  ["tiao"] = "ㄉㄧㄠ",
    ["tieh"] = "ㄉㄧㄝ",    ["tien"] = "ㄉㄧㄢ",   ["ting"] = "ㄉㄧㄥ",
    ["tiu"] = "ㄉㄧㄡ",     ["to"] = "ㄉㄨㄛ",     ["tou"] = "ㄉㄡ",
    ["tsa"] = "ㄗㄚ",       ["tsai"] = "ㄗㄞ",     ["tsan"] = "ㄗㄢ",
    ["tsang"] = "ㄗㄤ",     ["tsao"] = "ㄗㄠ",     ["tse"] = "ㄗㄜ",
    ["tsei"] = "ㄗㄟ",      ["tsen"] = "ㄗㄣ",     ["tseng"] = "ㄗㄥ",
    ["tso"] = "ㄗㄨㄛ",     ["tsou"] = "ㄗㄡ",     ["tsu"] = "ㄗㄨ",
    ["tsuan"] = "ㄗㄨㄢ",   ["tsui"] = "ㄗㄨㄟ",   ["tsun"] = "ㄗㄨㄣ",
    ["tsung"] = "ㄗㄨㄥ",   ["ts'a"] = "ㄘㄚ",     ["ts'ai"] = "ㄘㄞ",
    ["ts'an"] = "ㄘㄢ",     ["ts'ang"] = "ㄘㄤ",   ["ts'ao"] = "ㄘㄠ",
    ["ts'e"] = "ㄘㄜ",      ["ts'en"] = "ㄘㄣ",    ["ts'eng"] = "ㄘㄥ",
    ["ts'o"] = "ㄘㄨㄛ",    ["ts'ou"] = "ㄘㄡ",    ["ts'u"] = "ㄘㄨ",
    ["ts'uan"] = "ㄘㄨㄢ",  ["ts'ui"] = "ㄘㄨㄟ",  ["ts'un"] = "ㄘㄨㄣ",
    ["ts'ung"] = "ㄘㄨㄥ",  ["tu"] = "ㄉㄨ",       ["tuan"] = "ㄉㄨㄢ",
    ["tui"] = "ㄉㄨㄟ",     ["tun"] = "ㄉㄨㄣ",    ["tung"] = "ㄉㄨㄥ",
    ["tzu"] = "ㄗ",         ["tz'u"] = "ㄘ",       ["t'a"] = "ㄊㄚ",
    ["t'ai"] = "ㄊㄞ",      ["t'an"] = "ㄊㄢ",     ["t'ang"] = "ㄊㄤ",
    ["t'ao"] = "ㄊㄠ",      ["t'e"] = "ㄊㄜ",      ["t'eng"] = "ㄊㄥ",
    ["t'i"] = "ㄊㄧ",       ["t'iao"] = "ㄊㄧㄠ",  ["t'ieh"] = "ㄊㄧㄝ",
    ["t'ien"] = "ㄊㄧㄢ",   ["t'ing"] = "ㄊㄧㄥ",  ["t'o"] = "ㄊㄨㄛ",
    ["t'ou"] = "ㄊㄡ",      ["t'u"] = "ㄊㄨ",      ["t'uan"] = "ㄊㄨㄢ",
    ["t'ui"] = "ㄊㄨㄟ",    ["t'un"] = "ㄊㄨㄣ",   ["t'ung"] = "ㄊㄨㄥ",
    ["wa"] = "ㄨㄚ",        ["wai"] = "ㄨㄞ",      ["wan"] = "ㄨㄢ",
    ["wang"] = "ㄨㄤ",      ["wei"] = "ㄨㄟ",      ["wen"] = "ㄨㄣ",
    ["weng"] = "ㄨㄥ",      ["wo"] = "ㄨㄛ",       ["wu"] = "ㄨ",
    ["ya"] = "ㄧㄚ",        ["yan"] = "ㄧㄢ",      ["yang"] = "ㄧㄤ",
    ["yao"] = "ㄧㄠ",       ["yeh"] = "ㄧㄝ",      ["yin"] = "ㄧㄣ",
    ["ying"] = "ㄧㄥ",      ["yu"] = "ㄧㄡ",       ["yung"] = "ㄩㄥ",
    ["yv"] = "ㄩ",          ["yvan"] = "ㄩㄢ",     ["yveh"] = "ㄩㄝ",
    ["yvn"] = "ㄩㄣ"
  };

  // MARK: - Maps for Keyboard-to-Phonabet parsers

  /// <summary>
  /// 標準大千排列專用處理陣列。<br /><br />
  /// 威注音輸入法 macOS 版使用了 Ukelele
  /// 佈局來完成對諸如倚天傳統等其它注音鍵盤排列的支援。
  /// 如果要將鐵恨模組拿給別的平台的輸入法使用的話，恐怕需要針對這些注音鍵盤排列各自新增專用陣列才可以。
  /// </summary>
  public readonly static Dictionary<string, string> MapQwertyDachen =
      new() { ["0"] = "ㄢ", ["1"] = "ㄅ", ["2"] = "ㄉ", ["3"] = "ˇ",
              ["4"] = "ˋ",  ["5"] = "ㄓ", ["6"] = "ˊ",  ["7"] = "˙",
              ["8"] = "ㄚ", ["9"] = "ㄞ", ["-"] = "ㄦ", [","] = "ㄝ",
              ["."] = "ㄡ", ["/"] = "ㄥ", [";"] = "ㄤ", ["a"] = "ㄇ",
              ["b"] = "ㄖ", ["c"] = "ㄏ", ["d"] = "ㄎ", ["e"] = "ㄍ",
              ["f"] = "ㄑ", ["g"] = "ㄕ", ["h"] = "ㄘ", ["i"] = "ㄛ",
              ["j"] = "ㄨ", ["k"] = "ㄜ", ["l"] = "ㄠ", ["m"] = "ㄩ",
              ["n"] = "ㄙ", ["o"] = "ㄟ", ["p"] = "ㄣ", ["q"] = "ㄆ",
              ["r"] = "ㄐ", ["s"] = "ㄋ", ["t"] = "ㄔ", ["u"] = "ㄧ",
              ["v"] = "ㄒ", ["w"] = "ㄊ", ["x"] = "ㄌ", ["y"] = "ㄗ",
              ["z"] = "ㄈ", [" "] = " " };

  /// <summary>
  /// 酷音大千二十六鍵排列專用處理陣列，但未包含全部的處理內容。<br /><br />
  /// 在這裡將二十六個字母寫全，也只是為了方便做 validity check。<br />
  /// 這裡提前對複音按鍵做處理，然後再用程式判斷介母類型、據此判斷是否需要做複音切換。
  /// </summary>
  public readonly static Dictionary<string, string> MapDachenCp26StaticKeys =
      new() { ["a"] = "ㄇ", ["b"] = "ㄖ", ["c"] = "ㄏ", ["d"] = "ㄎ",
              ["e"] = "ㄍ", ["f"] = "ㄑ", ["g"] = "ㄕ", ["h"] = "ㄘ",
              ["i"] = "ㄞ", ["j"] = "ㄨ", ["k"] = "ㄜ", ["l"] = "ㄤ",
              ["m"] = "ㄩ", ["n"] = "ㄙ", ["o"] = "ㄢ", ["p"] = "ㄦ",
              ["q"] = "ㄅ", ["r"] = "ㄐ", ["s"] = "ㄋ", ["t"] = "ㄓ",
              ["u"] = "ㄧ", ["v"] = "ㄒ", ["w"] = "ㄉ", ["x"] = "ㄌ",
              ["y"] = "ㄗ", ["z"] = "ㄈ", [" "] = " " };

  /// <summary>
  /// 許氏排列專用處理陣列，但未包含全部的映射內容。<br /><br />
  /// 在這裡將二十六個字母寫全，也只是為了方便做 validity check。<br />
  /// 這裡提前對複音按鍵做處理，然後再用程式判斷介母類型、據此判斷是否需要做複音切換。
  /// </summary>
  public readonly static Dictionary<string, string> MapHsuStaticKeys =
      new() { ["a"] = "ㄘ", ["b"] = "ㄅ", ["c"] = "ㄕ", ["d"] = "ㄉ",
              ["e"] = "ㄧ", ["f"] = "ㄈ", ["g"] = "ㄍ", ["h"] = "ㄏ",
              ["i"] = "ㄞ", ["j"] = "ㄐ", ["k"] = "ㄎ", ["l"] = "ㄌ",
              ["m"] = "ㄇ", ["n"] = "ㄋ", ["o"] = "ㄡ", ["p"] = "ㄆ",
              ["r"] = "ㄖ", ["s"] = "ㄙ", ["t"] = "ㄊ", ["u"] = "ㄩ",
              ["v"] = "ㄔ", ["w"] = "ㄠ", ["x"] = "ㄨ", ["y"] = "ㄚ",
              ["z"] = "ㄗ", [" "] = " " };

  /// <summary>
  /// 倚天忘形排列預處理專用陣列，但未包含全部的映射內容。<br /><br />
  /// 在這裡將二十六個字母寫全，也只是為了方便做 validity check。<br />
  /// 這裡提前對複音按鍵做處理，然後再用程式判斷介母類型、據此判斷是否需要做複音切換。
  /// </summary>
  public readonly static Dictionary<string, string> MapETen26StaticKeys =
      new() { ["a"] = "ㄚ", ["b"] = "ㄅ", ["c"] = "ㄕ", ["d"] = "ㄉ",
              ["e"] = "ㄧ", ["f"] = "ㄈ", ["g"] = "ㄓ", ["h"] = "ㄏ",
              ["i"] = "ㄞ", ["j"] = "ㄖ", ["k"] = "ㄎ", ["l"] = "ㄌ",
              ["m"] = "ㄇ", ["n"] = "ㄋ", ["o"] = "ㄛ", ["p"] = "ㄆ",
              ["q"] = "ㄗ", ["r"] = "ㄜ", ["s"] = "ㄙ", ["t"] = "ㄊ",
              ["u"] = "ㄩ", ["v"] = "ㄍ", ["w"] = "ㄘ", ["x"] = "ㄨ",
              ["y"] = "ㄔ", ["z"] = "ㄠ", [" "] = " " };

  /// <summary>
  /// 星光排列排列預處理專用陣列，但未包含全部的映射內容。<br /><br />
  /// 在這裡將二十六個字母寫全，也只是為了方便做 validity check。<br />
  /// 這裡提前對複音按鍵做處理，然後再用程式判斷介母類型、據此判斷是否需要做複音切換。
  /// </summary>
  public readonly static Dictionary<string, string> MapStarlightStaticKeys =
      new() { ["a"] = "ㄚ", ["b"] = "ㄅ", ["c"] = "ㄘ", ["d"] = "ㄉ",
              ["e"] = "ㄜ", ["f"] = "ㄈ", ["g"] = "ㄍ", ["h"] = "ㄏ",
              ["i"] = "ㄧ", ["j"] = "ㄓ", ["k"] = "ㄎ", ["l"] = "ㄌ",
              ["m"] = "ㄇ", ["n"] = "ㄋ", ["o"] = "ㄛ", ["p"] = "ㄆ",
              ["q"] = "ㄔ", ["r"] = "ㄖ", ["s"] = "ㄙ", ["t"] = "ㄊ",
              ["u"] = "ㄨ", ["v"] = "ㄩ", ["w"] = "ㄡ", ["x"] = "ㄕ",
              ["y"] = "ㄞ", ["z"] = "ㄗ", ["1"] = " ",  ["2"] = "ˊ",
              ["3"] = "ˇ",  ["4"] = "ˋ",  ["5"] = "˙",  ["6"] = " ",
              ["7"] = "ˊ",  ["8"] = "ˇ",  ["9"] = "ˋ",  ["0"] = "˙",
              [" "] = " " };

  /// <summary>
  /// 劉氏擬音注音排列預處理專用陣列，但未包含全部的映射內容。<br /><br />
  /// 在這裡將二十六個字母寫全，也只是為了方便做 validity check。<br />
  /// 這裡提前對複音按鍵做處理，然後再用程式判斷介母類型、據此判斷是否需要做複音切換。
  /// </summary>
  public readonly static Dictionary<string, string> MapAlvinLiuStaticKeys =
      new() { ["q"] = "ㄑ", ["w"] = "ㄠ", ["e"] = "ㄜ", ["r"] = "ㄖ",
              ["t"] = "ㄊ", ["y"] = "ㄩ", ["u"] = "ㄨ", ["i"] = "ㄧ",
              ["o"] = "ㄛ", ["p"] = "ㄆ", ["a"] = "ㄚ", ["s"] = "ㄙ",
              ["d"] = "ㄉ", ["f"] = "ㄈ", ["g"] = "ㄍ", ["h"] = "ㄏ",
              ["j"] = "ㄐ", ["k"] = "ㄎ", ["l"] = "ㄦ", ["z"] = "ㄗ",
              ["x"] = "ㄒ", ["c"] = "ㄘ", ["v"] = "ㄡ", ["b"] = "ㄅ",
              ["n"] = "ㄋ", ["m"] = "ㄇ", [" "] = " " };

  /// <summary>
  /// 倚天傳統排列專用處理陣列。
  /// </summary>
  public readonly static Dictionary<string, string> MapQwertyETenTraditional =
      new() { ["'"] = "ㄘ", [","] = "ㄓ", ["-"] = "ㄥ", ["."] = "ㄔ",
              ["/"] = "ㄕ", ["0"] = "ㄤ", ["1"] = "˙",  ["2"] = "ˊ",
              ["3"] = "ˇ",  ["4"] = "ˋ",  ["7"] = "ㄑ", ["8"] = "ㄢ",
              ["9"] = "ㄣ", [";"] = "ㄗ", ["="] = "ㄦ", ["a"] = "ㄚ",
              ["b"] = "ㄅ", ["c"] = "ㄒ", ["d"] = "ㄉ", ["e"] = "ㄧ",
              ["f"] = "ㄈ", ["g"] = "ㄐ", ["h"] = "ㄏ", ["i"] = "ㄞ",
              ["j"] = "ㄖ", ["k"] = "ㄎ", ["l"] = "ㄌ", ["m"] = "ㄇ",
              ["n"] = "ㄋ", ["o"] = "ㄛ", ["p"] = "ㄆ", ["q"] = "ㄟ",
              ["r"] = "ㄜ", ["s"] = "ㄙ", ["t"] = "ㄊ", ["u"] = "ㄩ",
              ["v"] = "ㄍ", ["w"] = "ㄝ", ["x"] = "ㄨ", ["y"] = "ㄡ",
              ["z"] = "ㄠ", [" "] = " " };

  /// <summary>
  /// IBM排列專用處理陣列。
  /// </summary>
  public readonly static Dictionary<string, string> MapQwertyIBM =
      new() { [","] = "ˇ",  ["-"] = "ㄏ", ["."] = "ˋ",  ["/"] = "˙",
              ["0"] = "ㄎ", ["1"] = "ㄅ", ["2"] = "ㄆ", ["3"] = "ㄇ",
              ["4"] = "ㄈ", ["5"] = "ㄉ", ["6"] = "ㄊ", ["7"] = "ㄋ",
              ["8"] = "ㄌ", ["9"] = "ㄍ", [";"] = "ㄠ", ["a"] = "ㄧ",
              ["b"] = "ㄥ", ["c"] = "ㄣ", ["d"] = "ㄩ", ["e"] = "ㄒ",
              ["f"] = "ㄚ", ["g"] = "ㄛ", ["h"] = "ㄜ", ["i"] = "ㄗ",
              ["j"] = "ㄝ", ["k"] = "ㄞ", ["l"] = "ㄟ", ["m"] = "ˊ",
              ["n"] = "ㄦ", ["o"] = "ㄘ", ["p"] = "ㄙ", ["q"] = "ㄐ",
              ["r"] = "ㄓ", ["s"] = "ㄨ", ["t"] = "ㄔ", ["u"] = "ㄖ",
              ["v"] = "ㄤ", ["w"] = "ㄑ", ["x"] = "ㄢ", ["y"] = "ㄕ",
              ["z"] = "ㄡ", [" "] = " " };

  /// <summary>
  /// 精業排列專用處理陣列。
  /// </summary>
  public readonly static Dictionary<string, string> MapSeigyou =
      new() { ["a"] = "ˇ",  ["b"] = "ㄒ", ["c"] = "ㄌ", ["d"] = "ㄋ",
              ["e"] = "ㄊ", ["f"] = "ㄎ", ["g"] = "ㄑ", ["h"] = "ㄕ",
              ["i"] = "ㄛ", ["j"] = "ㄘ", ["k"] = "ㄜ", ["l"] = "ㄠ",
              ["m"] = "ㄙ", ["n"] = "ㄖ", ["o"] = "ㄟ", ["p"] = "ㄣ",
              ["q"] = "ˊ",  ["r"] = "ㄍ", ["s"] = "ㄇ", ["t"] = "ㄐ",
              ["u"] = "ㄗ", ["v"] = "ㄏ", ["w"] = "ㄆ", ["x"] = "ㄈ",
              ["y"] = "ㄔ", ["z"] = "ˋ",  ["1"] = "˙",  ["2"] = "ㄅ",
              ["3"] = "ㄉ", ["6"] = "ㄓ", ["8"] = "ㄚ", ["9"] = "ㄞ",
              ["0"] = "ㄢ", ["-"] = "ㄧ", [";"] = "ㄤ", [","] = "ㄝ",
              ["."] = "ㄡ", ["/"] = "ㄥ", ["'"] = "ㄩ", ["["] = "ㄨ",
              ["="] = "ㄦ", [" "] = " " };

  /// <summary>
  /// 偽精業排列專用處理陣列。
  /// </summary>
  public readonly static Dictionary<string, string> MapFakeSeigyou =
      new() { ["a"] = "ˇ",  ["b"] = "ㄒ", ["c"] = "ㄌ", ["d"] = "ㄋ",
              ["e"] = "ㄊ", ["f"] = "ㄎ", ["g"] = "ㄑ", ["h"] = "ㄕ",
              ["i"] = "ㄛ", ["j"] = "ㄘ", ["k"] = "ㄜ", ["l"] = "ㄠ",
              ["m"] = "ㄙ", ["n"] = "ㄖ", ["o"] = "ㄟ", ["p"] = "ㄣ",
              ["q"] = "ˊ",  ["r"] = "ㄍ", ["s"] = "ㄇ", ["t"] = "ㄐ",
              ["u"] = "ㄗ", ["v"] = "ㄏ", ["w"] = "ㄆ", ["x"] = "ㄈ",
              ["y"] = "ㄔ", ["z"] = "ˋ",  ["1"] = "˙",  ["2"] = "ㄅ",
              ["3"] = "ㄉ", ["6"] = "ㄓ", ["8"] = "ㄚ", ["9"] = "ㄞ",
              ["0"] = "ㄢ", ["4"] = "ㄧ", [";"] = "ㄤ", [","] = "ㄝ",
              ["."] = "ㄡ", ["/"] = "ㄥ", ["7"] = "ㄩ", ["5"] = "ㄨ",
              ["-"] = "ㄦ", [" "] = " " };

  /// <summary>
  /// 神通排列專用處理陣列。
  /// </summary>
  public readonly static Dictionary<string, string> MapQwertyMiTAC =
      new() { [","] = "ㄓ", ["-"] = "ㄦ", ["."] = "ㄔ", ["/"] = "ㄕ",
              ["0"] = "ㄥ", ["1"] = "˙",  ["2"] = "ˊ",  ["3"] = "ˇ",
              ["4"] = "ˋ",  ["5"] = "ㄞ", ["6"] = "ㄠ", ["7"] = "ㄢ",
              ["8"] = "ㄣ", ["9"] = "ㄤ", [";"] = "ㄝ", ["a"] = "ㄚ",
              ["b"] = "ㄅ", ["c"] = "ㄘ", ["d"] = "ㄉ", ["e"] = "ㄜ",
              ["f"] = "ㄈ", ["g"] = "ㄍ", ["h"] = "ㄏ", ["i"] = "ㄟ",
              ["j"] = "ㄐ", ["k"] = "ㄎ", ["l"] = "ㄌ", ["m"] = "ㄇ",
              ["n"] = "ㄋ", ["o"] = "ㄛ", ["p"] = "ㄆ", ["q"] = "ㄑ",
              ["r"] = "ㄖ", ["s"] = "ㄙ", ["t"] = "ㄊ", ["u"] = "ㄡ",
              ["v"] = "ㄩ", ["w"] = "ㄨ", ["x"] = "ㄒ", ["y"] = "ㄧ",
              ["z"] = "ㄗ", [" "] = " " };

  /// <summary>
  /// 用以判定「是否是拼音鍵盤佈局」的集合。
  /// </summary>
  public readonly static MandarinParser[] ArrPinyinParsers = {
    MandarinParser.OfHanyuPinyin,     MandarinParser.OfSecondaryPinyin,
    MandarinParser.OfYalePinyin,      MandarinParser.OfHualuoPinyin,
    MandarinParser.OfUniversalPinyin, MandarinParser.OfWadeGilesPinyin
  };
}
internal static class StringExtensions {
  /// <summary>
  /// 檢查某個字串是否包含其他的合規字串。
  /// </summary>
  /// <param name="self">該字串自身。</param>
  /// <param name="value">檢查是否要被包含的字串。</param>
  /// <returns>如果要檢查的被包含的物件是 null 或空字串的話，這個函式不會扔
  /// Exception，而是會返回說「主體是不是空的或是不是 null」。</returns>
  public static bool DoesHave(this string self, string value) {
    return (string.IsNullOrEmpty(value)) ? string.IsNullOrEmpty(self)
                                         : self.Contains(value);
  }
}
}
