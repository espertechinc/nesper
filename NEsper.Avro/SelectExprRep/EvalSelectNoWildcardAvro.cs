///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.typable;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace NEsper.Avro.SelectExprRep
{
    public class EvalSelectNoWildcardAvro : SelectExprProcessorForge
    {
        private readonly SelectExprForgeContext _selectExprForgeContext;
        private readonly AvroEventType _resultEventTypeAvro;
        private readonly ExprForge[] _forges;

        public EvalSelectNoWildcardAvro(
            SelectExprForgeContext selectExprForgeContext,
            ExprForge[] exprForges,
            EventType resultEventTypeAvro,
            string statementName)
        {
            _selectExprForgeContext = selectExprForgeContext;
            _resultEventTypeAvro = (AvroEventType) resultEventTypeAvro;

            _forges = new ExprForge[selectExprForgeContext.ExprForges.Length];
            var typeWidenerCustomizer =
                selectExprForgeContext.EventTypeAvroHandler.GetTypeWidenerCustomizer(resultEventTypeAvro);
            for (var i = 0; i < _forges.Length; i++) {
                _forges[i] = selectExprForgeContext.ExprForges[i];
                var forge = exprForges[i];
                var forgeEvaluationType = forge.EvaluationType;

                if (forge is ExprEvalByGetterFragment) {
                    _forges[i] = HandleFragment((ExprEvalByGetterFragment) forge);
                }
                else if (forge is ExprEvalStreamInsertUnd) {
                    var und = (ExprEvalStreamInsertUnd) forge;
                    _forges[i] =
                        new SelectExprInsertEventBeanFactory.ExprForgeStreamUnderlying(und.StreamNum, typeof(object));
                }
                else if (forge is SelectExprProcessorTypableMapForge) {
                    var typableMap = (SelectExprProcessorTypableMapForge) forge;
                    _forges[i] = new SelectExprProcessorEvalAvroMapToAvro(
                        typableMap.InnerForge,
                        ((AvroEventType) resultEventTypeAvro).SchemaAvro,
                        selectExprForgeContext.ColumnNames[i]);
                }
                else if (forge is ExprEvalStreamInsertNamedWindow) {
                    var nw = (ExprEvalStreamInsertNamedWindow) forge;
                    _forges[i] =
                        new SelectExprInsertEventBeanFactory.ExprForgeStreamUnderlying(nw.StreamNum, typeof(object));
                }
                else if (forgeEvaluationType != null && forgeEvaluationType.IsArray) {
                    var widener = TypeWidenerFactory.GetArrayToCollectionCoercer(forgeEvaluationType.GetElementType());
                    var resultType = typeof(ICollection<object>);
                    _forges[i] = new SelectExprProcessorEvalAvroArrayCoercer(forge, widener, resultType);
                }
                else {
                    var propertyName = selectExprForgeContext.ColumnNames[i];
                    var propertyType = resultEventTypeAvro.GetPropertyType(propertyName);
                    TypeWidenerSPI widener;
                    try {
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            propertyName,
                            forgeEvaluationType,
                            propertyType,
                            propertyName,
                            true,
                            typeWidenerCustomizer,
                            statementName);
                    }
                    catch (TypeWidenerException ex) {
                        throw new ExprValidationException(ex.Message, ex);
                    }

                    if (widener != null) {
                        _forges[i] = new SelectExprProcessorEvalAvroArrayCoercer(forge, widener, propertyType);
                    }
                }
            }
        }

        public EventType ResultEventType {
            get => _resultEventTypeAvro;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var schema = codegenClassScope.NamespaceScope.AddDefaultFieldUnshared(
                true,
                typeof(RecordSchema),
                StaticMethod(
                    typeof(AvroSchemaUtil),
                    "ResolveRecordSchema",
                    EventTypeUtility.ResolveTypeCodegen(_resultEventTypeAvro, EPStatementInitServicesConstants.REF)));
            var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar<GenericRecord>(
                    "record",
                    NewInstance(typeof(GenericRecord), schema));
            for (var i = 0; i < _selectExprForgeContext.ColumnNames.Length; i++) {
                var expression = _forges[i].EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope);
                block.Expression(
                    StaticMethod(
                        typeof(GenericRecordExtensions), 
                        "Put", 
                        Ref("record"),
                        Constant(_selectExprForgeContext.ColumnNames[i]),
                        expression));
            }

            block.MethodReturn(
                ExprDotMethod(
                    eventBeanFactory,
                    "AdapterForTypedAvro",
                    Ref("record"),
                    resultEventType));
            return methodNode;
        }

        private ExprForge HandleFragment(ExprEvalByGetterFragment eval)
        {
            if (eval.EvaluationType == typeof(GenericRecord[])) {
                return new SelectExprProcessorEvalByGetterFragmentAvroArray(
                    eval.StreamNum,
                    eval.Getter,
                    typeof(ICollection<object>));
            }

            if (eval.EvaluationType == typeof(GenericRecord)) {
                return new SelectExprProcessorEvalByGetterFragmentAvro(
                    eval.StreamNum,
                    eval.Getter,
                    typeof(GenericRecord));
            }

            throw new EPException("Unrecognized return type " + eval.EvaluationType + " for use with Avro");
        }
    }
} // end of namespace