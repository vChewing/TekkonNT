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
  /// 拼音組音區。
  /// </summary>
  public string RomajiBuffer = "";

  /// <summary>
  /// 注音排列種類。預設情況下是大千排列（Windows / macOS 預設注音排列）。
  /// </summary>
  public MandarinParser Parser = MandarinParser.OfDachen;

  /// <summary>
  /// 是否對錯誤的注音讀音組合做出自動糾正處理。
  /// </summary>
  public bool PhonabetCombinationCorrectionEnabled { get; set; }

  /// <summary>
  /// 內容值，會直接按照正確的順序拼裝自己的聲介韻調內容、再回傳。
  /// 注意：直接取這個參數的內容的話，陰平聲調會成為一個空格。
  /// 如果是要取不帶空格的注音的話，請使用「.getComposition()」而非「.Value」。
  /// </summary>
  public string Value => $"{Consonant}{Semivowel}{Vowel}{Intonation}";

  /// <summary>
  /// 當前注拼槽是否處於拼音模式。
  /// </summary>
  public bool IsPinyinMode => (int)Parser >= 100;

  /// <summary>
  /// 與 value 類似，這個函式就是用來決定輸入法組字區內顯示的注音/拼音內容，
  /// 但可以指定是否輸出教科書格式（拼音的調號在字母上方、注音的輕聲寫在左側）。
  /// </summary>
  /// <param name="isHanyuPinyin">是否將輸出結果轉成漢語拼音。</param>
  /// <param
  /// name="isTextBookStyle">是否將輸出的注音/拼音結果轉成教科書排版格式。</param>
  /// <returns>拼音/注音讀音字串，依照指定的格式。</returns>
  public string GetComposition(bool isHanyuPinyin = false,
                               bool isTextBookStyle = false) {
    switch (isHanyuPinyin) {
      case false:  // 注音輸出的場合
        string valReturnZhuyin = Value.Replace(" ", "");
        return isTextBookStyle
                   ? Shared.CnvPhonaToTextbookReading(valReturnZhuyin)
                   : valReturnZhuyin;
      case true:  // 拼音輸出的場合
        string valReturnPinyin = Shared.CnvPhonaToHanyuPinyin(Value);
        return isTextBookStyle
                   ? Shared.CnvHanyuPinyinToTextbookStyle(valReturnPinyin)
                   : valReturnPinyin;
    }
  }

  /// <summary>
  /// 該函式僅用來獲取給 macOS InputMethod Kit 的內文組字區使用的顯示字串。
  /// </summary>
  /// <param name="isHanyuPinyin">是否將輸出結果轉成漢語拼音。</param>
  /// <returns>拼音/注音讀音字串，依照適合在輸入法組字區內顯示出來的格式。</returns>
  public string GetInlineCompositionForDisplay(bool isHanyuPinyin = false) {
    if (!IsPinyinMode) return GetComposition(isHanyuPinyin);
    string toneReturned =
        Intonation.Value switch { " " => "1", "ˊ" => "2", "ˇ" => "3",
                                  "ˋ" => "4", "˙" => "5",
                                  _ => "" };
    return RomajiBuffer.Replace("v", "ü") + toneReturned;
  }

  /// <summary>
  /// 注拼槽內容是否為空。
  /// </summary>
  public bool IsEmpty {
    get {
      if (IsPinyinMode) return Intonation.IsEmpty && RomajiBuffer == "";
      return Consonant.Value == "" && Semivowel.Value == "" &&
             Vowel.Value == "" && Intonation.Value == "";
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
  /// <param name="input">傳入的 String 內容，用以處理單個字符。</param>
  /// <param name="arrange">要使用的注音排列。</param>
  /// <param
  /// name="correction">是否對錯誤的注音讀音組合做出自動糾正處理。</param>
  public Composer(string input = "", MandarinParser arrange = 0,
                  bool correction = false) {
    PhonabetCombinationCorrectionEnabled = correction;
    EnsureParser(arrange);
    ReceiveKey(input);
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
  /// <param name="inputCharCode">傳入的 UniChar 內容。</param>
  /// <returns>傳入的字符是否合規。</returns>
  public bool InputValidityCheck(int inputCharCode) {
    char inputKey = (char)Math.Abs(inputCharCode);
    if (inputKey >= 128) return false;
    switch (Parser) {
      case MandarinParser.OfDachen:
        return Shared.MapQwertyDachen.ContainsKey(inputKey.ToString());
      case MandarinParser.OfDachen26:
        return Shared.MapDachenCp26StaticKeys.ContainsKey(inputKey.ToString());
      case MandarinParser.OfETen:
        return Shared.MapQwertyETenTraditional.ContainsKey(inputKey.ToString());
      case MandarinParser.OfHsu:
        return Shared.MapHsuStaticKeys.ContainsKey(inputKey.ToString());
      case MandarinParser.OfETen26:
        return Shared.MapETen26StaticKeys.ContainsKey(inputKey.ToString());
      case MandarinParser.OfIBM:
        return Shared.MapQwertyIBM.ContainsKey(inputKey.ToString());
      case MandarinParser.OfMiTAC:
        return Shared.MapQwertyMiTAC.ContainsKey(inputKey.ToString());
      case MandarinParser.OfSeigyou:
        return Shared.MapSeigyou.ContainsKey(inputKey.ToString());
      case MandarinParser.OfFakeSeigyou:
        return Shared.MapFakeSeigyou.ContainsKey(inputKey.ToString());
      case MandarinParser.OfStarlight:
        return Shared.MapStarlightStaticKeys.ContainsKey(inputKey.ToString());
      case MandarinParser.OfWadeGilesPinyin:
        return Shared.MapWadeGilesPinyinKeys.Contains(inputKey);
      case MandarinParser.OfHanyuPinyin:
      case MandarinParser.OfSecondaryPinyin:
      case MandarinParser.OfYalePinyin:
      case MandarinParser.OfHualuoPinyin:
      case MandarinParser.OfUniversalPinyin:
        return Shared.MapArayuruPinyin.Contains(inputKey);
      default:
        return false;
    }
  }

  /// <summary>
  /// 按需更新拼音組音區的內容顯示。
  /// </summary>
  public void UpdateRomajiBuffer() {
    RomajiBuffer = Shared.CnvPhonaToHanyuPinyin(targetJoined: Consonant.Value +
                                                Semivowel.Value + Vowel.Value);
  }

  /// <summary>
  /// 自我變換單個注音資料值。
  /// </summary>
  /// <param name="strOf">要取代的內容。</param>
  /// <param name="strWith">要取代成的內容。</param>
  private void FixValue(string strOf, string strWith) {
    Phonabet theOld = new(strOf);
    if (!string.IsNullOrEmpty(strOf)) {
      switch (theOld.Type) {
        case PhoneType.Consonant when Consonant.Value == theOld.Value:
          Consonant.Clear();
          break;
        case PhoneType.Semivowel when Semivowel.Value == theOld.Value:
          Semivowel.Clear();
          break;
        case PhoneType.Vowel when Vowel.Value == theOld.Value:
          Vowel.Clear();
          break;
        case PhoneType.Intonation when Intonation.Value == theOld.Value:
          Intonation.Clear();
          break;
        case PhoneType.Null:
          return;
        default:
          return;
      }
    }
    Phonabet theNew = new(strWith);
    switch (theNew.Type) {
      case PhoneType.Consonant:
        Consonant = theNew;
        break;
      case PhoneType.Semivowel:
        Semivowel = theNew;
        break;
      case PhoneType.Vowel:
        Vowel = theNew;
        break;
      case PhoneType.Intonation:
        Intonation = theNew;
        break;
      case PhoneType.Null:
        return;
      default:
        return;
    }
  }

  /// <summary>
  /// 接受傳入的按鍵訊號時的處理，處理對象為 String。<br />
  /// 另有同名函式可處理 UniChar 訊號。<br />
  /// <br />
  /// 如果是諸如複合型注音排列的話，翻譯結果有可能為空，但翻譯過程已經處理好聲介韻調分配了。
  /// </summary>
  /// <param name="input">傳入的 String 內容。</param>
  public void ReceiveKey(string input) {
    if (!IsPinyinMode) {
      ReceiveKeyFromPhonabet(Translate(input));
      return;
    }
    if (Shared.MapArayuruPinyinIntonation.ContainsKey(input)) {
      string theTone = Shared.MapArayuruPinyinIntonation[input];
      Intonation = new(theTone);
    } else {
      // 為了防止 RomajiBuffer 越敲越長帶來算力負擔，
      // 這裡讓它在要溢出時自動丟掉最早輸入的音頭。
      int maxCount = (Parser == MandarinParser.OfWadeGilesPinyin) ? 7 : 6;
      if (RomajiBuffer.Length > maxCount - 1) {
        RomajiBuffer = RomajiBuffer.Skip(1).ToString();
      }
      string romajiBufferBackup = RomajiBuffer + input;
      ReceiveSequence(romajiBufferBackup, true);
      RomajiBuffer = romajiBufferBackup;
    }
  }

  /// <summary>
  /// 接受傳入的按鍵訊號時的處理，處理對象為 UniChar。<br />
  /// 其實也就是先將 char(int) 轉為 String
  /// 再交給某個同名異參的函式來處理而已。<br /> <br />
  /// 如果是諸如複合型注音排列的話，翻譯結果有可能為空，但翻譯過程已經處理好聲介韻調分配了。
  /// </summary>
  /// <param name="inputChar">傳入的 char 內容，格式為 int。</param>
  public void ReceiveKey(int inputChar) =>
      ReceiveKey(((char)Math.Abs(inputChar)).ToString());

  /// <summary>
  /// 接受傳入的按鍵訊號時的處理，處理對象為單個注音符號。
  /// 主要就是將注音符號拆分辨識且分配到正確的貯存位置而已。
  /// </summary>
  /// <param name="phonabet">傳入的單個注音符號字串。</param>
  public void ReceiveKeyFromPhonabet(string phonabet = "") {
    Phonabet thePhone = new(phonabet);
    if (PhonabetCombinationCorrectionEnabled) {
      switch (phonabet) {
        case "ㄧ":
        case "ㄩ":
          if (Vowel.Value is "ㄜ") Vowel = new("ㄝ");
          break;
        case "ㄜ":
          if (Semivowel.Value is "ㄨ") Semivowel = new("ㄩ");
          if (Semivowel.Value is "ㄧ" or "ㄩ") thePhone = new("ㄝ");
          break;
        case "ㄝ":
          if (Semivowel.Value is "ㄨ") Semivowel = new("ㄩ");
          break;
        case "ㄛ":
        case "ㄥ":
          if (Consonant.Value is "ㄅ" or "ㄆ" or "ㄇ" or "ㄈ" &&
              Semivowel.Value == "ㄨ")
            Semivowel.Clear();
          break;
        case "ㄟ":
          if (Consonant.Value is "ㄋ" or "ㄌ" && Semivowel.Value == "ㄨ")
            Semivowel.Clear();
          break;
        case "ㄨ":
          switch (Consonant.Value) {
            case "ㄅ" or "ㄆ" or "ㄇ" or "ㄈ" when Vowel.Value is "ㄛ" or "ㄥ":
            case "ㄋ" or "ㄌ" when Vowel.Value is "ㄟ":
              Vowel.Clear();
              break;
          }
          if (Vowel.Value is "ㄜ") Vowel = new("ㄝ");
          if (Vowel.Value is "ㄝ") thePhone = new("ㄩ");
          break;
        case "ㄅ":
        case "ㄆ":
        case "ㄇ":
        case "ㄈ":
          if (Semivowel.Value + Vowel.Value == "ㄨㄛ" ||
              Semivowel.Value + Vowel.Value == "ㄨㄥ")
            Semivowel.Clear();
          break;
      }
      if ((thePhone.Type is PhoneType.Intonation or PhoneType.Vowel) &&
          (Consonant.Value is "ㄓ" or "ㄔ" or "ㄕ" or "ㄗ" or "ㄘ" or "ㄙ")) {
        switch (Semivowel.Value) {
          case "ㄧ":
            Semivowel.Clear();
            break;
          case "ㄩ":
            Consonant = Consonant.Value switch { "ㄓ" or "ㄗ" => new("ㄐ"),
                                                 "ㄔ" or "ㄘ" => new("ㄑ"),
                                                 "ㄕ" or "ㄙ" => new("ㄒ"),
                                                 _ => Consonant };
            break;
        }
      }
    }
    switch (thePhone.Type) {
      case PhoneType.Consonant:
        Consonant = thePhone;
        break;
      case PhoneType.Semivowel:
        Semivowel = thePhone;
        break;
      case PhoneType.Vowel:
        Vowel = thePhone;
        break;
      case PhoneType.Intonation:
        Intonation = thePhone;
        break;
      case PhoneType.Null:
      default:
        break;
    }
    UpdateRomajiBuffer();
  }

  /// <summary>
  /// 處理一連串的按鍵輸入。
  /// </summary>
  /// <param name="givenSequence">傳入的 String
  /// 內容，用以處理一整串擊鍵輸入。</param>
  /// <param
  /// name="isRomaji">如果輸入的字串是諸如漢語拼音這樣的西文字母拼音的話，請啟用此選項。</param>
  public void ReceiveSequence(string givenSequence = "",
                              bool isRomaji = false) {
    Clear();
    if (!isRomaji) {
      foreach (char key in givenSequence) ReceiveKey(key);
      return;
    }
    string dictResult = "";
    switch (Parser) {
      case MandarinParser.OfHanyuPinyin:
        if (Shared.MapHanyuPinyin.ContainsKey(givenSequence))
          dictResult = Shared.MapHanyuPinyin[givenSequence];
        break;
      case MandarinParser.OfSecondaryPinyin:
        if (Shared.MapSecondaryPinyin.ContainsKey(givenSequence))
          dictResult = Shared.MapSecondaryPinyin[givenSequence];
        break;
      case MandarinParser.OfYalePinyin:
        if (Shared.MapYalePinyin.ContainsKey(givenSequence))
          dictResult = Shared.MapYalePinyin[givenSequence];
        break;
      case MandarinParser.OfHualuoPinyin:
        if (Shared.MapHualuoPinyin.ContainsKey(givenSequence))
          dictResult = Shared.MapHualuoPinyin[givenSequence];
        break;
      case MandarinParser.OfUniversalPinyin:
        if (Shared.MapUniversalPinyin.ContainsKey(givenSequence))
          dictResult = Shared.MapUniversalPinyin[givenSequence];
        break;
      case MandarinParser.OfWadeGilesPinyin:
        if (Shared.MapWadeGilesPinyin.ContainsKey(givenSequence))
          dictResult = Shared.MapWadeGilesPinyin[givenSequence];
        break;
    }
    foreach (char phonabet in dictResult)
      ReceiveKeyFromPhonabet(phonabet.ToString());
  }

  /// <summary>
  /// 處理一連串的按鍵輸入、且返回被處理之後的注音（陰平為空格）。
  /// </summary>
  /// <param name="givenSequence">傳入的 String
  /// 內容，用以處理一整串擊鍵輸入。</param>
  /// <returns>在處理該輸入順序後，注拼槽根據目前狀態生成的拼音/注音字串。</returns>
  public string CnvSequence(string givenSequence = "") {
    ReceiveSequence(givenSequence);
    return Value;
  }

  /// <summary>
  /// 專門用來響應使用者摁下 BackSpace 按鍵時的行為。<br />
  /// 刪除順序：調、韻、介、聲。<br />
  /// <br />
  /// 基本上就是按順序從游標前方開始往後刪。
  /// </summary>
  public void DoBackSpace() {
    if (IsPinyinMode && RomajiBuffer.Length != 0) {
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
  /// <param name="withNothingElse">追加判定「槽內是否僅有調號」。</param>
  /// <returns>有則真，無則假。</returns>
  public bool HasIntonation(bool withNothingElse = false) =>
      withNothingElse ? !Intonation.IsEmpty && Vowel.IsEmpty
                            && Semivowel.IsEmpty && Consonant.IsEmpty
                      : !Intonation.IsEmpty;

  /// <summary>
  /// 設定該 Composer 處於何種鍵盤排列分析模式。
  /// </summary>
  /// <param name="arrange">給該注拼槽指定注音排列。</param>
  public void EnsureParser(MandarinParser arrange = 0) { Parser = arrange; }

  /// <summary>
  /// 拿取用來進行索引檢索用的注音字串。
  ///
  /// 如果輸入法的辭典索引是漢語拼音的話，你可能用不上這個函式。
  /// <remarks>該字串結果不能為空，否則組字引擎會炸。
  /// 因為 C# 沒有 string? 類型，所以必須用 string.IsNullOrEmpty()
  /// 專門檢查。</remarks>
  /// </summary>
  /// <param name="Pronouncable">是否可以唸出。</param>
  /// <returns>可用的查詢用注音字串，或者 nil。</returns>
  public string PhonabetKeyForQuery(bool Pronouncable) {
    string readingKey = GetComposition();
    bool validKeyGeneratable = IsPinyinMode switch {
      false => Pronouncable switch { false => !string.IsNullOrEmpty(readingKey),
                                     true => IsPronouncable },
      true => IsPronouncable
    };
    return validKeyGeneratable ? readingKey : null;
  }

  // MARK: - Parser Processing

  // 注拼槽對內處理用函式都在這一小節。

  /// <summary>
  /// 根據目前的注音排列設定來翻譯傳入的 String 訊號。<br />
  /// <br />
  /// 倚天/許氏鍵盤/酷音大千二十六鍵的處理函式會代為處理分配過程，此時回傳結果可能為空字串。
  /// </summary>
  /// <param name="key">傳入的 String 訊號。</param>
  /// <returns></returns>
  private string Translate(string key) {
    if (IsPinyinMode) return "";
    return Parser switch {
      MandarinParser.OfDachen => Shared.MapQwertyDachen.ContainsKey(key)
                                     ? Shared.MapQwertyDachen[key]
                                     : "",
      MandarinParser.OfDachen26 => HandleDachen26(key),
      MandarinParser.OfETen => Shared.MapQwertyETenTraditional.ContainsKey(key)
                                   ? Shared.MapQwertyETenTraditional[key]
                                   : "",
      MandarinParser.OfHsu => HandleHsu(key),
      MandarinParser.OfETen26 => HandleETen26(key),
      MandarinParser.OfIBM =>
          Shared.MapQwertyIBM.ContainsKey(key) ? Shared.MapQwertyIBM[key] : "",
      MandarinParser.OfMiTAC => Shared.MapQwertyMiTAC.ContainsKey(key)
                                    ? Shared.MapQwertyMiTAC[key]
                                    : "",
      MandarinParser.OfSeigyou =>
          Shared.MapSeigyou.ContainsKey(key) ? Shared.MapSeigyou[key] : "",
      MandarinParser.OfFakeSeigyou => Shared.MapFakeSeigyou.ContainsKey(key)
                                          ? Shared.MapFakeSeigyou[key]
                                          : "",
      MandarinParser.OfStarlight => HandleStarlight(key),
      _ => ""
    };
  }

  /// <summary>
  /// 倚天忘形注音排列比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  private string HandleETen26(string key = "") {
    string strReturn = Shared.MapETen26StaticKeys.ContainsKey(key)
                           ? Shared.MapETen26StaticKeys[key]
                           : "";
    Phonabet incomingPhonabet = new(strReturn);

    switch (key) {
      case "d":
        if (!IsPronouncable)
          Consonant = new("ㄉ");
        else
          Intonation = new("˙");
        break;
      case "e":
        if (Consonant.Value == "ㄍ") Consonant = new("ㄑ");
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
        switch (Consonant.IsEmpty) {
          case true when Semivowel.IsEmpty:
            Consonant = new("ㄆ");
            break;
          case true when Semivowel.Value == "ㄧ":
            Vowel = new("ㄡ");
            break;
          case true:
            Consonant = new("ㄆ");
            break;
          default:
            Vowel = new("ㄡ");
            break;
        }
        break;
    }

    // 處理特殊情形。
    CommonFixWhenHandlingDynamicArrangeInputs(incomingPhonabet);

    if ("dfjk ".Contains(key) && !Consonant.IsEmpty && Semivowel.IsEmpty &&
        Vowel.IsEmpty) {
      FixValue("ㄆ", "ㄡ");
      FixValue("ㄇ", "ㄢ");
      FixValue("ㄊ", "ㄤ");
      FixValue("ㄋ", "ㄣ");
      FixValue("ㄌ", "ㄥ");
      FixValue("ㄏ", "ㄦ");
    }

    // 後置修正
    if (Value == "ㄍ˙") Consonant = new("ㄑ");

    // 這些按鍵在上文處理過了，就不要再回傳了。
    if ("dfhjklmnpqtw".Contains(key)) strReturn = "";

    // 回傳結果是空字串的話，不要緊，因為上文已經代處理過分配過程了。
    return strReturn;
  }

  /// <summary>
  /// 許氏鍵盤與倚天忘形一樣同樣也比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  private string HandleHsu(string key = "") {
    string strReturn = Shared.MapHsuStaticKeys.ContainsKey(key)
                           ? Shared.MapHsuStaticKeys[key]
                           : "";
    Phonabet incomingPhonabet = new(strReturn);

    if (key == " " && Value == "ㄋ") {
      Consonant.Clear();
      Vowel = new("ㄣ");
    }

    switch (key) {
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
    }

    // 處理特殊情形。
    CommonFixWhenHandlingDynamicArrangeInputs(incomingPhonabet);

    if ("dfjs ".Contains(key)) {
      if (!Consonant.IsEmpty && Semivowel.IsEmpty && Vowel.IsEmpty) {
        FixValue("ㄍ", "ㄜ");
        FixValue("ㄋ", "ㄣ");
        FixValue("ㄌ", "ㄦ");
        FixValue("ㄎ", "ㄤ");
        FixValue("ㄇ", "ㄢ");
      }
      if (!Vowel.IsEmpty && "ㄢㄣㄤㄥ".Contains(Vowel.Value) &&
          Semivowel.IsEmpty) {
        FixValue("ㄐ", "ㄓ");
        FixValue("ㄑ", "ㄔ");
        FixValue("ㄒ", "ㄕ");
      }
      if (!Consonant.IsEmpty && "ㄐㄑㄒ".Contains(Consonant.Value) &&
          Semivowel.IsEmpty) {
        FixValue("ㄐ", "ㄓ");
        FixValue("ㄑ", "ㄔ");
        FixValue("ㄒ", "ㄕ");
      }
      if (Consonant.Value == "ㄏ" && Semivowel.IsEmpty && Vowel.IsEmpty) {
        Consonant.Clear();
        Vowel = new("ㄛ");
      }
    }

    // 後置修正
    if (Value == "ㄔ˙") Consonant = new("ㄑ");

    // 這些按鍵在上文處理過了，就不要再回傳了。;
    if ("acdefghjklmns".Contains(key)) strReturn = "";

    // 回傳結果是空的話，不要緊，因為上文已經代處理過分配過程了。
    return strReturn;
  }

  /// <summary>
  /// 星光排列一樣同樣也比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  private string HandleStarlight(string key = "") {
    string strReturn = Shared.MapStarlightStaticKeys.ContainsKey(key)
                           ? Shared.MapStarlightStaticKeys[key]
                           : "";
    Phonabet incomingPhonabet = new(strReturn);

    switch (key) {
      case "e":
        return Semivowel.Value is "ㄧ" or "ㄩ" ? "ㄝ" : "ㄜ";
      case "f":
        return Vowel.Value is "ㄠ" || !IsPronouncable ? "ㄈ" : "ㄠ";
      case "g":
        return Vowel.Value is "ㄥ" || !IsPronouncable ? "ㄍ" : "ㄥ";
      case "k":
        return Vowel.Value is "ㄤ" || !IsPronouncable ? "ㄎ" : "ㄤ";
      case "l":
        return Vowel.Value is "ㄦ" || !IsPronouncable ? "ㄌ" : "ㄦ";
      case "m":
        return Vowel.Value is "ㄢ" || !IsPronouncable ? "ㄇ" : "ㄢ";
      case "n":
        return Vowel.Value is "ㄣ" || !IsPronouncable ? "ㄋ" : "ㄣ";
      case "t":
        return Vowel.Value is "ㄟ" || !IsPronouncable ? "ㄊ" : "ㄟ";
    }

    // 處理特殊情形。
    CommonFixWhenHandlingDynamicArrangeInputs(incomingPhonabet);

    if ("67890 ".Contains(key)) {
      if (!Consonant.IsEmpty && Semivowel.IsEmpty && Vowel.IsEmpty) {
        FixValue("ㄈ", "ㄠ");
        FixValue("ㄍ", "ㄥ");
        FixValue("ㄎ", "ㄤ");
        FixValue("ㄌ", "ㄦ");
        FixValue("ㄇ", "ㄢ");
        FixValue("ㄋ", "ㄣ");
        FixValue("ㄊ", "ㄟ");
      }
    }

    // 這些按鍵在上文處理過了，就不要再回傳了。;
    if ("efgklmn".Contains(key)) strReturn = "";

    // 回傳結果是空的話，不要緊，因為上文已經代處理過分配過程了。
    return strReturn;
  }

  /// <summary>
  /// 酷音大千二十六鍵一樣同樣也比較麻煩，需要單獨處理。<br />
  /// <br />
  /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
  /// </summary>
  /// <param name="key">傳入的 string 訊號。</param>
  /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
  private string HandleDachen26(string key = "") {
    string strReturn = Shared.MapDachenCp26StaticKeys.ContainsKey(key)
                           ? Shared.MapDachenCp26StaticKeys[key]
                           : "";

    switch (key) {
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
        if (!Consonant.IsEmpty || !Semivowel.IsEmpty) {
          if (Consonant.Value == "ㄙ" && Semivowel.IsEmpty && Vowel.IsEmpty)
            Consonant.Clear();
          Vowel = new("ㄥ");
        } else
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
        else {
          ReceiveKeyFromPhonabet(
              !Consonant.IsEmpty && "ㄐㄑㄒ".Contains(Consonant.Value) ? "ㄩ"
                                                                       : "ㄡ");
        }
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
    }

    // 這些按鍵在上文處理過了，就不要再回傳了。
    if ("qwtilopnbmuerdy".Contains(key)) strReturn = "";

    // 回傳結果是空的話，不要緊，因為上文已經代處理過分配過程了。
    return strReturn;
  }

  /// <summary>
  /// 所有動態注音排列都會用到的共用糾錯處理步驟。
  /// </summary>
  /// <param name="incomingPhonabet">傳入的注音 Phonabet。</param>
  private void CommonFixWhenHandlingDynamicArrangeInputs(
      Phonabet incomingPhonabet) {
    // 處理特殊情形。
    switch (incomingPhonabet.Type) {
      case PhoneType.Semivowel:
        switch (Consonant.Value) {
          case "ㄍ":
            // 這裡不處理「ㄍㄧ」到「ㄑㄧ」的轉換，因為只有倚天26需要處理這個。
            // 星光鍵盤應該也需要這個自動糾正，與許氏雷同；
            Consonant = incomingPhonabet.Value switch { "ㄨ" => new("ㄍ"),
                                                        "ㄩ" => new("ㄑ"),
                                                        _ => Consonant };
            break;
          case "ㄓ":
            if (Intonation.IsEmpty) {
              Consonant = incomingPhonabet.Value switch { "ㄧ" => new("ㄐ"),
                                                          "ㄨ" => new("ㄓ"),
                                                          "ㄩ" => new("ㄐ"),
                                                          _ => Consonant };
            }
            break;
          case "ㄔ":
            if (Intonation.IsEmpty) {
              Consonant = incomingPhonabet.Value switch { "ㄧ" => new("ㄑ"),
                                                          "ㄨ" => new("ㄔ"),
                                                          "ㄩ" => new("ㄑ"),
                                                          _ => Consonant };
            }
            break;
          case "ㄕ":
            Consonant = incomingPhonabet.Value switch { "ㄧ" => new("ㄒ"),
                                                        "ㄨ" => new("ㄕ"),
                                                        "ㄩ" => new("ㄒ"),
                                                        _ => Consonant };
            break;
        }
        break;
      case PhoneType.Vowel:
        if (Semivowel.IsEmpty && !Consonant.IsEmpty) {
          FixValue("ㄐ", "ㄓ");
          FixValue("ㄑ", "ㄔ");
          FixValue("ㄒ", "ㄕ");
        }
        break;
      case PhoneType.Null:
      case PhoneType.Consonant:
      case PhoneType.Intonation:
      default:
        break;
    }
  }
}
}