///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Part of a select-clause to describe individual select-clause expressions.
    /// </summary>
    [Serializable]
    public class SelectClauseExpression : SelectClauseElement
    {
        /// <summary>Ctor. </summary>
        public SelectClauseExpression()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="expression">is the selection expression</param>
        public SelectClauseExpression(Expression expression)
        {
            Expression = expression;
        }

        /// <summary>Ctor. </summary>
        /// <param name="expression">is the selection expression</param>
        /// <param name="optionalAsName">is the "as"-tag for the expression</param>
        public SelectClauseExpression(
            Expression expression,
            String optionalAsName)
        {
            Expression = expression;
            AsName = optionalAsName;
        }

        /// <summary>Returns the selection expression. </summary>
        /// <value>expression</value>
        public Expression Expression { get; set; }

        /// <summary>Returns the optional "as"-name of the expression, or null if not defined </summary>
        /// <value>tag or null for selection expression</value>
        public string AsName { get; set; }

        /// <summary>Returns indicator whether annotated as "@eventbean" </summary>
        public bool IsAnnotatedByEventFlag { get; set; }

        /// <summary>Renders the element in textual representation. </summary>
        /// <param name="writer">to output to</param>
        public void ToEPLElement(TextWriter writer)
        {
            Expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            if (IsAnnotatedByEventFlag)
            {
                writer.Write(" @eventbean");
            }

            if (AsName != null)
            {
                writer.Write(" as ");
                writer.Write(AsName);
            }
        }
    }
}