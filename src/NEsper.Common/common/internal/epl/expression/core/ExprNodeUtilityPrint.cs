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
using com.espertech.esper.common.@internal.epl.expression.chain;
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
                nodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, ExprNodeRenderableFlags.DEFAULTFLAGS);
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
                node.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, ExprNodeRenderableFlags.DEFAULTFLAGS);
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
                expressions[i].ExprForgeRenderable.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, ExprNodeRenderableFlags.DEFAULTFLAGS);
                texts[i] = writer.ToString();
            }

            return texts;
        }

        public static string ToExpressionStringMinPrecedence(ExprForge expression)
        {
            var writer = new StringWriter();
            expression.ExprForgeRenderable.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, ExprNodeRenderableFlags.DEFAULTFLAGS);
            return writer.ToString();
        }

        public static string ToExpressionStringMinPrecedence(ExprNode expression, ExprNodeRenderableFlags flags) {
            var writer = new StringWriter();
            expression.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            return writer.ToString();
        }

        public static string ToExpressionStringMinPrecedenceSafe(ExprNode node)
        {
            try {
                var writer = new StringWriter();
                node.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, ExprNodeRenderableFlags.DEFAULTFLAGS);
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
            node.ToEPL(buffer, ExprPrecedenceEnum.MINIMUM, ExprNodeRenderableFlags.DEFAULTFLAGS);
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
            IList<Chainable> chainSpec,
            TextWriter buffer,
            bool prefixDot,
            string functionName)
        {
            var delimiterOuter = "";
            if (prefixDot) {
                delimiterOuter = ".";
            }

            foreach (Chainable element in chainSpec) {
                if (element.IsDistinct) {
                    buffer.Write("distinct ");
                }

                if (element is ChainableArray array) {
                    buffer.Write("[");
                    ToExpressionStringParameterList(array.Indexes, buffer);
                    buffer.Write("]");
                }
                else {
                    buffer.Write(delimiterOuter);
                    if (functionName != null) {
                        buffer.Write(functionName);
                    }
                    else {
                        string name = element.GetRootNameOrEmptyString();
                        buffer.Write(name);
                    }

                    if (element is ChainableCall call) {
                        ToExpressionStringIncludeParen(call.Parameters, buffer);
                    }
                }

                if (element.IsOptional) {
                    buffer.Write("?");
                }

                delimiterOuter = ".";
            }
        }
    }
} // end of namespace