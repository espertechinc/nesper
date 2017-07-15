///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>Represents a contained-event selection.</summary>
    [Serializable]
    public class ContainedEventSelect
    {
        /// <summary>Ctor.</summary>
        public ContainedEventSelect()
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="splitExpression">the property expression or other expression for splitting the event</param>
        public ContainedEventSelect(Expression splitExpression)
        {
            SplitExpression = splitExpression;
        }

        /// <summary>
        /// Render contained-event select
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="formatter">to use</param>
        /// <param name="items">to render</param>
        public static void ToEPL(TextWriter writer, EPStatementFormatter formatter, IList<ContainedEventSelect> items)
        {
            foreach (ContainedEventSelect propertySelect in items)
            {
                writer.Write('[');
                propertySelect.ToEPL(writer, formatter);
                writer.Write(']');
            }
        }

        /// <summary>
        /// Returns the property alias.
        /// </summary>
        /// <value>alias</value>
        public string OptionalAsName { get; set; }

        /// <summary>
        /// Returns the select clause.
        /// </summary>
        /// <value>select clause</value>
        public SelectClause SelectClause { get; set; }

        /// <summary>
        /// Returns the where clause.
        /// </summary>
        /// <value>where clause</value>
        public Expression WhereClause { get; set; }

        /// <summary>
        /// Returns the event type name assigned to events that result by applying the split (contained @event) expression.
        /// </summary>
        /// <value>type name, or null if none assigned</value>
        public string OptionalSplitExpressionTypeName { get; set; }

        /// <summary>
        /// Returns the expression that returns the contained events.
        /// </summary>
        /// <value>contained event expression</value>
        public Expression SplitExpression { get; set; }

        /// <summary>
        /// Returns the EPL.
        /// </summary>
        /// <param name="writer">to write to</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter) {
            if (SelectClause != null) {
                SelectClause.ToEPL(writer, formatter, false, false);
                writer.Write(" from ");
            }
            SplitExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (OptionalSplitExpressionTypeName != null) {
                writer.Write("@Type(");
                writer.Write(OptionalSplitExpressionTypeName);
                writer.Write(")");
            }
            if (OptionalAsName != null) {
                writer.Write(" as ");
                writer.Write(OptionalAsName);
            }
            if (WhereClause != null) {
                writer.Write(" where ");
                WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }
    }
} // end of namespace
