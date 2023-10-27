///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the TYPEOF(a) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprTypeofNode : ExprNodeBase,
        ExprFilterOptimizableNode
    {
        [JsonIgnore]
        [NonSerialized]
        private ExprTypeofNodeForge forge;
        [JsonIgnore]
        [NonSerialized]
        private ExprValidationContext exprValidationContext;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge => forge;

        public bool IsConstantResult => false;

        public Type Type => typeof(string);

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsFilterLookupEligible => true;

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                EventPropertyValueGetterForge eventPropertyForge = new ProxyEventPropertyValueGetterForge {
                    ProcEventBeanGetCodegen = (
                        beanExpression,
                        parent,
                        classScope) => {
                        var method = parent.MakeChild(typeof(string), GetType(), classScope)
                            .AddParam<EventBean>("bean");
                        method.Block.MethodReturn(ExprDotMethodChain(Ref("bean")).Get("EventType").Get("Name"));
                        return LocalMethod(method, beanExpression);
                    }
                };

                var serde = exprValidationContext.SerdeResolver.SerdeForFilter(
                    typeof(string),
                    exprValidationContext.StatementRawInfo);
                var eval = new ExprEventEvaluatorForgeFromProp(eventPropertyForge);
                return new ExprFilterSpecLookupableForge(
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(this),
                    eval,
                    null,
                    typeof(string),
                    true,
                    serde);
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            exprValidationContext = validationContext;

            if (ChildNodes.Length != 1) {
                throw new ExprValidationException(
                    "Typeof node must have 1 child expression node supplying the expression to test");
            }

            if (ChildNodes[0] is ExprStreamUnderlyingNode) {
                var stream = (ExprStreamUnderlyingNode)ChildNodes[0];
                forge = new ExprTypeofNodeForgeStreamEvent(this, stream.StreamId);
                return null;
            }

            if (ChildNodes[0] is ExprIdentNode) {
                var ident = (ExprIdentNode)ChildNodes[0];
                var streamNum = validationContext.StreamTypeService.GetStreamNumForStreamName(ident.FullUnresolvedName);
                if (streamNum != -1) {
                    forge = new ExprTypeofNodeForgeStreamEvent(this, streamNum);
                    return null;
                }

                var eventType = validationContext.StreamTypeService.EventTypes[ident.StreamId];
                var fragmentEventType = eventType.GetFragmentType(ident.ResolvedPropertyName);
                if (fragmentEventType != null) {
                    var getter = ((EventTypeSPI)eventType).GetGetterSPI(ident.ResolvedPropertyName);
                    forge = new ExprTypeofNodeForgeFragmentType(
                        this,
                        ident.StreamId,
                        getter,
                        fragmentEventType.FragmentType.Name);
                    return null;
                }
            }

            forge = new ExprTypeofNodeForgeInnerEval(this);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("typeof(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(')');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprTypeofNode;
        }
    }
} // end of namespace