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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.compiletime
{
    public class VariableReaderPerCPCodegenFieldSharable : CodegenFieldSharable
    {
        private readonly VariableMetaData metaWVisibility;

        public VariableReaderPerCPCodegenFieldSharable(VariableMetaData metaWVisibility)
        {
            this.metaWVisibility = metaWVisibility;
        }

        public Type Type()
        {
            return typeof(IDictionary<int, VariableReader>);
        }

        public CodegenExpression InitCtorScoped()
        {
            return StaticMethod(
                typeof(VariableDeployTimeResolver),
                "ResolveVariableReaderPerCP",
                Constant(metaWVisibility.VariableName),
                Constant(metaWVisibility.VariableVisibility),
                Constant(metaWVisibility.VariableModuleName),
                Constant(metaWVisibility.OptionalContextName),
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

            var that = (VariableReaderPerCPCodegenFieldSharable) o;

            return metaWVisibility.VariableName.Equals(that.metaWVisibility.VariableName);
        }

        public override int GetHashCode()
        {
            return metaWVisibility.VariableName.GetHashCode();
        }
    }
} // end of namespace