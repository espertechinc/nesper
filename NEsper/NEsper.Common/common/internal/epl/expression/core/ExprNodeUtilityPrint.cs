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
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityPrint
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string[] ToExpressionStringMinPrecedenceAsArray(ExprNode[] nodes)
        {
            var expressions = new string[nodes.Length];
            for (var i = 0; i < expressions.Length; i++) {
                var writer = new StringWriter();
                nodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                expressions[i] = writer.ToString();
            }

            return expressions;
        }

        public static string ToExpressionStringMinPrecedenceAsList(ExprNode[] nodes)
        {
            var writer = new StringWriter();
            ToExpressionStringMinPrecedenceAsList(nodes, writer);
            return writer.ToString();
        }

        public static void ToExpressionStringMinPrecedenceAsList(
            ExprNode[] nodes,
            TextWriter writer)
        {
            var delimiter = "";
            foreach (var node in nodes) {
                writer.Write(delimiter);
                node.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
        }

        public static string[] ToExpressionStringsMinPrecedence(ExprNode[] expressions)
        {
            var texts = new string[expressions.Length];
            for (var i = 0; i < expressions.Length; i++) {
                texts[i] = ToExpressionStringMinPrecedenceSafe(expressions[i]);
            }

            return texts;
        }

        public static string[] ToExpressionStringsMinPrecedence(ExprForge[] expressions)
        {
            var texts = new string[expressions.Length];
            for (var i = 0; i < expressions.Length; i++) {
                var writer = new StringWriter();
                expressions[i].ForgeRenderable.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                texts[i] = writer.ToString();
            }

            return texts;
        }

        public static string ToExpressionStringMinPrecedence(ExprForge expression)
        {
            var writer = new StringWriter();
            expression.ForgeRenderable.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            return writer.ToString();
        }

        public static string PrintEvaluators(ExprEvaluator[] evaluators)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var evaluator in evaluators) {
                writer.Write(delimiter);
                writer.Write(evaluator.GetType().Name);
                delimiter = ", ";
            }

            return writer.ToString();
        }

        public static string ToExpressionStringMinPrecedenceSafe(ExprNode node)
        {
            try {
                var writer = new StringWriter();
                node.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                return writer.ToString();
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                Log.Debug("Failed to render expression text: " + ex.Message, ex);
                return "";
            }
        }

        public static void ToExpressionStringParameterList(
            ExprNode[] childNodes,
            TextWriter buffer)
        {
            var delimiter = "";
            foreach (var childNode in childNodes) {
                buffer.Write(delimiter);
                buffer.Write(ToExpressionStringMinPrecedenceSafe(childNode));
                delimiter = ",";
            }
        }

        public static void ToExpressionStringWFunctionName(
            string functionName,
            ExprNode[] childNodes,
            TextWriter writer)
        {
            writer.Write(functionName);
            writer.Write("(");
            ToExpressionStringParameterList(childNodes, writer);
            writer.Write(')');
        }

        public static void ToExpressionStringParams(
            TextWriter writer,
            ExprNode[] @params)
        {
            writer.Write('(');
            var delimiter = "";
            foreach (var childNode in @params) {
                writer.Write(delimiter);
                delimiter = ",";
                writer.Write(ToExpressionStringMinPrecedenceSafe(childNode));
            }

            writer.Write(')');
        }

        public static void ToExpressionStringParameterList(
            IList<ExprNode> parameters,
            TextWriter buffer)
        {
            var delimiter = "";
            foreach (var param in parameters) {
                buffer.Write(delimiter);
                delimiter = ",";
                buffer.Write(ToExpressionStringMinPrecedenceSafe(param));
            }
        }

        public static void ToExpressionString(
            ExprNode node,
            TextWriter buffer)
        {
            node.ToEPL(buffer, ExprPrecedenceEnum.MINIMUM);
        }

        public static void ToExpressionStringIncludeParen(
            IList<ExprNode> parameters,
            TextWriter buffer)
        {
            buffer.Write("(");
            ToExpressionStringParameterList(parameters, buffer);
            buffer.Write(")");
        }

        public static void ToExpressionString(
            IList<ExprChainedSpec> chainSpec,
            TextWriter buffer,
            bool prefixDot,
            string functionName)
        {
            var delimiterOuter = "";
            if (prefixDot) {
                delimiterOuter = ".";
            }

            var isFirst = true;
            foreach (var element in chainSpec) {
                buffer.Write(delimiterOuter);
                if (functionName != null) {
                    buffer.Write(functionName);
                }
                else {
                    buffer.Write(element.Name);
                }

                // the first item without dot-prefix and empty parameters should not be appended with parenthesis
                if (!isFirst || prefixDot || !element.Parameters.IsEmpty()) {
                    ToExpressionStringIncludeParen(element.Parameters, buffer);
                }

                delimiterOuter = ".";
                isFirst = false;
            }
        }
    }
} // end of namespace