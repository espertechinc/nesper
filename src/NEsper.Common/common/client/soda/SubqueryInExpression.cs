///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// In-expression for a set of values returned by a lookup.
    /// </summary>
    public class SubqueryInExpression : ExpressionBase
    {
        private bool notIn;
        private EPStatementObjectModel model;

        /// <summary>
        /// Ctor.
        /// </summary>
        public SubqueryInExpression()
        {
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="model">is the lookup statement object model</param>
        /// <param name="isNotIn">is true for not-in</param>
        public SubqueryInExpression(
            EPStatementObjectModel model,
            bool isNotIn)
        {
            this.model = model;
            notIn = isNotIn;
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="expression">is the expression providing the value to match</param>
        /// <param name="model">is the lookup statement object model</param>
        /// <param name="isNotIn">is true for not-in</param>
        public SubqueryInExpression(
            Expression expression,
            EPStatementObjectModel model,
            bool isNotIn)
        {
            Children.Add(expression);
            this.model = model;
            notIn = isNotIn;
        }

        /// <summary>
        /// Returns true for not-in, or false for in-lookup.
        /// </summary>
        /// <returns>true for not-in</returns>
        public bool IsNotIn {
            get => notIn;
            set => notIn = value;
        }

        /// <summary>
        /// Set to true for not-in, or false for in-lookup.
        /// </summary>
        /// <param name="notIn">true for not-in</param>
        public SubqueryInExpression SetNotIn(bool notIn)
        {
            this.notIn = notIn;
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, Precedence);
            if (notIn) {
                writer.Write(" not in (");
            }
            else {
                writer.Write(" in (");
            }

            writer.Write(model.ToEPL());
            writer.Write(')');
        }

        /// <summary>
        /// Returns the lookup statement object model.
        /// </summary>
        /// <returns>lookup model</returns>
        public EPStatementObjectModel Model {
            get => model;
            set => model = value;
        }

        /// <summary>
        /// Sets the lookup statement object model.
        /// </summary>
        /// <param name="model">is the lookup model to set</param>
        public SubqueryInExpression SetModel(EPStatementObjectModel model)
        {
            this.model = model;
            return this;
        }
    }
} // end of namespace