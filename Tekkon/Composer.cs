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
/// <summary>
/// 注音並擊處理的對外介面以注拼槽（Syllable Composer）的形式存在。<br />
/// 使用時需要單獨初期化為一個副本變數（因為是 Struct 所以必須得是變數）。<br />
/// 注拼槽只有四格：聲、介、韻、調。<br /><br />
/// 初期化時可以藉由 @input 參數指定初期已經傳入的按鍵訊號，<br />
/// 還可以在初期化時藉由
/// @arrange參數來指定注音排列（預設為「.ofDachen」大千佈局）。
/// </summary>
public struct Composer {
  /// 聲介韻調。
  public Phonabet Consonant = new(), Semivowel = new(), Vowel = new(),
                  Intonation = new();

  /// <summary>
  /// 為拉丁字母專用的組音區。
  /// </summary>
  public string RomajiBuffer = "";

  /// <summary>
  /// 注音排列種類。預設情況下是大千排列（Windows / macOS 預設注音排列）。
  /// </summary>
  public MandarinParser Parser = MandarinParser.OfDachen;

  /// <summary>
  /// 內容值，會直接按照正確的順序拼裝自己的聲介韻調內容、再回傳。
  /// 注意：直接取這個參數的內容的話，陰平聲調會成為一個空格。
  /// 如果是要取不帶空格的注音的話，請使用「.getComposition()」而非「.Value」。
  /// </summary>
  public string Value => $"{Consonant}{Semivowel}{Vowel}{Intonation}";

  /// <summary>
  /// 與 value 類似，這個函式就是用來決定輸入法組字區內顯示的注音/拼音內容，
  /// 但可以指定是否輸出教科書格式（拼音的調號在字母上方、注音的輕聲寫在左側）。
  /// </summary>
  /// <param name="IsHanyuPinyin">是否將輸出結果轉成漢語拼音。</param>
  /// <param
  /// name="IsTextBookStyle">是否將輸出的注音/拼音結果轉成教科書排版格式。</param>
  /// <returns>拼音/注音讀音字串，依照指定的格式。</returns>
  public string GetComposition(bool IsHanyuPinyin = false,
                               bool IsTextBookStyle = false) {
    switch (IsHanyuPinyin) {
      case false:  // 注音輸出的場合
        string ValReturnZhuyin = Value.Replace(" ", "");
        if (IsTextBookStyle && ValReturnZhuyin.Contains("˙")) {
          ValReturnZhuyin = ValReturnZhuyin.Remove(ValReturnZhuyin.Length - 1);
          ValReturnZhuyin = "˙" + ValReturnZhuyin;
        }
        return ValReturnZhuyin;
      case true:  // 拼音輸出的場合
        string ValReturnPinyin = Shared.CnvPhonaToHanyuPinyin(Value);
        if (IsTextBookStyle)
          ValReturnPinyin =
              Shared.CnvHanyuPinyinToTextbookStyle(ValReturnPinyin);
        return ValReturnPinyin;
    }
  }

  /// <summary>
  /// 該函式僅用來獲取給 macOS InputMethod Kit 的內文組字區使用的顯示字串。
  /// </summary>
  /// <param name="IsHanyuPinyin">是否將輸出結果轉成漢語拼音。</param>
  /// <returns>拼音/注音讀音字串，依照適合在輸入法組字區內顯示出來的格式。</returns>
  public string GetInlineCompositionForIMK(bool IsHanyuPinyin = false) {
    switch (Parser) {
      case MandarinParser.OfHanyuPinyin:
      case MandarinParser.OfSecondaryPinyin:
      case MandarinParser.OfYalePinyin:
      case MandarinParser.OfHualuoPinyin:
      case MandarinParser.OfUniversalPinyin:
        var ToneReturned = "";
        ToneReturned =
            Intonation.Value switch { " " => "1", "ˊ" => "2", "ˇ" => "3",
                                      "ˋ" => "4", "˙" => "5",
                                      _ => "" };
        return RomajiBuffer + ToneReturned;
      default:
        return GetComposition(IsHanyuPinyin);
    }
  }

