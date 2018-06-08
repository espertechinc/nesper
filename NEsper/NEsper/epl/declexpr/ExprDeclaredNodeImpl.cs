///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.declexpr
{
    /// <summary>
    /// Expression instance as declared elsewhere.
    /// </summary>
    [Serializable]
    public class ExprDeclaredNodeImpl 
        : ExprNodeBase 
        , ExprDeclaredNode
        , ExprDeclaredOrLambdaNode
        , ExprFilterOptimizableNode
        , ExprNodeInnerNodeProvider
        , ExprConstantNode
    {
        private readonly ExpressionDeclItem _prototype;
        private IList<ExprNode> _chainParameters;
        [NonSerialized]
        private ExprEvaluator _exprEvaluator;
        private ExprNode _expressionBodyCopy;

        public ExprDeclaredNodeImpl(
            IContainer container,
            ExpressionDeclItem prototype, 
            IList<ExprNode> chainParameters, 
            ContextDescriptor contextDescriptor)
        {
            _prototype = prototype;
            _chainParameters = chainParameters;
    
            // copy expression - we do it at this time and not later
            try {
                 _expressionBodyCopy = (ExprNode) SerializableObjectCopier.Copy(container, prototype.Inner);
            } catch (Exception e) {
                throw new Exception("Internal error providing expression tree: " + e.Message, e);
            }

            // replace context-properties where they are currently identifiers
            if (contextDescriptor == null) {
                return;
            }
            var visitorWParent = new ExprNodeIdentVisitorWParent();
            _expressionBodyCopy.Accept(visitorWParent);
            foreach (var pair in visitorWParent.IdentNodes) {
                var streamOrProp = pair.Second.StreamOrPropertyName;
                if (streamOrProp != null && contextDescriptor.ContextPropertyRegistry.IsContextPropertyPrefix(streamOrProp)) {
                    var context = new ExprContextPropertyNode(pair.Second.UnresolvedPropertyName);
                    if (pair.First == null) {
                        _expressionBodyCopy = context;
                    }
                    else {
                        ExprNodeUtility.ReplaceChildNode(pair.First, pair.Second, context);
                    }
                }
            }
        }

        public ExprNode Body
        {
            get { return _expressionBodyCopy; }
        }

        public IList<ExprNode> AdditionalNodes
        {
            get { return _chainParameters; }
        }

        public bool IsValidated
        {
            get { return _exprEvaluator != null; }
        }

        public Type ConstantType
        {
            get { return _exprEvaluator.ReturnType; }
        }

        public Object GetConstantValue(ExprEvaluatorContext context)
        {
            return _exprEvaluator.Evaluate(new EvaluateParams(null, true, context));
        }

        public bool IsConstantValue
        {
            get { return _expressionBodyCopy.IsConstantResult; }
        }

        public LinkedHashMap<string, int> GetOuterStreamNames(IDictionary<string, int> outerStreamNames)
        {
            CheckParameterCount();
    
            // determine stream ids for each parameter
            var streamParameters = new LinkedHashMap<string, int>();
            for (var param = 0; param < _chainParameters.Count; param++) {
                if (!(_chainParameters[param] is ExprIdentNode)) {
                    throw new ExprValidationException("Sub-selects in an expression declaration require passing only stream names as parameters");
                }

                var parameterName = ((ExprIdentNode) _chainParameters[param]).UnresolvedPropertyName;

                int streamIdFound;
                if (!outerStreamNames.TryGetValue(parameterName, out streamIdFound))
                {
                    throw new ExprValidationException("Failed validation of expression declaration '" + _prototype.Name + "': Invalid parameter to expression declaration, parameter " + param + " is not the name of a stream in the query");
                }
                var prototypeName = _prototype.ParametersNames[param];
                streamParameters.Put(prototypeName, streamIdFound);
            }
        
            return streamParameters;
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (_prototype.IsAlias)
            {
                try
                {
                    _expressionBodyCopy = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.ALIASEXPRBODY, _expressionBodyCopy, validationContext);
                }
                catch (ExprValidationException ex)
                {
                    var message = "Error validating expression alias '" + _prototype.Name + "': " + ex.Message;
                    throw new ExprValidationException(message, ex);
                }

                _exprEvaluator = _expressionBodyCopy.ExprEvaluator;
                return null;
            }

            if (_exprEvaluator != null)
            {
                return null; // already evaluated
            }

            if (ChildNodes.Count > 0) {
                throw new IllegalStateException("Execution node has its own child nodes");
            }
    
            // validate chain
            var validated = _chainParameters
                .Select(expr => ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.DECLAREDEXPRPARAM, expr, validationContext))
                .ToList();
            _chainParameters = validated;
    
            // validate parameter count
            CheckParameterCount();
    
            // create context for expression body
            var eventTypes = new EventType[_prototype.ParametersNames.Count];
            var streamNames = new String[_prototype.ParametersNames.Count];
            var isIStreamOnly = new bool[_prototype.ParametersNames.Count];
            var streamsIdsPerStream = new int[_prototype.ParametersNames.Count];
            var allStreamIdsMatch = true;
    
            for (var i = 0; i < _prototype.ParametersNames.Count; i++) {
                var parameter = _chainParameters[i];
                streamNames[i] = _prototype.ParametersNames[i];
    
                if (parameter is ExprStreamUnderlyingNode) {
                    var und = (ExprStreamUnderlyingNode) parameter;
                    eventTypes[i] = validationContext.StreamTypeService.EventTypes[und.StreamId];
                    isIStreamOnly[i] = validationContext.StreamTypeService.IsIStreamOnly[und.StreamId];
                    streamsIdsPerStream[i] = und.StreamId;
                }
                else if (parameter is ExprWildcard)
                {
                    if (validationContext.StreamTypeService.EventTypes.Length != 1) {
                        throw new ExprValidationException("Expression '" + _prototype.Name + "' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead");
                    }
                    eventTypes[i] = validationContext.StreamTypeService.EventTypes[0];
                    isIStreamOnly[i] = validationContext.StreamTypeService.IsIStreamOnly[0];
                    streamsIdsPerStream[i] = 0;
                }
                else {
                    throw new ExprValidationException("Expression '" + _prototype.Name + "' requires a stream name as a parameter");
                }
    
                if (streamsIdsPerStream[i] != i) {
                    allStreamIdsMatch = false;
                }
            }
    
            var streamTypeService = validationContext.StreamTypeService;
            var copyTypes = new StreamTypeServiceImpl(eventTypes, streamNames, isIStreamOnly, streamTypeService.EngineURIQualifier, streamTypeService.IsOnDemandStreams);
            copyTypes.RequireStreamNames = true;
    
            // validate expression body in this context
            try {
                var expressionBodyContext = new ExprValidationContext(copyTypes, validationContext);
                _expressionBodyCopy = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.DECLAREDEXPRBODY, _expressionBodyCopy, expressionBodyContext);
            }
            catch (ExprValidationException ex) {
                var message = "Error validating expression declaration '" + _prototype.Name + "': " + ex.Message;
                throw new ExprValidationException(message, ex);
            }
    
            // analyze child node
            var summaryVisitor = new ExprNodeSummaryVisitor();
            _expressionBodyCopy.Accept(summaryVisitor);

            var isCache = !(summaryVisitor.HasAggregation || summaryVisitor.HasPreviousPrior);
            isCache &= validationContext.ExprEvaluatorContext.ExpressionResultCacheService.IsDeclaredExprCacheEnabled;
    
            // determine a suitable evaluation
            if (_expressionBodyCopy.IsConstantResult) {
                // pre-evaluated
                _exprEvaluator = new ExprDeclaredEvalConstant(
                    _expressionBodyCopy.ExprEvaluator.ReturnType, _prototype, 
                    _expressionBodyCopy.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null)));
            }
            else if (_prototype.ParametersNames.IsEmpty() ||
                    (allStreamIdsMatch && _prototype.ParametersNames.Count == streamTypeService.EventTypes.Length)) {
                _exprEvaluator = new ExprDeclaredEvalNoRewrite(_expressionBodyCopy.ExprEvaluator, _prototype, isCache);
            }
            else {
                _exprEvaluator = new ExprDeclaredEvalRewrite(_expressionBodyCopy.ExprEvaluator, _prototype, isCache, streamsIdsPerStream);
            }
    
            var audit = AuditEnum.EXPRDEF.GetAudit(validationContext.Annotations);
            if (audit != null) {
                _exprEvaluator = (ExprEvaluator) ExprEvaluatorProxy.NewInstance(validationContext.StreamTypeService.EngineURIQualifier, validationContext.StatementName, _prototype.Name, _exprEvaluator);
            }

            return null;
        }

        public bool IsFilterLookupEligible
        {
            get { return true; }
        }

        public FilterSpecLookupable FilterLookupable
        {
            get
            {
                return new FilterSpecLookupable(
                    this.ToExpressionStringMinPrecedenceSafe(),
                    new DeclaredNodeEventPropertyGetter(_exprEvaluator), 
                    _exprEvaluator.ReturnType, true);
            }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var otherExprCaseNode = node as ExprDeclaredNodeImpl;
            if (otherExprCaseNode == null)
                return false;

            return ExprNodeUtility.DeepEquals(_expressionBodyCopy, otherExprCaseNode._expressionBodyCopy, false);
        }
    
        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            if (ChildNodes.Count == 0) {
                _expressionBodyCopy.Accept(visitor);
            }
        }
    
        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            if (ChildNodes.Count == 0) {
                _expressionBodyCopy.Accept(visitor);
            }
        }
    
        public override void AcceptChildnodes(ExprNodeVisitorWithParent visitor, ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            if (visitor.IsVisit(this) && ChildNodes.Count == 0) {
                _expressionBodyCopy.Accept(visitor);
            }
        }

        public ExprNode ExpressionBodyCopy
        {
            get { return _expressionBodyCopy; }
        }

        public ExpressionDeclItem Prototype
        {
            get { return _prototype; }
        }

        public IList<ExprNode> ChainParameters
        {
            get { return _chainParameters; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _exprEvaluator; }
        }

        private void CheckParameterCount() {
            if (_chainParameters.Count != _prototype.ParametersNames.Count) {
                throw new ExprValidationException(
                    string.Format("Parameter count mismatches for declared expression '{0}', expected {1} parameters but received {2} parameters",
                        _prototype.Name, _prototype.ParametersNames.Count, _chainParameters.Count));
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_prototype.Name);

            if (_prototype.IsAlias)
                return;

            writer.Write("(");
            var delimiter = "";
            foreach (var parameter in _chainParameters)
            {
                writer.Write(delimiter);
                parameter.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
            writer.Write(")");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        internal sealed class DeclaredNodeEventPropertyGetter : EventPropertyGetter
        {
            private readonly ExprEvaluator _exprEvaluator;

            internal DeclaredNodeEventPropertyGetter(ExprEvaluator exprEvaluator)
            {
                _exprEvaluator = ((ExprDeclaredEvalBase) exprEvaluator).InnerEvaluator;
            }

            public Object Get(EventBean eventBean)
            {
                var events = new EventBean[1];
                events[0] = eventBean;
                return _exprEvaluator.Evaluate(new EvaluateParams(events, true, null));
            }

            public bool IsExistsProperty(EventBean eventBean)
            {
                return false;
            }

            public Object GetFragment(EventBean eventBean)
            {
                return null;
            }
        }
    }
}
