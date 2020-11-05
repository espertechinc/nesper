///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    public enum DotMethodFPInputEnum
    {
        /// <summary>
        /// Numeric scalar values.
        /// </summary>
        SCALAR_NUMERIC,
        /// <summary>
        /// Any values.
        /// </summary>
        SCALAR_ANY,
        /// <summary>
        /// Collection of events.
        /// </summary>
        EVENTCOLL,
        /// <summary>
        /// Any input.
        /// </summary>
        ANY
    };

    public static class DotMethodFPInputEnumExtensions
    {
        public static bool IsScalar(this DotMethodFPInputEnum value)
        {
            return
                value == DotMethodFPInputEnum.SCALAR_ANY ||
                value == DotMethodFPInputEnum.SCALAR_NUMERIC;
        }
    }
}