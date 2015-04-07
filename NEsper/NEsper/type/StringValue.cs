///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Globalization;
using System.Text;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Placeholder for a String value in an event expression.
    /// </summary>

    [Serializable]
    public sealed class StringValue : PrimitiveValueBase
    {
        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        override public PrimitiveValueType Type
        {
            get { return PrimitiveValueType.STRING; }
        }

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        override public Object ValueObject
        {
            get { return stringValue; }
        }

        /// <summary>
        /// Set a string value.
        /// </summary>
        /// <value></value>
        override public String _String
        {
            set { this.stringValue = value; }
        }

        private String stringValue;

        /// <summary> Constructor.</summary>
        /// <param name="stringValue">sets initial value
        /// </param>
        public StringValue(String stringValue)
        {
            this.stringValue = stringValue;
        }

        /// <summary> Constructor.</summary>
        public StringValue()
        {
        }

        /// <summary> Parse the string array returning a string array.</summary>
        /// <param name="values">string array
        /// </param>
        /// <returns> typed array
        /// </returns>
        public static String[] ParseString(String[] values)
        {
            String[] result = new String[values.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ParseString(values[i]);
            }
            return result;
        }

        /// <summary>
        /// Parse the string literal value into the specific data type.
        /// </summary>
        /// <param name="value">is the textual value to parse</param>
        public override void Parse(String value)
        {
            stringValue = ParseString(value);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            if (stringValue == null)
            {
                return "null";
            }
            return stringValue;
        }

        /// <summary> Parse the string literal consisting of text between double-quotes or single-quotes.</summary>
        /// <param name="value">is the text wthin double or single quotes
        /// </param>
        /// <returns> parsed value
        /// </returns>
        public static String ParseString(String value)
        {
            if ((value.StartsWith("\"")) & (value.EndsWith("\"")) || (value.StartsWith("'")) & (value.EndsWith("'")))
            {
                if (value.Length > 1)
                {
                    if (value.IndexOf('\\') != -1)
                    {
                        return Unescape(value.Substring(1, value.Length - 2));
                    }

                    return value.Substring(1, value.Length - 2);
                }
            }

            throw new ArgumentException("String value of '" + value + "' cannot be parsed");
        }

        private static String Unescape(String s)
        {
            var provider = System.Threading.Thread.CurrentThread.CurrentCulture;

            int i = 0, len = s.Length;
            char c;
            StringBuilder sb = new StringBuilder(len);
            while (i < len)
            {
                c = s[i++];
                if (c == '\\')
                {
                    if (i < len)
                    {
                        c = s[i++];
                        if (c == 'u')
                        {
                            c = (char) Int16.Parse(s.Substring(i, 4), NumberStyles.HexNumber, provider);
                            i += 4;
                        } // add other cases here as desired...
                    }
                } // fall through: \ escapes itself, quotes any character but u
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
