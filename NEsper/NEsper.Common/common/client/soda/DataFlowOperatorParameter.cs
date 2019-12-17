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
    /// Object model of a data flow operator parameter.
    /// </summary>
    [Serializable]
    public class DataFlowOperatorParameter
    {
        /// <summary>Ctor. </summary>
        /// <param name="parameterName">parameter name</param>
        /// <param name="parameterValue">parameter value</param>
        public DataFlowOperatorParameter(
            string parameterName,
            object parameterValue)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }

        /// <summary>Ctor. </summary>
        public DataFlowOperatorParameter()
        {
        }

        /// <summary>
        /// Get the parameter name.
        /// </summary>
        /// <value>parameter name</value>
        public string ParameterName { get; set; }

        /// <summary>
        /// Get the parameter value, which can be either a constant, an <seealso cref="Expression" />
        /// or a JSON object or a <seealso cref="EPStatementObjectModel" />.
        /// </summary>
        /// <value>parameter value</value>
        public object ParameterValue { get; set; }

        /// <summary>RenderAny parameter. </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(ParameterName);
            writer.Write(": ");
            RenderValue(writer, ParameterValue);
        }

        /// <summary>RenderAny prameter. </summary>
        /// <param name="writer">to render to</param>
        /// <param name="parameterValue">value</param>
        public static void RenderValue(
            TextWriter writer,
            object parameterValue)
        {
            if (parameterValue is EPStatementObjectModel statementObjectModel) {
                writer.Write("(");
                statementObjectModel.ToEPL(writer);
                writer.Write(")");
            }
            else if (parameterValue is Expression expression) {
                expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            else if (parameterValue == null) {
                writer.Write("null");
            }
            else if (parameterValue is string stringValue) {
                writer.Write("\"");
                writer.Write(stringValue);
                writer.Write("\"");
            }
            else {
                writer.Write(parameterValue.ToString());
            }
        }
    }
}