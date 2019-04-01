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
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    /// A factory for row-limit processor instances.
    /// </summary>
    public class RowLimitProcessorFactoryForge
    {
        private readonly VariableMetaData numRowsVariableMetaData;
        private readonly VariableMetaData offsetVariableMetaData;
        private int currentRowLimit;
        private int currentOffset;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="rowLimitSpec">specification for row limit, or null if no row limit is defined</param>
        /// <param name="variableCompileTimeResolver">for retrieving variable state for use with row limiting</param>
        /// <param name="optionalContextName">context name</param>
        /// <throws>ExprValidationException exception</throws>
        public RowLimitProcessorFactoryForge(RowLimitSpec rowLimitSpec, VariableCompileTimeResolver variableCompileTimeResolver, string optionalContextName)
        {
            if (rowLimitSpec.NumRowsVariable != null)
            {
                numRowsVariableMetaData = variableCompileTimeResolver.Resolve(rowLimitSpec.NumRowsVariable);
                if (numRowsVariableMetaData == null)
                {
                    throw new ExprValidationException("Limit clause variable by name '" + rowLimitSpec.NumRowsVariable + "' has not been declared");
                }
                string message = VariableUtil.CheckVariableContextName(optionalContextName, numRowsVariableMetaData);
                if (message != null)
                {
                    throw new ExprValidationException(message);
                }
                if (!TypeHelper.IsNumeric(numRowsVariableMetaData.Type))
                {
                    throw new ExprValidationException("Limit clause requires a variable of numeric type");
                }
            }
            else
            {
                numRowsVariableMetaData = null;
                currentRowLimit = rowLimitSpec.NumRows.GetValueOrDefault(Int32.MaxValue);
                if (currentRowLimit < 0)
                {
                    currentRowLimit = Int32.MaxValue;
                }
            }

            if (rowLimitSpec.OptionalOffsetVariable != null)
            {
                offsetVariableMetaData = variableCompileTimeResolver.Resolve(rowLimitSpec.OptionalOffsetVariable);
                if (offsetVariableMetaData == null)
                {
                    throw new ExprValidationException("Limit clause variable by name '" + rowLimitSpec.OptionalOffsetVariable + "' has not been declared");
                }
                string message = VariableUtil.CheckVariableContextName(optionalContextName, offsetVariableMetaData);
                if (message != null)
                {
                    throw new ExprValidationException(message);
                }
                if (!TypeHelper.IsNumeric(offsetVariableMetaData.Type))
                {
                    throw new ExprValidationException("Limit clause requires a variable of numeric type");
                }
            }
            else
            {
                offsetVariableMetaData = null;
                if (rowLimitSpec.OptionalOffset != null) {
                    if (!rowLimitSpec.OptionalOffset.HasValue || rowLimitSpec.OptionalOffset.Value <= 0)
                    {
                        throw new ExprValidationException("Limit clause requires a positive offset");
                    }

                    currentOffset = rowLimitSpec.OptionalOffset.Value;
                }
                else
                {
                    currentOffset = 0;
                }
            }
        }

        public CodegenExpression Make(CodegenMethodScope parent, CodegenClassScope classScope)
        {
            CodegenExpression numRowsVariable = ConstantNull();
            if (numRowsVariableMetaData != null)
            {
                numRowsVariable = VariableDeployTimeResolver.MakeVariableField(numRowsVariableMetaData, classScope, this.GetType());
            }

            CodegenExpression offsetVariable = ConstantNull();
            if (offsetVariableMetaData != null)
            {
                offsetVariable = VariableDeployTimeResolver.MakeVariableField(offsetVariableMetaData, classScope, this.GetType());
            }

            CodegenMethod method = parent.MakeChild(typeof(RowLimitProcessorFactory), this.GetType(), classScope);
            method.Block
                    .DeclareVar(typeof(RowLimitProcessorFactory), "factory", NewInstance(typeof(RowLimitProcessorFactory)))
                    .ExprDotMethod(@Ref("factory"), "setNumRowsVariable", numRowsVariable)
                    .ExprDotMethod(@Ref("factory"), "setOffsetVariable", offsetVariable)
                    .ExprDotMethod(@Ref("factory"), "setCurrentRowLimit", Constant(currentRowLimit))
                    .ExprDotMethod(@Ref("factory"), "setCurrentOffset", Constant(currentOffset))
                    .MethodReturn(@Ref("factory"));
            return LocalMethod(method);
        }
    }
} // end of namespace