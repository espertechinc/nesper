///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    /// <summary>
    /// Expression instance as declared elsewhere.
    /// </summary>
    public partial class ExprDeclaredNodeImpl : ExprNodeBase,
        ExprDeclaredNode,
        ExprDeclaredOrLambdaNode,
        ExprFilterOptimizableNode,
        ExprNodeInnerNodeProvider,
        ExprConstantNode
    {
        private const string INTERNAL_VALUE_STREAMNAME = "esper_declared_expr_internal";
        
        private readonly ExpressionDeclItem prototypeWVisibility;
        private IList<ExprNode> chainParameters;
        [NonSerialized] private ExprForge forge;
        private ExprNode expressionBodyCopy;
        [NonSerialized] private ExprValidationContext exprValidationContext;
        private bool allStreamIdsMatch;

        public ExprDeclaredNodeImpl(
            ExpressionDeclItem prototype,
            IList<ExprNode> chainParameters,
            ContextCompileTimeDescriptor contextDescriptor,
            ExprNode expressionBodyCopy)
        {
            prototypeWVisibility = prototype;
            this.chainParameters = chainParameters;
            this.expressionBodyCopy = expressionBodyCopy;
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
                    var context = new ExprContextPropertyNodeImpl(pair.Second.UnresolvedPropertyName);
                    if (pair.First == null) {
                        this.expressionBodyCopy = context;
                    }
                    else {
                        ExprNodeUtilityModify.ReplaceChildNode(pair.First, pair.Second, context);
                    }
                }
            }
        }

        public bool IsValidated => forge != null;

        public bool ConstantAvailable => forge != null;

        public string StringConstantWhenProvided => null;
        
        public IDictionary<string, int> GetOuterStreamNames(IDictionary<string, int> outerStreamNames)
        {
            CheckParameterCount();
            var prototype = prototypeWVisibility;
            // determine stream ids for each parameter
            var streamParameters = new LinkedHashMap<string, int>();
            for (var param = 0; param < chainParameters.Count; param++) {
                if (!(chainParameters[param] is ExprIdentNode)) {
                    throw new ExprValidationException(
                        "Sub-selects in an expression declaration require passing only stream names as parameters");
                }

                var parameterName = ((ExprIdentNode)chainParameters[param]).UnresolvedPropertyName;
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
            exprValidationContext = validationContext;
            var prototype = prototypeWVisibility;
            if (prototype.IsAlias) {
                if (!chainParameters.IsEmpty()) {
                    throw new ExprValidationException(
                        "Expression '" + prototype.Name + " is an expression-alias and does not allow parameters");
                }

                try {
                    expressionBodyCopy = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.ALIASEXPRBODY,
                        expressionBodyCopy,
                        validationContext);
                }
                catch (ExprValidationException ex) {
                    var message = "Failed to validate expression alias '" + prototype.Name + "': " + ex.Message;
                    throw new ExprValidationException(message, ex);
                }

                forge = expressionBodyCopy.Forge;
                return null;
            }

            if (forge != null) {
                return null; // already evaluated
            }

            if (ChildNodes.Length > 0) {
                throw new IllegalStateException("Execution node has its own child nodes");
            }

            // validate chain
            IList<ExprNode> validated = new List<ExprNode>();
            foreach (var expr in chainParameters) {
                validated.Add(
                    ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.DECLAREDEXPRPARAM,
                        expr,
                        validationContext));
            }

            chainParameters = validated;
            // validate parameter count
            CheckParameterCount();
            // collect event and value (non-event) parameters
            IList<int?> valueParameters = new List<int?>();
            IList<int?> eventParameters = new List<int?>();
            for (var i = 0; i < prototype.ParametersNames.Length; i++) {
                var parameter = chainParameters[i];
                if (parameter is ExprWildcard) {
                    if (validationContext.StreamTypeService.EventTypes.Length != 1) {
                        throw new ExprValidationException(
                            "Expression '" +
                            prototype.Name +
                            "' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead");
                    }
                }

                if (IsEventProviding(parameter, validationContext)) {
                    eventParameters.Add(i);
                }
                else {
                    valueParameters.Add(i);
                }
            }

            // determine value event type for holding non-event parameter values, if any
            ObjectArrayEventType valueEventType = null;
            IList<ExprNode> valueExpressions = new List<ExprNode>(valueParameters.Count);
            if (!valueParameters.IsEmpty()) {
                IDictionary<string, object> valuePropertyTypes = new LinkedHashMap<string, object>();
                foreach (int index in valueParameters) {
                    var name = prototype.ParametersNames[index];
                    var expr = chainParameters[index];
                    var type = expr.Forge.EvaluationType;
                    var result = type.GetBoxedType();
                    valuePropertyTypes.Put(name, result);
                    valueExpressions.Add(expr);
                }

                valueEventType = ExprDotNodeUtility.MakeTransientOAType(
                    prototypeWVisibility.Name,
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
            allStreamIdsMatch = true;
            var offsetEventType = 0;
            if (valueEventType != null) {
                offsetEventType = 1;
                eventTypes[0] = valueEventType;
                streamNames[0] = INTERNAL_VALUE_STREAMNAME;
                isIStreamOnly[0] = true;
                allStreamIdsMatch = false;
            }

            var forceOptionalStream = false;
            foreach (int index in eventParameters) {
                var parameter = chainParameters[index];
                streamNames[offsetEventType] = prototype.ParametersNames[index];
                int streamId;
                bool istreamOnlyFlag;
                ExprEnumerationForge forge;
                if (parameter is ExprEnumerationForgeProvider enumerationForgeProvider) {
                    var desc = enumerationForgeProvider.GetEnumerationForge(
                        validationContext.StreamTypeService,
                        validationContext.ContextDescriptor);
                    forge = desc.Forge;
                    streamId = desc.DirectIndexStreamNumber;
                    istreamOnlyFlag = desc.IsIstreamOnly;
                }
                else {
                    forge = (ExprEnumerationForge)parameter.Forge;
                    istreamOnlyFlag = false;
                    streamId = -1;
                    forceOptionalStream =
                        true; // since they may return null, i.e. subquery returning no row or multiple rows etc.
                }

                isIStreamOnly[offsetEventType] = istreamOnlyFlag;
                eventEnumerationForges[offsetEventType] = forge;
                eventTypes[offsetEventType] = forge.GetEventTypeSingle(
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);
                if (streamId != index) {
                    allStreamIdsMatch = false;
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
                expressionBodyCopy = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.DECLAREDEXPRBODY,
                    expressionBodyCopy,
                    expressionBodyContext);
            }
            catch (ExprValidationException ex) {
                var message = "Failed to validate expression declaration '" + prototype.Name + "': " + ex.Message;
                throw new ExprValidationException(message, ex);
            }

            // analyze child node
            var summaryVisitor = new ExprNodeSummaryVisitor();
            expressionBodyCopy.Accept(summaryVisitor);
            var isCache = !(summaryVisitor.HasAggregation || summaryVisitor.HasPreviousPrior);
            isCache &= validationContext.StatementCompileTimeService.Configuration.Compiler.Execution
                .IsEnabledDeclaredExprValueCache;
            // determine a suitable evaluation
            var audit = AuditEnum.EXPRDEF.GetAudit(validationContext.Annotations) != null;
            var statementName = validationContext.StatementName;
            if (expressionBodyCopy.Forge.ForgeConstantType.IsConstant) {
                // pre-evaluated
                forge = new ExprDeclaredForgeConstant(
                    this,
                    expressionBodyCopy.Forge.EvaluationType,
                    prototype,
                    expressionBodyCopy.Forge.ExprEvaluator.Evaluate(null, true, null),
                    audit,
                    statementName);
            }
            else if ((valueEventType == null && prototype.ParametersNames.Length == 0) || allStreamIdsMatch) {
                forge = new ExprDeclaredForgeNoRewrite(this, expressionBodyCopy.Forge, isCache, audit, statementName);
            }
            else if (valueEventType == null) {
                forge = new ExprDeclaredForgeRewrite(
                    this,
                    expressionBodyCopy.Forge,
                    isCache,
                    eventEnumerationForges,
                    audit,
                    statementName);
            }
            else {
                // cache is always false
                forge = new ExprDeclaredForgeRewriteWValue(
                    this,
                    expressionBodyCopy.Forge,
                    false,
                    audit,
                    statementName,
                    eventEnumerationForges,
                    valueEventType,
                    valueExpressions);
            }

            return null;
        }

        private bool IsEventProviding(
            ExprNode parameter,
            ExprValidationContext validationContext)
        {
            if (parameter is ExprEnumerationForgeProvider provider) {
                var desc = provider.GetEnumerationForge(
                    validationContext.StreamTypeService,
                    validationContext.ContextDescriptor);
                if (desc == null) {
                    return false;
                }

                var eventType = desc.Forge.GetEventTypeSingle(
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);
                return eventType != null;
            }

            var forge = parameter.Forge;
            if (forge is ExprEnumerationForge enumerationForge) {
                return enumerationForge.GetEventTypeSingle(
                           validationContext.StatementRawInfo,
                           validationContext.StatementCompileTimeService) !=
                       null;
            }

            return false;
        }

        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprDeclaredNodeImpl otherExprCaseNode)) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(expressionBodyCopy, otherExprCaseNode, false);
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            AcceptNoVisitParams(visitor);
            if (WalkParams(visitor)) {
                ExprNodeUtilityQuery.AcceptParams(visitor, chainParameters);
            }
        }

        public void AcceptNoVisitParams(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            if (visitor.IsVisit(this) && ChildNodes.Length == 0) {
                expressionBodyCopy.Accept(visitor);
            }
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            AcceptNoVisitParams(visitor);
            if (WalkParams(visitor)) {
                ExprNodeUtilityQuery.AcceptParams(visitor, chainParameters);
            }
        }

        public void AcceptNoVisitParams(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            if (ChildNodes.Length == 0) {
                expressionBodyCopy.Accept(visitor);
            }
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            if (visitor.IsVisit(this) && ChildNodes.Length == 0) {
                expressionBodyCopy.Accept(visitor);
            }
        }

        private void CheckParameterCount()
        {
            var prototype = prototypeWVisibility;
            if (chainParameters.Count != prototype.ParametersNames.Length) {
                throw new ExprValidationException(
                    "Parameter count mismatches for declared expression '" +
                    prototype.Name +
                    "', expected " +
                    prototype.ParametersNames.Length +
                    " parameters but received " +
                    chainParameters.Count +
                    " parameters");
            }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            var prototype = prototypeWVisibility;
            writer.Write(prototype.Name);
            if (prototype.IsAlias) {
                return;
            }

            writer.Write("(");
            var delimiter = "";
            foreach (var parameter in chainParameters) {
                writer.Write(delimiter);
                parameter.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
                delimiter = ",";
            }

            writer.Write(")");
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        private bool WalkParams(ExprNodeVisitor visitor)
        {
            // we do not walk parameters when all stream ids match and the visitor skips declared-expression parameters
            // this is because parameters are streams and don't need to be collected by some visitors
            return visitor.IsWalkDeclExprParam || !allStreamIdsMatch;
        }

        private bool WalkParams(ExprNodeVisitorWithParent visitor)
        {
            // we do not walk parameters when all stream ids match and the visitor skips declared-expression parameters
            // this is because parameters are streams and don't need to be collected by some visitors
            return visitor.IsWalkDeclExprParam || !allStreamIdsMatch;
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public ExprNode Body => expressionBodyCopy;

        public IList<ExprNode> AdditionalNodes => chainParameters;

        public Type ConstantType {
            get {
                CheckValidated(forge);
                return forge.EvaluationType;
            }
        }

        public object ConstantValue => forge.ExprEvaluator.Evaluate(null, true, null);

        public bool IsFilterLookupEligible => forge is ExprDeclaredForgeBase;

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                if (!(Forge is ExprDeclaredForgeBase) || Forge.EvaluationType == null) {
                    return null;
                }

                var declaredForge = (ExprDeclaredForgeBase) Forge;
                var forge = declaredForge.InnerForge;
                var serde = exprValidationContext.SerdeResolver.SerdeForFilter(
                    forge.EvaluationType,
                    exprValidationContext.StatementRawInfo);
                return new ExprFilterSpecLookupableForge(
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(this),
                    new DeclaredNodeEventPropertyGetterForge(forge),
                    null,
                    forge.EvaluationType,
                    true,
                    serde);
            }
        }

        public ExprNode ExpressionBodyCopy => expressionBodyCopy;

        public ExpressionDeclItem Prototype => prototypeWVisibility;

        public ExpressionDeclItem PrototypeWVisibility => prototypeWVisibility;

        public IList<ExprNode> ChainParameters => chainParameters;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }
    }
} // end of namespace