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
    public class SelectClauseStreamSelectorEnum
    {
        /// <summary>
        ///     Indicates selection of the remove stream only.
        /// </summary>
        public static readonly SelectClauseStreamSelectorEnum RSTREAM_ONLY =
            new SelectClauseStreamSelectorEnum();

        /// <summary>
        ///     Indicates selection of the insert stream only.
        /// </summary>
        public static readonly SelectClauseStreamSelectorEnum ISTREAM_ONLY =
            new SelectClauseStreamSelectorEnum();

        /// <summary>
        ///     Indicates selection of both the insert and the remove stream.
        /// </summary>
        public static readonly SelectClauseStreamSelectorEnum RSTREAM_ISTREAM_BOTH =
            new SelectClauseStreamSelectorEnum();

        private SelectClauseStreamSelectorEnum()
        {
        }

        public bool IsSelectsRStream => this != ISTREAM_ONLY;

        public bool IsSelectsIStream => this != RSTREAM_ONLY;
    }
} // end of namespace