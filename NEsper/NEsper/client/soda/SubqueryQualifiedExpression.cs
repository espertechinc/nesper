///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Exists-expression for a set of values returned by a lookup.
    /// </summary>
    [Serializable]
    public class SubqueryQualifiedExpression : ExpressionBase
    {
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="model">is the lookup statement object model</param>
        /// <param name="operator">the op</param>
        /// <param name="all">true for ALL, false for ANY</param>
        public SubqueryQualifiedExpression(EPStatementObjectModel model, String @operator, bool all)
        {
            Model = model;
            Operator = @operator;
            IsAll = all;
        }

        /// <summary>
        /// Returns the lookup statement object model.
        /// </summary>
        /// <returns>
        /// lookup model
        /// </returns>
        public EPStatementObjectModel Model { get; set; }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>The operator.</value>
        public string Operator { get; set; }

        /// <summary>
        /// Returns true for ALL, false for ANY.
        /// </summary>
        /// <returns>
        /// all/any flag
        /// </returns>
        public bool IsAll { get; set; }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

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
    }
}
