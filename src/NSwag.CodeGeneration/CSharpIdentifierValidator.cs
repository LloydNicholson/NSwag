﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NSwag.CodeGeneration
{
    /// <summary>
    /// class to help produce valid C# identifiers
    /// </summary>
    public static class CSharpIdentifierValidator
    {
        /// <summary>
        /// remove invalid characters from C# identifier per spec
        /// </summary>
        /// <param name="identifier">identifier to be cleaned</param>
        /// <returns>identifier with invalid characters removed</returns>
        public static string RemoveInvalidIdentifierCharacters(string identifier)
        {
            // based on https://github.com/dotnet/roslyn/blob/e123a4207123d5df8c45fb1caba0fb3acbae0e00/src/Compilers/Core/Portable/InternalUtilities/UnicodeCharacterUtilities.cs#L88-L110
            if (string.IsNullOrEmpty(identifier))
            {
                return identifier;
            }

            // common path is the string is already valid, so exit for that
            if (IsValidIdentifier(identifier))
            {
                return identifier;
            }

            var characters = identifier.ToCharArray().ToList();

            // TODO: alternatively, add a valid prefix like an underscore instead of stripping at start?
            while (characters.Any() && !IsIdentifierStartCharacter(characters[0]))
            {
                characters.RemoveAt(0);
            }

            // now that we know the first character is valid (or the identifier is empty), we can remove any other invalid characters
            characters.RemoveAll(c => !IsIdentifierPartCharacter(c));

            var newIdentifier = new string(characters.ToArray());
            return newIdentifier;
        }

        /// <summary>
        /// Returns true if the Unicode character can be the first character of an identifier.
        /// </summary>
        /// <param name="ch">character to check</param>
        /// <returns>whether it is a valid first character for an identifier</returns>
        public static bool IsIdentifierStartCharacter(char ch)
        {
            // identifier-start-character:
            //   letter-character
            //   _ (the underscore character U+005F)

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return false;
                }

                return ch <= 'Z'  // '\u005A'
                    || ch == '_'; // '\u005F'
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            return IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
        }

        /// <summary>
        /// Returns true if the Unicode character can be a part of an identifier.
        /// </summary>
        /// <param name="ch">The Unicode character.</param>
        public static bool IsIdentifierPartCharacter(char ch)
        {
            // identifier-part-character:
            //   letter-character
            //   decimal-digit-character
            //   connecting-character
            //   combining-character
            //   formatting-character

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return ch >= '0'  // '\u0030'
                        && ch <= '9'; // '\u0039'
                }

                return ch <= 'Z'  // '\u005A'
                    || ch == '_'; // '\u005F'
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            return IsLetterChar(cat)
                || IsDecimalDigitChar(cat)
                || IsConnectingChar(cat)
                || IsCombiningChar(cat)
                || IsFormattingChar(cat);
        }

        /// <summary>
        /// Check that the name is a valid Unicode identifier.
        /// </summary>
        public static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (!IsIdentifierStartCharacter(name[0]))
            {
                return false;
            }

            int nameLength = name.Length;
            for (int i = 1; i < nameLength; i++) //NB: start at 1
            {
                if (!IsIdentifierPartCharacter(name[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLetterChar(UnicodeCategory cat)
        {
            // letter-character:
            //   A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl
            //   A Unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl

            switch (cat)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
            }

            return false;
        }

        private static bool IsCombiningChar(UnicodeCategory cat)
        {
            // combining-character:
            //   A Unicode character of classes Mn or Mc
            //   A Unicode-escape-sequence representing a character of classes Mn or Mc

            switch (cat)
            {
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                    return true;
            }

            return false;
        }

        private static bool IsDecimalDigitChar(UnicodeCategory cat)
        {
            // decimal-digit-character:
            //   A Unicode character of the class Nd
            //   A unicode-escape-sequence representing a character of the class Nd

            return cat == UnicodeCategory.DecimalDigitNumber;
        }

        private static bool IsConnectingChar(UnicodeCategory cat)
        {
            // connecting-character:
            //   A Unicode character of the class Pc
            //   A unicode-escape-sequence representing a character of the class Pc

            return cat == UnicodeCategory.ConnectorPunctuation;
        }

        /// <summary>
        /// Returns true if the Unicode character is a formatting character (Unicode class Cf).
        /// </summary>
        /// <param name="ch">The Unicode character.</param>
        internal static bool IsFormattingChar(char ch)
        {
            // There are no FormattingChars in ASCII range

            return ch > 127 && IsFormattingChar(CharUnicodeInfo.GetUnicodeCategory(ch));
        }

        /// <summary>
        /// Returns true if the Unicode character is a formatting character (Unicode class Cf).
        /// </summary>
        /// <param name="cat">The Unicode character.</param>
        private static bool IsFormattingChar(UnicodeCategory cat)
        {
            // formatting-character:
            //   A Unicode character of the class Cf
            //   A unicode-escape-sequence representing a character of the class Cf

            return cat == UnicodeCategory.Format;
        }

    }
}
