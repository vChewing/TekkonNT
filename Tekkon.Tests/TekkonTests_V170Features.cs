// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Text;
using NUnit.Framework;

namespace Tekkon.Tests {
  /// <summary>
  /// Tests to verify Swift Tekkon v1.7.0 features are properly ported to C#.
  /// </summary>
  public class TekkonTests_V170Features {
    [Test]
    public void TestUnicodeScalarAPI() {
      // v1.7.0 changed to use Unicode.Scalar (Rune in C#) instead of String
      var composer = new Composer();
      
      // Test ReceiveKey with Rune
      composer.ReceiveKey(new Rune('1'));
      composer.ReceiveKey(new Rune('j'));
      composer.ReceiveKey(new Rune(' '));
      Assert.AreEqual("ㄅㄨ ", composer.Value);
      
      // Test ReceiveKeyFromPhonabet with Rune
      composer.Clear();
      composer.ReceiveKeyFromPhonabet(new Rune('ㄉ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄧ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ㄠ'));
      composer.ReceiveKeyFromPhonabet(new Rune('ˇ'));
      Assert.AreEqual("ㄉㄧㄠˇ", composer.Value);
    }

    [Test]
    public void TestReadOnlyProperties() {
      // v1.7.0 made properties public internal(set) - read-only from outside
      var composer = new Composer();
      composer.ReceiveKey("1");
      
      // These should be readable
      Assert.IsNotNull(composer.Consonant);
      Assert.IsNotNull(composer.Semivowel);
      Assert.IsNotNull(composer.Vowel);
      Assert.IsNotNull(composer.Intonation);
      Assert.IsNotNull(composer.RomajiBuffer);
      Assert.AreEqual(MandarinParser.OfDachen, composer.Parser);
      
      // Properties cannot be set from outside (compilation would fail)
      // composer.Consonant = new Phonabet(); // This would not compile
    }

    [Test]
    public void TestPhonabetWithRune() {
      // v1.7.0 Phonabet now has init(_ input: Unicode.Scalar)
      var phonabet1 = new Phonabet(new Rune('ㄉ'));
      Assert.AreEqual(PhoneType.Consonant, phonabet1.Type);
      Assert.AreEqual(new Rune('ㄉ'), phonabet1.ScalarValue);
      
      var phonabet2 = new Phonabet(new Rune('ㄧ'));
      Assert.AreEqual(PhoneType.Semivowel, phonabet2.Type);
      
      var phonabet3 = new Phonabet(new Rune('ㄠ'));
      Assert.AreEqual(PhoneType.Vowel, phonabet3.Type);
      
      var phonabet4 = new Phonabet(new Rune('ˇ'));
      Assert.AreEqual(PhoneType.Intonation, phonabet4.Type);
    }

    [Test]
    public void TestAllowedPhonabetsUseRune() {
      // v1.7.0 changed allowedConsonants etc. to [Unicode.Scalar]
      Assert.IsNotEmpty(Phonabet.AllowedConsonants);
      Assert.IsNotEmpty(Phonabet.AllowedSemivowels);
      Assert.IsNotEmpty(Phonabet.AllowedVowels);
      Assert.IsNotEmpty(Phonabet.AllowedIntonations);
      Assert.IsNotEmpty(Phonabet.AllowedPhonabets);
      
      // Verify they contain Runes
      Assert.IsTrue(System.Linq.Enumerable.Contains(Phonabet.AllowedConsonants, new Rune('ㄅ')));
      Assert.IsTrue(System.Linq.Enumerable.Contains(Phonabet.AllowedSemivowels, new Rune('ㄧ')));
      Assert.IsTrue(System.Linq.Enumerable.Contains(Phonabet.AllowedVowels, new Rune('ㄚ')));
      Assert.IsTrue(System.Linq.Enumerable.Contains(Phonabet.AllowedIntonations, new Rune('ˊ')));
    }

    [Test]
    public void TestMandarinParserExtensions() {
      // v1.7.0 added AllStaticZhuyinCases, AllDynamicZhuyinCases, AllPinyinCases
      Assert.IsNotEmpty(MandarinParserExtensions.AllStaticZhuyinCases);
      Assert.IsNotEmpty(MandarinParserExtensions.AllDynamicZhuyinCases);
      Assert.IsNotEmpty(MandarinParserExtensions.AllPinyinCases);
      
      // Verify classifications
      Assert.IsTrue(MandarinParser.OfDachen26.IsDynamic());
      Assert.IsFalse(MandarinParser.OfDachen.IsDynamic());
      Assert.IsTrue(MandarinParser.OfHanyuPinyin.IsPinyin());
      Assert.IsFalse(MandarinParser.OfDachen.IsPinyin());
    }

    [Test]
    public void TestIsEmptyAndIsPronounceable() {
      // v1.7.0 added isEmpty and isPronounceable properties
      var composer = new Composer();
      Assert.IsTrue(composer.IsEmpty);
      Assert.IsFalse(composer.IsPronounceable);
      
      composer.ReceiveKey("1");  // ㄅ
      Assert.IsFalse(composer.IsEmpty);
      Assert.IsTrue(composer.IsPronounceable);
      
      composer.Clear();
      composer.ReceiveKey("3");  // ˇ (just tone)
      Assert.IsFalse(composer.IsEmpty);
      Assert.IsFalse(composer.IsPronounceable);
    }

    [Test]
    public void TestPinyinTrie() {
      // v1.7.0 added PinyinTrie for pinyin chopping
      var trie = new PinyinTrie(MandarinParser.OfHanyuPinyin);
      
      var chopped = trie.Chop("women");
      Assert.Contains("wo", chopped);
      Assert.Contains("men", chopped);
      
      var candidates = trie.DeductChoppedPinyinToZhuyin(chopped);
      Assert.IsNotEmpty(candidates);
    }
  }
}
