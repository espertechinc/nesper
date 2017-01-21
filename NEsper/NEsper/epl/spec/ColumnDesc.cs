///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Describes a column name and type.
    /// </summary>
    [Serializable]
    public class ColumnDesc : MetaDefItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">column name</param>
        /// <param name="type">type</param>
        /// <param name="array">true for array</param>
        /// <param name="isPrimitiveArray">if set to <c>true</c> [is primitive array].</param>
        public ColumnDesc(string name, string type, bool array, bool isPrimitiveArray)
        {
            Name = name;
            Type = type;
            IsArray = array;
            IsPrimitiveArray = isPrimitiveArray;
        }

        /// <summary>Returns column name. </summary>
        /// <value>name</value>
        public string Name { get; private set; }

        /// <summary>Return column type </summary>
        /// <value>type</value>
        public string Type { get; private set; }

        /// <summary>Return true for array </summary>
        /// <value>array indicator</value>
        public bool IsArray { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is primitive array.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is primitive array; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimitiveArray { get; private set; }
    }
}