  /// <summary>
  /// 注拼槽內容是否為空。
  /// </summary>
  public bool IsEmpty {
    get {
      switch (Parser) {
        case MandarinParser.OfHanyuPinyin:
        case MandarinParser.OfSecondaryPinyin:
        case MandarinParser.OfYalePinyin:
        case MandarinParser.OfHualuoPinyin:
        case MandarinParser.OfUniversalPinyin:
          return (Intonation.IsEmpty) && (RomajiBuffer == "");
        default:
          return (Consonant.Value == "") && (Semivowel.Value == "") &&
                 (Vowel.Value == "") && (Intonation.Value == "");
      }
    }
  }

  /// <summary>
  /// 注拼槽內容是否可唸。
  /// </summary>
  public bool IsPronouncable =>
      !Vowel.IsEmpty || !Semivowel.IsEmpty || !Consonant.IsEmpty;

  // MARK: 注拼槽對外處理函式.

  /// <summary>
  /// 初期化一個新的注拼槽。可以藉由 @input 參數指定初期已經傳入的按鍵訊號。
  /// 還可以在初期化時藉由 @arrange
  /// 參數來指定注音排列（預設為「.ofDachen」大千佈局）。
  /// </summary>
  /// <param name="Input">傳入的 String 內容，用以處理單個字符。</param>
  /// <param name="Arrange">要使用的注音排列。</param>
  public Composer(string Input = "", MandarinParser Arrange = 0) {
    EnsureParser(Arrange);
    ReceiveKey(Input);
  }

  /// <summary>
  /// 清除自身的內容，就是將聲介韻調全部清空。
  /// 嚴格而言，「注音排列」這個屬性沒有需要清空的概念，只能用 ensureParser
  /// 參數變更之。
  /// </summary>
  public void Clear() {
    Consonant = new();
    Semivowel = new();
    Vowel = new();
    Intonation = new();
    RomajiBuffer = "";
  }

  // MARK: - Public Functions

  /// <summary>
  /// 用於檢測「某個輸入字符訊號的合規性」的函式。<br />
  /// <br />
  /// 注意：回傳結果會受到當前注音排列 parser 屬性的影響。
  /// </summary>
  /// <param name="InputCharCode">傳入的 UniChar 內容。</param>
  /// <returns>傳入的字符是否合規。</returns>
  public bool InputValidityCheck(int InputCharCode) {
    char InputKey = (char)Math.Abs(InputCharCode);
    if (InputKey >= 128) return false;
    switch (Parser) {
      case MandarinParser.OfDachen:
        return Shared.MapQwertyDachen.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfDachen26:
        return Shared.MapDachenCP26StaticKeys.Keys.Contains(
            InputKey.ToString());
      case MandarinParser.OfETen:
        return Shared.MapQwertyETenTraditional.Keys.Contains(
            InputKey.ToString());
      case MandarinParser.OfHsu:
        return Shared.MapHsuStaticKeys.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfETen26:
        return Shared.MapETen26StaticKeys.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfIBM:
        return Shared.MapQwertyIBM.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfMiTAC:
        return Shared.MapQwertyMiTAC.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfSeigyou:
        return Shared.MapSeigyou.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfFakeSeigyou:
        return Shared.MapFakeSeigyou.Keys.Contains(InputKey.ToString());
      case MandarinParser.OfHanyuPinyin:
      case MandarinParser.OfSecondaryPinyin:
      case MandarinParser.OfYalePinyin:
      case MandarinParser.OfHualuoPinyin:
      case MandarinParser.OfUniversalPinyin:
        return Shared.MapArayuruPinyin.Contains(InputKey);
    }
    return false;
  }

