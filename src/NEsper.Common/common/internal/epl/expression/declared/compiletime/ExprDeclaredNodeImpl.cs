///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    /// <summary>
    ///     Expression instance as declared elsewhere.
    /// </summary>
    public class ExprDeclaredNodeImpl : ExprNodeBase,
        ExprDeclaredNode,
        ExprDeclaredOrLambdaNode,
        ExprFilterOptimizableNode,
        ExprNodeInnerNodeProvider,
        ExprConstantNode
    {
        private static readonly string InternalValueStreamname = "esper_declared_expr_internal";

        [NonSerialized] private ExprForge _forge;
        [NonSerialized] private ExprValidationContext _exprValidationContext;
        private bool _allStreamIdsMatch;

        public ExprDeclaredNodeImpl(
            ExpressionDeclItem prototype,
            IList<ExprNode> chainParameters,
            ContextCompileTimeDescriptor contextDescriptor,
            ExprNode expressionBodyCopy)
        {
            PrototypeWVisibility = prototype;
            ChainParameters = chainParameters;
            ExpressionBodyCopy = expressionBodyCopy;

            // replace context-properties where they are currently identifiers
            if (contextDescriptor == null) {
                return;
            }

            var visitorWParent = new ExprNodeIdentVisitorWParent();
            expressionBodyCopy.Accept(visitorWParent);
            foreach (var pair in visitorWParent.IdentNodes) {
                var streamOrProp = pair.Second.StreamOrPropertyName;
                if (streamOrProp != null &&
                    contextDescriptor.ContextPropertyRegistry.IsContextPropertyPrefix(streamOrProp)) {
                    var context =
                        new ExprContextPropertyNodeImpl(pair.Second.UnresolvedPropertyName);
                    if (pair.First == null) {
                        ExpressionBodyCopy = context;
                    }
                    else {
                        ExprNodeUtilityModify.ReplaceChildNode(pair.First, pair.Second, context);
                    }
                }
            }
        }

        public string StringConstantWhenProvided => null;
        
        public bool IsConstantResult => false;

        public ExprNode ExpressionBodyCopy { get; private set; }

        public ExpressionDeclItem PrototypeWVisibility { get; }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public Type ConstantType {
            get {
                CheckValidated(_forge);
                return _forge.EvaluationType;
            }
        }

        public object ConstantValue => _forge.ExprEvaluator.Evaluate(null, true, null);

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public ExprNode Body => ExpressionBodyCopy;

        public IDictionary<string, int> GetOuterStreamNames(IDictionary<string, int> outerStreamNames)
        {
            CheckParameterCount();
            var prototype = PrototypeWVisibility;

            // determine stream ids for each parameter
            var streamParameters = new LinkedHashMap<string, int>();
            for (var param = 0; param < ChainParameters.Count; param++) {
                if (!(ChainParameters[param] is ExprIdentNode)) {
                    throw new ExprValidationException(
                        "Sub-selects in an expression declaration require passing only stream names as parameters");
                }

                var parameterName = ((ExprIdentNode) ChainParameters[param]).UnresolvedPropertyName;

                if (!outerStreamNames.TryGetValue(parameterName, out var streamIdFound)) {
                    throw new ExprValidationException(
                        "Failed validation of expression declaration '" +
                        prototype.Name +
                        "': Invalid parameter to expression declaration, parameter " +
                        param +
                        " is not the name of a stream in the query");
                }

                var prototypeName = prototype.ParametersNames[param];
                streamParameters.Put(prototypeName, streamIdFound);
            }

            return streamParameters;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _exprValidationContext = validationContext;

            var prototype = PrototypeWVisibility;
            if (prototype.IsAlias) {
                if (!ChainParameters.IsEmpty()) {
                    throw new ExprValidationException("Expression '" + prototype.Name + " is an expression-alias and does not allow parameters");
                }
                try {
                    ExpressionBodyCopy = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.ALIASEXPRBODY,
                        ExpressionBodyCopy,
                        validationContext);
                }
                catch (ExprValidationException ex) {
                    var message = "Failed to validate expression alias '" + prototype.Name + "': " + ex.Message;
                    throw new ExprValidationException(message, ex);
                }

                _forge = ExpressionBodyCopy.Forge;
                return null;
            }

            if (_forge != null) {
                return null; // already evaluated
            }

            if (ChildNodes.Length > 0) {
                throw new IllegalStateException("Execution node has its own child nodes");
            }

            // validate chain
            IList<ExprNode> validated = new List<ExprNode>();
            foreach (var expr in ChainParameters) {
                validated.Add(
                    ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.DECLAREDEXPRPARAM,
                        expr,
                        validationContext));
            }

            ChainParameters = validated;

            // validate parameter count
            CheckParameterCount();

            // collect event and value (non-event) parameters
            var valueParameters = new List<int>();
            var eventParameters = new List<int>();
            for (var i = 0; i < prototype.ParametersNames.Length; i++) {
                var parameter = ChainParameters[i];
                if (parameter is ExprWildcard) {
                    if (validationContext.StreamTypeService.EventTypes.Length != 1) {
                        throw new ExprValidationException("Expression '" + prototype.Name + "' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead");
                    }
                }
                if (IsEventProviding(parameter, validationContext)) {
                    eventParameters.Add(i);
                } else {
                    valueParameters.Add(i);
                }
            }

            // determine value event type for holding non-event parameter values, if any
            ObjectArrayEventType valueEventType = null;
            var valueExpressions = new List<ExprNode>(valueParameters.Count);
            if (!valueParameters.IsEmpty()) {
                var valuePropertyTypes = new LinkedHashMap<string, object>();
                foreach (var index in valueParameters) {
                    var name = prototype.ParametersNames[index];
                    var expr = ChainParameters[index];
                    var result = Boxing.GetBoxedType(expr.Forge.EvaluationType);
                    valuePropertyTypes.Put(name, result);
                    valueExpressions.Add(expr);
                }

                valueEventType = ExprDotNodeUtility.MakeTransientOAType(
                    PrototypeWVisibility.Name,
                    valuePropertyTypes,
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);
            }

            // create context for expression body
            var numEventTypes = eventParameters.Count + (valueEventType == null ? 0 : 1);
            var eventTypes = new EventType[numEventTypes];
            var streamNames = new string[numEventTypes];
            var isIStreamOnly = new bool[numEventTypes];
            var eventEnumerationForges = new ExprEnumerationForge[numEventTypes];
            _allStreamIdsMatch = true;

            var offsetEventType = 0;
            if (valueEventType != null) {
                offsetEventType = 1;
                eventTypes[0] = valueEventType;
                streamNames[0] = InternalValueStreamname;
                isIStreamOnly[0] = true;
                _allStreamIdsMatch = false;
            }

            var forceOptionalStream = false;
            foreach (var index in eventParameters) {
                var parameter = ChainParameters[index];
                streamNames[offsetEventType] = prototype.ParametersNames[index];
                int streamId;
                bool istreamOnlyFlag;
                ExprEnumerationForge forge;

                if (parameter is ExprEnumerationForgeProvider) {
                    var enumerationForgeProvider = (ExprEnumerationForgeProvider) parameter;
                    var desc = enumerationForgeProvider.GetEnumerationForge(
                        validationContext.StreamTypeService, validationContext.ContextDescriptor);
                    forge = desc.Forge;
                    streamId = desc.DirectIndexStreamNumber;
                    istreamOnlyFlag = desc.IsIstreamOnly;
                } else {
                    forge = (ExprEnumerationForge) parameter.Forge;
                    istreamOnlyFlag = false;
                    streamId = -1;
                    forceOptionalStream = true; // since they may return null, i.e. subquery returning no row or multiple rows etc.
                }

                isIStreamOnly[offsetEventType] = istreamOnlyFlag;
                eventEnumerationForges[offsetEventType] = forge;
                eventTypes[offsetEventType] = forge.GetEventTypeSingle(validationContext.StatementRawInfo, validationContext.StatementCompileTimeService);

                if (streamId != index) {
                    _allStreamIdsMatch = false;
                }
                offsetEventType++;
            }

            var streamTypeService = validationContext.StreamTypeService;
            var optionalStream = forceOptionalStream || streamTypeService.IsOptionalStreams;
            var copyTypes = new StreamTypeServiceImpl(
                eventTypes,
                streamNames,
                isIStreamOnly,
                streamTypeService.IsOnDemandStreams,
                optionalStream);

            copyTypes.RequireStreamNames = true;

            // validate expression body in this context
            try {
                var expressionBodyContext = new ExprValidationContext(copyTypes, validationContext);
                ExpressionBodyCopy = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.DECLAREDEXPRBODY,
                    ExpressionBodyCopy,
                    expressionBodyContext);
            }
            catch (ExprValidationException ex) {
                var message = "Failed to validate expression declaration '" + prototype.Name + "': " + ex.Message;
                throw new ExprValidationException(message, ex);
            }

            // analyze child node
            var summaryVisitor = new ExprNodeSummaryVisitor();
            ExpressionBodyCopy.Accept(summaryVisitor);
            var isCache = !(summaryVisitor.HasAggregation || summaryVisitor.HasPreviousPrior);
            isCache &= validationContext.StatementCompileTimeService.Configuration.Compiler.Execution
                .IsEnabledDeclaredExprValueCache;

            // determine a suitable evaluation
            var audit = AuditEnum.EXPRDEF.GetAudit(validationContext.Annotations) != null;
            var statementName = validationContext.StatementName;
            if (ExpressionBodyCopy.Forge.ForgeConstantType.IsConstant) {
                // pre-evaluated
                _forge = new ExprDeclaredForgeConstant(
                    this,
                    ExpressionBodyCopy.Forge.EvaluationType,
                    prototype,
                    ExpressionBodyCopy.Forge.ExprEvaluator.Evaluate(null, true, null),
                    audit,
                    statementName);
            }
            else if (valueEventType == null &&
                     prototype.ParametersNames.Length == 0 ||
                     _allStreamIdsMatch && prototype.ParametersNames.Length == streamTypeService.EventTypes.Length) {
                _forge = new ExprDeclaredForgeNoRewrite(
                    this, ExpressionBodyCopy.Forge, isCache, audit, statementName);
            }
            else if (valueEventType == null) {
                _forge = new ExprDeclaredForgeRewrite(
                    this, ExpressionBodyCopy.Forge, isCache, eventEnumerationForges, audit, statementName);
            }
            else {
                // cache is always false
                _forge = new ExprDeclaredForgeRewriteWValue(
                    this, ExpressionBodyCopy.Forge, false, audit, statementName, eventEnumerationForges, valueEventType, valueExpressions);
            }

            return null;
        }

        private bool IsEventProviding(
            ExprNode parameter,
            ExprValidationContext validationContext)
        {
            if (parameter is ExprEnumerationForgeProvider) {
                var provider = (ExprEnumerationForgeProvider) parameter;
                var desc = provider.GetEnumerationForge(
                    validationContext.StreamTypeService,
                    validationContext.ContextDescriptor);

                var eventType = desc?.Forge.GetEventTypeSingle(validationContext.StatementRawInfo, validationContext.StatementCompileTimeService);
                return eventType != null;
            }

            var forge = parameter.Forge;
            var enumerationForge = forge as ExprEnumerationForge;
            return enumerationForge?.GetEventTypeSingle(
                validationContext.StatementRawInfo, validationContext.StatementCompileTimeService) != null;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprDeclaredNodeImpl)) {
                return false;
            }

            var otherExprCaseNode = (ExprDeclaredNodeImpl) node;
            return ExprNodeUtilityCompare.DeepEquals(ExpressionBodyCopy, otherExprCaseNode, false);
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            AcceptNoVisitParams(visitor);
            if (WalkParams(visitor)) {
                ExprNodeUtilityQuery.AcceptParams(visitor, ChainParameters);
            }
        }

        public void AcceptNoVisitParams(ExprNodeVisitor visitor) {
            base.Accept(visitor);
            if (ChildNodes.Length == 0) {
                ExpressionBodyCopy.Accept(visitor);
            }
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            AcceptNoVisitParams(visitor);
            if (WalkParams(visitor)) {
                ExprNodeUtilityQuery.AcceptParams(visitor, ChainParameters);
            }
        }

        public void AcceptNoVisitParams(ExprNodeVisitorWithParent visitor) {
            base.Accept(visitor);
            if (ChildNodes.Length == 0) {
                ExpressionBodyCopy.Accept(visitor);
            }
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            if (visitor.IsVisit(this) && ChildNodes.Length == 0) {
                ExpressionBodyCopy.Accept(visitor);
            }
        }

        public ExpressionDeclItem Prototype => PrototypeWVisibility;

        public IList<ExprNode> ChainParameters { get; private set; }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsValidated => _forge != null;

        public bool FilterLookupEligible => _forge is ExprDeclaredForgeBase;

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                if (!(_forge is ExprDeclaredForgeBase declaredForge) || _forge.EvaluationType.IsNullTypeSafe()) {
                    return null;
                }
                
                var forgeX = declaredForge.InnerForge;
                var serde = _exprValidationContext.SerdeResolver.SerdeForFilter(
                    forgeX.EvaluationType, _exprValidationContext.StatementRawInfo);
                return new ExprFilterSpecLookupableForge(
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(this),
                    new DeclaredNodeEventPropertyGetterForge(forgeX),
                    null, 
                    forgeX.EvaluationType,
                    true,
                    serde);
            }
        }

        public IList<ExprNode> AdditionalNodes => ChainParameters;

        private void CheckParameterCount()
        {
            var prototype = PrototypeWVisibility;
            if (ChainParameters.Count != prototype.ParametersNames.Length) {
                throw new ExprValidationException(
                    "Parameter count mismatches for declared expression '" +
                    prototype.Name +
                    "', expected " +
                    prototype.ParametersNames.Length +
                    " parameters but received " +
                    ChainParameters.Count +
                    " parameters");
            }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            var prototype = PrototypeWVisibility;
            writer.Write(prototype.Name);

            if (prototype.IsAlias) {
                return;
            }

            writer.Write("(");
            var delimiter = "";
            foreach (var parameter in ChainParameters) {
                writer.Write(delimiter);
                parameter.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
                delimiter = ",";
            }

            writer.Write(")");
        }

        private bool WalkParams(ExprNodeVisitor visitor)
        {
            // we do not walk parameters when all stream ids match and the visitor skips declared-expression parameters
            // this is because parameters are streams and don't need to be collected by some visitors
            return visitor.IsWalkDeclExprParam || !_allStreamIdsMatch;
        }

        private bool WalkParams(ExprNodeVisitorWithParent visitor)
        {
            // we do not walk parameters when all stream ids match and the visitor skips declared-expression parameters
            // this is because parameters are streams and don't need to be collected by some visitors
            return visitor.IsWalkDeclExprParam || !_allStreamIdsMatch;
        }


        private class DeclaredNodeEventPropertyGetterForge : ExprEventEvaluatorForge
        {
            private readonly ExprForge _exprForge;

            public DeclaredNodeEventPropertyGetterForge(ExprForge exprForge)
            {
                this._exprForge = exprForge;
            }

            public CodegenExpression EventBeanWithCtxGet(
                CodegenExpression beanExpression,
                CodegenExpression ctxExpression,
                CodegenMethodScope parent,
                CodegenClassScope classScope)
            {
                if (_exprForge.EvaluationType.IsNullTypeSafe()) {
                    return ConstantNull();
                }
                
                var method = parent.MakeChild(_exprForge.EvaluationType, GetType(), classScope)
                    .AddParam<EventBean>("bean");
                var exprMethod = CodegenLegoMethodExpression.CodegenExpression(_exprForge, method, classScope);

                method.Block
                    .DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .AssignArrayElement(Ref("events"), Constant(0), Ref("bean"))
                    .MethodReturn(LocalMethod(exprMethod, Ref("events"), ConstantTrue(), ConstantNull()));

                return LocalMethod(method, beanExpression);
            }
        }
    }
} // end of namespace