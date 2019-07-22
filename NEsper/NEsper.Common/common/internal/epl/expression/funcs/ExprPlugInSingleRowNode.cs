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
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents an invocation of a plug-in single-row function  in the expression tree.
    /// </summary>
    public class ExprPlugInSingleRowNode : ExprNodeBase,
        ExprFilterOptimizableNode,
        ExprNodeInnerNodeProvider
    {
        private readonly Type clazz;
        private readonly ImportSingleRowDesc config;

        private ExprPlugInSingleRowNodeForge forge;

        public ExprPlugInSingleRowNode(
            string functionName,
            Type clazz,
            IList<ExprChainedSpec> chainSpec,
            ImportSingleRowDesc config)
        {
            FunctionName = functionName;
            this.clazz = clazz;
            ChainSpec = chainSpec;
            this.config = config;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public IList<ExprChainedSpec> ChainSpec { get; }

        public string FunctionName { get; }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool FilterLookupEligible {
            get {
                var eligible = !forge.IsReturnsConstantResult;
                if (eligible) {
                    eligible = ChainSpec.Count == 1;
                }

                if (eligible) {
                    eligible = config.FilterOptimizable ==
                               ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
                }

                if (eligible) {
                    // We disallow context properties in a filter-optimizable expression if they are passed in since
                    // the evaluation is context-free and shared.
                    var visitor = new ExprNodeContextPropertiesVisitor();
                    ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec);
                    eligible = !visitor.IsFound;
                }

                if (eligible) {
                    var visitor = new ExprNodeStreamRequiredVisitor();
                    ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec);
                    foreach (var stream in visitor.StreamsRequired) {
                        if (stream != 0) {
                            eligible = false;
                        }
                    }
                }

                if (eligible) {
                    var visitor = new ExprNodeSubselectDeclaredDotVisitor();
                    ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec);
                    if (!visitor.Subselects.IsEmpty()) {
                        eligible = false;
                    }
                }

                if (eligible) {
                    if (forge.HasMethodInvocationContextParam()) {
                        eligible = false;
                    }
                }

                return eligible;
            }
        }

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                CheckValidated(forge);
                return new ExprFilterSpecLookupableForge(
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(this),
                    forge,
                    forge.EvaluationType,
                    true);
            }
        }

        public IList<ExprNode> AdditionalNodes => ExprNodeUtilityQuery.CollectChainParameters(ChainSpec);

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExprNodeUtilityPrint.ToExpressionString(ChainSpec, writer, false, FunctionName);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprPlugInSingleRowNode)) {
                return false;
            }

            var other = (ExprPlugInSingleRowNode) node;
            if (other.ChainSpec.Count != ChainSpec.Count) {
                return false;
            }

            for (var i = 0; i < ChainSpec.Count; i++) {
                if (!ChainSpec[i].Equals(other.ChainSpec[i])) {
                    return false;
                }
            }

            return other.clazz == clazz && other.FunctionName.EndsWith(FunctionName);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            ExprNodeUtilityValidate.Validate(ExprNodeOrigin.PLUGINSINGLEROWPARAM, ChainSpec, validationContext);

            // get first chain item
            IList<ExprChainedSpec> chainList = new List<ExprChainedSpec>(ChainSpec);
            var firstItem = chainList.DeleteAt(0);

            // Get the types of the parameters for the first invocation
            var allowWildcard = validationContext.StreamTypeService.EventTypes.Length == 1;
            EventType streamZeroType = null;
            if (validationContext.StreamTypeService.EventTypes.Length > 0) {
                streamZeroType = validationContext.StreamTypeService.EventTypes[0];
            }

            var staticMethodDesc = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                clazz.Name,
                null,
                firstItem.Name,
                firstItem.Parameters,
                allowWildcard,
                streamZeroType,
                new ExprNodeUtilResolveExceptionHandlerDefault(firstItem.Name, true),
                FunctionName,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);

            var allowValueCache = true;
            bool isReturnsConstantResult;
            switch (config.ValueCache) {
                case ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED:
                    isReturnsConstantResult = false;
                    allowValueCache = false;
                    break;

                case ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.CONFIGURED: {
                    var isUDFCache = validationContext.StatementCompileTimeService.Configuration.Compiler.Expression
                        .IsUdfCache;
                    isReturnsConstantResult = isUDFCache && staticMethodDesc.IsAllConstants && chainList.IsEmpty();
                    allowValueCache = isUDFCache;
                    break;
                }

                case ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.ENABLED:
                    isReturnsConstantResult = staticMethodDesc.IsAllConstants && chainList.IsEmpty();
                    break;

                default:
                    throw new IllegalStateException("Invalid value cache code " + config.ValueCache);
            }

            // this may return a pair of null if there is no lambda or the result cannot be wrapped for lambda-function use
            ExprDotStaticMethodWrap optionalLambdaWrap = ExprDotStaticMethodWrapFactory.Make(
                staticMethodDesc.ReflectionMethod,
                chainList,
                config.OptionalEventTypeName,
                validationContext);
            var typeInfo = optionalLambdaWrap != null
                ? optionalLambdaWrap.TypeInfo
                : EPTypeHelper.SingleValue(staticMethodDesc.ReflectionMethod.ReturnType);

            var eval = ExprDotNodeUtility.GetChainEvaluators(
                    -1,
                    typeInfo,
                    chainList,
                    validationContext,
                    false,
                    new ExprDotNodeFilterAnalyzerInputStatic())
                .ChainWithUnpack;
            var staticMethodForge = new ExprDotNodeForgeStaticMethod(
                this,
                isReturnsConstantResult,
                clazz.Name,
                staticMethodDesc.ReflectionMethod,
                staticMethodDesc.ChildForges,
                allowValueCache && staticMethodDesc.IsAllConstants,
                eval,
                optionalLambdaWrap,
                config.IsRethrowExceptions,
                null,
                validationContext.StatementName);

            // If caching the result, evaluate now and return the result.
            if (isReturnsConstantResult) {
                forge = new ExprPlugInSingleRowNodeForgeConst(this, staticMethodForge);
            }
            else {
                forge = new ExprPlugInSingleRowNodeForgeNC(this, staticMethodForge);
            }

            return null;
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec, this);
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            ExprNodeUtilityQuery.AcceptChain(visitor, ChainSpec, this);
        }

        public override void ReplaceUnlistedChildNode(
            ExprNode nodeToReplace,
            ExprNode newNode)
        {
            ExprNodeUtilityModify.ReplaceChainChildNode(nodeToReplace, newNode, ChainSpec);
        }
    }
} // end of namespace