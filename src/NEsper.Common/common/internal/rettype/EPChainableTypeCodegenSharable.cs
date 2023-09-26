///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;

namespace com.espertech.esper.common.@internal.rettype
{
    public class EPChainableTypeCodegenSharable : CodegenFieldSharable
    {
        private readonly CodegenClassScope classScope;
        private readonly EPChainableType epType;

        public EPChainableTypeCodegenSharable(
            EPChainableType epType,
            CodegenClassScope classScope)
        {
            this.epType = epType;
            this.classScope = classScope;
        }

        public Type Type()
        {
            return typeof(EPChainableType);
        }

        public CodegenExpression InitCtorScoped()
        {
            return epType.Codegen(
                classScope.NamespaceScope.InitMethod,
                classScope,
                EPStatementInitServicesConstants.REF);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (EPChainableTypeCodegenSharable)o;

            return epType?.Equals(that.epType) ?? that.epType == null;
        }

        public override int GetHashCode()
        {
            return epType != null ? epType.GetHashCode() : 0;
        }

        public class CodegenSharableSerdeName
        {
            public static readonly CodegenSharableSerdeName OBJECTARRAYMAYNULLNULL =
                new CodegenSharableSerdeName("ObjectArrayMayNullNull");

            private CodegenSharableSerdeName(string methodName)
            {
                MethodName = methodName;
            }

            public string MethodName { get; }
        }
    }
} // end of namespace