  /// <summary>
  /// 接受傳入的按鍵訊號時的處理，處理對象為 String。<br />
  /// 另有同名函式可處理 UniChar 訊號。<br />
  /// <br />
  /// 如果是諸如複合型注音排列的話，翻譯結果有可能為空，但翻譯過程已經處理好聲介韻調分配了。
  /// </summary>
  /// <param name="Input">傳入的 String 內容。</param>
  public void ReceiveKey(string Input) {
    switch (Parser) {
      case MandarinParser.OfHanyuPinyin:
      case MandarinParser.OfSecondaryPinyin:
      case MandarinParser.OfYalePinyin:
      case MandarinParser.OfHualuoPinyin:
      case MandarinParser.OfUniversalPinyin:
        if (Shared.MapArayuruPinyinIntonation.Keys.Contains(Input)) {
          string TheTone = Shared.MapArayuruPinyinIntonation[Input];
          Intonation = new(TheTone);
        } else {
          // 為了防止 RomajiBuffer 越敲越長帶來算力負擔，
          // 這裡讓它在要溢出時自動丟掉最早輸入的音頭。
          if (RomajiBuffer.Length > 5) {
            RomajiBuffer = RomajiBuffer.Skip(1).ToString();
          }
          string RomajiBufferBackup = RomajiBuffer + Input;
          ReceiveSequence(RomajiBufferBackup, true);
          RomajiBuffer = RomajiBufferBackup;
        }
        break;
      default:
        ReceiveKeyFromPhonabet(Translate(Input));
        break;
    }
  }

  /// <summary>
  /// 接受傳入的按鍵訊號時的處理，處理對象為 UniChar。<br />
  /// 其實也就是先將 char(int) 轉為 String
  /// 再交給某個同名異參的函式來處理而已。<br /> <br />
  /// 如果是諸如複合型注音排列的話，翻譯結果有可能為空，但翻譯過程已經處理好聲介韻調分配了。
  /// </summary>
  /// <param name="InputChar">傳入的 char 內容，格式為 int。</param>
  public void ReceiveKey(int InputChar) =>
      ReceiveKey(((char)Math.Abs(InputChar)).ToString());

  /// <summary>
  /// 接受傳入的按鍵訊號時的處理，處理對象為單個注音符號。
  /// 主要就是將注音符號拆分辨識且分配到正確的貯存位置而已。
  /// </summary>
  /// <param name="Phonabet">傳入的單個注音符號字串。</param>
  public void ReceiveKeyFromPhonabet(string Phonabet = "") {
    Phonabet ThePhone = new(Phonabet);
    switch (ThePhone.Type) {
      case PhoneType.Consonant:
        Consonant = ThePhone;
        break;
      case PhoneType.Semivowel:
        Semivowel = ThePhone;
        break;
      case PhoneType.Vowel:
        Vowel = ThePhone;
        break;
      case PhoneType.Intonation:
        Intonation = ThePhone;
        break;
      default:
        break;
    }
  }

  /// <summary>
  /// 處理一連串的按鍵輸入。
  /// </summary>
  /// <param name="GivenSequence">傳入的 String
  /// 內容，用以處理一整串擊鍵輸入。</param>
  /// <param
  /// name="IsRomaji">如果輸入的字串是諸如漢語拼音這樣的西文字母拼音的話，請啟用此選項。</param>
  public void ReceiveSequence(string GivenSequence = "",
                              bool IsRomaji = false) {
    Clear();
    if (!IsRomaji) {
      foreach (char Key in GivenSequence) {
        ReceiveKey(Key);
      }
    } else {
      switch (Parser) {
        case MandarinParser.OfHanyuPinyin:
          string DictResult1 = "";
          if (Shared.MapHanyuPinyin.Keys.Contains(GivenSequence))
            DictResult1 = Shared.MapHanyuPinyin[GivenSequence];
          if (DictResult1 != "") {
            foreach (char Phonabet in DictResult1) {
              ReceiveKeyFromPhonabet(Phonabet.ToString());
            }
          }
          break;
        case MandarinParser.OfSecondaryPinyin:
          string DictResult2 = "";
          if (Shared.MapSecondaryPinyin.Keys.Contains(GivenSequence))
            DictResult2 = Shared.MapSecondaryPinyin[GivenSequence];
          if (DictResult2 != "") {
            foreach (char Phonabet in DictResult2) {
              ReceiveKeyFromPhonabet(Phonabet.ToString());
            }
          }
          break;
        case MandarinParser.OfYalePinyin:
          string DictResult3 = "";
          if (Shared.MapYalePinyin.Keys.Contains(GivenSequence))
            DictResult3 = Shared.MapYalePinyin[GivenSequence];
          if (DictResult3 != "") {
            foreach (char Phonabet in DictResult3) {
              ReceiveKeyFromPhonabet(Phonabet.ToString());
            }
          }
          break;
        case MandarinParser.OfHualuoPinyin:
          string DictResult4 = "";
          if (Shared.MapHualuoPinyin.Keys.Contains(GivenSequence))
            DictResult4 = Shared.MapHualuoPinyin[GivenSequence];
          if (DictResult4 != "") {
            foreach (char Phonabet in DictResult4) {
              ReceiveKeyFromPhonabet(Phonabet.ToString());
            }
          }
          break;
        case MandarinParser.OfUniversalPinyin:
          string DictResult5 = "";
          if (Shared.MapUniversalPinyin.Keys.Contains(GivenSequence))
            DictResult5 = Shared.MapUniversalPinyin[GivenSequence];
          if (DictResult5 != "") {
            foreach (char Phonabet in DictResult5) {
              ReceiveKeyFromPhonabet(Phonabet.ToString());
            }
          }
          break;
        default:
          break;
      }
    }
  }

