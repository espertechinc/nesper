///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Enumeration for representing select-clause selection of the remove stream or the insert stream, or both.
    /// </summary>
    public enum SelectClauseStreamSelectorEnum
    {
        /// <summary>
        ///     Indicates selection of the remove stream only.
        /// </summary>
        RSTREAM_ONLY,

        /// <summary>
        ///     Indicates selection of the insert stream only.
        /// </summary>
        ISTREAM_ONLY,

        /// <summary>
        ///     Indicates selection of both the insert and the remove stream.
        /// </summary>
        RSTREAM_ISTREAM_BOTH
    }

    public static class SelectClauseStreamSelectorEnumExtensions
    {
        public static bool IsSelectsRStream(this SelectClauseStreamSelectorEnum value)
        {
            return value != SelectClauseStreamSelectorEnum.ISTREAM_ONLY;
        }

        public static bool IsSelectsIStream(this SelectClauseStreamSelectorEnum value)
        {
            return value != SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
        }
    }
} // end of namespace