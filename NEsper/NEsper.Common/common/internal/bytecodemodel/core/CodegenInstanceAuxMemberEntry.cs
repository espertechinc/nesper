///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenInstanceAuxMemberEntry
    {
        public CodegenInstanceAuxMemberEntry(
            string name,
            Type clazz,
            CodegenExpression initializer)
        {
            Name = name;
            Clazz = clazz;
            Initializer = initializer;
        }

        public string Name { get; }

        public Type Clazz { get; }

        public CodegenExpression Initializer { get; }
    }
} // end of namespace