  /// <summary>
  /// 處理一連串的按鍵輸入、且返回被處理之後的注音（陰平為空格）。
  /// </summary>
  /// <param name="GivenSequence">傳入的 String
  /// 內容，用以處理一整串擊鍵輸入。</param>
  /// <returns>在處理該輸入順序後，注拼槽根據目前狀態生成的拼音/注音字串。</returns>
  public string CnvSequence(string GivenSequence = "") {
    ReceiveSequence(GivenSequence);
    return Value;
  }

  /// <summary>
  /// 專門用來響應使用者摁下 BackSpace 按鍵時的行為。<br />
  /// 刪除順序：調、韻、介、聲。<br />
  /// <br />
  /// 基本上就是按順序從游標前方開始往後刪。
  /// </summary>
  public void DoBackSpace() {
    if (Shared.ArrPinyinParsers.Contains(Parser) && RomajiBuffer.Length != 0) {
      if (!Intonation.IsEmpty) {
        Intonation.Clear();
      } else {
        RomajiBuffer = RomajiBuffer.SkipLast(1).ToString();
      }
    } else if (!Intonation.IsEmpty) {
      Intonation.Clear();
    } else if (!Vowel.IsEmpty) {
      Vowel.Clear();
    } else if (!Semivowel.IsEmpty) {
      Semivowel.Clear();
    } else if (!Consonant.IsEmpty) {
      Consonant.Clear();
    }
  }

  /// <summary>
  /// 用來檢測是否有調號的函式，預設情況下不判定聲調以外的內容的存無。
  /// </summary>
  /// <param name="WithNothingElse">追加判定「槽內是否僅有調號」。</param>
  /// <returns>有則真，無則假。</returns>
  public bool HasToneMarker(bool WithNothingElse = false) =>
      WithNothingElse ? (!Intonation.IsEmpty && Vowel.IsEmpty &&
                         Semivowel.IsEmpty && Consonant.IsEmpty)
                      : !Intonation.IsEmpty;

  /// <summary>
  /// 設定該 Composer 處於何種鍵盤排列分析模式。
  /// </summary>
  /// <param name="Arrange">給該注拼槽指定注音排列。</param>
  public void EnsureParser(MandarinParser Arrange = 0) { Parser = Arrange; }

  // MARK: - Parser Processings

  // 注拼槽對內處理用函式都在這一小節。

