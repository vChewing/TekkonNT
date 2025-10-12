#if NET46
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Text {
  /// <summary>
  /// Minimal implementation of <see cref="Rune"/> sufficient for Tekkon on .NET Framework 4.6.
  /// </summary>
  public readonly struct Rune : IEquatable<Rune> {
    private readonly int value;

    public Rune(char ch) {
      value = ch;
    }

    public Rune(int scalarValue) {
      if (scalarValue < 0 || scalarValue > 0x10FFFF)
        throw new ArgumentOutOfRangeException(nameof(scalarValue));
      value = scalarValue;
    }

    public int Value => value;

    public override string ToString() { return char.ConvertFromUtf32(value); }

    public override int GetHashCode() { return value; }

    public override bool Equals(object obj) { return obj is Rune other && Equals(other); }

    public bool Equals(Rune other) { return value == other.value; }

    public static bool operator ==(Rune left, Rune right) { return left.value == right.value; }

    public static bool operator !=(Rune left, Rune right) { return left.value != right.value; }

    public static bool TryGetRuneAt(string input, int index, out Rune result) {
      if (input == null) throw new ArgumentNullException(nameof(input));
      if (index < 0 || index >= input.Length) {
        result = default(Rune);
        return false;
      }
      char first = input[index];
      if (char.IsHighSurrogate(first)) {
        if (index + 1 < input.Length) {
          char second = input[index + 1];
          if (char.IsLowSurrogate(second)) {
            result = new Rune(char.ConvertToUtf32(first, second));
            return true;
          }
        }
        result = default(Rune);
        return false;
      }
      if (char.IsLowSurrogate(first)) {
        result = default(Rune);
        return false;
      }
      result = new Rune(first);
      return true;
    }

    public string ToString(IFormatProvider _) { return ToString(); }
  }

  internal static class RuneStringExtensions {
    public static IEnumerable<Rune> EnumerateRunes(this string text) {
      if (text == null) throw new ArgumentNullException(nameof(text));
      for (int i = 0; i < text.Length; i++) {
        char current = text[i];
        if (char.IsHighSurrogate(current) && i + 1 < text.Length) {
          char next = text[i + 1];
          if (char.IsLowSurrogate(next)) {
            yield return new Rune(char.ConvertToUtf32(current, next));
            i++;
            continue;
          }
        }
        if (char.IsLowSurrogate(current)) continue;
        yield return new Rune(current);
      }
    }
  }
}

#endif
#if !(NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System {
  /// <summary>
  /// 針對舊版 .NET 平台提供的 <see cref="HashCode"/> Polyfill。
  /// </summary>
  public struct HashCode {
    private const int Seed = 17;
    private const int Factor = 31;

    private int _value;
    private bool _initialized;

    /// <summary>
    /// 加入新的哈希值來源。
    /// </summary>
    /// <typeparam name="T">來源型別。</typeparam>
    /// <param name="value">來源內容。</param>
    public void Add<T>(T value) {
      Add(value, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// 加入新的哈希值來源。
    /// </summary>
    /// <typeparam name="T">來源型別。</typeparam>
    /// <param name="value">來源內容。</param>
    /// <param name="comparer">自訂比較子。</param>
    public void Add<T>(T value, IEqualityComparer<T>? comparer) {
      int next = value is null ? 0 : (comparer ?? EqualityComparer<T>.Default).GetHashCode(value);
      unchecked {
        if (!_initialized) {
          _value = Seed + next;
          _initialized = true;
        } else {
          _value = _value * Factor + next;
        }
      }
    }

    /// <summary>
    /// 輸出最終哈希值。
    /// </summary>
    /// <returns>整數哈希值。</returns>
    public int ToHashCode() => _initialized ? _value : 0;

    /// <summary>
    /// Mimic the BCL helper for combining two values into a hash code.
    /// </summary>
    public static int Combine<T1, T2>(T1 value1, T2 value2) {
      HashCode hash = new HashCode();
      hash.Add(value1);
      hash.Add(value2);
      return hash.ToHashCode();
    }
  }
}
#endif
