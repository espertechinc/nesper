///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.codegen.core
{
    public class CodeGenerationHelper
    {
        public static string CompliantName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                var genericArgumentAtoms = string.Join(
                    ",", genericArguments.Select(CompliantName));
                return type.FullName + "<" + genericArgumentAtoms + ">";
            }
            else if (type.IsArray)
            {
                return CompliantName(type.GetElementType()) + "[]";
            }
            else
            {
                return type.FullName;
            }
        }

        public static TextWriter AppendClassName(TextWriter textWriter, Type clazz, Type optionalTypeParam)
        {
            textWriter.Write(CompliantName(clazz));
            return textWriter;
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