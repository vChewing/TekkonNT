# TekkonNT v1.7.0 變更日誌

此版本移植了 Swift Tekkon v1.6.0 → v1.7.0 的所有變更。

## 重大變更（可能造成不相容）

### 公開 API 改以 `Rune` 為核心型別

- `Phonabet.AllowedConsonants` / `AllowedSemivowels` / `AllowedVowels` / `AllowedIntonations` / `AllowedPhonabets` 改為 `IReadOnlyList<Rune>`（原為 `String[]`）
- `Phonabet` 以 `Rune` 儲存值；新增 `Phonabet(Rune input)` 建構子
- 多數內部處理與對外方法改以 `Rune` 參數（如 `ReceiveKey(Rune input)`、`ReceiveKeyFromPhonabet(Rune phonabet)`）

### 屬性改為唯讀（對外）

`Composer` 多數屬性改為 `{ get; internal set; }`（對外唯讀，對內可寫）：
- `Consonant`（聲母）
- `Semivowel`（介母）
- `Vowel`（韻母）
- `Intonation`（聲調）
- `RomajiBuffer`（拼音組音區）
- `Parser`（注音排列種類）

需改用對應方法變更狀態，無法直接從外部賦值。

### 動態注音排列處理方法改為私有

以下方法改為 `private`：
- `HandleETen26`
- `HandleHsu`
- `HandleStarlight`
- `HandleDachen26`
- `HandleAlvinLiu`

## 新功能與能力

### MandarinParser 擴充方法

新增以下靜態集合與方法：
- `AllCases` - 列出所有可用的鍵盤排列
- `AllPinyinCases` - 列出所有拼音排列
- `AllDynamicZhuyinCases` - 列出所有動態注音排列
- `AllStaticZhuyinCases` - 列出所有靜態注音排列
- `IsPinyin()` - 判定指定排列是否為拼音模式
- `IsDynamic()` - 判定指定排列是否為動態注音排列
- `MapZhuyinPinyin()` - 取得與拼音排列對應的注音查表
- `NameTag()` - 提供排列的識別名稱

### PinyinTrie 改進

- 修正 `DeductChoppedPinyinToZhuyin()` 的處理問題
- 改進節點識別符的處理機制

## 測試結構重構

### 移除的測試檔案
- `TekkonTests_Advanced.cs`（462KB 的硬編碼測試）

### 新增/重組的測試檔案
- **`TekkonTests_Arrangements.cs`** - 資料驅動的鍵盤排列測試（2 個測試）
  - 靜態大千排列測試
  - 動態排列測試（自動測試所有 5 種動態排列）
- **`TekkonTests_Basic.cs`** - 核心功能測試（8 個測試）
  - 包含 MandarinParser、Phonabet、組音、PinyinTrie 測試
- **`TekkonTests_Pinyin.cs`** - 拼音專屬測試（6 個測試）
  - 涵蓋所有 6 種拼音變體

總計 16 個測試，全數通過 ✅

新的資料驅動方法將測試程式碼從 462KB 降至 32KB，同時維持完整的測試覆蓋率。

## 重構與維護性提升

### 型別系統改進
- 核心型別由 `String` → `Rune`，降低字串切割與比較成本，避免多字元誤用
- 屬性封裝確保內部狀態一致性

### 關鍵修復
由於 C# 的 `struct` 特性，在屬性上呼叫方法會建立副本，因此將所有 `property.Clear()` 呼叫改為 `property = new Phonabet()` 賦值，確保正確修改原始值（共修復 15+ 處）。

## 建置與環境

- 需求不變

## 授權

授權維持 LGPL v3.0 or later。

## 升級指引摘要

### API 變更
- 若外部程式碼使用到公開常數陣列，需改以 `IReadOnlyList<Rune>` 處理
- 直接寫入 `Composer` 的欄位需改用對應的輸入路徑（`ReceiveKey` / `ReceiveSequence` 等）
- 需要使用 `Rune` 型別處理單一注音符號，而非 `String`

### 相容性
- 保留以 `String` 為參數的建構子與方法，以維持向後相容
- 舊有 API 仍可使用，但建議遷移至新的 `Rune` 為基礎的 API

### 測試
- 測試結構已完全重構為資料驅動模式
- 若有自訂測試，建議參考新的測試檔案結構

## 已知問題

無。

---

完整變更請參閱：https://github.com/vChewing/Tekkon/releases/tag/v1.7.0（Swift 版本）
