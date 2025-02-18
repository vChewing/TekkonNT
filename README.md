# Tekkon Engine 鐵恨引擎

- Gitee: [Swift](https://gitee.com/vChewing/Tekkon) | [C#](https://gitee.com/vChewing/TekkonNT) | [C++](https://gitee.com/vChewing/TekkonCC)
- GitHub: [Swift](https://github.com/vChewing/Tekkon) | [C#](https://github.com/vChewing/TekkonNT) | [C++](https://github.com/vChewing/TekkonCC)

相關使用說明請參見 Swift 版的倉庫的 README.MD。函式用法完全一致。

鐵恨引擎的 C# 版本，依 .NET 6 標準編寫而成。

> 注意：該引擎會將「ㄅㄨㄥ ㄆㄨㄥ ㄇㄨㄥ ㄈㄨㄥ」這四種讀音自動轉換成「ㄅㄥ ㄆㄥ ㄇㄥ ㄈㄥ」、將「ㄅㄨㄛ ㄆㄨㄛ ㄇㄨㄛ ㄈㄨㄛ」這四種讀音自動轉換成「ㄅㄛ ㄆㄛ ㄇㄛ ㄈㄛ」。如果您正在開發的輸入法的詞庫內的「甮」字的讀音沒有從「ㄈㄨㄥˋ」改成「ㄈㄥˋ」、或者說需要保留「ㄈㄨㄥˋ」的讀音的話，請按需修改「ReceiveKeyfromPhonabet()」函式當中的相關步驟、來跳過該轉換。該情形為十分罕見之情形。類似情形則是台澎金馬審音的慣用讀音「ㄌㄩㄢˊ」，因為使用者眾、所以不會被該引擎自動轉換成「ㄌㄨㄢˊ」。威注音輸入法內部已經從辭典角度做了處理、允許在敲「ㄌㄨㄢˊ」的時候出現以「ㄌㄩㄢˊ」為讀音的漢字。我們鼓勵輸入法開發者們使用 [威注音語彙庫](https://gitee.com/vChewing/libvchewing-data) 來實現對兩岸讀音習慣的同時兼顧。

---

鐵恨引擎是用來處理注音輸入法並擊行為的一個模組。該倉庫乃威注音專案的弒神行動（Operation Longinus）的一部分。

Tekkon Engine is a module made for processing combo-composition of stroke-based Mandarin Chinese phonetics (i.e. Zhuyin / Bopomofo). This repository is part of Operation Longinus of The vChewing Project.

羅馬拼音輸入目前僅支援漢語拼音、國音二式、耶魯拼音、華羅拼音、通用拼音、韋氏拼音（威妥瑪拼音）。

- 因為**趙元任國語羅馬字拼音「無法製作通用的聲調確認鍵」**，故鐵恨引擎在技術上無法實現對趙元任國語羅馬字拼音的支援。

Regarding pinyin input support, we only support: Hanyu Pinyin, Secondary Pinyin, Yale Pinyin, Hualuo Pinyin, Wade-Giles Pinyin and Universal Pinyin.

- **Tekkon is unable to provide support for Zhao Yuan-Ren's Gwoyeu Romatzyh at this moment** because there is no consistent method to check whether the intonation key has been pressed. Tekkon is designed to confirm input with intonation keys.


## 著作權 (Credits)

- Development by (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
	- Original Swift developer: Shiki Suen
	- C# and Cpp version developer: Shiki Suen

## 著作權 (Credits)

- Development by (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
	- Original Swift developer: Shiki Suen
	- C# and Cpp version developer: Shiki Suen

```
// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.
```

敝專案採雙授權發佈措施。除了 LGPLv3 以外，對商業使用者也提供不同的授權條款（比如允許閉源使用等）。詳情請[電郵聯絡作者](shikisuen@yeah.net)。

$ EOF.
