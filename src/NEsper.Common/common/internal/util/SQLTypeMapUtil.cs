///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Utility for mapping SQL types to native types.
    /// </summary>
    public class SQLTypeMapUtil
    {
        /// <summary>
        /// Converts a SQLType to a native type.
        /// </summary>
        /// <param name="sqlType">to return type for</param>
        /// <param name="className">
        /// is the classname that result metadata returns for a column
        /// </param>
        /// <returns>Type for sql types</returns>
        public static Type SqlTypeToClass(
            int sqlType,
            string className)
        {
            throw new ArgumentException("Logic path is not active");
        }
    }
} // end of namespace