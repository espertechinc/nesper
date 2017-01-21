///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Enum for NFA types.
    /// </summary>
    public enum RegexNFATypeEnum
    {
        /// <summary>
        /// For greedy '?' multiplicity.
        /// </summary>
        ONE_OPTIONAL,

        /// <summary>
        /// For reluctant '?' multiplicity.
        /// </summary>
        ONE_OPTIONAL_RELUCTANT,

        /// <summary>
        /// For greedy '+' multiplicity.
        /// </summary>
        ONE_TO_MANY,

        /// <summary>
        /// For reluctant '+' multiplicity.
        /// </summary>
        ONE_TO_MANY_RELUCTANT,

        /// <summary>
        /// For single multiplicity.
        /// </summary>
        SINGLE,

        /// <summary>
        /// For greedy '*' multiplicity.
        /// </summary>
        ZERO_TO_MANY,

        /// <summary>
        /// For reluctant '*' multiplicity.
        /// </summary>
        ZERO_TO_MANY_RELUCTANT
    }

    public static class RegexNFATypeEnumExtensions
    {
        /// <summary>
        /// Returns indicator if single or multiple matches.
        /// </summary>
        /// <returns>
        /// indicator
        /// </returns>
        public static bool IsMultipleMatches(this RegexNFATypeEnum value)
        {
            switch (value)
            {
                case RegexNFATypeEnum.ONE_OPTIONAL:
                    return false;
                case RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return false;
                case RegexNFATypeEnum.ONE_TO_MANY:
                    return true;
                case RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return true;
                case RegexNFATypeEnum.SINGLE:
                    return false;
                case RegexNFATypeEnum.ZERO_TO_MANY:
                    return true;
                case RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return true;
            }
         
            throw new ArgumentException();
        }

        /// <summary>
        /// Returns indicator if optional matches.
        /// </summary>
        /// <returns>
        /// indicator
        /// </returns>
        public static bool IsOptional(this RegexNFATypeEnum value)
        {
            switch (value)
            {
                case RegexNFATypeEnum.ONE_OPTIONAL:
                    return true;
                case RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return true;
                case RegexNFATypeEnum.ONE_TO_MANY:
                    return false;
                case RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return false;
                case RegexNFATypeEnum.SINGLE:
                    return false;
                case RegexNFATypeEnum.ZERO_TO_MANY:
                    return true;
                case RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return true;
            }
         
            throw new ArgumentException();
        }

        /// <summary>
        /// Returns indicator if greedy or reluctant.
        /// </summary>
        /// <returns>
        /// indicator
        /// </returns>
        public static bool? IsGreedy(this RegexNFATypeEnum value)
        {
            switch (value)
            {
                case RegexNFATypeEnum.ONE_OPTIONAL:
                    return true;
                case RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return false;
                case RegexNFATypeEnum.ONE_TO_MANY:
                    return true;
                case RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return false;
                case RegexNFATypeEnum.SINGLE:
                    return null;
                case RegexNFATypeEnum.ZERO_TO_MANY:
                    return true;
                case RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return false;
            }
         
            throw new ArgumentException();
        }

        /// <summary>
        /// Return postfix.
        /// </summary>
        /// <returns>
        /// postfix
        /// </returns>
        public static string OptionalPostfix(this RegexNFATypeEnum value)
        {
            switch (value)
            {
                case RegexNFATypeEnum.ONE_OPTIONAL:
                    return "?";
                case RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return "??";
                case RegexNFATypeEnum.ONE_TO_MANY:
                    return "+";
                case RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return "+?";
                case RegexNFATypeEnum.SINGLE:
                    return "";
                case RegexNFATypeEnum.ZERO_TO_MANY:
                    return "*";
                case RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return "*?";
            }
         
            throw new ArgumentException("Invalid pattern type: " + value);
        }

        /// <summary>
        /// Inspect code and return enum for code.
        /// </summary>
        /// <param name="code">to inspect</param>
        /// <param name="reluctantQuestion">null for greedy or questionmark for reluctant</param>
        /// <returns>
        /// enum
        /// </returns>
        public static RegexNFATypeEnum FromString(String code, String reluctantQuestion)
        {
            bool reluctant = false;
            if (reluctantQuestion != null)
            {
                if (!reluctantQuestion.Equals("?"))
                {
                    throw new ArgumentException("Invalid code for pattern type: " + code + " reluctant '" +
                                                reluctantQuestion + "'");
                }
                reluctant = true;
            }

            if (code == null)
            {
                return RegexNFATypeEnum.SINGLE;
            }
            if (code.Equals("*"))
            {
                return reluctant ? RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT : RegexNFATypeEnum.ZERO_TO_MANY;
            }
            if (code.Equals("+"))
            {
                return reluctant ? RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT : RegexNFATypeEnum.ONE_TO_MANY;
            }
            if (code.Equals("?"))
            {
                return reluctant ? RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT : RegexNFATypeEnum.ONE_OPTIONAL;
            }
            throw new ArgumentException("Invalid code for pattern type: " + code);
        }
    }
}
