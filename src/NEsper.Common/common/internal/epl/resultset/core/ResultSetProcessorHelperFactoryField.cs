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

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetProcessorHelperFactoryField : CodegenFieldSharable
    {
        public static readonly ResultSetProcessorHelperFactoryField INSTANCE =
            new ResultSetProcessorHelperFactoryField();

        private ResultSetProcessorHelperFactoryField()
        {
        }

        public Type Type()
        {
            return typeof(ResultSetProcessorHelperFactory);
        }

        public CodegenExpression InitCtorScoped()
        {
            return ExprDotName(
                EPStatementInitServicesConstants.REF,
                EPStatementInitServicesConstants.RESULTSETPROCESSORHELPERFACTORY);
        }
    }
} // end of namespace