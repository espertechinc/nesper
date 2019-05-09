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
    /// Abstract base class for all pattern expressions.
    /// </summary>
    [Serializable]
    public abstract class PatternExprBase : PatternExpr
    {
        public string TreeObjectName { get; set; }
        public IList<PatternExpr> Children { get; set; }

        /// <summary>Ctor. </summary>
        protected PatternExprBase()
        {
            Children = new List<PatternExpr>();
        }

        /// <summary>Adds a sub-expression to the pattern expression. </summary>
        /// <param name="expression">to add</param>
        protected void AddChild(PatternExpr expression)
        {
            Children.Add(expression);
        }

        public virtual void ToEPL(
            TextWriter writer,
            PatternExprPrecedenceEnum parentPrecedence,
            EPStatementFormatter formatter)
        {
            if (Precedence.GetLevel() < parentPrecedence.GetLevel()) {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer, formatter);
                writer.Write(")");
            }
            else {
                ToPrecedenceFreeEPL(writer, formatter);
            }
        }

        /// <summary>Returns the Precedence. </summary>
        /// <value>Precedence</value>
        public abstract PatternExprPrecedenceEnum Precedence { get; }

        /// <summary>Renders the expressions and all it's child expression, in full tree depth, as a string in language syntax. </summary>
        /// <param name="writer">is the output to use</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public abstract void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter);
    }
}