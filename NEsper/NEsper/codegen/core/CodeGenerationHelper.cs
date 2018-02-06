///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.codegen.core
{
    public class CodeGenerationHelper
    {
        public static StringBuilder AppendClassName(
            StringBuilder builder, Type clazz, Type optionalTypeParam, IDictionary<Type, string> imports)
        {
            if (!clazz.IsArray)
            {
                var assignedName = GetAssignedName(clazz, imports);
                builder.Append(assignedName);
                if (optionalTypeParam != null)
                {
                    builder.Append("<");
                    AppendClassName(builder, optionalTypeParam, null, imports);
                    builder.Append(">");
                }

                return builder;
            }

            AppendClassName(builder, clazz.GetElementType(), null, imports);
            builder.Append("[]");
            return builder;
        }

        private static string GetAssignedName(Type clazz, IDictionary<Type, string> imports)
        {
            var assigned = imports.Get(clazz);
            if (assigned != null)
            {
                return assigned;
            }

            return clazz.FullName;
        }
    }
} // end of namespace