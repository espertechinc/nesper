///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    /// <summary>
    /// Works in conjunction with <seealso cref = "SelectExprProcessor"/> to present
    /// a result as an object array for 'natural' delivery.
    /// </summary>
    public class BindProcessorForge
    {
        private ExprForge[] expressionForges;
        private Type[] expressionTypes;
        private string[] columnNamesAssigned;

        public BindProcessorForge(
            SelectExprProcessorForge synthetic,
            SelectClauseElementCompiled[] selectionList,
            EventType[] typesPerStream,
            string[] streamNames,
            TableCompileTimeResolver tableService)
        {
            var expressions = new List<ExprForge>();
            var types = new List<Type>();
            var columnNames = new List<string>();
            foreach (var element in selectionList) {
                // handle wildcards by outputting each stream's underlying event
                if (element is SelectClauseElementWildcard) {
                    for (var i = 0; i < typesPerStream.Length; i++) {
                        var returnType = typesPerStream[i].UnderlyingType;
                        var tableMetadata = tableService.ResolveTableFromEventType(typesPerStream[i]);
                        ExprForge forge;
                        if (tableMetadata != null) {
                            forge = new BindProcessorStreamTable(i, returnType, tableMetadata);
                        }
                        else {
                            forge = new BindProcessorStream(i, returnType);
                        }

                        expressions.Add(forge);
                        types.Add(returnType);
                        columnNames.Add(streamNames[i]);
                    }
                }
                else if (element is SelectClauseStreamCompiledSpec streamSpec) {
                    // handle stream wildcards by outputting the stream underlying event
                    var type = typesPerStream[streamSpec.StreamNumber];
                    var returnType = type.UnderlyingType;
                    var tableMetadata = tableService.ResolveTableFromEventType(type);
                    ExprForge forge;
                    if (tableMetadata != null) {
                        forge = new BindProcessorStreamTable(streamSpec.StreamNumber, returnType, tableMetadata);
                    }
                    else {
                        forge = new BindProcessorStream(streamSpec.StreamNumber, returnType);
                    }

                    expressions.Add(forge);
                    types.Add(returnType);
                    columnNames.Add(streamNames[streamSpec.StreamNumber]);
                }
                else if (element is SelectClauseExprCompiledSpec expr) {
                    // handle expressions
                    var forge = expr.SelectExpression.Forge;
                    expressions.Add(forge);
                    var evaluationType = forge.EvaluationType;
                    types.Add(evaluationType == null ? null : evaluationType);
                    if (expr.AssignedName != null) {
                        columnNames.Add(expr.AssignedName);
                    }
                    else {
                        columnNames.Add(
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression));
                    }
                }
                else {
                    throw new IllegalStateException(
                        "Unrecognized select expression element of type " + element.GetType());
                }
            }

            expressionForges = expressions.ToArray();
            expressionTypes = types.ToArray();
            columnNamesAssigned = columnNames.ToArray();
        }

        public CodegenMethod ProcessCodegen(
            CodegenMethod processMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = processMethod.MakeChild(typeof(object[]), GetType(), codegenClassScope);
            var block = methodNode.Block.DeclareVar<object[]>(
                "parameters",
                NewArrayByLength(typeof(object), Constant(expressionForges.Length)));
            for (var i = 0; i < expressionForges.Length; i++) {
                block.AssignArrayElement(
                    "parameters",
                    Constant(i),
                    CodegenLegoMayVoid.ExpressionMayVoid(
                        typeof(object),
                        expressionForges[i],
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
            }

            block.MethodReturn(Ref("parameters"));
            return methodNode;
        }

        public ExprForge[] ExpressionForges => expressionForges;

        public Type[] ExpressionTypes => expressionTypes;

        public string[] ColumnNamesAssigned => columnNamesAssigned;
    }
} // end of namespace