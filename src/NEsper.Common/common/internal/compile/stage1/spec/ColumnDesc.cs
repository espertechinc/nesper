///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Describes a column name and type.
    /// </summary>
    public class ColumnDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">column name</param>
        /// <param name="type">type</param>
        public ColumnDesc(
            string name,
            string type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        ///     Returns column name.
        /// </summary>
        /// <returns>name</returns>
        public string Name { get; }

        /// <summary>
        ///     Return column type
        /// </summary>
        /// <returns>type</returns>
        public string Type { get; }
    }
} // end of namespace