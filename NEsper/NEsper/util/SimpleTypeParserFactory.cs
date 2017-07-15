///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.type;

namespace com.espertech.esper.util
{
    /// <summary>
    /// A factory for creating an instance of a parser that parses a string and returns a target type.
    /// </summary>
    public class SimpleTypeParserFactory {
        /// <summary>
        /// Returns a parsers for the string value using the given Java built-in class for parsing.
        /// </summary>
        /// <param name="clazz">is the class to parse the value to</param>
        /// <returns>value matching the type passed in</returns>
        public static SimpleTypeParser GetParser(Type clazz) {
            Type classBoxed = TypeHelper.GetBoxedType(clazz);
    
            if (classBoxed == typeof(string)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (value) => {
                        return value;
                    };
                };
            }
            if (classBoxed == typeof(Character)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (value) => {
                        return Value.CharAt(0);
                    };
                };
            }
            if (classBoxed == typeof(bool?)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return BoolValue.ParseString(text.ToLowerInvariant().Trim());
                    };
                };
            }
            if (classBoxed == typeof(Byte)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return ByteValue.ParseString(text.Trim());
                    };
                };
            }
            if (classBoxed == typeof(Short)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return ShortValue.ParseString(text.Trim());
                    };
                };
            }
            if (classBoxed == typeof(long)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return LongValue.ParseString(text.Trim());
                    };
                };
            }
            if (classBoxed == typeof(Float)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return FloatValue.ParseString(text.Trim());
                    };
                };
            }
            if (classBoxed == typeof(double?)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return DoubleValue.ParseString(text.Trim());
                    };
                };
            }
            if (classBoxed == typeof(int?)) {
                return new ProxySimpleTypeParser() {
                    ProcParse = (text) => {
                        return IntValue.ParseString(text.Trim());
                    };
                };
            }
            return null;
        }
    }
} // end of namespace
