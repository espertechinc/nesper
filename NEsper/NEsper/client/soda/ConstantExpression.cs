///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.core.service;

namespace com.espertech.esper.client.soda
{
    /// <summary>IsConstant value returns a fixed value for use in expressions. </summary>
    [Serializable]
    public class ConstantExpression : ExpressionBase
    {
        /// <summary>Ctor. </summary>
        public ConstantExpression() {
        }

        /// <summary>Returns the type of the constant. </summary>
        /// <value>type</value>
        public string ConstantType { get; private set; }

        /// <summary>Ctor. </summary>
        /// <param name="constant">is the constant value, or null to represent the null value</param>
        public ConstantExpression(Object constant)
        {
            Constant = constant;
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="constant">value</param>
        /// <param name="constantType">type</param>
        public ConstantExpression(Object constant, String constantType)
        {
            Constant = constant;
            ConstantType = constantType;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var constant = Constant;
            if (constant is IDictionary<string,object>) {
                var map = (IDictionary<string, object>) constant;
                writer.Write("{");
                var delimiter = "";

                foreach (var entry in map) {
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
            else if (constant is IEnumerable) {
                var iterable = (IEnumerable) constant;
                writer.Write("[");
                var delimiter = "";

                foreach(var next in iterable) {
                    writer.Write(delimiter);
                    DataFlowOperatorParameter.RenderValue(writer, next);
                    delimiter = ",";
                }
                writer.Write("]");
            }
            else
            {
                EPStatementObjectModelHelper.RenderEPL(writer, constant);
            }
        }

        /// <summary>Returns the constant value that the expression represents. </summary>
        /// <value>value of constant</value>
        public object Constant { get; set; }
    }
}
