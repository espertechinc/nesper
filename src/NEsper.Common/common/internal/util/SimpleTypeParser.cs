///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Parser of a String input to an object.
    /// </summary>
    public interface SimpleTypeParser
    {
        /// <summary>
        ///     Parses the text and returns an object value.
        /// </summary>
        /// <param name="text">to parse</param>
        /// <returns>object value</returns>
        object Parse(string text);
    }

    public class ProxySimpleTypeParser : SimpleTypeParser
    {
        public delegate object ParseFunc(string text);

        public ParseFunc ProcParse;

        public ProxySimpleTypeParser()
        {
        }

        public ProxySimpleTypeParser(ParseFunc procParse)
        {
            ProcParse = procParse;
        }

        public object Parse(string text) => ProcParse?.Invoke(text);
    }
} // end of namespace