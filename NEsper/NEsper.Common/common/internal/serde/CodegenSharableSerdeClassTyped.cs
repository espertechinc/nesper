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

        public CodegenSharableSerdeClassTyped(
            CodegenSharableSerdeName name,
            Type valueType)
        {
            this.name = name;
            this.valueType = valueType;
        }

        public Type Type()
        {
            return typeof(DataInputOutputSerdeWCollation<object>);
        }

        public CodegenExpression InitCtorScoped()
        {
            return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                .Get(EPStatementInitServicesConstants.DATAINPUTOUTPUTSERDEPROVIDER)
                .Add(name.MethodName, name.MethodTypeArgs, Constant(valueType));
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
                new CodegenSharableSerdeName("ValueNullable", typeof(object));

            public static readonly CodegenSharableSerdeName REFCOUNTEDSET =
                new CodegenSharableSerdeName("RefCountedSet", typeof(object));

            public static readonly CodegenSharableSerdeName SORTEDREFCOUNTEDSET =
                new CodegenSharableSerdeName("SortedRefCountedSet", typeof(object));

            private CodegenSharableSerdeName(string methodName, params Type[] methodTypeArgs)
            {
                MethodName = methodName;
                MethodTypeArgs = methodTypeArgs;
            }

            public string MethodName { get; }

            public Type[] MethodTypeArgs { get; set; }
        }
    }
} // end of namespace