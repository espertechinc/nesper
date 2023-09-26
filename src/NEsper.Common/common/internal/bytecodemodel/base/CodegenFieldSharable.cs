///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public interface CodegenFieldSharable
    {
        Type Type();

        CodegenExpression InitCtorScoped();
    }

    public class ProxyCodegenFieldSharable : CodegenFieldSharable
    {
        public Func<Type> ProcType;
        public Func<CodegenExpression> ProcInitCtorScoped;

        public Type Type()
        {
            return ProcType();
        }

        public CodegenExpression InitCtorScoped()
        {
            return ProcInitCtorScoped();
        }
    }
} // end of namespace