///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public abstract class SettingsApplicationDotMethodBase : SettingsApplicationDotMethod
    {
        protected internal const string LHS_VALIDATION_NAME = "left-hand-side";
        protected internal const string RHS_VALIDATION_NAME = "right-hand-side";

        private readonly ExprNode[] indexNamedParameter;

        internal readonly ExprDotNodeImpl parent;

        [NonSerialized] private ExprForge forge;

        private AdvancedIndexConfigContextPartition optionalIndexConfig;
        private string optionalIndexName;

        public SettingsApplicationDotMethodBase(
            ExprDotNodeImpl parent,
            string lhsName,
            ExprNode[] lhs,
            string dotMethodName,
            string rhsName,
            ExprNode[] rhs,
            ExprNode[] indexNamedParameter)
        {
            this.parent = parent;
            LhsName = lhsName;
            Lhs = lhs;
            DotMethodName = dotMethodName;
            RhsName = rhsName;
            Rhs = rhs;
            this.indexNamedParameter = indexNamedParameter;
        }

        public ExprForge Forge => forge;

        public string LhsName { get; }

        public ExprNode[] Lhs { get; }

        public string DotMethodName { get; }

        public string RhsName { get; }

        public ExprNode[] Rhs { get; }

        public ExprNode Validate(ExprValidationContext validationContext)
        {
            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.DOTNODEPARAMETER, Lhs, validationContext);
            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.DOTNODEPARAMETER, Rhs, validationContext);

            forge = ValidateAll(LhsName, Lhs, RhsName, Rhs, validationContext);

            if (indexNamedParameter != null) {
                ValidateIndexNamedParameter(validationContext);
            }

            return null;
        }

        public FilterExprAnalyzerAffector FilterExprAnalyzerAffector {
            get {
                var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
                foreach (var lhsNode in Lhs) {
                    lhsNode.Accept(visitor);
                }

                ISet<int> indexedPropertyStreams = new HashSet<int>();
                foreach (ExprNodePropOrStreamDesc @ref in visitor.Refs) {
                    indexedPropertyStreams.Add(@ref.StreamNum);
                }

                if (indexedPropertyStreams.Count == 0 || indexedPropertyStreams.Count > 1) {
                    return
                        null; // there are no properties from any streams that could be used for building an index, or the properties come from different disjoint streams
                }

                var streamNumIndex = indexedPropertyStreams.First();

                IList<Pair<ExprNode, int[]>> keyExpressions = new List<Pair<ExprNode, int[]>>();
                ISet<int> dependencies = new HashSet<int>();
                foreach (var node in Rhs) {
                    visitor.Reset();
                    dependencies.Clear();
                    node.Accept(visitor);
                    foreach (ExprNodePropOrStreamDesc @ref in visitor.Refs) {
                        dependencies.Add(@ref.StreamNum);
                    }

                    if (dependencies.Contains(streamNumIndex)) {
                        return null;
                    }

                    Pair<ExprNode, int[]> pair = new Pair<ExprNode, int[]>(node, CollectionUtil.IntArray(dependencies));
                    keyExpressions.Add(pair);
                }

                return new FilterExprAnalyzerAffectorIndexProvision(
                    OperationName,
                    Lhs,
                    keyExpressions,
                    streamNumIndex);
            }
        }

        public FilterSpecCompilerAdvIndexDesc FilterSpecCompilerAdvIndexDesc {
            get {
                if (indexNamedParameter == null) {
                    return null;
                }

                return new FilterSpecCompilerAdvIndexDesc(
                    Lhs,
                    Rhs,
                    optionalIndexConfig,
                    IndexTypeName,
                    optionalIndexName);
            }
        }

        protected abstract ExprForge ValidateAll(
            string lhsName,
            ExprNode[] lhs,
            string rhsName,
            ExprNode[] rhs,
            ExprValidationContext validationContext);

        protected abstract string IndexTypeName { get; }

        protected abstract string OperationName { get; }

        private void ValidateIndexNamedParameter(ExprValidationContext validationContext)
        {
            if (indexNamedParameter.Length != 1 || !(indexNamedParameter[0] is ExprDeclaredNode)) {
                throw GetIndexNameMessage("requires an expression name");
            }

            var node = (ExprDeclaredNode) indexNamedParameter[0];
            if (!(node.Body is ExprDotNode)) {
                throw GetIndexNameMessage("requires an index expression");
            }

            var dotNode = (ExprDotNode) node.Body;
            if (dotNode.ChainSpec.Count > 1) {
                throw GetIndexNameMessage("invalid chained index expression");
            }

            IList<ExprNode> @params = dotNode.ChainSpec[0].Parameters;
            string indexTypeName = dotNode.ChainSpec[0].Name;
            optionalIndexName = node.Prototype.Name;

            AdvancedIndexFactoryProvider provider = null;
            try {
                provider = validationContext.ImportService.ResolveAdvancedIndexProvider(indexTypeName);
            }
            catch (ImportException e) {
                throw new ExprValidationException(e.Message, e);
            }

            if (!indexTypeName.ToLowerInvariant().Equals(IndexTypeName)) {
                throw new ExprValidationException(
                    "Invalid index type '" + indexTypeName + "', expected '" + IndexTypeName + "'");
            }

            optionalIndexConfig = provider.ValidateConfigureFilterIndex(
                optionalIndexName,
                indexTypeName,
                ExprNodeUtilityQuery.ToArray(@params),
                validationContext);
        }

        private ExprValidationException GetIndexNameMessage(string message)
        {
            return new ExprValidationException(
                "Named parameter '" + ExprDotNodeConstants.FILTERINDEX_NAMED_PARAMETER + "' " + message);
        }
    }
} // end of namespace