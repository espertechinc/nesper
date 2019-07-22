///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenCtor : CodegenMethod
    {
        public CodegenCtor(
            Type generator,
            bool includeDebugSymbols,
            IList<CodegenTypedParam> @params)
            : base(null, null, generator, CodegenSymbolProviderEmpty.INSTANCE, new CodegenScope(includeDebugSymbols))
        {
            CtorParams = @params;
        }

        public CodegenCtor(
            Type generator,
            CodegenClassScope classScope,
            IList<CodegenTypedParam> @params)
            : base(null, null, generator, CodegenSymbolProviderEmpty.INSTANCE, new CodegenScope(classScope.IsDebug))
        {
            CtorParams = @params;
        }

        public IList<CodegenTypedParam> CtorParams { get; }

        public override void MergeClasses(ISet<Type> classes)
        {
            base.MergeClasses(classes);
            foreach (var param in CtorParams) {
                param.MergeClasses(classes);
            }
        }
    }
} // end of namespace