///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    /// Enum for NFA types.
    /// </summary>
    public enum RowRecogNFATypeEnum
    {
        /// <summary>
        /// For single multiplicity.
        /// </summary>
        SINGLE,

        /// <summary>
        /// For greedy '*' multiplicity.
        /// </summary>
        ZERO_TO_MANY,

        /// <summary>
        /// For greedy '+' multiplicity.
        /// </summary>
        ONE_TO_MANY,

        /// <summary>
        /// For greedy '?' multiplicity.
        /// </summary>
        ONE_OPTIONAL,

        /// <summary>
        /// For reluctant '*' multiplicity.
        /// </summary>
        ZERO_TO_MANY_RELUCTANT,

        /// <summary>
        /// For reluctant '+' multiplicity.
        /// </summary>
        ONE_TO_MANY_RELUCTANT,

        /// <summary>
        /// For reluctant '?' multiplicity.
        /// </summary>
        ONE_OPTIONAL_RELUCTANT
    }

    public static class RowRecogNFATypeEnumExtensions
    {
        /// <summary>
        /// Returns indicator if single or multiple matches.
        /// </summary>
        /// <returns>indicator</returns>
        public static bool IsMultipleMatches(this RowRecogNFATypeEnum value)
        {
            switch (value) {
                case RowRecogNFATypeEnum.SINGLE:
                    return false;

                case RowRecogNFATypeEnum.ZERO_TO_MANY:
                    return true;

                case RowRecogNFATypeEnum.ONE_TO_MANY:
                    return true;

                case RowRecogNFATypeEnum.ONE_OPTIONAL:
                    return false;

                case RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return true;

                case RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return true;

                case RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return false;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Returns indicator if optional matches.
        /// </summary>
        /// <returns>indicator</returns>
        public static bool IsOptional(this RowRecogNFATypeEnum value)
        {
            switch (value) {
                case RowRecogNFATypeEnum.SINGLE:
                    return false;

                case RowRecogNFATypeEnum.ZERO_TO_MANY:
                    return true;

                case RowRecogNFATypeEnum.ONE_TO_MANY:
                    return false;

                case RowRecogNFATypeEnum.ONE_OPTIONAL:
                    return true;

                case RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return true;

                case RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return false;

                case RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return true;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Returns indicator if greedy or reluctant.
        /// </summary>
        /// <returns>indicator</returns>
        public static bool? IsGreedy(this RowRecogNFATypeEnum value)
        {
            switch (value) {
                case RowRecogNFATypeEnum.SINGLE:
                    return null;

                case RowRecogNFATypeEnum.ZERO_TO_MANY:
                    return true;

                case RowRecogNFATypeEnum.ONE_TO_MANY:
                    return true;

                case RowRecogNFATypeEnum.ONE_OPTIONAL:
                    return true;

                case RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return false;

                case RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return false;

                case RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return false;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Return postfix.
        /// </summary>
        /// <returns>postfix</returns>
        public static string GetOptionalPostfix(this RowRecogNFATypeEnum value)
        {
            switch (value) {
                case RowRecogNFATypeEnum.SINGLE:
                    return "";

                case RowRecogNFATypeEnum.ZERO_TO_MANY:
                    return "*";

                case RowRecogNFATypeEnum.ONE_TO_MANY:
                    return "+";

                case RowRecogNFATypeEnum.ONE_OPTIONAL:
                    return "?";

                case RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT:
                    return "*?";

                case RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT:
                    return "+?";

                case RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT:
                    return "??";
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Inspect code and return enum for code.
        /// </summary>
        /// <param name="code">to inspect</param>
        /// <param name="reluctantQuestion">null for greedy or questionmark for reluctant</param>
        /// <returns>enum</returns>
        public static RowRecogNFATypeEnum FromString(
            string code,
            string reluctantQuestion)
        {
            bool reluctant = false;
            if (reluctantQuestion != null) {
                if (!reluctantQuestion.Equals("?")) {
                    throw new ArgumentException(
                        "Invalid code for pattern type: " + code + " reluctant '" + reluctantQuestion + "'");
                }

                reluctant = true;
            }

            switch (code) {
                case null:
                    return RowRecogNFATypeEnum.SINGLE;

                case "*":
                    return reluctant ? RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT : RowRecogNFATypeEnum.ZERO_TO_MANY;

                case "+":
                    return reluctant ? RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT : RowRecogNFATypeEnum.ONE_TO_MANY;

                case "?":
                    return reluctant ? RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT : RowRecogNFATypeEnum.ONE_OPTIONAL;
            }

            throw new ArgumentException("Invalid code for pattern type: " + code);
        }
    }
} // end of namespace