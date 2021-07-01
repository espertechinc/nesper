///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Constant value returns a fixed value for use in expressions.
    /// </summary>
    [Serializable]
    public class ConstantExpression : ExpressionBase
    {
        private object constant;
        private string constantType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConstantExpression()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="constant">is the constant value, or null to represent the null value</param>
        public ConstantExpression(object constant)
        {
            this.constant = constant;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="constant">value</param>
        /// <param name="constantType">type</param>
        public ConstantExpression(
            object constant,
            string constantType)
        {
            this.constant = constant;
            this.constantType = constantType;
        }

        /// <summary>
        ///     Returns the type of the constant.
        /// </summary>
        /// <returns>type</returns>
        public string ConstantType
        {
            get => constantType;
            set => constantType = value;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns the constant value that the expression represents.
        /// </summary>
        /// <returns>value of constant</returns>
        public object Constant
        {
            get => constant;
            set => constant = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (constant is IDictionary<string, object> map)
            {
                writer.Write("{");
                var delimiter = "";
                foreach (var entry in map)
                {
                    writer.Write(delimiter);
                    writer.Write(entry.Key);
                    writer.Write(": ");
                    DataFlowOperatorParameter.RenderValue(writer, entry.Value);
                    delimiter = ",";
                }

                writer.Write("}");
            }
            else if (constant is string)
            {
                EPStatementObjectModelHelper.RenderEPL(writer, constant);
            }
            else if (constant is IEnumerable iterable)
            {
                writer.Write("[");
                var delimiter = "";

                foreach (var next in iterable)
                {
                    writer.Write(delimiter);
                    DataFlowOperatorParameter.RenderValue(writer, next);
                    delimiter = ",";
                }

                writer.Write("]");
            }
            else
            {
                StringValue.RenderConstantAsEPL(writer, constant);
            }
        }
    }
} // end of namespace