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
    public class ContainedEventSelect
    {
        private string _optionalAsName;
        private string _optionalSplitExpressionTypeName;
        private SelectClause _selectClause;
        private Expression _splitExpression;
        private Expression _whereClause;

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
            _splitExpression = splitExpression;
        }

        /// <summary>
        ///     Returns the property alias.
        /// </summary>
        /// <returns>alias</returns>
        public string OptionalAsName {
            get => _optionalAsName;
            set => _optionalAsName = value;
        }

        /// <summary>
        ///     Returns the select clause.
        /// </summary>
        /// <returns>select clause</returns>
        public SelectClause SelectClause {
            get => _selectClause;
            set => _selectClause = value;
        }

        /// <summary>
        ///     Returns the where clause.
        /// </summary>
        /// <returns>where clause</returns>
        public Expression WhereClause {
            get => _whereClause;
            set => _whereClause = value;
        }

        /// <summary>
        ///     Returns the event type name assigned to events that result by applying the split (contained event) expression.
        /// </summary>
        /// <returns>type name, or null if none assigned</returns>
        public string OptionalSplitExpressionTypeName {
            get => _optionalSplitExpressionTypeName;
            set => _optionalSplitExpressionTypeName = value;
        }

        /// <summary>
        ///     Returns the expression that returns the contained events.
        /// </summary>
        /// <returns>contained event expression</returns>
        public Expression SplitExpression {
            get => _splitExpression;
            set => _splitExpression = value;
        }

        /// <summary>
        ///     Returns the EPL.
        /// </summary>
        /// <param name="writer">to write to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            if (_selectClause != null) {
                _selectClause.ToEPL(writer, formatter, false, false);
                writer.Write(" from ");
            }

            _splitExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (_optionalSplitExpressionTypeName != null) {
                writer.Write("@type(");
                writer.Write(_optionalSplitExpressionTypeName);
                writer.Write(")");
            }

            if (_optionalAsName != null) {
                writer.Write(" as ");
                writer.Write(_optionalAsName);
            }

            if (_whereClause != null) {
                writer.Write(" where ");
                _whereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }

        /// <summary>
        ///     Render contained-event select
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="formatter">to use</param>
        /// <param name="items">to render</param>
        public static void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter,
            IList<ContainedEventSelect> items)
        {
            foreach (var propertySelect in items) {
                writer.Write('[');
                propertySelect.ToEPL(writer, formatter);
                writer.Write(']');
            }
        }
    }
} // end of namespace