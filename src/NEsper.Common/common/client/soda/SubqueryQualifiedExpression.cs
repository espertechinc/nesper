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
    ///     Exists-expression for a set of values returned by a lookup.
    /// </summary>
    public class SubqueryQualifiedExpression : ExpressionBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public SubqueryQualifiedExpression()
        {
        }

        /// <summary>
        ///     Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="model">is the lookup statement object model</param>
        /// <param name="operator">the op</param>
        /// <param name="all">true for ALL, false for ANY</param>
        public SubqueryQualifiedExpression(
            EPStatementObjectModel model,
            string @operator,
            bool all)
        {
            Model = model;
            Operator = @operator;
            IsAll = all;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns the lookup statement object model.
        /// </summary>
        /// <returns>lookup model</returns>
        public EPStatementObjectModel Model { get; set; }

        /// <summary>
        ///     Returns the operator.
        /// </summary>
        /// <returns>operator</returns>
        public string Operator { get; set; }

        /// <summary>
        ///     Returns true for ALL, false for ANY.
        /// </summary>
        /// <returns>all/any flag</returns>
        public bool IsAll { get; set; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            Children[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            writer.Write(' ');
            writer.Write(Operator);
            if (IsAll) {
                writer.Write(" all (");
            }
            else {
                writer.Write(" any (");
            }

            writer.Write(Model.ToEPL());
            writer.Write(')');
        }

        /// <summary>
        ///     Sets the lookup statement object model.
        /// </summary>
        /// <param name="model">is the lookup model to set</param>
        public SubqueryQualifiedExpression SetModel(EPStatementObjectModel model)
        {
            Model = model;
            return this;
        }

        /// <summary>
        ///     Sets the operator.
        /// </summary>
        /// <param name="operator">op</param>
        public SubqueryQualifiedExpression SetOperator(string @operator)
        {
            Operator = @operator;
            return this;
        }

        /// <summary>
        ///     Set to true for ALL, false for ANY.
        /// </summary>
        /// <param name="all">true for ALL</param>
        public SubqueryQualifiedExpression SetAll(bool all)
        {
            IsAll = all;
            return this;
        }
    }
} // end of namespace