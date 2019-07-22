///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Text;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionUtil
    {
        public static void RenderConstant(
            StringBuilder builder,
            object constant)
        {
            if (constant is string) {
                builder.Append('"');
                builder.Append(constant); // StringEscapeUtils.EscapeJava((string) constant));
                builder.Append('"');
            }
            else if (constant is char) {
                var c = (char) constant;
                if (c == '\'') {
                    builder.Append('\'');
                    builder.Append('\\');
                    builder.Append('\'');
                    builder.Append('\'');
                }
                else if (c == '\\') {
                    builder.Append('\'');
                    builder.Append('\\');
                    builder.Append('\\');
                    builder.Append('\'');
                }
                else {
                    builder.Append('\'');
                    builder.Append(c);
                    builder.Append('\'');
                }
            }
            else if (constant == null) {
                builder.Append("null");
            }
            else if (constant is long) {
                builder.Append(constant).Append("L");
            }
            else if (constant is float) {
                builder.Append(constant).Append("F");
            }
            else if (constant is double) {
                builder.Append(constant).Append("d");
            }
            else if (constant is decimal) {
                builder.Append(constant).Append("m");
            }
            else if (constant is short) {
                builder.Append("(short) ").Append(constant);
            }
            else if (constant is byte) {
                builder.Append("(byte)").Append(constant);
            }
            else if (constant is Array asArray) {
                if (asArray.Length == 0) {
                    builder.Append("new ");
                    AppendClassName(builder, constant.GetType().GetElementType());
                    builder.Append("[]{}");
                }
                else {
                    builder.Append("new ");
                    AppendClassName(builder, constant.GetType().GetElementType());
                    builder.Append("[] {");
                    var delimiter = "";
                    for (var i = 0; i < asArray.Length; i++) {
                        builder.Append(delimiter);
                        RenderConstant(builder, asArray.GetValue(i));
                        delimiter = ",";
                    }

                    builder.Append("}");
                }
            }
            else if (constant.GetType().IsEnum) {
                AppendClassName(builder, constant.GetType());
                builder.Append(".").Append(constant);
            }
            else if (constant is Type) {
                CodegenExpressionClass.RenderClass((Type) constant, builder);
            }
            else if (constant is BigInteger) {
                RenderBigInteger((BigInteger) constant, builder);
            }
            else {
                builder.Append(constant);
            }
        }

        private static void RenderBigInteger(
            BigInteger constant,
            StringBuilder builder)
        {
            builder
                .Append("new ")
                .Append(typeof(BigInteger).FullName)
                .Append("(");
            RenderConstant(builder, constant.ToByteArray());
            builder.Append(")");
        }

        private static void AppendSequenceEscapeDQ(
            StringBuilder builder,
            string seq)
        {
            for (var i = 0; i < seq.Length; i++) {
                var c = seq[i];
                if (c == '\"') {
                    builder.Append('\\');
                    builder.Append(c);
                }
                else {
                    builder.Append(c);
                }
            }
        }
    }
} // end of namespace