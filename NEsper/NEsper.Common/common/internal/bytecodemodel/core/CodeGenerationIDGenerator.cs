///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodeGenerationIDGenerator
    {
        public static string GenerateClassNameSimple(Type interfaceClass, string postfix)
        {
            return interfaceClass.Name + "_" + postfix;
        }

        public static string GenerateClassNameWithNamespace(string @namespace, Type interfaceClass, string postfix)
        {
            var simple = GenerateClassNameSimple(interfaceClass, postfix);
            if (string.IsNullOrEmpty(@namespace)) {
                return simple;
            }

            return @namespace + "." + simple;
        }
    }
} // end of namespace