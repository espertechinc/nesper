///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.index.quadtree
{
    public abstract class EngineImportApplicationDotMethodBase : EngineImportApplicationDotMethod
    {
        internal const string LHS_VALIDATION_NAME = "left-hand-side";
        internal const string RHS_VALIDATION_NAME = "right-hand-side";

        private readonly string _lhsName;
        private readonly IList<ExprNode> _lhs;
        private readonly string _dotMethodName;
        private readonly string _rhsName;
        private readonly IList<ExprNode> _rhs;
        private readonly IList<ExprNode> _indexNamedParameter;
        private string _optionalIndexName;
        private AdvancedIndexConfigContextPartition _optionalIndexConfig;

        private ExprEvaluator _evaluator;

        protected abstract ExprEvaluator ValidateAll(
            string lhsName, IList<ExprNode> lhs,
            string rhsName, IList<ExprNode> rhs,
            ExprValidationContext validationContext);

        protected abstract string IndexTypeName { get; }
        protected abstract string OperationName { get; }

        protected EngineImportApplicationDotMethodBase(
            string lhsName, IList<ExprNode> lhs, string dotMethodName,
            string rhsName, IList<ExprNode> rhs, IList<ExprNode> indexNamedParameter)
        {
            _lhsName = lhsName;
            _lhs = lhs;
            _dotMethodName = dotMethodName;
            _rhsName = rhsName;
            _rhs = rhs;
            _indexNamedParameter = indexNamedParameter;
        }

        public string LhsName => _lhsName;

        public IList<ExprNode> Lhs => _lhs;

        public string DotMethodName => _dotMethodName;

        public string RhsName => _rhsName;

        public IList<ExprNode> Rhs => _rhs;

        public ExprNode Validate(ExprValidationContext validationContext)
        {
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.DOTNODEPARAMETER, _lhs, validationContext);
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.DOTNODEPARAMETER, _rhs, validationContext);

            _evaluator = ValidateAll(_lhsName, _lhs, _rhsName, _rhs, validationContext);

            if (_indexNamedParameter != null)
            {
                ValidateIndexNamedParameter(validationContext);
            }

            return null;
        }

        public ExprEvaluator ExprEvaluator => _evaluator;

        public FilterExprAnalyzerAffector GetFilterExprAnalyzerAffector()
        {
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
            foreach (ExprNode lhsNode in _lhs)
            {
                lhsNode.Accept(visitor);
            }

            var indexedPropertyStreams = new HashSet<int>();
            foreach (ExprNodePropOrStreamDesc @ref in visitor.GetRefs())
            {
                indexedPropertyStreams.Add(@ref.StreamNum);
            }

            if (indexedPropertyStreams.Count == 0 || indexedPropertyStreams.Count > 1)
            {
                return
                    null; // there are no properties from any streams that could be used for building an index, or the properties come from different disjoint streams
            }

            var streamNumIndex = indexedPropertyStreams.First();

            var keyExpressions = new List<Pair<ExprNode, int[]>>();
            var dependencies = new HashSet<int>();
            foreach (ExprNode node in _rhs)
            {
                visitor.Reset();
                dependencies.Clear();
                node.Accept(visitor);

                foreach (ExprNodePropOrStreamDesc @ref in visitor.GetRefs())
                {
                    dependencies.Add(@ref.StreamNum);
                }

                if (dependencies.Contains(streamNumIndex))
                {
                    return null;
                }

                var pair = new Pair<ExprNode, int[]>(node, CollectionUtil.IntArray(dependencies));
                keyExpressions.Add(pair);
            }

            return new FilterExprAnalyzerAffectorIndexProvision(OperationName, _lhs, keyExpressions, streamNumIndex);
        }

        public FilterSpecCompilerAdvIndexDesc GetFilterSpecCompilerAdvIndexDesc()
        {
            if (_indexNamedParameter == null)
            {
                return null;
            }

            return new FilterSpecCompilerAdvIndexDesc(
                _lhs, _rhs, _optionalIndexConfig, IndexTypeName, _optionalIndexName);
        }

        private void ValidateIndexNamedParameter(ExprValidationContext validationContext)
        {
            if (_indexNamedParameter.Count != 1 || !(_indexNamedParameter[0] is ExprDeclaredNode))
            {
                throw GetIndexNameMessage("requires an expression name");
            }

            ExprDeclaredNode node = (ExprDeclaredNode) _indexNamedParameter[0];
            if (!(node.Body is ExprDotNode))
            {
                throw GetIndexNameMessage("requires an index expression");
            }

            ExprDotNode dotNode = (ExprDotNode) node.Body;
            if (dotNode.ChainSpec.Count > 1)
            {
                throw GetIndexNameMessage("invalid chained index expression");
            }

            IList<ExprNode> @params = dotNode.ChainSpec[0].Parameters;
            string indexTypeName = dotNode.ChainSpec[0].Name;
            _optionalIndexName = node.Prototype.Name;

            AdvancedIndexFactoryProvider provider;
            try
            {
                provider = validationContext.EngineImportService.ResolveAdvancedIndexProvider(indexTypeName);
            }
            catch (EngineImportException e)
            {
                throw new ExprValidationException(e.Message, e);
            }

            if (indexTypeName.ToLowerInvariant() != IndexTypeName)
            {
                throw new ExprValidationException("Invalid index type '" + indexTypeName + "', expected '" +
                                                  IndexTypeName + "'");
            }

            _optionalIndexConfig = provider.ValidateConfigureFilterIndex(_optionalIndexName, indexTypeName,
                ExprNodeUtility.ToArray(@params), validationContext);
        }

        private ExprValidationException GetIndexNameMessage(string message)
        {
            return new ExprValidationException("Named parameter '" + ExprDotNodeConstants.FILTERINDEX_NAMED_PARAMETER +
                                               "' " + message);
        }
    }
} // end of namespace
