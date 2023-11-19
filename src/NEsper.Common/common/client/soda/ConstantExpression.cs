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
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Constant value returns a fixed value for use in expressions.
    /// </summary>
    public class ConstantExpression : ExpressionBase
    {
        private object _constant;
        private string _constantType;

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
            _constant = constant;
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
            _constant = constant;
            _constantType = constantType;
        }

        /// <summary>
        ///     Returns the type of the constant.
        /// </summary>
        /// <returns>type</returns>
        public string ConstantType {
            get => _constantType;
            set => _constantType = value;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns the constant value that the expression represents.
        /// </summary>
        /// <returns>value of constant</returns>
        [JsonConverter(typeof(JsonConverterAbstract<object>))]
        public object Constant {
            get => _constant;
            set => _constant = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (_constant is IDictionary<string, object> map) {
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
            else if (_constant is string) {
                EPStatementObjectModelHelper.RenderEPL(writer, _constant);
            }
            else if (_constant is IEnumerable iterable) {
                writer.Write("[");
                var delimiter = "";

                foreach (var next in iterable) {
                    writer.Write(delimiter);
                    DataFlowOperatorParameter.RenderValue(writer, next);
                    delimiter = ",";
                }

                writer.Write("]");
            }
            else {
                StringValue.RenderConstantAsEPL(writer, _constant);
            }
        }
    }
} // end of namespace