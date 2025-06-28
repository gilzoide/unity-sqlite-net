/*
 * Copyright (c) 2025 Gil Barbosa Reis
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SQLite.Csv
{
    public static class CsvReader
    {
        public enum SeparatorChar
        {
            Comma,
            Semicolon,
            Tabs,
        }

        /// <summary>
        /// Parse a stream of CSV-formatted data.
        /// </summary>
        /// <param name="stream">Stream of CSV-formatted data.</param>
        /// <param name="separator">Character used for separating fields.</param>
        /// <param name="maxFieldSize">Maximum field size allowed.</param>
        /// <returns>
        /// The enumeration returns each field's contents, even if it is empty.
        /// <see langword="null"/> is returned at the end of the lines, meaning a new row will start next.
        /// Empty lines are ignored and will not be enumerated.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> is null.</exception>
        /// <exception cref="CsvException">Thrown if any field size is greater than <paramref name="maxFieldSize"/>.</exception>
        public static IEnumerable<string> ParseStream(TextReader stream, SeparatorChar separator = SeparatorChar.Comma, int maxFieldSize = int.MaxValue)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            SkipEmptyLines(stream);
            if (stream.Peek() < 0)
            {
                yield break;
            }

            bool insideQuotes = false;
            var stringBuilder = new StringBuilder();
            while (true)
            {
                int c = stream.Read();
                switch (c)
                {
                    case '\r':
                        if (!insideQuotes && stream.Peek() == '\n')
                        {
                            stream.Read();
                            goto case '\n';
                        }
                        else
                        {
                            goto default;
                        }

                    case '\n':
                        if (!insideQuotes)
                        {
                            yield return stringBuilder.ToString();
                            stringBuilder.Clear();
                            yield return null;

                            SkipEmptyLines(stream);
                            if (stream.Peek() < 0)
                            {
                                yield break;
                            }
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case ',':
                        if (!insideQuotes && separator == SeparatorChar.Comma)
                        {
                            yield return stringBuilder.ToString();
                            stringBuilder.Clear();
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case ';':
                        if (!insideQuotes && separator == SeparatorChar.Semicolon)
                        {
                            yield return stringBuilder.ToString();
                            stringBuilder.Clear();
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case '\t':
                        if (!insideQuotes && separator == SeparatorChar.Tabs)
                        {
                            yield return stringBuilder.ToString();
                            stringBuilder.Clear();
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case '"':
                        if (insideQuotes && stream.Peek() == '"')
                        {
                            stream.Read();
                            goto default;
                        }
                        else
                        {
                            insideQuotes = !insideQuotes;
                        }
                        break;

                    case < 0:
                        yield return stringBuilder.ToString();
                        yield return null;
                        yield break;

                    default:
                        if (stringBuilder.Length >= maxFieldSize)
                        {
                            throw new CsvException("Field size is greater than maximum allowed size.");
                        }
                        stringBuilder.Append((char) c);
                        break;
                }
            }
        }

        private static void SkipEmptyLines(TextReader streamReader)
        {
            while (true)
            {
                int c = streamReader.Peek();
                switch (c)
                {
                    case '\n':
                    case '\r':
                        streamReader.Read();
                        continue;

                    default:
                        return;
                }
            }
        }
    }
}
