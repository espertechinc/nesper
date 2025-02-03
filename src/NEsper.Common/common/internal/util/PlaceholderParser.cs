///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Parser for strings with substitution parameters of the form ${parameter}.
    /// </summary>
    public class PlaceholderParser
    {
        /// <summary>
        ///     Parses a string to find placeholders of format ${placeholder}.
        ///     <para>
        ///         Example: "My ${thing} is ${color}"
        ///     </para>
        ///     <para>
        ///         The example above parses into 4 fragements: a text fragment of value "My ",
        ///         a parameter fragment "thing", a text fragement " is " and a parameter
        ///         fragment "color".
        ///     </para>
        /// </summary>
        /// <param name="parseString">
        ///     is the string to parse
        /// </param>
        /// <returns>
        ///     list of fragments that can be either text fragments or placeholder fragments
        /// </returns>
        /// <throws>  PlaceholderParseException if the string cannot be parsed to indicate syntax errors </throws>
        public static IList<Fragment> ParsePlaceholder(string parseString)
        {
            var result = new List<Fragment>();
            var currOutputIndex = 0;
            var currSearchIndex = 0;

            while (true) {
                if (currSearchIndex == parseString.Length) {
                    break;
                }

                var startIndex = parseString.IndexOf("${", currSearchIndex);
                if (startIndex == -1) {
                    // no more parameters, add any remainder of string
                    if (currOutputIndex < parseString.Length) {
                        var endString = parseString.Substring(currOutputIndex);
                        var textFragment = new TextFragment(endString);
                        result.Add(textFragment);
                    }

                    break;
                }

                // add text so far
                if (startIndex > 0) {
                    var textSoFar = parseString.Substring(currOutputIndex, startIndex - currOutputIndex);
                    if (textSoFar.Length != 0) {
                        result.Add(new TextFragment(textSoFar));
                    }
                }

                // check if the parameter is escaped
                if (startIndex > 0 && parseString[startIndex - 1] == '$') {
                    currOutputIndex = startIndex + 1;
                    currSearchIndex = startIndex + 1;
                    continue;
                }

                var endIndex = parseString.IndexOf("}", startIndex);
                if (endIndex == -1) {
                    throw new PlaceholderParseException(
                        $"Syntax error in property or variable: '{parseString.Substring(startIndex)}'");
                }

                // add placeholder
                var between = parseString.Substring(startIndex + 2, endIndex - startIndex - 2);
                var parameterFragment = new ParameterFragment(between);
                result.Add(parameterFragment);
                currOutputIndex = endIndex + 1;
                currSearchIndex = endIndex;
            }

            // Combine adjacent text fragments
            var fragments = new LinkedList<Fragment>();
            fragments.AddLast(result[0]);
            for (var i = 1; i < result.Count; i++) {
                var fragment = result[i];
                if (!(result[i] is TextFragment)) {
                    fragments.AddLast(fragment);
                    continue;
                }

                if (!(fragments.Last.Value is TextFragment textFragment)) {
                    fragments.AddLast(fragment);
                    continue;
                }

                fragments.RemoveLast();
                fragments.AddLast(new TextFragment(textFragment.Value + fragment.Value));
            }

            return new List<Fragment>(fragments);
        }

        /// <summary>
        ///     Fragment is a parse result, a parse results in an ordered list of fragments.
        /// </summary>
        public abstract class Fragment
        {
            private readonly string value;

            /// <summary> Ctor.</summary>
            /// <param name="value">
            ///     is the fragment text
            /// </param>
            internal Fragment(string value)
            {
                this.value = value;
            }

            /// <summary> Returns the string text of the fragment.</summary>
            /// <returns>
            ///     fragment string
            /// </returns>
            public virtual string Value => value;

            /// <summary> Returns true to indicate this is a parameter and not a text fragment.</summary>
            /// <returns>
            ///     true if parameter fragment, false if text fragment.
            /// </returns>
            public abstract bool IsParameter { get; }

            /// <summary>
            ///     Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use
            ///     in hashing algorithms and data structures like a hash table.
            /// </summary>
            /// <returns>
            ///     A hash code for the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override int GetHashCode()
            {
                return value != null ? value.GetHashCode() : 0;
            }
        }

        /// <summary>
        ///     Represents a piece of text in a parse string with placeholder values.
        /// </summary>
        public class TextFragment : Fragment
        {
            /// <summary> Ctor.</summary>
            /// <param name="value">
            ///     is the text
            /// </param>
            public TextFragment(string value)
                : base(value)
            {
            }

            /// <summary>
            ///     Returns true to indicate this is a parameter and not a text fragment.
            /// </summary>
            /// <value></value>
            /// <returns>
            ///     true if parameter fragement, false if text fragment.
            /// </returns>
            public override bool IsParameter => false;

            /// <summary>
            ///     Determines whether the specified <see cref="T:System.Object"></see> is equal to the current
            ///     <see cref="T:System.Object"></see>.
            /// </summary>
            /// <param name="obj">
            ///     The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>
            ///     .
            /// </param>
            /// <returns>
            ///     true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>
            ///     ; otherwise, false.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (!(obj is TextFragment other)) {
                    return false;
                }

                return other.Value.Equals(Value);
            }


            /// <summary>
            ///     Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use
            ///     in hashing algorithms and data structures like a hash table.
            /// </summary>
            /// <returns>
            ///     A hash code for the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            /// <summary>
            ///     Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            ///     A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                return $"text={Value}";
            }
        }

        /// <summary>
        ///     Represents a parameter in a parsed string of texts and parameters.
        /// </summary>
        public class ParameterFragment : Fragment
        {
            /// <summary> Ctor.</summary>
            /// <param name="value">
            ///     is the parameter name
            /// </param>
            public ParameterFragment(string value)
                : base(value)
            {
            }

            /// <summary>
            ///     Returns true to indicate this is a parameter and not a text fragment.
            /// </summary>
            /// <value></value>
            /// <returns>
            ///     true if parameter fragement, false if text fragment.
            /// </returns>
            public override bool IsParameter => true;

            /// <summary>
            ///     Determines whether the specified <see cref="T:System.Object"></see> is equal to the current
            ///     <see cref="T:System.Object"></see>.
            /// </summary>
            /// <param name="obj">
            ///     The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>
            ///     .
            /// </param>
            /// <returns>
            ///     true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>
            ///     ; otherwise, false.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (!(obj is ParameterFragment other)) {
                    return false;
                }

                return other.Value.Equals(Value);
            }

            /// <summary>
            ///     Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use
            ///     in hashing algorithms and data structures like a hash table.
            /// </summary>
            /// <returns>
            ///     A hash code for the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            /// <summary>
            ///     Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <returns>
            ///     A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
            /// </returns>
            public override string ToString()
            {
                return $"param={Value}";
            }
        }
    }
}