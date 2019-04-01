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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde
{
    public class CodegenSharableSerdeClassTyped : CodegenFieldSharable
    {
        private readonly CodegenSharableSerdeName name;
        private readonly Type valueType;

        public CodegenSharableSerdeClassTyped(CodegenSharableSerdeName name, Type valueType)
        {
            this.name = name;
            this.valueType = valueType;
        }

        public Type Type()
        {
            return typeof(DataInputOutputSerdeWCollation);
        }

        public CodegenExpression InitCtorScoped()
        {
            return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                .Add(EPStatementInitServicesConstants.GETDATAINPUTOUTPUTSERDEPROVIDER)
                .Add(name.methodName, Constant(valueType));
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (CodegenSharableSerdeClassTyped) o;

            if (name != that.name) {
                return false;
            }

            return valueType == that.valueType;
        }

        public override int GetHashCode()
        {
            var result = name.GetHashCode();
            result = 31 * result + valueType.GetHashCode();
            return result;
        }

        public class CodegenSharableSerdeName
        {
            public static readonly CodegenSharableSerdeName VALUE_NULLABLE =
                new CodegenSharableSerdeName("valueNullable");

            public static readonly CodegenSharableSerdeName REFCOUNTEDSET =
                new CodegenSharableSerdeName("refCountedSet");

            public static readonly CodegenSharableSerdeName SORTEDREFCOUNTEDSET =
                new CodegenSharableSerdeName("sortedRefCountedSet");

            private CodegenSharableSerdeName(string methodName)
            {
                MethodName = methodName;
            }

            public string MethodName { get; }
        }
    }
} // end of namespace