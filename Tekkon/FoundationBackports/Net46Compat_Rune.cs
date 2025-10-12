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

    /// <summary>
    /// Initializes a new instance of the <see cref="Rune"/> struct from a BMP character.
    /// </summary>
    /// <param name="ch">The character to wrap.</param>
    public Rune(char ch) {
      value = ch;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rune"/> struct from a Unicode scalar value.
    /// </summary>
    /// <param name="scalarValue">The Unicode scalar value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside the valid Unicode range.</exception>
    public Rune(int scalarValue) {
      if (scalarValue < 0 || scalarValue > 0x10FFFF)
        throw new ArgumentOutOfRangeException(nameof(scalarValue));
      value = scalarValue;
    }

    /// <summary>
    /// Gets the underlying Unicode scalar value represented by the rune.
    /// </summary>
    public int Value => value;

    /// <summary>
    /// Converts the rune to its string representation.
    /// </summary>
    /// <returns>A string containing the rune.</returns>
    public override string ToString() { return char.ConvertFromUtf32(value); }

    /// <summary>
    /// Returns the hash code for this rune.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() { return value; }

    /// <summary>
    /// Determines whether the specified object is equal to the current rune.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> if the objects represent the same rune; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) { return obj is Rune other && Equals(other); }

    /// <summary>
    /// Determines whether another rune represents the same Unicode scalar value.
    /// </summary>
    /// <param name="other">The rune to compare.</param>
    /// <returns><c>true</c> if the runes are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Rune other) { return value == other.value; }

    /// <summary>
    /// Determines whether two runes are equal.
    /// </summary>
    /// <param name="left">The first rune.</param>
    /// <param name="right">The second rune.</param>
    /// <returns><c>true</c> if the runes are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Rune left, Rune right) { return left.value == right.value; }

    /// <summary>
    /// Determines whether two runes are not equal.
    /// </summary>
    /// <param name="left">The first rune.</param>
    /// <param name="right">The second rune.</param>
    /// <returns><c>true</c> if the runes are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Rune left, Rune right) { return left.value != right.value; }

    /// <summary>
    /// Attempts to obtain a rune at the specified index of a string.
    /// </summary>
    /// <param name="input">The string containing the rune.</param>
    /// <param name="index">The index of the rune.</param>
    /// <param name="result">When this method returns, contains the rune if the operation succeeded; otherwise, the default rune.</param>
    /// <returns><c>true</c> if a rune was read; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Returns the string representation using the specified format provider.
    /// </summary>
    /// <param name="_">The format provider (ignored).</param>
    /// <returns>A string containing the rune.</returns>
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
