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
    /// <summary>Base expression.</summary>
    [Serializable]
    public abstract class ExpressionBase : Expression
    {
        private IList<Expression> _children;
        private string _treeObjectName;

        /// <summary>Ctor.</summary>
        public ExpressionBase()
        {
            _children = new List<Expression>();
        }

        /// <summary>
        ///     Renders child expression of a function in a comma-separated list.
        /// </summary>
        /// <param name="functionName">function name</param>
        /// <param name="children">child nodes</param>
        /// <param name="writer">writer</param>
        internal static void ToPrecedenceFreeEPL(string functionName, IList<Expression> children, TextWriter writer)
        {
            writer.Write(functionName);
            writer.Write("(");
            ToPrecedenceFreeEPL(children, writer);
            writer.Write(')');
        }

        /// <summary>
        ///     RenderAny expression list
        /// </summary>
        /// <param name="children">expressions to render</param>
        /// <param name="writer">writer to render to</param>
        public static void ToPrecedenceFreeEPL(IList<Expression> children, TextWriter writer)
        {
            string delimiter = "";
            foreach (Expression expr in children)
            {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
        }

        /// <summary>
        ///     RenderAny an aggregation function with distinct and parameter expressions
        /// </summary>
        /// <param name="writer">to render to</param>
        /// <param name="name">function name</param>
        /// <param name="distinct">distinct flag</param>
        /// <param name="children">parameters to render</param>
        internal static void RenderAggregation(
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
            string delimiter = "";
            foreach (Expression param in children)
            {
                writer.Write(delimiter);
                delimiter = ",";
                param.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            writer.Write(")");
        }

        public string TreeObjectName
        {
            get { return _treeObjectName; }
            set { _treeObjectName = value; }
        }

        /// <summary>
        ///     Returns the list of sub-expressions to the current expression.
        /// </summary>
        /// <value>list of child expressions</value>
        public IList<Expression> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>
        ///     Adds a new child expression to the current expression.
        /// </summary>
        /// <param name="expression">to add</param>
        public void AddChild(Expression expression)
        {
            _children.Add(expression);
        }

        public void ToEPL(TextWriter writer, ExpressionPrecedenceEnum parentPrecedence)
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
        ///     Renders the expressions and all it's child expression, in full tree depth, as a string in
        ///     language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        public abstract void ToPrecedenceFreeEPL(TextWriter writer);

        /// <summary>
        /// Returns the precedence.
        /// </summary>
        public abstract  ExpressionPrecedenceEnum Precedence { get;  }
    }
} // end of namespace