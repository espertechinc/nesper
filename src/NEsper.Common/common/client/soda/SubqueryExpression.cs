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
    /// Subquery-expression returns values returned by a lookup modelled by a further <seealso cref="EPStatementObjectModel" />.
    /// </summary>
    [Serializable]
    public class SubqueryExpression : ExpressionBase
    {
        private EPStatementObjectModel model;

        /// <summary>
        /// Ctor.
        /// </summary>
        public SubqueryExpression()
        {
        }

        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// </summary>
        /// <param name="model">is the lookup statement object model</param>
        public SubqueryExpression(EPStatementObjectModel model)
        {
            this.model = model;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write('(');
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
        public SubqueryExpression SetModel(EPStatementObjectModel model)
        {
            this.model = model;
            return this;
        }
    }
} // end of namespace