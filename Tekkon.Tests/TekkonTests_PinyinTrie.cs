// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Tekkon.Tests {
  public class TekkonTestsPinyinTrie {
    [Test]
    public void PinyinTrieSearchReturnsExpectedEntries() {
      PinyinTrie trie = new PinyinTrie(MandarinParser.OfHanyuPinyin);
      List<string> results = trie.Search("shi");
      Assert.IsNotNull(results);
      Assert.IsTrue(results.Any(item => item.Contains("ㄕ")));
      List<string> missing = trie.Search("xyz");
      Assert.IsNotNull(missing);
      Assert.IsEmpty(missing);
    }

    [Test]
    public void PinyinTrieDeductChoppedPinyinToZhuyinProducesCandidates() {
      PinyinTrie trie = new PinyinTrie(MandarinParser.OfHanyuPinyin);
      List<string> chopped = new List<string> { "shi", "jie", "da", "zhan" };
      List<string> zhuyin = trie.DeductChoppedPinyinToZhuyin(chopped, initialZhuyinOnly: false);
      Assert.AreEqual(chopped.Count, zhuyin.Count);
      Assert.IsTrue(zhuyin[0].Contains("ㄕ"));
      Assert.IsTrue(zhuyin[1].Contains("ㄐㄧ"));
      Assert.IsTrue(zhuyin[2].Contains("ㄉ"));
      Assert.IsTrue(zhuyin[3].Contains("ㄓ"));
    }

    [Test]
    public void PinyinTrieChopSplitsSimplifiedSpelling() {
      PinyinTrie trie = new PinyinTrie(MandarinParser.OfHanyuPinyin);
      List<string> segments = trie.Chop("shjdaz");
      CollectionAssert.AreEqual(new[] { "sh", "j", "da", "z" }, segments);
    }
  }
}