  /// <summary>
  /// 根據目前的注音排列設定來翻譯傳入的 String 訊號。<br />
  /// <br />
  /// 倚天/許氏鍵盤/酷音大千二十六鍵的處理函式會代為處理分配過程，此時回傳結果可能為空字串。
  /// </summary>
  /// <param name="Key">傳入的 String 訊號。</param>
  /// <returns></returns>
  string Translate(string Key) {
    switch (Parser) {
      case MandarinParser.OfDachen:
        return (Shared.MapQwertyDachen.Keys.Contains(Key))
                   ? Shared.MapQwertyDachen[Key]
                   : "";
      case MandarinParser.OfDachen26:
        return HandleDachen26(Key);
      case MandarinParser.OfETen:
        return (Shared.MapQwertyETenTraditional.Keys.Contains(Key))
                   ? Shared.MapQwertyETenTraditional[Key]
                   : "";
      case MandarinParser.OfHsu:
        return HandleHsu(Key);
      case MandarinParser.OfETen26:
        return HandleETen26(Key);
      case MandarinParser.OfIBM:
        return (Shared.MapQwertyIBM.Keys.Contains(Key))
                   ? Shared.MapQwertyIBM[Key]
                   : "";
      case MandarinParser.OfMiTAC:
        return (Shared.MapQwertyMiTAC.Keys.Contains(Key))
                   ? Shared.MapQwertyMiTAC[Key]
                   : "";
      case MandarinParser.OfSeigyou:
        return (Shared.MapSeigyou.Keys.Contains(Key)) ? Shared.MapSeigyou[Key]
                                                      : "";
      case MandarinParser.OfFakeSeigyou:
        return (Shared.MapFakeSeigyou.Keys.Contains(Key))
                   ? Shared.MapFakeSeigyou[Key]
                   : "";
      case MandarinParser.OfHanyuPinyin:
      case MandarinParser.OfSecondaryPinyin:
      case MandarinParser.OfYalePinyin:
      case MandarinParser.OfHualuoPinyin:
      case MandarinParser.OfUniversalPinyin:
        break;
    }
    return "";
  }

  /// <summary>
  /// 倚天忘形注音排列比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="Key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  string HandleETen26(string Key = "") {
    string StrReturn = (Shared.MapETen26StaticKeys.Keys.Contains(Key))
                           ? Shared.MapETen26StaticKeys[Key]
                           : "";
    Phonabet IncomingPhonabet = new(StrReturn);

