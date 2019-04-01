///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Represents a contained-event selection.
    /// </summary>
    [Serializable]
    public class ContainedEventSelect
    {
        private string optionalAsName;
        private string optionalSplitExpressionTypeName;
        private SelectClause selectClause;
        private Expression splitExpression;
        private Expression whereClause;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ContainedEventSelect()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="splitExpression">the property expression or other expression for splitting the event</param>
        public ContainedEventSelect(Expression splitExpression)
        {
            this.splitExpression = splitExpression;
        }

        /// <summary>
        ///     Returns the property alias.
        /// </summary>
        /// <returns>alias</returns>
        public string OptionalAsName {
            get => optionalAsName;
            set => optionalAsName = value;
        }

        /// <summary>
        ///     Returns the select clause.
        /// </summary>
        /// <returns>select clause</returns>
        public SelectClause SelectClause {
            get => selectClause;
            set => selectClause = value;
        }

        /// <summary>
        ///     Returns the where clause.
        /// </summary>
        /// <returns>where clause</returns>
        public Expression WhereClause {
            get => whereClause;
            set => whereClause = value;
        }

        /// <summary>
        ///     Returns the event type name assigned to events that result by applying the split (contained event) expression.
        /// </summary>
        /// <returns>type name, or null if none assigned</returns>
        public string OptionalSplitExpressionTypeName {
            get => optionalSplitExpressionTypeName;
            set => optionalSplitExpressionTypeName = value;
        }

        /// <summary>
        ///     Returns the expression that returns the contained events.
        /// </summary>
        /// <returns>contained event expression</returns>
        public Expression SplitExpression {
            get => splitExpression;
            set => splitExpression = value;
        }

        /// <summary>
        ///     Returns the EPL.
        /// </summary>
        /// <param name="writer">to write to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            if (selectClause != null) {
                selectClause.ToEPL(writer, formatter, false, false);
                writer.Write(" from ");
            }

            splitExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (optionalSplitExpressionTypeName != null) {
                writer.Write("@type(");
                writer.Write(optionalSplitExpressionTypeName);
                writer.Write(")");
            }

            if (optionalAsName != null) {
                writer.Write(" as ");
                writer.Write(optionalAsName);
            }

            if (whereClause != null) {
                writer.Write(" where ");
                whereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        /// <summary>
        ///     Render contained-event select
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="formatter">to use</param>
        /// <param name="items">to render</param>
        public static void ToEPL(TextWriter writer, EPStatementFormatter formatter, IList<ContainedEventSelect> items)
        {
            foreach (var propertySelect in items) {
                writer.Write('[');
                propertySelect.ToEPL(writer, formatter);
                writer.Write(']');
            }
        }
    }
} // end of namespace