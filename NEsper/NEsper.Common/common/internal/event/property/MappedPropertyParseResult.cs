///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.property
{
    /// <summary>
    ///     Encapsulates the parse result parsing a mapped property as a class and method name with args.
    /// </summary>
    public class MappedPropertyParseResult
    {
        /// <summary>
        ///     Returns the parse result of the mapped property.
        /// </summary>
        /// <param name="className">is the class name, or null if there isn't one</param>
        /// <param name="methodName">is the method name</param>
        /// <param name="argString">is the argument</param>
        public MappedPropertyParseResult(string className, string methodName, string argString)
        {
            ClassName = className;
            MethodName = methodName;
            ArgString = argString;
        }

        /// <summary>
        ///     Returns class name.
        /// </summary>
        /// <returns>name of class</returns>
        public string ClassName { get; }

        /// <summary>
        ///     Returns the method name.
        /// </summary>
        /// <returns>method name</returns>
        public string MethodName { get; }

        /// <summary>
        ///     Returns the method argument.
        /// </summary>
        /// <returns>arg</returns>
        public string ArgString { get; }
    }
} // end of namespace