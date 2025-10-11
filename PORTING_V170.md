# Swift Tekkon v1.6.0 → v1.7.0 Porting to TekkonNT (C#)

This document summarizes the changes ported from Swift Tekkon v1.7.0 to TekkonNT.

## Changes Ported

### 1. Unicode.Scalar → Rune (C# equivalent)

Swift v1.7.0 changed from using `String` to `Unicode.Scalar` for phonabet handling. In C#, we use `System.Text.Rune` which is the equivalent type representing a single Unicode code point.

**Changes made:**
- ✅ `Phonabet.AllowedConsonants` changed from `String[]` to `IReadOnlyList<Rune>`
- ✅ `Phonabet.AllowedSemivowels` changed from `String[]` to `IReadOnlyList<Rune>`
- ✅ `Phonabet.AllowedVowels` changed from `String[]` to `IReadOnlyList<Rune>`
- ✅ `Phonabet.AllowedIntonations` changed from `String[]` to `IReadOnlyList<Rune>`
- ✅ `Phonabet.AllowedPhonabets` changed from `String[]` to `IReadOnlyList<Rune>`
- ✅ Added `Phonabet(Rune input)` constructor
- ✅ `Phonabet.ScalarValue` is now `Rune` type
- ✅ Added `Composer.ReceiveKey(Rune input)` method
- ✅ Added `Composer.ReceiveKeyFromPhonabet(Rune phonabet)` method

### 2. Properties Made Read-Only

Swift v1.7.0 changed properties to `public internal(set)` (read-only from outside). In C#, we use properties with `{ get; internal set; }`.

**Changes made:**
- ✅ `Composer.Consonant` changed from public field to property with `{ get; internal set; }`
- ✅ `Composer.Semivowel` changed from public field to property with `{ get; internal set; }`
- ✅ `Composer.Vowel` changed from public field to property with `{ get; internal set; }`
- ✅ `Composer.Intonation` changed from public field to property with `{ get; internal set; }`
- ✅ `Composer.RomajiBuffer` changed from public field to property with `{ get; internal set; }`
- ✅ `Composer.Parser` changed from public field to property with `{ get; internal set; }`

**Technical note:** Since `Phonabet` is a struct and these are now properties, we had to fix all `.Clear()` calls on these properties. Calling a method on a struct property creates a copy, so the original is not modified. Fixed by replacing all `.Clear()` calls with assignment to `new Phonabet()`.

### 3. MandarinParser Extensions

Swift v1.7.0 added new static collections for classifying parsers.

**Changes made:**
- ✅ Added `MandarinParserExtensions.AllCases`
- ✅ Added `MandarinParserExtensions.AllPinyinCases`
- ✅ Added `MandarinParserExtensions.AllDynamicZhuyinCases`
- ✅ Added `MandarinParserExtensions.AllStaticZhuyinCases`
- ✅ Extended `IsPinyin()` extension method
- ✅ Extended `IsDynamic()` extension method
- ✅ Added `MapZhuyinPinyin()` extension method

### 4. Composer API Changes

**Changes made:**
- ✅ `isEmpty` property (already existed)
- ✅ `isPronounceable` property (already existed)
- ✅ Handler methods (`handleETen26`, `handleHsu`, etc.) are now private

### 5. PinyinTrie and Chopper

Swift v1.7.0 added new `PinyinTrie` functionality for intelligent pinyin chopping and conversion.

**Status:**
- ✅ `PinyinTrie` class already exists with full functionality
- ✅ `Chop()` method for prefix-based splitting
- ✅ `DeductChoppedPinyinToZhuyin()` for converting pinyin to zhuyin candidates

### 6. Test Structure

Swift v1.7.0 consolidated test files and changed test data to use large strings for in-place parsing.

**Status:**
- ✅ Test data structure already in `Tekkon_TestData.cs` with multi-line string format
- ✅ Existing tests (21 original tests) all pass
- ✅ Added 7 comprehensive tests to verify v1.7.0 features

### 7. Build and Platform

**Changes made:**
- ✅ Updated from .NET 6.0 to .NET 8.0
- ✅ All tests pass (28 total)

### 8. License

**Status:**
- ✅ License already updated to LGPL v3.0 or later
- ✅ License headers in all source files updated

## Test Results

All 28 tests pass:

```
Test Run Successful.
Total tests: 28
     Passed: 28
 Total time: ~0.7s
```

### Test Categories:
- **Basic Tests (TekkonTests.cs)**: 10 tests
- **Advanced Tests (TekkonTests_Advanced.cs)**: 5 tests  
- **Intermediate Tests (TekkonTests_Intermediate.cs)**: 3 tests
- **PinyinTrie Tests (TekkonTests_PinyinTrie.cs)**: 3 tests
- **v1.7.0 Feature Tests (TekkonTests_V170Features.cs)**: 7 tests

## Breaking Changes

As noted in the problem statement, breaking changes are acceptable for this version update:
- Properties are now read-only from outside the assembly
- Some methods that took `String` now require `Rune`
- Public phonabet arrays return `IReadOnlyList<Rune>` instead of `String[]`

Old string-based APIs are still available for backward compatibility where appropriate.

## Commit History

1. **3254af2** - Update to .NET 8.0 target framework
2. **2c2e1b7** - Make Composer properties read-only from outside (public internal set)
3. **25ced2b** - Add comprehensive tests for Swift Tekkon v1.7.0 features
4. **287e29a** - Clean up test code based on code review feedback

## Verification

The following v1.7.0 features have been verified through tests:
- ✅ Unicode.Scalar (Rune) API works correctly
- ✅ Properties are read-only from outside
- ✅ Phonabet constructor with Rune works
- ✅ All allowed phonabet arrays return Runes
- ✅ MandarinParser extensions work correctly
- ✅ isEmpty and isPronounceable properties work
- ✅ PinyinTrie chopping and conversion works

## Conclusion

All changes from Swift Tekkon v1.6.0 → v1.7.0 have been successfully ported to TekkonNT (C#). The codebase is now fully synchronized with Swift v1.7.0.

