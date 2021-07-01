///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public interface MultiKeyClassRef
    {
        NameOrType ClassNameMK { get; }
        Type[] MKTypes { get; }

        CodegenExpression GetExprMKSerde(
            CodegenMethod method,
            CodegenClassScope classScope);
    }

    public class NameOrType
    {
        public string Name { get; }
        public Type Type { get; }

        public NameOrType(string name)
        {
            Name = name;
            Type = null;
        }

        public NameOrType(Type type)
        {
            Name = null;
            Type = type;
        }
    }
} // end of namespace