///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    /// <summary>
    /// Precendence levels for expressions.
    /// </summary>
    public class RowRecogExprNodePrecedenceEnum
    {
        /// <summary>
        /// Precedence.
        /// </summary>
        public static readonly RowRecogExprNodePrecedenceEnum UNARY =
            new RowRecogExprNodePrecedenceEnum(4);

        /// <summary>
        /// Precedence.
        /// </summary>
        public static readonly RowRecogExprNodePrecedenceEnum GROUPING =
            new RowRecogExprNodePrecedenceEnum(3);

        /// <summary>
        /// Precedence.
        /// </summary>
        public static readonly RowRecogExprNodePrecedenceEnum CONCATENATION =
            new RowRecogExprNodePrecedenceEnum(2);

        /// <summary>
        /// Precedence.
        /// </summary>
        public static readonly RowRecogExprNodePrecedenceEnum ALTERNATION =
            new RowRecogExprNodePrecedenceEnum(1);

        /// <summary>
        /// Precedence.
        /// </summary>
        public static readonly RowRecogExprNodePrecedenceEnum MINIMUM =
            new RowRecogExprNodePrecedenceEnum(int.MinValue);

        private RowRecogExprNodePrecedenceEnum(int level)
        {
            this.Level = level;
        }

        /// <summary>
        /// Level.
        /// </summary>
        /// <value>level</value>
        public int Level { get; }
    }
} // end of namespace