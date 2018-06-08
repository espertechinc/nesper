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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents an invocation of a plug-in single-row function  in the expression tree.
    /// </summary>
    [Serializable]
    public class ExprPlugInSingleRowNode
        : ExprNodeBase
        , ExprNodeInnerNodeProvider
        , ExprFilterOptimizableNode
    {
        private readonly String _functionName;
        private readonly Type _clazz;
        private readonly IList<ExprChainedSpec> _chainSpec;
        private readonly EngineImportSingleRowDesc _config;

        [NonSerialized]
        private bool _isReturnsConstantResult;
        [NonSerialized]
        private ExprEvaluator _evaluator;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="clazz">The clazz.</param>
        /// <param name="chainSpec">the class and name of the method that this node will invoke plus parameters</param>
        /// <param name="config">The config.</param>
        public ExprPlugInSingleRowNode(String functionName, Type clazz, IList<ExprChainedSpec> chainSpec, EngineImportSingleRowDesc config)
        {
            _functionName = functionName;
            _clazz = clazz;
            _chainSpec = chainSpec;
            _config = config;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _evaluator; }
        }

        public IList<ExprChainedSpec> ChainSpec
        {
            get { return _chainSpec; }
        }

        public override bool IsConstantResult
        {
            get { return _isReturnsConstantResult; }
        }

        public string FunctionName
        {
            get { return _functionName; }
        }

        public bool IsFilterLookupEligible
        {
            get
            {
                var eligible = !_isReturnsConstantResult;
                if (eligible)
                {
                    eligible = _chainSpec.Count == 1;
                }
                if (eligible)
                {
                    eligible = _config.FilterOptimizable == FilterOptimizableEnum.ENABLED;
                }
                if (eligible)
                {
                    // We disallow context properties in a filter-optimizable expression if they are passed in since
                    // the evaluation is context-free and shared.
                    ExprNodeContextPropertiesVisitor visitor = new ExprNodeContextPropertiesVisitor();
                    ExprNodeUtility.AcceptChain(visitor, _chainSpec);
                    eligible = !visitor.IsFound;
                }
                if (eligible)
                {
                    ExprNodeStreamRequiredVisitor visitor = new ExprNodeStreamRequiredVisitor();
                    ExprNodeUtility.AcceptChain(visitor, _chainSpec);
                    foreach (int stream in visitor.StreamsRequired.Where(stream => stream != 0))
                    {
                        eligible = false;
                    }
                }
                return eligible;
            }
        }

        public FilterSpecLookupable FilterLookupable
        {
            get
            {
                var eval = (ExprDotEvalStaticMethod)_evaluator;
                return new FilterSpecLookupable(this.ToExpressionStringMinPrecedenceSafe(), eval, _evaluator.ReturnType, true);
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExprNodeUtility.ToExpressionString(_chainSpec, writer, false, _functionName);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprPlugInSingleRowNode))
            {
                return false;
            }

            var other = (ExprPlugInSingleRowNode)node;
            if (other._chainSpec.Count != _chainSpec.Count)
            {
                return false;
            }
            for (var i = 0; i < _chainSpec.Count; i++)
            {
                if (!Equals(_chainSpec[i], other._chainSpec[i]))
                {
                    return false;
                }
            }
            return other._clazz == _clazz && other._functionName.EndsWith(_functionName);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            ExprNodeUtility.Validate(ExprNodeOrigin.PLUGINSINGLEROWPARAM, _chainSpec, validationContext);

            // get first chain item
            var chainList = new List<ExprChainedSpec>(_chainSpec);
            var firstItem = chainList.DeleteAt(0);

            // Get the types of the parameters for the first invocation
            var allowWildcard = validationContext.StreamTypeService.EventTypes.Length == 1;
            EventType streamZeroType = null;
            if (validationContext.StreamTypeService.EventTypes.Length > 0)
            {
                streamZeroType = validationContext.StreamTypeService.EventTypes[0];
            }
            var staticMethodDesc = ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                _clazz.FullName, null, firstItem.Name, firstItem.Parameters, validationContext.EngineImportService,
                validationContext.EventAdapterService, validationContext.StatementId, allowWildcard, streamZeroType,
                new ExprNodeUtilResolveExceptionHandlerDefault(firstItem.Name, true), _functionName,
                validationContext.TableService,
                validationContext.StreamTypeService.EngineURIQualifier);

            var allowValueCache = true;
            switch (_config.ValueCache)
            {
                case ValueCacheEnum.DISABLED:
                    _isReturnsConstantResult = false;
                    allowValueCache = false;
                    break;
                case ValueCacheEnum.CONFIGURED:
                    _isReturnsConstantResult = validationContext.EngineImportService.IsUdfCache && staticMethodDesc.IsAllConstants && chainList.IsEmpty();
                    allowValueCache = validationContext.EngineImportService.IsUdfCache;
                    break;
                case ValueCacheEnum.ENABLED:
                    _isReturnsConstantResult = staticMethodDesc.IsAllConstants && chainList.IsEmpty();
                    break;
                default:
                    throw new IllegalStateException("Invalid value cache code " + _config.ValueCache);
            }

            // this may return a pair of null if there is no lambda or the result cannot be wrapped for lambda-function use
            var optionalLambdaWrap = ExprDotStaticMethodWrapFactory.Make(
                staticMethodDesc.ReflectionMethod, validationContext.EventAdapterService, chainList,
                _config.OptionalEventTypeName);
            var typeInfo = optionalLambdaWrap != null ? optionalLambdaWrap.TypeInfo : EPTypeHelper.SingleValue(staticMethodDesc.ReflectionMethod.ReturnType);

            var eval = ExprDotNodeUtility.GetChainEvaluators(-1, typeInfo, chainList, validationContext, false, new ExprDotNodeFilterAnalyzerInputStatic()).ChainWithUnpack;
            _evaluator = new ExprDotEvalStaticMethod(
                validationContext.StatementName, _clazz.FullName, staticMethodDesc.FastMethod,
                staticMethodDesc.ChildEvals, allowValueCache && staticMethodDesc.IsAllConstants, optionalLambdaWrap,
                eval, _config.IsRethrowExceptions, null);

            // If caching the result, evaluate now and return the result.
            if (_isReturnsConstantResult)
            {
                var result = _evaluator.Evaluate(new EvaluateParams(null, true, null));
                _evaluator = new ProxyExprEvaluator
                {
                    ProcEvaluate = args =>
                    {
                        if (InstrumentationHelper.ENABLED)
                        {
                            InstrumentationHelper.Get().QExprPlugInSingleRow(staticMethodDesc.ReflectionMethod);
                            InstrumentationHelper.Get().AExprPlugInSingleRow(result);
                        }
                        return result;
                    },
                    ReturnType = staticMethodDesc.FastMethod.ReturnType
                };
            }

            return null;
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            ExprNodeUtility.AcceptChain(visitor, _chainSpec);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            ExprNodeUtility.AcceptChain(visitor, _chainSpec, this);
        }

        public override void AcceptChildnodes(ExprNodeVisitorWithParent visitor, ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            ExprNodeUtility.AcceptChain(visitor, _chainSpec, this);
        }

        public override void ReplaceUnlistedChildNode(ExprNode nodeToReplace, ExprNode newNode)
        {
            ExprNodeUtility.ReplaceChainChildNode(nodeToReplace, newNode, _chainSpec);
        }

        public IList<ExprNode> AdditionalNodes
        {
            get { return ExprNodeUtility.CollectChainParameters(_chainSpec); }
        }
    }
}
