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
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the case-when-then-else control flow function is an expression tree.
    /// </summary>
    public class ExprCaseNode : ExprNodeBase
    {
        [NonSerialized] private ExprCaseNodeForge forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isCase2">
        ///     is an indicator of which Case statement we are working on.
        ///     <para />
        ///     True indicates a 'Case2' statement with syntax "case a when a1 then b1 else b2".
        ///     <para />
        ///     False indicates a 'Case1' statement with syntax "case when a=a1 then b1 else b2".
        /// </param>
        public ExprCaseNode(bool isCase2)
        {
            IsCase2 = isCase2;
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

        /// <summary>
        ///     Returns true if this is a switch-type case.
        /// </summary>
        /// <returns>true for switch-type case, or false for when-then type</returns>
        public bool IsCase2 { get; }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.CASE;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var analysis = AnalyzeCase();

            foreach (var pair in analysis.WhenThenNodeList) {
                if (!IsCase2) {
                    var returnType = pair.First.Forge.EvaluationType;
                    if (returnType != typeof(bool) && returnType != typeof(bool?)) {
                        throw new ExprValidationException("Case node 'when' expressions must return a boolean value");
                    }
                }
            }

            var mustCoerce = false;
            Coercer coercer = null;
            if (IsCase2) {
                // validate we can compare result types
                var comparedTypes = new List<Type>();
                comparedTypes.Add(analysis.OptionalCompareExprNode.Forge.EvaluationType);
                foreach (var pair in analysis.WhenThenNodeList) {
                    comparedTypes.Add(pair.First.Forge.EvaluationType);
                }

                // Determine common denominator type
                try {
                    var coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());

                    // Determine if we need to coerce numbers when one type doesn't match any other type
                    if (coercionType.IsNumeric()) {
                        mustCoerce = false;
                        foreach (var comparedType in comparedTypes) {
                            if (comparedType != coercionType) {
                                mustCoerce = true;
                            }
                        }

                        if (mustCoerce) {
                            coercer = SimpleNumberCoercerFactory.GetCoercer(null, coercionType);
                        }
                    }
                }
                catch (CoercionException ex) {
                    throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
                }
            }

            // Determine type of each result (then-node and else node) child node expression
            IList<Type> childTypes = new List<Type>();
            IList<IDictionary<string, object>> childMapTypes = new List<IDictionary<string, object>>();
            foreach (var pair in analysis.WhenThenNodeList) {
                if (pair.Second.Forge is ExprTypableReturnForge) {
                    var typableReturn = (ExprTypableReturnForge) pair.Second.Forge;
                    var rowProps = typableReturn.RowProperties;
                    if (rowProps != null) {
                        childMapTypes.Add(rowProps);
                        continue;
                    }
                }

                childTypes.Add(pair.Second.Forge.EvaluationType);
            }

            if (analysis.OptionalElseExprNode != null) {
                if (analysis.OptionalElseExprNode.Forge is ExprTypableReturnForge) {
                    var typableReturn = (ExprTypableReturnForge) analysis.OptionalElseExprNode.Forge;
                    var rowProps = typableReturn.RowProperties;
                    if (rowProps != null) {
                        childMapTypes.Add(rowProps);
                    }
                    else {
                        childTypes.Add(analysis.OptionalElseExprNode.Forge.EvaluationType);
                    }
                }
                else {
                    childTypes.Add(analysis.OptionalElseExprNode.Forge.EvaluationType);
                }
            }

            if (!childMapTypes.IsEmpty() && !childTypes.IsEmpty()) {
                var message =
                    "Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value";
                string check;
                var count = -1;
                foreach (var pair in analysis.WhenThenNodeList) {
                    count++;
                    if (pair.Second.Forge.EvaluationType != null &&
                        pair.Second.Forge.EvaluationType.IsNotGenericDictionary()) {
                        check = ", check when-condition number " + count;
                        throw new ExprValidationException(message + check);
                    }
                }

                if (analysis.OptionalElseExprNode != null) {
                    if (analysis.OptionalElseExprNode.Forge.EvaluationType != null &&
                        analysis.OptionalElseExprNode.Forge.EvaluationType.IsNotGenericDictionary()) {
                        check = ", check the else-condition";
                        throw new ExprValidationException(message + check);
                    }
                }

                throw new ExprValidationException(message);
            }

            IDictionary<string, object> mapResultType = null;
            Type resultType = null;
            var isNumericResult = false;
            if (childMapTypes.IsEmpty()) {
                // Determine common denominator type
                try {
                    resultType = TypeHelper
                        .GetCommonCoercionType(childTypes.ToArray())
                        .GetBoxedType();
                    if (resultType.IsNumeric()) {
                        isNumericResult = true;
                    }
                }
                catch (CoercionException ex) {
                    throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
                }
            }
            else {
                resultType = typeof(IDictionary<string, object>);
                mapResultType = childMapTypes[0];
                for (var i = 1; i < childMapTypes.Count; i++) {
                    var other = childMapTypes[i];
                    var messageEquals = BaseNestableEventType.IsDeepEqualsProperties(
                        "Case-when number " + i,
                        mapResultType,
                        other);
                    if (messageEquals != null) {
                        throw new ExprValidationException(
                            "Incompatible case-when return types by new-operator in case-when number " +
                            i +
                            ": " +
                            messageEquals.Message,
                            messageEquals);
                    }
                }
            }

            forge = new ExprCaseNodeForge(
                this,
                resultType,
                mapResultType,
                isNumericResult,
                mustCoerce,
                coercer,
                analysis.WhenThenNodeList,
                analysis.OptionalCompareExprNode,
                analysis.OptionalElseExprNode);
            return null;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprCaseNode)) {
                return false;
            }

            var otherExprCaseNode = (ExprCaseNode) node;
            return IsCase2 == otherExprCaseNode.IsCase2;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            CaseAnalysis analysis;
            try {
                analysis = AnalyzeCase();
            }
            catch (ExprValidationException e) {
                throw new EPRuntimeException(e.Message, e);
            }

            writer.Write("case");
            if (IsCase2) {
                writer.Write(' ');
                analysis.OptionalCompareExprNode.ToEPL(writer, Precedence, flags);
            }

            foreach (var p in analysis.WhenThenNodeList) {
                writer.Write(" when ");
                p.First.ToEPL(writer, Precedence, flags);
                writer.Write(" then ");
                p.Second.ToEPL(writer, Precedence, flags);
            }

            if (analysis.OptionalElseExprNode != null) {
                writer.Write(" else ");
                analysis.OptionalElseExprNode.ToEPL(writer, Precedence, flags);
            }

            writer.Write(" end");
        }

        private CaseAnalysis AnalyzeCaseOne()
        {
            // Case 1 expression example:
            //      case when a=b then x [when c=d then y...] [else y]
            //
            var children = ChildNodes;
            if (children.Length < 2) {
                throw new ExprValidationException("Case node must have at least 2 parameters");
            }

            IList<UniformPair<ExprNode>> whenThenNodeList = new List<UniformPair<ExprNode>>();
            var numWhenThen = children.Length >> 1;
            for (var i = 0; i < numWhenThen; i++) {
                var whenExpr = children[i << 1];
                var thenExpr = children[(i << 1) + 1];
                whenThenNodeList.Add(new UniformPair<ExprNode>(whenExpr, thenExpr));
            }

            ExprNode optionalElseExprNode = null;
            if (children.Length % 2 != 0) {
                optionalElseExprNode = children[children.Length - 1];
            }

            return new CaseAnalysis(whenThenNodeList, null, optionalElseExprNode);
        }

        private CaseAnalysis AnalyzeCaseTwo()
        {
            // Case 2 expression example:
            //      case p when p1 then x [when p2 then y...] [else z]
            //
            var children = ChildNodes;
            if (children.Length < 3) {
                throw new ExprValidationException("Case node must have at least 3 parameters");
            }

            var optionalCompareExprNode = children[0];

            IList<UniformPair<ExprNode>> whenThenNodeList = new List<UniformPair<ExprNode>>();
            var numWhenThen = (children.Length - 1) / 2;
            for (var i = 0; i < numWhenThen; i++) {
                whenThenNodeList.Add(new UniformPair<ExprNode>(children[i * 2 + 1], children[i * 2 + 2]));
            }

            ExprNode optionalElseExprNode = null;
            if (numWhenThen * 2 + 1 < children.Length) {
                optionalElseExprNode = children[children.Length - 1];
            }

            return new CaseAnalysis(whenThenNodeList, optionalCompareExprNode, optionalElseExprNode);
        }

        private CaseAnalysis AnalyzeCase()
        {
            if (IsCase2) {
                return AnalyzeCaseTwo();
            }

            return AnalyzeCaseOne();
        }

        public class CaseAnalysis
        {
            public CaseAnalysis(
                IList<UniformPair<ExprNode>> whenThenNodeList,
                ExprNode optionalCompareExprNode,
                ExprNode optionalElseExprNode)
            {
                WhenThenNodeList = whenThenNodeList;
                OptionalCompareExprNode = optionalCompareExprNode;
                OptionalElseExprNode = optionalElseExprNode;
            }

            public IList<UniformPair<ExprNode>> WhenThenNodeList { get; }

            public ExprNode OptionalCompareExprNode { get; }

            public ExprNode OptionalElseExprNode { get; }
        }
    }
} // end of namespace