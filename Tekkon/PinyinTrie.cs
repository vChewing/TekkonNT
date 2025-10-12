// (c) 2022 and onwards The vChewing Project (LGPL v3.0 License or later).
// ====================
// This code is released under the SPDX-License-Identifier: `LGPL-3.0-or-later`.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tekkon {
  /// <summary>
  /// 用來處理拼音轉注音的字首樹實作。
  /// </summary>
  public sealed class PinyinTrie {
    /// <summary>
    /// 代表字首樹節點。
    /// </summary>
    public sealed class TNode : IEquatable<TNode> {
      /// <summary>
      /// 建立新的節點。
      /// </summary>
      public TNode(int id = 0, IEnumerable<string> entries = null,
                   string character = "", string readingKey = "") {
        Id = id;
        Entries = entries?.ToList() ?? new List<string>();
        Character = character;
        ReadingKey = readingKey;
        Children = new Dictionary<string, int>();
      }

      /// <summary>節點識別碼。</summary>
      public int Id { get; internal set; }
      /// <summary>節點對應的注音詞條集合。</summary>
      public List<string> Entries { get; }
      /// <summary>節點所代表的拼音字元。</summary>
      public string Character { get; internal set; }
      /// <summary>節點對應的完整讀音鍵。</summary>
      public string ReadingKey { get; internal set; }
      /// <summary>子節點索引表。</summary>
      public Dictionary<string, int> Children { get; }

      /// <inheritdoc />
      public bool Equals(TNode? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Character == other.Character && ReadingKey == other.ReadingKey &&
               Entries.SequenceEqual(other.Entries) && Children.OrderBy(kv => kv.Key).SequenceEqual(other.Children.OrderBy(kv => kv.Key));
      }

      /// <inheritdoc />
      public override bool Equals(object? obj) => Equals(obj as TNode);

      /// <inheritdoc />
      public override int GetHashCode() {
        HashCode hash = new HashCode();
        hash.Add(Id);
        hash.Add(Character);
        hash.Add(ReadingKey);
        foreach (string entry in Entries) hash.Add(entry);
        foreach (KeyValuePair<string, int> child in Children.OrderBy(kv => kv.Key)) {
          hash.Add(child.Key);
          hash.Add(child.Value);
        }
        return hash.ToHashCode();
      }
    }

    /// <summary>
    /// 以指定排列建立拼音字首樹。
    /// </summary>
    /// <param name="parser">要使用的拼音排列。</param>
    public PinyinTrie(MandarinParser parser) {
      Parser = parser;
      Root = new TNode(id: 0, character: string.Empty, readingKey: string.Empty);
      Nodes = new Dictionary<int, TNode> { [0] = Root };
      AllPossibleReadings = BuildPossibleReadings();
      IReadOnlyDictionary<string, string>? table = parser.MapZhuyinPinyin();
      if (table == null) return;
      foreach (KeyValuePair<string, string> kvp in table) {
        Insert(kvp.Key, kvp.Value);
      }
    }

    /// <summary>當前所使用的拼音排列。</summary>
    public MandarinParser Parser { get; }
    /// <summary>字首樹根節點。</summary>
    public TNode Root { get; }
    /// <summary>依節點識別碼存取節點的索引表。</summary>
    public Dictionary<int, TNode> Nodes { get; }
    /// <summary>已排序的可能讀音集合。</summary>
    public List<string> AllPossibleReadings { get; private set; }

    /// <summary>
    /// 向字首樹插入新的拼音與注音對應紀錄。
    /// </summary>
    /// <param name="key">拼音字串。</param>
    /// <param name="entry">對應的注音字串。</param>
    public void Insert(string key, string entry) {
      if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(entry)) return;
      TNode currentNode = Root;

      foreach (char ch in key) {
        string symbol = ch.ToString();
        if (currentNode.Children.TryGetValue(symbol, out int childNodeId)) {
          if (Nodes.TryGetValue(childNodeId, out TNode? matchedNode)) {
            currentNode = matchedNode;
            continue;
          }
        }

        int newNodeId = Nodes.Count;
        TNode newNode = new TNode(id: newNodeId, character: symbol);
        currentNode.Children[symbol] = newNodeId;
        Nodes[newNodeId] = newNode;
        currentNode = newNode;
      }

      if (!currentNode.Entries.Contains(entry)) currentNode.Entries.Add(entry);
      currentNode.ReadingKey = key;
    }

    /// <summary>
    /// 搜尋特定拼音前綴所對應的所有注音詞條。
    /// </summary>
    /// <param name="key">拼音前綴。</param>
    /// <returns>找到的注音詞條。</returns>
    public List<string> Search(string key) {
      if (string.IsNullOrEmpty(key)) return new List<string>();
      TNode currentNode = Root;
      foreach (char ch in key) {
        string symbol = ch.ToString();
        if (!currentNode.Children.TryGetValue(symbol, out int childNodeId)) return new List<string>();
        if (!Nodes.TryGetValue(childNodeId, out TNode? childNode)) return new List<string>();
        currentNode = childNode;
      }
      return CollectAllDescendantEntries(currentNode);
    }

    /// <summary>
    /// 將切分後的拼音序列推斷為注音候選。
    /// </summary>
    /// <param name="chopped">切分後的拼音序列。</param>
    /// <param name="chopCaseSeparator">多個候選的分隔符號。</param>
    /// <param name="initialZhuyinOnly">是否僅保留注音開頭部分。</param>
    /// <returns>對應的注音候選序列。</returns>
    public List<string> DeductChoppedPinyinToZhuyin(IReadOnlyList<string> chopped,
                                                    char chopCaseSeparator = '&',
                                                    bool initialZhuyinOnly = true) {
      if (!Parser.IsPinyin()) return chopped?.ToList() ?? new List<string>();
      List<string> result = new List<string>();
      if (chopped == null || chopped.Count == 0) return result;

      foreach (string slice in chopped) {
        List<string> fetched = Search(slice);
        if (fetched.Count == 0) {
          result.Add(slice);
          continue;
        }

        if (fetched.Count == 1) {
          result.Add(fetched[0]);
          continue;
        }

        List<string> uniqueFetched = fetched.Distinct().OrderBy(s => s, StringComparer.Ordinal).ToList();
        if (initialZhuyinOnly) {
          for (int prefixLength = 3; prefixLength >= 1; prefixLength--) {
            if (uniqueFetched.Count <= prefixLength) break;
            uniqueFetched = fetched.Select(item => item.Substring(0, Math.Min(prefixLength, item.Length)))
                                   .Distinct()
                                   .OrderBy(s => s, StringComparer.Ordinal)
                                   .ToList();
          }
        }

        result.Add(string.Join(chopCaseSeparator, uniqueFetched));
      }

      return result;
    }

    /// <summary>
    /// 將連續拼音字串切割成合理的前綴片段。
    /// </summary>
    /// <param name="readingComplex">連續拼音字串。</param>
    /// <returns>切割後的前綴片段。</returns>
    public List<string> Chop(string readingComplex) {
      List<string> result = new List<string>();
      if (string.IsNullOrEmpty(readingComplex)) return result;

      char[] buffer = readingComplex.ToCharArray();
      int complexLength = buffer.Length;
      if (AllPossibleReadings.Count == 0) return readingComplex.Select(c => c.ToString()).ToList();
      int longestReadingLength = AllPossibleReadings[0].Length;
      int maxScopeSize = Math.Min(complexLength, longestReadingLength);
      int currentPosition = 0;

      while (currentPosition < complexLength) {
        bool foundMatch = false;
        int remaining = complexLength - currentPosition;
        int longPossibleScopeSize = Math.Min(maxScopeSize, remaining);

        for (int scopeSize = longPossibleScopeSize; scopeSize >= 1; scopeSize--) {
          string currentBlob = new string(buffer, currentPosition, scopeSize);
          if (AllPossibleReadings.Any(reading => reading.StartsWith(currentBlob, StringComparison.Ordinal))) {
            result.Add(currentBlob);
            currentPosition += scopeSize;
            foundMatch = true;
            break;
          }
        }

        if (!foundMatch) {
          result.Add(new string(buffer, currentPosition, 1));
          currentPosition += 1;
        }
      }

      return result;
    }

    private List<string> CollectAllDescendantEntries(TNode node) {
      List<string> result = new List<string>(node.Entries);
      foreach (int childNodeId in node.Children.Values) {
        if (!Nodes.TryGetValue(childNodeId, out TNode? childNode)) continue;
        result.AddRange(CollectAllDescendantEntries(childNode));
      }
      return result;
    }

    private List<string> BuildPossibleReadings() {
      IEnumerable<string> readings = Parser.AllPossibleReadings();
      return readings.OrderByDescending(s => s.Length).ThenBy(s => s, StringComparer.Ordinal).ToList();
    }
  }
}