    switch (Key) {
      case "d":
        if (!IsPronouncable)
          Consonant = new("ㄉ");
        else
          Intonation = new("˙");
        break;
      case "f":
        if (!IsPronouncable)
          Consonant = new("ㄈ");
        else
          Intonation = new("ˊ");
        break;
      case "j":
        if (!IsPronouncable)
          Consonant = new("ㄖ");
        else
          Intonation = new("ˇ");
        break;
      case "k":
        if (!IsPronouncable)
          Consonant = new("ㄎ");
        else
          Intonation = new("ˋ");

        break;
      case "h":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄏ");
        else
          Vowel = new("ㄦ");
        break;
      case "l":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄌ");
        else
          Vowel = new("ㄥ");
        break;
      case "m":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄇ");
        } else {
          Vowel = new("ㄢ");
        }
        break;
      case "n":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄋ");
        else
          Vowel = new("ㄣ");
        break;
      case "q":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄗ");
        else
          Vowel = new("ㄟ");
        break;
      case "t":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄊ");
        else
          Vowel = new("ㄤ");
        break;
      case "w":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄘ");
        else
          Vowel = new("ㄝ");
        break;
      case "p":
        if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄆ");
        else if (Consonant.IsEmpty && Semivowel.Value == "ㄧ")
          Vowel = new("ㄡ");
        else if (Consonant.IsEmpty)
          Vowel = new("ㄆ");
        else
          Vowel = new("ㄡ");
        break;
      default:
        break;
    };

    // 處理「一個按鍵對應兩個聲母」的情形。
    if (!Consonant.IsEmpty && IncomingPhonabet.Type == PhoneType.Semivowel) {
      switch (Consonant.Value) {
        case "ㄍ":
          switch (IncomingPhonabet.Value) {
            case "ㄧ":
              Consonant = new("ㄑ");
              break;  // ㄑㄧ
            case "ㄨ":
              Consonant = new("ㄍ");
              break;  // ㄍㄨ
            case "ㄩ":
              Consonant = new("ㄑ");
              break;  // ㄑㄩ
            default:
              break;
          };
          break;
        case "ㄓ":
          switch (IncomingPhonabet.Value) {
            case "ㄧ":
              Consonant = new("ㄐ");
              break;  // ㄐㄧ
            case "ㄨ":
              Consonant = new("ㄓ");
              break;  // ㄓㄨ
            case "ㄩ":
              Consonant = new("ㄐ");
              break;  // ㄐㄩ
            default:
              break;
          };
          break;
        case "ㄕ":
          switch (IncomingPhonabet.Value) {
            case "ㄧ":
              Consonant = new("ㄒ");
              break;  // ㄒㄧ
            case "ㄨ":
              Consonant = new("ㄕ");
              break;  // ㄕㄨ
            case "ㄩ":
              Consonant = new("ㄒ");
              break;  // ㄒㄩ
            default:
              break;
          };
          break;
        default:
          break;
      };
    };

    if ("dfjk ".Contains(Key) && !Consonant.IsEmpty && Semivowel.IsEmpty &&
        Vowel.IsEmpty) {
      Consonant.SelfReplace("ㄆ", "ㄡ");
      Consonant.SelfReplace("ㄇ", "ㄢ");
      Consonant.SelfReplace("ㄊ", "ㄤ");
      Consonant.SelfReplace("ㄋ", "ㄣ");
      Consonant.SelfReplace("ㄌ", "ㄥ");
      Consonant.SelfReplace("ㄏ", "ㄦ");
    };

    // 後置修正
    if (Value == "ㄍ˙") Consonant = new("ㄑ");

    // 這些按鍵在上文處理過了，就不要再回傳了。
    if ("dfhjklmnpqtw".Contains(Key)) StrReturn = "";

    // 回傳結果是空字串的話，不要緊，因為上文已經代處理過分配過程了。
    return StrReturn;
  }

  /// <summary>
  /// 許氏鍵盤與倚天忘形一樣同樣也比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="Key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  string HandleHsu(string Key = "") {
    string StrReturn = (Shared.MapHsuStaticKeys.Keys.Contains(Key))
                           ? Shared.MapHsuStaticKeys[Key]
                           : "";
    Phonabet IncomingPhonabet = new(StrReturn);

    if (Key == " " && Value == "ㄋ") {
      Consonant.Clear();
      Vowel = new("ㄣ");
    };

    switch (Key) {
      case "d":
        if (IsPronouncable) {
          Intonation = new("ˊ");
        } else {
          Consonant = new("ㄉ");
        }
        break;
      case "f":
        if (IsPronouncable) {
          Intonation = new("ˇ");
        } else {
          Consonant = new("ㄈ");
        }
        break;
      case "s":
        if (IsPronouncable) {
          Intonation = new("˙");
        } else {
          Consonant = new("ㄙ");
        }
        break;
      case "j":
        if (IsPronouncable) {
          Intonation = new("ˋ");
        } else {
          Consonant = new("ㄓ");
        }
        break;
      case "a":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄘ");
        } else {
          Vowel = new("ㄟ");
        }
        break;
      case "v":
        if (Semivowel.IsEmpty) {
          Consonant = new("ㄔ");
        } else {
          Consonant = new("ㄑ");
        }
        break;
      case "c":
        if (Semivowel.IsEmpty) {
          Consonant = new("ㄕ");
        } else {
          Consonant = new("ㄒ");
        }
        break;
      case "e":
        if (Semivowel.IsEmpty) {
          Semivowel = new("ㄧ");
        } else {
          Vowel = new("ㄝ");
        }
        break;
      case "g":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄍ");
        } else {
          Vowel = new("ㄜ");
        }
        break;
      case "h":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄏ");
        } else {
          Vowel = new("ㄛ");
        }
        break;
      case "k":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄎ");
        } else {
          Vowel = new("ㄤ");
        }
        break;
      case "m":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄇ");
        } else {
          Vowel = new("ㄢ");
        }
        break;
      case "n":
        if (Consonant.IsEmpty && Semivowel.IsEmpty) {
          Consonant = new("ㄋ");
        } else {
          Vowel = new("ㄣ");
        }
        break;
      case "l":
        if (Value == "" && !Consonant.IsEmpty && !Semivowel.IsEmpty)
          Vowel = new("ㄦ");
        else if (Consonant.IsEmpty && Semivowel.IsEmpty)
          Consonant = new("ㄌ");
        else
          Vowel = new("ㄥ");
        break;
      default:
        break;
    };

    // 處理特殊情形。
    switch (IncomingPhonabet.Type) {
      case PhoneType.Semivowel:
        switch (Consonant.Value) {
          case "ㄍ":  // 許氏鍵盤應該也需要這個自動糾正
            switch (IncomingPhonabet.Value) {
              case "ㄧ":
                Consonant = new("ㄑ");
                break;  // ㄑㄧ
              case "ㄨ":
                Consonant = new("ㄍ");
                break;  // ㄍㄨ
              case "ㄩ":
                Consonant = new("ㄑ");
                break;  // ㄑㄩ
              default:
                break;
            };
            break;
          case "ㄓ":
            if (Intonation.IsEmpty) {
              switch (IncomingPhonabet.Value) {
                case "ㄧ":
                  Consonant = new("ㄐ");
                  break;  // ㄐㄧ
                case "ㄨ":
                  Consonant = new("ㄓ");
                  break;  // ㄓㄨ
                case "ㄩ":
                  Consonant = new("ㄐ");
                  break;  // ㄐㄩ
                default:
                  break;
              };
            };
            break;
          case "ㄔ":
            if (Intonation.IsEmpty) {
              switch (IncomingPhonabet.Value) {
                case "ㄧ":
                  Consonant = new("ㄑ");
                  break;  // ㄐㄧ
                case "ㄨ":
                  Consonant = new("ㄔ");
                  break;  // ㄓㄨ
                case "ㄩ":
                  Consonant = new("ㄑ");
                  break;  // ㄐㄩ
                default:
                  break;
              };
            };
            break;
          case "ㄕ":
            switch (IncomingPhonabet.Value) {
              case "ㄧ":
                Consonant = new("ㄒ");
                break;  // ㄒㄧ
              case "ㄨ":
                Consonant = new("ㄕ");
                break;  // ㄕㄨ
              case "ㄩ":
                Consonant = new("ㄒ");
                break;  // ㄒㄩ
              default:
                break;
            };
            break;
          default:
            break;
        }
        break;
      case PhoneType.Vowel:
        if (Semivowel.IsEmpty && !Consonant.IsEmpty) {
          Consonant.SelfReplace("ㄐ", "ㄓ");
          Consonant.SelfReplace("ㄑ", "ㄔ");
          Consonant.SelfReplace("ㄒ", "ㄕ");
        };
        break;
      default:
        break;
    };

    if ("dfjs ".Contains(Key)) {
      if (!Consonant.IsEmpty && Semivowel.IsEmpty && Vowel.IsEmpty) {
        Consonant.SelfReplace("ㄍ", "ㄜ");
        Consonant.SelfReplace("ㄋ", "ㄣ");
        Consonant.SelfReplace("ㄌ", "ㄦ");
        Consonant.SelfReplace("ㄎ", "ㄤ");
        Consonant.SelfReplace("ㄇ", "ㄢ");
      };
      if (!Consonant.IsEmpty && Vowel.IsEmpty) {
        Consonant.SelfReplace("ㄧ", "ㄝ");
      };
      if (!Vowel.IsEmpty && "ㄢㄣㄤㄥ".Contains(Vowel.Value) &&
          Semivowel.IsEmpty) {
        Consonant.SelfReplace("ㄐ", "ㄓ");
        Consonant.SelfReplace("ㄑ", "ㄔ");
        Consonant.SelfReplace("ㄒ", "ㄕ");
      };
      if (!Consonant.IsEmpty && "ㄐㄑㄒ".Contains(Consonant.Value) &&
          Semivowel.IsEmpty) {
        Consonant.SelfReplace("ㄐ", "ㄓ");
        Consonant.SelfReplace("ㄑ", "ㄔ");
        Consonant.SelfReplace("ㄒ", "ㄕ");
      };
      if (Consonant.Value == "ㄏ" && Semivowel.IsEmpty && Vowel.IsEmpty) {
        Consonant.Clear();
        Vowel = new("ㄛ");
      };
    };

    // 後置修正
    if (Value == "ㄔ˙") Consonant = new("ㄑ");

    // 這些按鍵在上文處理過了，就不要再回傳了。;
    if ("acdefghjklmns".Contains(Key)) StrReturn = "";

    // 回傳結果是空的話，不要緊，因為上文已經代處理過分配過程了。
    return StrReturn;
  }

  /// <summary>
  /// 酷音大千二十六鍵一樣同樣也比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="Key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  string HandleDachen26(string Key = "") {
    string StrReturn = (Shared.MapDachenCP26StaticKeys.Keys.Contains(Key))
                           ? Shared.MapDachenCP26StaticKeys[Key]
                           : "";

    switch (Key) {
      case "e":
        if (IsPronouncable)
          Intonation = new("ˊ");
        else
          Consonant = new("ㄍ");
        break;
      case "r":
        if (IsPronouncable)
          Intonation = new("ˇ");
        else
          Consonant = new("ㄐ");
        break;
      case "d":
        if (IsPronouncable)
          Intonation = new("ˋ");
        else
          Consonant = new("ㄎ");
        break;
      case "y":
        if (IsPronouncable)
          Intonation = new("˙");
        else
          Consonant = new("ㄗ");
        break;
      case "b":
        if (!Consonant.IsEmpty || !Semivowel.IsEmpty)
          Vowel = new("ㄝ");
        else
          Consonant = new("ㄖ");
        break;
      case "i":
        if (Vowel.IsEmpty || Vowel.Value == "ㄞ")
          Vowel = new("ㄛ");
        else
          Vowel = new("ㄞ");
        break;
      case "l":
        if (Vowel.IsEmpty || Vowel.Value == "ㄤ")
          Vowel = new("ㄠ");
        else
          Vowel = new("ㄤ");
        break;
      case "n":
        if (!Consonant.IsEmpty || !Semivowel.IsEmpty)
          Vowel = new("ㄥ");
        else
          Consonant = new("ㄙ");
        break;
      case "o":
        if (Vowel.IsEmpty || Vowel.Value == "ㄢ")
          Vowel = new("ㄟ");
        else
          Vowel = new("ㄢ");
        break;
      case "p":
        if (Vowel.IsEmpty || Vowel.Value == "ㄦ")
          Vowel = new("ㄣ");
        else
          Vowel = new("ㄦ");
        break;
      case "q":
        if (Consonant.IsEmpty || Consonant.Value == "ㄅ")
          Consonant = new("ㄆ");
        else
          Consonant = new("ㄅ");
        break;
      case "t":
        if (Consonant.IsEmpty || Consonant.Value == "ㄓ")
          Consonant = new("ㄔ");
        else
          Consonant = new("ㄓ");
        break;
      case "w":
        if (Consonant.IsEmpty || Consonant.Value == "ㄉ")
          Consonant = new("ㄊ");
        else
          Consonant = new("ㄉ");
        break;
      case "m":
        if (Semivowel.Value == "ㄩ" && Vowel.Value != "ㄡ") {
          Semivowel.Clear();
          Vowel = new("ㄡ");
        } else if (Semivowel.Value != "ㄩ" && Vowel.Value == "ㄡ") {
          Semivowel = new("ㄩ");
          Vowel.Clear();
        } else if (!Semivowel.IsEmpty)
          Vowel = new("ㄡ");
        else
          Semivowel = new("ㄩ");
        ;
        break;
      case "u":
        if (Semivowel.Value == "ㄧ" && Vowel.Value != "ㄚ") {
          Semivowel.Clear();
          Vowel = new("ㄚ");
        } else if (Semivowel.Value != "ㄧ" && Vowel.Value == "ㄚ")
          Semivowel = new("ㄧ");
        else if (Semivowel.Value == "ㄧ" && Vowel.Value == "ㄚ") {
          Semivowel.Clear();
          Vowel.Clear();
        } else if (!Semivowel.IsEmpty)
          Vowel = new("ㄚ");
        else
          Semivowel = new("ㄧ");
        break;
      default:
        break;
    };

    // 這些按鍵在上文處理過了，就不要再回傳了。
    if ("qwtilopnbmuerdy".Contains(Key)) StrReturn = "";

    // 回傳結果是空的話，不要緊，因為上文已經代處理過分配過程了。
    return StrReturn;
  }
}
}