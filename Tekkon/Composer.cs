// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Linq;
using System.Text;

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
    /// <summary>
    /// 聲母。
    /// </summary>
    public Phonabet Consonant { get; internal set; } = new Phonabet();

    /// <summary>
    /// 介母。
    /// </summary>
    public Phonabet Semivowel { get; internal set; } = new Phonabet();

    /// <summary>
    /// 韻母。
    /// </summary>
    public Phonabet Vowel { get; internal set; } = new Phonabet();

    /// <summary>
    /// 聲調。
    /// </summary>
    public Phonabet Intonation { get; internal set; } = new Phonabet();

    /// <summary>
    /// 拼音組音區。
    /// </summary>
    public string RomajiBuffer { get; internal set; } = "";

    /// <summary>
    /// 注音排列種類。預設情況下是大千排列（Windows / macOS 預設注音排列）。
    /// </summary>
    public MandarinParser Parser { get; internal set; } = MandarinParser.OfDachen;

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
    /// 統計有效的聲介韻（調）個數。
    /// </summary>
    /// <param name="withIntonation">是否統計聲調。</param>
    /// <returns>統計出的有效 Phonabet 個數。</returns>
    public int Count(bool withIntonation = false) {
      int result = (withIntonation && Intonation.IsValid) ? 1 : 0;
      result += Consonant.IsValid ? 1 : 0;
      result += Semivowel.IsValid ? 1 : 0;
      result += Vowel.IsValid ? 1 : 0;
      return result;
    }

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
      if (isHanyuPinyin) {
        string valReturnPinyin = Shared.CnvPhonaToHanyuPinyin(Value);
        return isTextBookStyle
                   ? Shared.CnvHanyuPinyinToTextbookStyle(valReturnPinyin)
                   : valReturnPinyin;
      }
      string valReturnZhuyin = Value.Replace(" ", "");
      return isTextBookStyle ? Shared.CnvPhonaToTextbookStyle(valReturnZhuyin)
                             : valReturnZhuyin;
    }

    /// <summary>
    /// 該函式僅用來獲取給 macOS InputMethod Kit 的內文組字區使用的顯示字串。
    /// </summary>
    /// <param name="isHanyuPinyin">是否將輸出結果轉成漢語拼音。</param>
    /// <returns>拼音/注音讀音字串，依照適合在輸入法組字區內顯示出來的格式。</returns>
    public string GetInlineCompositionForDisplay(bool isHanyuPinyin = false) {
      if (!IsPinyinMode) return GetComposition(isHanyuPinyin);
      string toneReturned;
      switch (Intonation.Value) {
        case " ":
          toneReturned = "1";
          break;
        case "ˊ":
          toneReturned = "2";
          break;
        case "ˇ":
          toneReturned = "3";
          break;
        case "ˋ":
          toneReturned = "4";
          break;
        case "˙":
          toneReturned = "5";
          break;
        default:
          toneReturned = "";
          break;
      }
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
    public bool IsPronounceable =>
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
      Consonant = new Phonabet();
      Semivowel = new Phonabet();
      Vowel = new Phonabet();
      Intonation = new Phonabet();
      RomajiBuffer = "";
      Parser = MandarinParser.OfDachen;
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
      Consonant = new Phonabet();
      Semivowel = new Phonabet();
      Vowel = new Phonabet();
      Intonation = new Phonabet();
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
      return (inputKey < 128) && InputValidityCheckStr(inputKey.ToString());
    }

    /// <summary>
    /// 用於檢測「某個輸入字符訊號的合規性」的函式。<br />
    /// <br />
    /// 注意：回傳結果會受到當前注音排列 parser 屬性的影響。
    /// </summary>
    /// <param name="charStr">傳入的字元（String）。</param>
    /// <returns>傳入的字符是否合規。</returns>
    public bool InputValidityCheckStr(string charStr) {
      if (string.IsNullOrEmpty(charStr)) return false;
      if (!Rune.TryGetRuneAt(charStr, 0, out Rune rune)) return false;
      string runeString = rune.ToString();
      switch (Parser) {
        case MandarinParser.OfDachen:
          return Shared.MapQwertyDachen.ContainsKey(runeString);
        case MandarinParser.OfDachen26:
          return Shared.MapDachenCp26StaticKeys.ContainsKey(runeString);
        case MandarinParser.OfETen:
          return Shared.MapQwertyETenTraditional.ContainsKey(runeString);
        case MandarinParser.OfHsu:
          return Shared.MapHsuStaticKeys.ContainsKey(runeString);
        case MandarinParser.OfETen26:
          return Shared.MapETen26StaticKeys.ContainsKey(runeString);
        case MandarinParser.OfIBM:
          return Shared.MapQwertyIBM.ContainsKey(runeString);
        case MandarinParser.OfMiTAC:
          return Shared.MapQwertyMiTAC.ContainsKey(runeString);
        case MandarinParser.OfSeigyou:
          return Shared.MapSeigyou.ContainsKey(runeString);
        case MandarinParser.OfFakeSeigyou:
          return Shared.MapFakeSeigyou.ContainsKey(runeString);
        case MandarinParser.OfStarlight:
          return Shared.MapStarlightStaticKeys.ContainsKey(runeString);
        case MandarinParser.OfAlvinLiu:
          return Shared.MapAlvinLiuStaticKeys.ContainsKey(runeString);
        case MandarinParser.OfWadeGilesPinyin:
          return Shared.MapWadeGilesPinyinRunes.Contains(rune);
        case MandarinParser.OfHanyuPinyin:
        case MandarinParser.OfSecondaryPinyin:
        case MandarinParser.OfYalePinyin:
        case MandarinParser.OfHualuoPinyin:
        case MandarinParser.OfUniversalPinyin:
          return Shared.MapArayuruPinyinRunes.Contains(rune);
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
      if (string.IsNullOrEmpty(strOf) || string.IsNullOrEmpty(strWith)) return;
      if (Consonant.Value == strOf)
        Consonant = new Phonabet();
      else if (Semivowel.Value == strOf)
        Semivowel = new Phonabet();
      else if (Vowel.Value == strOf)
        Vowel = new Phonabet();
      else if (Intonation.Value == strOf)
        Intonation = new Phonabet();
      else
        return;
      ReceiveKeyFromPhonabet(strWith);
    }

    /// <summary>
    /// 接受傳入的按鍵訊號時的處理，處理對象為 String。<br />
    /// 另有同名函式可處理 UniChar 訊號。<br />
    /// <br />
    /// 如果是諸如複合型注音排列的話，翻譯結果有可能為空，但翻譯過程已經處理好聲介韻調分配了。
    /// </summary>
    /// <param name="input">傳入的 String 內容。</param>
    public void ReceiveKey(string input) {
      if (string.IsNullOrEmpty(input)) return;
      if (!Rune.TryGetRuneAt(input, 0, out Rune rune)) return;
      ReceiveKey(rune);
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
    /// 接受傳入的按鍵訊號時的處理，處理對象為 Unicode Scalar。
    /// </summary>
    /// <param name="input">傳入的 Unicode Scalar 內容。</param>
    public void ReceiveKey(Rune input) {
      if (!IsPinyinMode) {
        ReceiveKeyFromPhonabet(Translate(input.ToString()));
        return;
      }
      string scalarString = input.ToString();
      if (Shared.MapArayuruPinyinIntonation.TryGetValue(input, out Rune toneRune)) {
        Intonation = new Phonabet(toneRune);
      } else {
        // 為了防止 RomajiBuffer 越敲越長帶來算力負擔，
        // 這裡讓它在要溢出時自動丟掉最早輸入的音頭。
        int maxCount = (Parser == MandarinParser.OfWadeGilesPinyin) ? 7 : 6;
        if (RomajiBuffer.Length > maxCount - 1)
          RomajiBuffer = new string(RomajiBuffer.Skip(1).ToArray());
        string romajiBufferBackup = RomajiBuffer + scalarString;
        ReceiveSequence(romajiBufferBackup, true);
        RomajiBuffer = romajiBufferBackup;
      }
    }

    /// <summary>
    /// 接受傳入的按鍵訊號時的處理，處理對象為單個注音符號。
    /// 主要就是將注音符號拆分辨識且分配到正確的貯存位置而已。
    /// </summary>
    /// <param name="phonabet">傳入的單個注音符號字串。</param>
    public void ReceiveKeyFromPhonabet(string phonabet = "") {
      Phonabet thePhone = new Phonabet(phonabet);
      if (PhonabetCombinationCorrectionEnabled) {
        switch (phonabet) {
          case "ㄧ":
          case "ㄩ":
            if (Vowel.Value == "ㄜ") Vowel = new Phonabet("ㄝ");
            break;
          case "ㄜ":
            if (Semivowel.Value == "ㄨ") Semivowel = new Phonabet("ㄩ");
            if ("ㄧㄩ".DoesHave(Semivowel.Value)) thePhone = new Phonabet("ㄝ");
            break;
          case "ㄝ":
            if (Semivowel.Value == "ㄨ") Semivowel = new Phonabet("ㄩ");
            break;
          case "ㄛ":
          case "ㄥ":
            if ("ㄅㄆㄇㄈ".DoesHave(Consonant.Value) && Semivowel.Value == "ㄨ")
              Semivowel = new Phonabet();
            break;
          case "ㄟ":
            if ("ㄋㄌ".DoesHave(Consonant.Value) && Semivowel.Value == "ㄨ")
              Semivowel = new Phonabet();
            break;
          case "ㄨ":
            switch (Vowel.Value) {
              case "ㄛ":
              case "ㄥ":
                switch (Consonant.Value) {
                  case "ㄅ":
                  case "ㄆ":
                  case "ㄇ":
                  case "ㄈ":
                    Vowel = new Phonabet();
                    break;
                }
                break;
              case "ㄟ":
                switch (Consonant.Value) {
                  case "ㄋ":
                  case "ㄌ":
                    Vowel = new Phonabet();
                    break;
                }
                break;
            }
            if (Vowel.Value == "ㄜ") Vowel = new Phonabet("ㄝ");
            if (Vowel.Value == "ㄝ") thePhone = new Phonabet("ㄩ");
            break;
          case "ㄅ":
          case "ㄆ":
          case "ㄇ":
          case "ㄈ":
            if (Semivowel.Value + Vowel.Value == "ㄨㄛ" ||
                Semivowel.Value + Vowel.Value == "ㄨㄥ")
              Semivowel = new Phonabet();
            break;
        }
        if (new[] { PhoneType.Intonation, PhoneType.Vowel }.Contains(
                thePhone.Type) &&
            "ㄓㄔㄕㄗㄘㄙ".DoesHave(Consonant.Value)) {
          switch (Semivowel.Value) {
            case "ㄧ":
              Semivowel = new Phonabet();
              break;
            case "ㄩ":
              switch (Consonant.Value) {
                case "ㄓ":
                case "ㄗ":
                  Consonant = new Phonabet("ㄐ");
                  break;
                case "ㄔ":
                case "ㄘ":
                  Consonant = new Phonabet("ㄑ");
                  break;
                case "ㄕ":
                case "ㄙ":
                  Consonant = new Phonabet("ㄒ");
                  break;
              }
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
    /// 接受傳入的按鍵訊號時的處理，處理對象為單個注音符號 Unicode Scalar。
    /// </summary>
    /// <param name="phonabet">傳入的單個注音符號 Unicode Scalar。</param>
    public void ReceiveKeyFromPhonabet(Rune phonabet) =>
        ReceiveKeyFromPhonabet(phonabet.ToString());

    /// <summary>
    /// 處理一連串的按鍵輸入。
    /// </summary>
    /// <param name="givenSequence">傳入的 String
    /// 內容，用以處理一整串擊鍵輸入。</param>
    /// <param
    /// name="isRomaji">如果輸入的字串是諸如漢語拼音這樣的西文字母拼音的話，請啟用此選項。</param>
    /// <returns>處理之後的結果。</returns>
    public string ReceiveSequence(string givenSequence = "",
                                  bool isRomaji = false) {
      Clear();
      if (!isRomaji) {
        foreach (char key in givenSequence) ReceiveKey(key);
        return Value;
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
      return Value;
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
        if (!Intonation.IsEmpty)
          Intonation = new Phonabet();
        else
          RomajiBuffer = RomajiBuffer.Remove(RomajiBuffer.Length - 1);
      } else if (!Intonation.IsEmpty)
        Intonation = new Phonabet();
      else if (!Vowel.IsEmpty)
        Vowel = new Phonabet();
      else if (!Semivowel.IsEmpty)
        Semivowel = new Phonabet();
      else if (!Consonant.IsEmpty) Consonant = new Phonabet();
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
    /// <param name="pronounceableOnly">是否可以唸出。</param>
    /// <returns>可用的查詢用注音字串，或者 nil。</returns>
    public string PhonabetKeyForQuery(bool pronounceableOnly) {
      string readingKey = GetComposition();
      bool isSelfPronouncable = IsPronounceable;
      bool validKeyAvailable = isSelfPronouncable;
      if (!IsPinyinMode && !isSelfPronouncable) validKeyAvailable = !string.IsNullOrEmpty(readingKey);
      return validKeyAvailable ? readingKey : null;
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
      switch (Parser) {
        case MandarinParser.OfDachen:
          return Shared.MapQwertyDachen.ContainsKey(key)
                     ? Shared.MapQwertyDachen[key]
                     : "";
        case MandarinParser.OfDachen26:
          return HandleDachen26(key);
        case MandarinParser.OfETen:
          return Shared.MapQwertyETenTraditional.ContainsKey(key)
                     ? Shared.MapQwertyETenTraditional[key]
                     : "";
        case MandarinParser.OfHsu:
          return HandleHsu(key);
        case MandarinParser.OfETen26:
          return HandleETen26(key);
        case MandarinParser.OfIBM:
          return Shared.MapQwertyIBM.ContainsKey(key) ? Shared.MapQwertyIBM[key]
                                                      : "";
        case MandarinParser.OfMiTAC:
          return Shared.MapQwertyMiTAC.ContainsKey(key)
                     ? Shared.MapQwertyMiTAC[key]
                     : "";
        case MandarinParser.OfSeigyou:
          return Shared.MapSeigyou.ContainsKey(key) ? Shared.MapSeigyou[key] : "";
        case MandarinParser.OfFakeSeigyou:
          return Shared.MapFakeSeigyou.ContainsKey(key)
                     ? Shared.MapFakeSeigyou[key]
                     : "";
        case MandarinParser.OfStarlight:
          return HandleStarlight(key);
        case MandarinParser.OfAlvinLiu:
          return HandleAlvinLiu(key);
        case MandarinParser.OfHanyuPinyin:
        case MandarinParser.OfSecondaryPinyin:
        case MandarinParser.OfYalePinyin:
        case MandarinParser.OfHualuoPinyin:
        case MandarinParser.OfUniversalPinyin:
        case MandarinParser.OfWadeGilesPinyin:
        default:
          return "";
      }
    }

    /// <summary>
    /// 倚天忘形注音排列是複合注音排列，需要單獨處理。<br />
    /// <br />
    /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
    /// </summary>
    /// <param name="key">傳入的 string 訊號。</param>
    /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
    private string HandleETen26(string key = "") {
      string strReturn = Shared.MapETen26StaticKeys.ContainsKey(key)
                             ? Shared.MapETen26StaticKeys[key]
                             : "";
      const string keysToHandleHere = "dfhjklmnpqtw";
      switch (key) {
        case "d":
          if (IsPronounceable) strReturn = "˙";
          break;
        case "f":
          if (IsPronounceable) strReturn = "ˊ";
          break;
        case "j":
          if (IsPronounceable) strReturn = "ˇ";
          break;
        case "k":
          if (IsPronounceable) strReturn = "ˋ";
          break;
        case "e":
          if (Consonant.Value == "ㄍ") Consonant = new Phonabet("ㄑ");
          break;
        case "p":
          if (!Consonant.IsEmpty || Semivowel.Value == "ㄧ") strReturn = "ㄡ";
          break;
        case "h":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄦ";
          break;
        case "l":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄥ";
          break;
        case "m":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄢ";
          break;
        case "n":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄣ";
          break;
        case "q":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄟ";
          break;
        case "t":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄤ";
          break;
        case "w":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄝ";
          break;
      }

      if (keysToHandleHere.DoesHave(key)) ReceiveKeyFromPhonabet(strReturn);

      // 處理公共特殊情形。
      CommonFixWhenHandlingDynamicArrangeInputs(new Phonabet(strReturn));

      if ("dfjk ".DoesHave(key) && Count() == 1) {
        FixValue("ㄆ", "ㄡ");
        FixValue("ㄇ", "ㄢ");
        FixValue("ㄊ", "ㄤ");
        FixValue("ㄋ", "ㄣ");
        FixValue("ㄌ", "ㄥ");
        FixValue("ㄏ", "ㄦ");
      }

      // 後置修正
      if (Value == "ㄍ˙") Consonant = new Phonabet("ㄑ");

      // 這些按鍵在上文處理過了，就不要再回傳了。
      if (keysToHandleHere.DoesHave(key)) strReturn = "";

      // 回傳結果是空字串的話，不要緊，因為上文已經代處理過分配過程了。
      return strReturn;
    }

    /// <summary>
    /// 許氏鍵盤注音排列是複合注音排列，需要單獨處理。<br />
    /// <br />
    /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
    /// </summary>
    /// <param name="key">傳入的 string 訊號。</param>
    /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
    private string HandleHsu(string key = "") {
      string strReturn = Shared.MapHsuStaticKeys.ContainsKey(key)
                             ? Shared.MapHsuStaticKeys[key]
                             : "";
      const string keysToHandleHere = "acdefghjklmns";
      switch (key) {
        case "d":
          if (IsPronounceable) strReturn = "ˊ";
          break;
        case "f":
          if (IsPronounceable) strReturn = "ˇ";
          break;
        case "s":
          if (IsPronounceable) strReturn = "˙";
          break;
        case "j":
          if (IsPronounceable) strReturn = "ˋ";
          break;
        case "a":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄟ";
          break;
        case "v":
          if (!Semivowel.IsEmpty) strReturn = "ㄑ";
          break;
        case "c":
          if (!Semivowel.IsEmpty) strReturn = "ㄒ";
          break;
        case "e":
          if (!Semivowel.IsEmpty) strReturn = "ㄝ";
          break;
        case "g":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄜ";
          break;
        case "h":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄛ";
          break;
        case "k":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄤ";
          break;
        case "m":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄢ";
          break;
        case "n":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄣ";
          break;
        case "l":
          if (string.IsNullOrEmpty(Value) && !Consonant.IsEmpty &&
              !Semivowel.IsEmpty)
            strReturn = "ㄦ";
          else if (Consonant.IsEmpty && Semivowel.IsEmpty)
            strReturn = "ㄌ";
          else
            strReturn = "ㄥ";
          break;
      }

      if (keysToHandleHere.DoesHave(key)) ReceiveKeyFromPhonabet(strReturn);

      // 處理公共特殊情形。
      CommonFixWhenHandlingDynamicArrangeInputs(new Phonabet(strReturn));

      if ("dfjs ".DoesHave(key) && Count() == 1) {
        FixValue("ㄒ", "ㄕ");
        FixValue("ㄍ", "ㄜ");
        FixValue("ㄋ", "ㄣ");
        FixValue("ㄌ", "ㄦ");
        FixValue("ㄎ", "ㄤ");
        FixValue("ㄇ", "ㄢ");
        FixValue("ㄐ", "ㄓ");
        FixValue("ㄑ", "ㄔ");
        FixValue("ㄒ", "ㄕ");
        FixValue("ㄏ", "ㄛ");
      }

      // 後置修正
      if (Value == "ㄔ˙") Consonant = new Phonabet("ㄑ");

      // 這些按鍵在上文處理過了，就不要再回傳了。
      if (keysToHandleHere.DoesHave(key)) strReturn = "";

      // 回傳結果是空字串的話，不要緊，因為上文已經代處理過分配過程了。
      return strReturn;
    }

    /// <summary>
    /// 星光注音排列是複合注音排列，需要單獨處理。<br />
    /// <br />
    /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
    /// </summary>
    /// <param name="key">傳入的 string 訊號。</param>
    /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
    private string HandleStarlight(string key = "") {
      string strReturn = Shared.MapStarlightStaticKeys.ContainsKey(key)
                             ? Shared.MapStarlightStaticKeys[key]
                             : "";
      const string keysToHandleHere = "efgklmnt";
      switch (key) {
        case "e":
          if ("ㄧㄩ".DoesHave(Semivowel.Value)) strReturn = "ㄝ";
          break;
        case "f":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄠ";
          break;
        case "g":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄥ";
          break;
        case "k":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄤ";
          break;
        case "l":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄦ";
          break;
        case "m":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄢ";
          break;
        case "n":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄣ";
          break;
        case "t":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄟ";
          break;
      }

      if (keysToHandleHere.DoesHave(key)) ReceiveKeyFromPhonabet(strReturn);

      // 處理公共特殊情形。
      CommonFixWhenHandlingDynamicArrangeInputs(new Phonabet(strReturn));

      if ("67890 ".DoesHave(key) && Count() == 1) {
        FixValue("ㄈ", "ㄠ");
        FixValue("ㄍ", "ㄥ");
        FixValue("ㄎ", "ㄤ");
        FixValue("ㄌ", "ㄦ");
        FixValue("ㄇ", "ㄢ");
        FixValue("ㄋ", "ㄣ");
        FixValue("ㄊ", "ㄟ");
      }

      // 這些按鍵在上文處理過了，就不要再回傳了。
      if (keysToHandleHere.DoesHave(key)) strReturn = "";

      // 回傳結果是空字串的話，不要緊，因為上文已經代處理過分配過程了。
      return strReturn;
    }

    /// <summary>
    /// 酷音大千二十六鍵注音排列是複合注音排列，需要單獨處理。<br />
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
          if (IsPronounceable) strReturn = "ˊ";
          break;
        case "r":
          if (IsPronounceable) strReturn = "ˇ";
          break;
        case "d":
          if (IsPronounceable) strReturn = "ˋ";
          break;
        case "y":
          if (IsPronounceable) strReturn = "˙";
          break;
        case "b":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄝ";
          break;
        case "i":
          if (Vowel.IsEmpty || Vowel.Value == "ㄞ") strReturn = "ㄛ";
          break;
        case "l":
          if (Vowel.IsEmpty || Vowel.Value == "ㄤ") strReturn = "ㄠ";
          break;
        case "n":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) {
            if (Value == "ㄙ") Consonant = new Phonabet();
            strReturn = "ㄥ";
          }
          break;
        case "o":
          if (Vowel.IsEmpty || Vowel.Value == "ㄢ") strReturn = "ㄟ";
          break;
        case "p":
          if (Vowel.IsEmpty || Vowel.Value == "ㄦ") strReturn = "ㄣ";
          break;
        case "q":
          if (Consonant.IsEmpty || Consonant.Value == "ㄅ") strReturn = "ㄆ";
          break;
        case "t":
          if (Consonant.IsEmpty || Consonant.Value == "ㄓ") strReturn = "ㄔ";
          break;
        case "w":
          if (Consonant.IsEmpty || Consonant.Value == "ㄉ") strReturn = "ㄊ";
          break;
        case "m":
          if (Semivowel.Value == "ㄩ" && Vowel.Value != "ㄡ") {
            Semivowel = new Phonabet();
            strReturn = "ㄡ";
          } else if (Semivowel.Value != "ㄩ" && Vowel.Value == "ㄡ") {
            Vowel = new Phonabet();
            strReturn = "ㄩ";
          } else if (!Semivowel.IsEmpty || !"ㄐㄑㄒ".DoesHave(Consonant.Value))
            strReturn = "ㄡ";
          else
            strReturn = "ㄩ";
          break;
        case "u":
          if (Semivowel.Value == "ㄧ" && Vowel.Value != "ㄚ") {
            Semivowel = new Phonabet();
            strReturn = "ㄚ";
          } else if (Semivowel.Value != "ㄧ" && Vowel.Value == "ㄚ")
            strReturn = "ㄧ";
          else if (Semivowel.Value == "ㄧ" && Vowel.Value == "ㄚ") {
            Semivowel = new Phonabet();
            Vowel = new Phonabet();
          } else if (!Semivowel.IsEmpty)
            strReturn = "ㄚ";
          else
            strReturn = "ㄧ";
          break;
      }

      // 回傳結果是空的話，不要緊，因為上文已經代處理過分配過程了。
      return strReturn;
    }

    /// <summary>
    /// 劉氏擬音注音排列是複合注音排列，需要單獨處理。<br />
    /// <br />
    /// 回傳結果是空字串的話，不要緊，因為該函式內部已經處理過分配過程了。
    /// <remarks>該處理兼顧了「原旨排列方案」與「微軟新注音相容排列方案」。</remarks>
    /// </summary>
    /// <param name="key">傳入的 string 訊號。</param>
    /// <returns>尚無追加處理而直接傳回的結果，或者是空字串。</returns>
    private string HandleAlvinLiu(string key = "") {
      string strReturn = Shared.MapAlvinLiuStaticKeys.ContainsKey(key)
                             ? Shared.MapAlvinLiuStaticKeys[key]
                             : "";

      // 前置處理專有特殊情形。
      if (strReturn != "ㄦ" && !Vowel.IsEmpty) FixValue("ㄦ", "ㄌ");

      const string keysToHandleHere = "dfjlegnhkbmc";
      switch (key) {
        case "d":
          if (IsPronounceable) strReturn = "˙";
          break;
        case "f":
          if (IsPronounceable) strReturn = "ˊ";
          break;
        case "j":
          if (IsPronounceable) strReturn = "ˇ";
          break;
        case "l":
          if (IsPronounceable) strReturn = "ˋ";
          break;
        case "e":
          if ("ㄧㄩ".DoesHave(Semivowel.Value)) strReturn = "ㄝ";
          break;
        case "g":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄤ";
          break;
        case "n":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄣ";
          break;
        case "h":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄞ";
          break;
        case "k":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄟ";
          break;
        case "b":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄢ";
          break;
        case "m":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄥ";
          break;
        case "c":
          if (!Consonant.IsEmpty || !Semivowel.IsEmpty) strReturn = "ㄝ";
          break;
      }

      if (keysToHandleHere.DoesHave(key)) ReceiveKeyFromPhonabet(strReturn);

      // 處理公共特殊情形。
      CommonFixWhenHandlingDynamicArrangeInputs(new Phonabet(strReturn));

      if ("dfjl ".DoesHave(key) && Count() == 1) {
        FixValue("ㄑ", "ㄔ");
        FixValue("ㄊ", "ㄦ");
        FixValue("ㄍ", "ㄤ");
        FixValue("ㄏ", "ㄞ");
        FixValue("ㄐ", "ㄓ");
        FixValue("ㄎ", "ㄟ");
        FixValue("ㄌ", "ㄦ");
        FixValue("ㄒ", "ㄕ");
        FixValue("ㄅ", "ㄢ");
        FixValue("ㄋ", "ㄣ");
        FixValue("ㄇ", "ㄥ");
      }

      // 這些按鍵在上文處理過了，就不要再回傳了。
      if (keysToHandleHere.DoesHave(key)) strReturn = "";

      // 回傳結果是空字串的話，不要緊，因為上文已經代處理過分配過程了。
      return strReturn;
    }

    /// <summary>
    /// 所有動態注音排列都會用到的共用糾錯處理步驟。
    /// </summary>
    /// <param name="incomingPhonabet">傳入的注音 Phonabet。</param>
    private void CommonFixWhenHandlingDynamicArrangeInputs(
        Phonabet incomingPhonabet) {
      // 處理公共特殊情形。
      switch (incomingPhonabet.Type) {
        case PhoneType.Semivowel:
          string newConstantStr;
          switch (Consonant.Value + incomingPhonabet.Value) {
            case "ㄓㄧ":
              newConstantStr = "ㄐ";
              break;
            case "ㄓㄩ":
              newConstantStr = "ㄐ";
              break;
            case "ㄍㄩ":
              newConstantStr = "ㄑ";
              break;
            case "ㄔㄧ":
              newConstantStr = "ㄑ";
              break;
            case "ㄔㄩ":
              newConstantStr = "ㄑ";
              break;
            case "ㄕㄧ":
              newConstantStr = "ㄒ";
              break;
            case "ㄕㄩ":
              newConstantStr = "ㄒ";
              break;
            default:
              newConstantStr = "";
              break;
          }
          if (!string.IsNullOrEmpty(newConstantStr)) Consonant = new Phonabet(newConstantStr);
          if (incomingPhonabet.Value == "ㄨ") {
            FixValue("ㄐ", "ㄓ");
            FixValue("ㄑ", "ㄔ");
            FixValue("ㄒ", "ㄕ");
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
