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
    ///     Base expression.
    /// </summary>
    [Serializable]
    public abstract class ExpressionBase : Expression
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        protected ExpressionBase()
        {
            Children = new List<Expression>();
        }

        public string TreeObjectName { get; set; }

        /// <summary>
        ///     Returns the list of sub-expressions to the current expression.
        /// </summary>
        /// <returns>list of child expressions</returns>
        public IList<Expression> Children { get; set; }

        public abstract ExpressionPrecedenceEnum Precedence { get; }

        public virtual void ToEPL(
            TextWriter writer,
            ExpressionPrecedenceEnum parentPrecedence)
        {
            if (Precedence < parentPrecedence)
            {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer);
                writer.Write(")");
            }
            else
            {
                ToPrecedenceFreeEPL(writer);
            }
        }

        /// <summary>
        ///     Adds a new child expression to the current expression.
        /// </summary>
        /// <param name="expression">to add</param>
        public void AddChild(Expression expression)
        {
            Children.Add(expression);
        }

        /// <summary>
        ///     Renders child expression of a function in a comma-separated list.
        /// </summary>
        /// <param name="functionName">function name</param>
        /// <param name="children">child nodes</param>
        /// <param name="writer">writer</param>
        protected internal static void ToPrecedenceFreeEPL(
            string functionName,
            IList<Expression> children,
            TextWriter writer)
        {
            writer.Write(functionName);
            writer.Write("(");
            ToPrecedenceFreeEPL(children, writer);
            writer.Write(')');
        }

        /// <summary>
        ///     Render expression list
        /// </summary>
        /// <param name="children">expressions to render</param>
        /// <param name="writer">writer to render to</param>
        public static void ToPrecedenceFreeEPL(
            IList<Expression> children,
            TextWriter writer)
        {
            var delimiter = "";
            foreach (var expr in children)
            {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
        }

        /// <summary>
        ///     Render an aggregation function with distinct and parameter expressions
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="name">function name</param>
        /// <param name="distinct">distinct flag</param>
        /// <param name="children">parameters to render</param>
        protected internal static void RenderAggregation(
            TextWriter writer,
            string name,
            bool distinct,
            IList<Expression> children)
        {
            writer.Write(name);
            writer.Write("(");
            if (distinct)
            {
                writer.Write("distinct ");
            }

            var delimiter = "";
            foreach (var param in children)
            {
                writer.Write(delimiter);
                delimiter = ",";
                param.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write(")");
        }

        /// <summary>
        ///     Renders the expressions and all it's child expression, in full tree depth, as a string in
        ///     language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        public abstract void ToPrecedenceFreeEPL(TextWriter writer);
    }
} // end of namespace