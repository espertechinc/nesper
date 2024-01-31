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
    /// <summary>Spec for defining a row limit. </summary>
    public class RowLimitSpec
    {
        /// <summary>Ctor. </summary>
        /// <param name="numRows">max num rows constant, null if using variable</param>
        /// <param name="optionalOffset">offset or null</param>
        /// <param name="numRowsVariable">max num rows variable, null if using constant</param>
        /// <param name="optionalOffsetVariable">offset variable or null</param>
        public RowLimitSpec(
            int? numRows,
            int? optionalOffset,
            string numRowsVariable,
            string optionalOffsetVariable)
        {
            NumRows = numRows;
            OptionalOffset = optionalOffset;
            NumRowsVariable = numRowsVariable;
            OptionalOffsetVariable = optionalOffsetVariable;
        }

        /// <summary>Returns max num rows constant or null if using variable. </summary>
        /// <returns>limit</returns>
        public int? NumRows { get; private set; }

        /// <summary>Returns offset constant or null. </summary>
        /// <returns>offset</returns>
        public int? OptionalOffset { get; private set; }

        /// <summary>Returns max num rows variable or null if using constant. </summary>
        /// <returns>limit</returns>
        public string NumRowsVariable { get; private set; }

        /// <summary>Returns offset variable or null </summary>
        /// <returns>offset variable</returns>
        public string OptionalOffsetVariable { get; private set; }
    }
}