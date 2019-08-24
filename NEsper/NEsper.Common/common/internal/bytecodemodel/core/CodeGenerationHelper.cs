///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Text;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodeGenerationHelper
    {
        public static StringBuilder AppendClassName(
            StringBuilder builder,
            Type clazz)
        {
            if (clazz == typeof(void)) {
                return builder.Append("void");
            } else if (clazz == typeof(short)) {
                return builder.Append("short");
            } else if (clazz == typeof(int)) {
                return builder.Append("int");
            } else if (clazz == typeof(long)) {
                return builder.Append("long");
            } else if (clazz == typeof(float)) {
                return builder.Append("float");
            } else if (clazz == typeof(double)) {
                return builder.Append("double");
            } else if (clazz == typeof(decimal)) {
                return builder.Append("decimal");
            } else if (clazz == typeof(byte)) {
                return builder.Append("byte");
            } else if (clazz == typeof(char)) {
                return builder.Append("char");
            } else if (clazz == typeof(bool)) {
                return builder.Append("bool");
            } else if (clazz == typeof(object)) {
                return builder.Append("object");
            } else if (clazz == typeof(string)) {
                return builder.Append("string");
            }

            if (clazz.IsNullable()) {
                AppendClassName(builder, clazz.GetGenericArguments()[0]);
                builder.Append("?");
                return builder;
            }

            if (clazz.IsArray) {
                AppendClassName(builder, clazz.GetElementType());
                builder.Append("[]");
                return builder;
            }

            if (clazz.IsGenericTypeDefinition) {
                var nameWithoutArgCount = clazz.Name.Substring(0, clazz.Name.IndexOf('`'));
                builder.Append(nameWithoutArgCount);
            } else if (clazz.IsGenericType) {
                var delimiter = "";
                var nameWithoutArgCount = clazz.Name.Substring(0, clazz.Name.IndexOf('`'));

                builder.Append(nameWithoutArgCount);
                builder.Append('<');
                foreach (var genericArgument in clazz.GetGenericArguments()) {
                    builder.Append(delimiter);
                    AppendClassName(builder, genericArgument);
                    delimiter = ",";
                }

                builder.Append('>');
            }
            else if (clazz.IsNested) {
                AppendClassName(builder, clazz.DeclaringType);
                builder.Append('.');
                builder.Append(clazz.Name);
            }
            else {
                builder.Append(clazz.Name);
            }

            return builder;
        }

#if false
        public static StringBuilder AppendClassName(
            StringBuilder builder,
            Type clazz,
            Type optionalTypeParam)
        {
            if (clazz == typeof(void)) {
                builder.Append("void");
                return builder;
            }

            if (!clazz.IsArray)
            {
                string assignedName = GetAssignedName(clazz);
                builder.Append(assignedName);
                if (optionalTypeParam != null)
                {
                    builder.Append("<");
                    AppendClassName(builder, optionalTypeParam, null);
                    builder.Append(">");
                }

                return builder;
            }

            AppendClassName(builder, clazz.GetElementType(), null);
            builder.Append("[]");
            return builder;
        }
#endif

        private static string GetAssignedName(Type clazz)
        {
            return clazz.Name;
        }
    }
} // end of namespace