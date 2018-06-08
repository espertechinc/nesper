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

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.map;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Represents the case-when-then-else control flow function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprCaseNode
        : ExprNodeBase
        , ExprEvaluator
        , ExprEvaluatorTypableReturn
    {
        private readonly bool _isCase2;
        private Type _resultType;
        [NonSerialized]
        private DataMap _mapResultType;
        private bool _isNumericResult;
        private bool _mustCoerce;

        [NonSerialized]
        private Coercer _coercer;
        [NonSerialized]
        private IList<UniformPair<ExprEvaluator>> _whenThenNodeList;
        [NonSerialized]
        private ExprEvaluator _optionalCompareExprNode;
        [NonSerialized]
        private ExprEvaluator _optionalElseExprNode;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isCase2">is an indicator of which Case statement we are working on.
        /// <para/> True indicates a 'Case2' statement with syntax "case a when a1 then b1 else b2".
        /// <para/> False indicates a 'Case1' statement with syntax "case when a=a1 then b1 else b2".
        /// </param>
        public ExprCaseNode(bool isCase2)
        {
            _isCase2 = isCase2;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns true if this is a switch-type case. </summary>
        /// <value>true for switch-type case, or false for when-then type</value>
        public bool IsCase2
        {
            get { return _isCase2; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            CaseAnalysis analysis = AnalyzeCase();

            _whenThenNodeList = new List<UniformPair<ExprEvaluator>>();
            foreach (UniformPair<ExprNode> pair in analysis.WhenThenNodeList)
            {
                if (!_isCase2)
                {
                    if (pair.First.ExprEvaluator.ReturnType.GetBoxedType() != typeof(bool?))
                    {
                        throw new ExprValidationException("Case node 'when' expressions must return a boolean value");
                    }
                }
                _whenThenNodeList.Add(new UniformPair<ExprEvaluator>(pair.First.ExprEvaluator, pair.Second.ExprEvaluator));
            }
            if (analysis.OptionalCompareExprNode != null)
            {
                _optionalCompareExprNode = analysis.OptionalCompareExprNode.ExprEvaluator;
            }
            if (analysis.OptionalElseExprNode != null)
            {
                _optionalElseExprNode = analysis.OptionalElseExprNode.ExprEvaluator;
            }

            if (_isCase2)
            {
                ValidateCaseTwo();
            }

            // Determine type of each result (then-node and else node) child node expression
            IList<Type> childTypes = new List<Type>();
            IList<IDictionary<String, Object>> childMapTypes = new List<IDictionary<String, Object>>();
            foreach (UniformPair<ExprEvaluator> pair in _whenThenNodeList)
            {
                if (pair.Second is ExprEvaluatorTypableReturn)
                {
                    var typableReturn = (ExprEvaluatorTypableReturn)pair.Second;
                    var rowProps = typableReturn.RowProperties;
                    if (rowProps != null)
                    {
                        childMapTypes.Add(rowProps);
                        continue;
                    }
                }
                childTypes.Add(pair.Second.ReturnType);

            }
            if (_optionalElseExprNode != null)
            {
                if (_optionalElseExprNode is ExprEvaluatorTypableReturn)
                {
                    var typableReturn = (ExprEvaluatorTypableReturn)_optionalElseExprNode;
                    var rowProps = typableReturn.RowProperties;
                    if (rowProps != null)
                    {
                        childMapTypes.Add(rowProps);
                    }
                    else
                    {
                        childTypes.Add(_optionalElseExprNode.ReturnType);
                    }
                }
                else
                {
                    childTypes.Add(_optionalElseExprNode.ReturnType);
                }
            }

            if (!childMapTypes.IsEmpty() && !childTypes.IsEmpty())
            {
                String message = "Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value";
                String check;
                int count = -1;
                foreach (UniformPair<ExprEvaluator> pair in _whenThenNodeList)
                {
                    count++;
                    if (pair.Second.ReturnType != typeof(DataMap) && pair.Second.ReturnType != null)
                    {
                        check = ", check when-condition number " + count;
                        throw new ExprValidationException(message + check);
                    }
                }
                if (_optionalElseExprNode != null)
                {
                    if (_optionalElseExprNode.ReturnType != typeof(DataMap) && _optionalElseExprNode.ReturnType != null)
                    {
                        check = ", check the else-condition";
                        throw new ExprValidationException(message + check);
                    }
                }
                throw new ExprValidationException(message);
            }

            if (childMapTypes.IsEmpty())
            {
                // Determine common denominator type
                try
                {
                    _resultType = TypeHelper.GetCommonCoercionType(childTypes);
                    if (_resultType.IsNumeric())
                    {
                        _isNumericResult = true;
                    }
                }
                catch (CoercionException ex)
                {
                    throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
                }
            }
            else
            {
                _mapResultType = childMapTypes[0];
                for (int i = 1; i < childMapTypes.Count; i++)
                {
                    DataMap other = childMapTypes[i];
                    String messageEquals = MapEventType.IsDeepEqualsProperties("Case-when number " + i, _mapResultType, other);
                    if (messageEquals != null)
                    {
                        throw new ExprValidationException("Incompatible case-when return types by new-operator in case-when number " + i + ": " + messageEquals);
                    }
                }
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return _resultType; }
        }

        public IDictionary<string, object> RowProperties
        {
            get { return _mapResultType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var result = new Mutable<object>();

            Instrument.With(
                i => i.QExprCase(this),
                i => i.AExprCase(result.Value),
                () =>
                {
                    if (!_isCase2)
                    {
                        result.Value = EvaluateCaseSyntax1(
                            evaluateParams.EventsPerStream,
                            evaluateParams.IsNewData,
                            evaluateParams.ExprEvaluatorContext);
                    }
                    else
                    {
                        result.Value = EvaluateCaseSyntax2(
                            evaluateParams.EventsPerStream,
                            evaluateParams.IsNewData,
                            evaluateParams.ExprEvaluatorContext);
                    }
                });

            return result.Value;
        }

        public bool? IsMultirow
        {
            get
            {
                if (_mapResultType == null)
                    return null;
                return false;
            }
        }

        public Object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var map = (IDictionary<String, Object>)Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            var row = new Object[map.Count];
            int index = -1;
            foreach (var entry in _mapResultType)
            {
                index++;
                row[index] = map.Get(entry.Key);
            }
            return row;
        }

        public Object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;    // always single-row
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var otherExprCaseNode = node as ExprCaseNode;
            if (otherExprCaseNode == null)
            {
                return false;
            }

            return _isCase2 == otherExprCaseNode._isCase2;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            CaseAnalysis analysis;
            try
            {
                analysis = AnalyzeCase();
            }
            catch (ExprValidationException e)
            {
                throw new Exception(e.Message, e);
            }

            writer.Write("case");
            if (_isCase2)
            {
                writer.Write(' ');
                analysis.OptionalCompareExprNode.ToEPL(writer, Precedence);
            }
            foreach (UniformPair<ExprNode> p in analysis.WhenThenNodeList)
            {
                writer.Write(" when ");
                p.First.ToEPL(writer, Precedence);
                writer.Write(" then ");
                p.Second.ToEPL(writer, Precedence);
            }
            if (analysis.OptionalElseExprNode != null)
            {
                writer.Write(" else ");
                analysis.OptionalElseExprNode.ToEPL(writer, Precedence);
            }
            writer.Write(" end");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.CASE; }
        }

        private CaseAnalysis AnalyzeCaseOne()
        {
            // Case 1 expression example:
            //      case when a=b then x [when c=d then y...] [else y]
            //
            var children = ChildNodes;
            if (children.Count < 2)
            {
                throw new ExprValidationException("Case node must have at least 2 parameters");
            }

            IList<UniformPair<ExprNode>> whenThenNodeList = new List<UniformPair<ExprNode>>();
            int numWhenThen = children.Count >> 1;
            for (int i = 0; i < numWhenThen; i++)
            {
                ExprNode whenExpr = children[(i << 1)];
                ExprNode thenExpr = children[(i << 1) + 1];
                whenThenNodeList.Add(new UniformPair<ExprNode>(whenExpr, thenExpr));
            }
            ExprNode optionalElseExprNode = null;
            if (children.Count % 2 != 0)
            {
                optionalElseExprNode = children[children.Count - 1];
            }
            return new CaseAnalysis(whenThenNodeList, null, optionalElseExprNode);
        }

        private CaseAnalysis AnalyzeCaseTwo()
        {
            // Case 2 expression example:
            //      case p when p1 then x [when p2 then y...] [else z]
            //
            var children = ChildNodes;
            if (children.Count < 3)
            {
                throw new ExprValidationException("Case node must have at least 3 parameters");
            }

            ExprNode optionalCompareExprNode = children[0];

            IList<UniformPair<ExprNode>> whenThenNodeList = new List<UniformPair<ExprNode>>();
            int numWhenThen = (children.Count - 1) / 2;
            for (int i = 0; i < numWhenThen; i++)
            {
                whenThenNodeList.Add(new UniformPair<ExprNode>(children[i * 2 + 1], children[i * 2 + 2]));
            }
            ExprNode optionalElseExprNode = null;
            if (numWhenThen * 2 + 1 < children.Count)
            {
                optionalElseExprNode = children[children.Count - 1];
            }
            return new CaseAnalysis(whenThenNodeList, optionalCompareExprNode, optionalElseExprNode);
        }

        private void ValidateCaseTwo()
        {
            // validate we can compare result types
            IList<Type> comparedTypes = new List<Type>();
            comparedTypes.Add(_optionalCompareExprNode.ReturnType);
            foreach (UniformPair<ExprEvaluator> pair in _whenThenNodeList)
            {
                comparedTypes.Add(pair.First.ReturnType);
            }

            // Determine common denominator type
            try
            {
                Type coercionType = TypeHelper.GetCommonCoercionType(comparedTypes);

                // Determine if we need to coerce numbers when one type doesn't match any other type
                if (coercionType.IsNumeric())
                {
                    _mustCoerce = false;
                    foreach (Type comparedType in comparedTypes)
                    {
                        if (comparedType != coercionType)
                        {
                            _mustCoerce = true;
                        }
                    }
                    if (_mustCoerce)
                    {
                        _coercer = CoercerFactory.GetCoercer(null, coercionType);
                    }
                }
            }
            catch (CoercionException ex)
            {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }
        }

        private Object EvaluateCaseSyntax1(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Case 1 expression example:
            //      case when a=b then x [when c=d then y...] [else y]

            Object caseResult = null;
            bool matched = false;
            foreach (UniformPair<ExprEvaluator> p in _whenThenNodeList)
            {
                var whenResult = (bool?)p.First.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));

                // If the 'when'-expression returns true
                if (whenResult ?? false)
                {
                    caseResult = p.Second.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    matched = true;
                    break;
                }
            }

            if ((!matched) && (_optionalElseExprNode != null))
            {
                caseResult = _optionalElseExprNode.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            }

            if (caseResult == null)
            {
                return null;
            }

            if ((caseResult.GetType() != _resultType) && (_isNumericResult))
            {
                return CoercerFactory.CoerceBoxed(caseResult, _resultType);
            }
            return caseResult;
        }

        private Object EvaluateCaseSyntax2(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);

            // Case 2 expression example:
            //      case p when p1 then x [when p2 then y...] [else z]

            Object checkResult = _optionalCompareExprNode.Evaluate(evaluateParams);
            Object caseResult = null;
            bool matched = false;

            foreach (UniformPair<ExprEvaluator> p in _whenThenNodeList)
            {
                var whenResult = p.First.Evaluate(evaluateParams);
                if (Compare(checkResult, whenResult))
                {
                    caseResult = p.Second.Evaluate(evaluateParams);
                    matched = true;
                    break;
                }
            }

            if ((!matched) && (_optionalElseExprNode != null))
            {
                caseResult = _optionalElseExprNode.Evaluate(evaluateParams);
            }

            if (caseResult == null)
            {
                return null;
            }

            if ((caseResult.GetType() != _resultType) && (_isNumericResult))
            {
                return CoercerFactory.CoerceBoxed(caseResult, _resultType);
            }
            return caseResult;
        }

        private bool Compare(Object leftResult, Object rightResult)
        {
            if (leftResult == null)
            {
                return (rightResult == null);
            }
            if (rightResult == null)
            {
                return false;
            }

            if (!_mustCoerce)
            {
                return leftResult.Equals(rightResult);
            }
            else
            {
                var left = _coercer.Invoke(leftResult);
                var right = _coercer.Invoke(rightResult);
                return left.Equals(right);
            }
        }

        private CaseAnalysis AnalyzeCase()
        {
            if (_isCase2)
            {
                return AnalyzeCaseTwo();
            }
            else
            {
                return AnalyzeCaseOne();
            }
        }

        public class CaseAnalysis
        {
            public CaseAnalysis(IList<UniformPair<ExprNode>> whenThenNodeList, ExprNode optionalCompareExprNode, ExprNode optionalElseExprNode)
            {
                WhenThenNodeList = whenThenNodeList;
                OptionalCompareExprNode = optionalCompareExprNode;
                OptionalElseExprNode = optionalElseExprNode;
            }

            public IList<UniformPair<ExprNode>> WhenThenNodeList { get; private set; }

            public ExprNode OptionalCompareExprNode { get; private set; }

            public ExprNode OptionalElseExprNode { get; private set; }
        }
    }

}
