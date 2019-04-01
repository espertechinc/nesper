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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
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
        [NonSerialized] private ExprForge forge;

        public ExprDeclaredNodeImpl(
            ExpressionDeclItem prototype, IList<ExprNode> chainParameters,
            ContextCompileTimeDescriptor contextDescriptor, ExprNode expressionBodyCopy)
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

        public bool IsConstantResult => false;

        public ExprNode ExpressionBodyCopy { get; private set; }

        public ExpressionDeclItem PrototypeWVisibility { get; }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public Type ConstantType {
            get {
                CheckValidated(forge);
                return forge.EvaluationType;
            }
        }

        public object ConstantValue => forge.ExprEvaluator.Evaluate(null, true, null);

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
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
                        "Failed validation of expression declaration '" + prototype.Name +
                        "': Invalid parameter to expression declaration, parameter " + param +
                        " is not the name of a stream in the query");
                }

                var prototypeName = prototype.ParametersNames[param];
                streamParameters.Put(prototypeName, streamIdFound);
            }

            return streamParameters;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var prototype = PrototypeWVisibility;
            if (prototype.IsAlias) {
                try {
                    ExpressionBodyCopy = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.ALIASEXPRBODY, ExpressionBodyCopy, validationContext);
                }
                catch (ExprValidationException ex) {
                    var message = "Error validating expression alias '" + prototype.Name + "': " + ex.Message;
                    throw new ExprValidationException(message, ex);
                }

                forge = ExpressionBodyCopy.Forge;
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
            foreach (var expr in ChainParameters) {
                validated.Add(
                    ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.DECLAREDEXPRPARAM, expr, validationContext));
            }

            ChainParameters = validated;

            // validate parameter count
            CheckParameterCount();

            // create context for expression body
            var eventTypes = new EventType[prototype.ParametersNames.Length];
            var streamNames = new string[prototype.ParametersNames.Length];
            var isIStreamOnly = new bool[prototype.ParametersNames.Length];
            var streamsIdsPerStream = new int[prototype.ParametersNames.Length];
            var allStreamIdsMatch = true;

            for (var i = 0; i < prototype.ParametersNames.Length; i++) {
                var parameter = ChainParameters[i];
                streamNames[i] = prototype.ParametersNames[i];

                if (parameter is ExprStreamUnderlyingNode) {
                    var und = (ExprStreamUnderlyingNode) parameter;
                    eventTypes[i] = validationContext.StreamTypeService.EventTypes[und.StreamId];
                    isIStreamOnly[i] = validationContext.StreamTypeService.IStreamOnly[und.StreamId];
                    streamsIdsPerStream[i] = und.StreamId;
                }
                else if (parameter is ExprWildcard) {
                    if (validationContext.StreamTypeService.EventTypes.Length != 1) {
                        throw new ExprValidationException(
                            "Expression '" + prototype.Name +
                            "' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead");
                    }

                    eventTypes[i] = validationContext.StreamTypeService.EventTypes[0];
                    isIStreamOnly[i] = validationContext.StreamTypeService.IStreamOnly[0];
                    streamsIdsPerStream[i] = 0;
                }
                else {
                    throw new ExprValidationException(
                        "Expression '" + prototype.Name + "' requires a stream name as a parameter");
                }

                if (streamsIdsPerStream[i] != i) {
                    allStreamIdsMatch = false;
                }
            }

            var streamTypeService = validationContext.StreamTypeService;
            var copyTypes = new StreamTypeServiceImpl(
                eventTypes, streamNames, isIStreamOnly,
                streamTypeService.IsOnDemandStreams,
                streamTypeService.IsOptionalStreams);
            copyTypes.RequireStreamNames = true;

            // validate expression body in this context
            try {
                var expressionBodyContext = new ExprValidationContext(copyTypes, validationContext);
                ExpressionBodyCopy = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.DECLAREDEXPRBODY, ExpressionBodyCopy, expressionBodyContext);
            }
            catch (ExprValidationException ex) {
                var message = "Error validating expression declaration '" + prototype.Name + "': " + ex.Message;
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
                forge = new ExprDeclaredForgeConstant(
                    this, ExpressionBodyCopy.Forge.EvaluationType, prototype,
                    ExpressionBodyCopy.Forge.ExprEvaluator.Evaluate(null, true, null), audit, statementName);
            }
            else if (prototype.ParametersNames.Length == 0 ||
                     allStreamIdsMatch && prototype.ParametersNames.Length == streamTypeService.EventTypes.Length) {
                forge = new ExprDeclaredForgeNoRewrite(this, ExpressionBodyCopy.Forge, isCache, audit, statementName);
            }
            else {
                forge = new ExprDeclaredForgeRewrite(
                    this, ExpressionBodyCopy.Forge, isCache, streamsIdsPerStream, audit, statementName);
            }

            return null;
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprDeclaredNodeImpl)) {
                return false;
            }

            var otherExprCaseNode = (ExprDeclaredNodeImpl) node;
            return ExprNodeUtilityCompare.DeepEquals(ExpressionBodyCopy, otherExprCaseNode, false);
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            if (ChildNodes.Length == 0) {
                ExpressionBodyCopy.Accept(visitor);
            }
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            if (ChildNodes.Length == 0) {
                ExpressionBodyCopy.Accept(visitor);
            }
        }

        public override void AcceptChildnodes(ExprNodeVisitorWithParent visitor, ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            if (visitor.IsVisit(this) && ChildNodes.Length == 0) {
                ExpressionBodyCopy.Accept(visitor);
            }
        }

        public ExpressionDeclItem Prototype => PrototypeWVisibility;

        public IList<ExprNode> ChainParameters { get; private set; }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsValidated => forge != null;

        public bool FilterLookupEligible => true;

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                if (!(forge is ExprDeclaredForgeBase)) {
                    return null;
                }

                var declaredForge = (ExprDeclaredForgeBase) forge;
                var forge = declaredForge.InnerForge;
                return new ExprFilterSpecLookupableForge(
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(this),
                    new DeclaredNodeEventPropertyGetterForge(forge), forge.EvaluationType, true);
            }
        }

        public IList<ExprNode> AdditionalNodes => ChainParameters;

        private void CheckParameterCount()
        {
            var prototype = PrototypeWVisibility;
            if (ChainParameters.Count != prototype.ParametersNames.Length) {
                throw new ExprValidationException(
                    "Parameter count mismatches for declared expression '" + prototype.Name + "', expected " +
                    prototype.ParametersNames.Length + " parameters but received " + ChainParameters.Count +
                    " parameters");
            }
        }

        public override void ToPrecedenceFreeEPL(StringWriter writer)
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
                parameter.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            writer.Write(")");
        }

        private class DeclaredNodeEventPropertyGetterForge : EventPropertyValueGetterForge
        {
            private readonly ExprForge exprForge;

            public DeclaredNodeEventPropertyGetterForge(ExprForge exprForge)
            {
                this.exprForge = exprForge;
            }

            public CodegenExpression EventBeanGetCodegen(
                CodegenExpression beanExpression, CodegenMethodScope parent, CodegenClassScope codegenClassScope)
            {
                var method = parent.MakeChild(exprForge.EvaluationType, GetType(), codegenClassScope)
                    .AddParam(typeof(EventBean), "bean");
                var exprMethod =
                    CodegenLegoMethodExpression.CodegenExpression(exprForge, method, codegenClassScope);

                method.Block
                    .DeclareVar(typeof(EventBean[]), "events", NewArrayByLength(typeof(EventBean), Constant(1)))
                    .AssignArrayElement(Ref("events"), Constant(0), Ref("bean"))
                    .MethodReturn(LocalMethod(exprMethod, Ref("events"), ConstantTrue(), ConstantNull()));

                return LocalMethod(method, beanExpression);
            }
        }
    }
} // end of namespace