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

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodeGenerationHelper
    {
        public static StringBuilder AppendClassName(
            StringBuilder builder,
            Type clazz)
        {
            if (clazz == typeof(void))
            {
                builder.Append("void");
                return builder;
            }

            if (clazz.IsArray) {
                AppendClassName(builder, clazz.GetElementType());
                builder.Append("[]");
                return builder;
            }

            builder.Append(clazz.Name);

            if (clazz.IsGenericType) {
                var delimiter = "";

                builder.Append('<');
                foreach (var genericArgument in clazz.GetGenericArguments()) {
                    builder.Append(delimiter);
                    AppendClassName(builder, genericArgument);
                    delimiter = ",";
                }

                builder.Append('>');
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