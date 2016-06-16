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

using Castle.Components.DictionaryAdapter.Xml;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
	/// <summary>
	/// Represents an equals (=) comparator in a filter expressiun tree.
	/// </summary>
	[Serializable]
    public class ExprEqualsNodeImpl
        : ExprNodeBase
        , ExprEqualsNode
	{
	    internal readonly bool vIsNotEquals;
	    internal readonly bool vIsIs;
	    [NonSerialized] private ExprEvaluator _evaluator;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="isNotEquals">true if this is a (!=) not equals rather then equals, false if its a '=' equals</param>
	    /// <param name="isIs">true when "is" or "is not" (instead of = or &lt;&gt;)</param>
	    public ExprEqualsNodeImpl(bool isNotEquals, bool isIs)
	    {
	        vIsNotEquals = isNotEquals;
	        vIsIs = isIs;
	    }

	    public override ExprEvaluator ExprEvaluator
	    {
	        get { return _evaluator; }
	    }

	    public bool IsNotEquals
	    {
	        get { return vIsNotEquals; }
	    }

	    public bool IsIs
	    {
	        get { return vIsIs; }
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
	    {
	        // Must have 2 child nodes
	        if (ChildNodes.Length != 2)
	        {
                throw new ExprValidationException("Invalid use of equals, expecting left-hand side and right-hand side but received " + ChildNodes.Length + " expressions");
	        }
	        var evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);

	        // Must be the same boxed type returned by expressions under this
	        var typeOne = evaluators[0].ReturnType.GetBoxedType();
	        var typeTwo = evaluators[1].ReturnType.GetBoxedType();

	        // Null constants can be compared for any type
	        if ((typeOne == null) || (typeTwo == null))
	        {
	            _evaluator = GetEvaluator(evaluators[0], evaluators[1]);
	            return null;
	        }

	        if (typeOne == typeTwo || typeOne.IsAssignableFrom(typeTwo))
	        {
	            _evaluator = GetEvaluator(evaluators[0], evaluators[1]);
	            return null;
	        }

	        // Get the common type such as Bool, String or Double and Long
	        Type coercionType;
	        try
	        {
	            coercionType = typeOne.GetCompareToCoercionType(typeTwo);
	        }
	        catch (CoercionException)
	        {
	            throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to '{1}' is not allowed", typeTwo.FullName, typeOne.FullName));
	        }

	        // Check if we need to coerce
	        if ((coercionType == typeOne.GetBoxedType()) &&
	            (coercionType == typeTwo.GetBoxedType()))
	        {
	            _evaluator = GetEvaluator(evaluators[0], evaluators[1]);
	        }
            else if ((typeOne.IsArray) && (typeTwo.IsArray) && (typeOne.GetElementType().GetBoxedType() == typeTwo.GetElementType().GetBoxedType()))
            {
                coercionType = typeOne.GetElementType().GetCompareToCoercionType(typeTwo.GetElementType());
                _evaluator = new ExprEqualsEvaluatorCoercingArray(
                    this, evaluators[0], evaluators[1],
                    CoercerFactory.GetCoercer(typeOne.GetComponentType(), coercionType),
                    CoercerFactory.GetCoercer(typeTwo.GetComponentType(), coercionType));
            }
            else
	        {
	            if (!coercionType.IsNumeric())
	            {
	                throw new ExprValidationException("Cannot convert datatype '" + coercionType.Name + "' to a numeric value");
	            }
	            _evaluator = new ExprEqualsEvaluatorCoercing(
	                this, evaluators[0], evaluators[1],
	                CoercerFactory.GetCoercer(typeOne, coercionType),
	                CoercerFactory.GetCoercer(typeTwo, coercionType));
	        }
	        return null;
	    }

	    public override bool IsConstantResult
	    {
	        get { return false; }
	    }

	    public IDictionary<string, object> EventType
	    {
	        get { return null; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        ChildNodes[0].ToEPL(writer, Precedence);
	        if (vIsIs) {
                writer.Write(" is ");
	            if (vIsNotEquals) {
                    writer.Write("not ");
	            }
	        }
	        else {
	            if (!vIsNotEquals) {
                    writer.Write("=");
	            }
	            else {
	                writer.Write("!=");
	            }
	        }
	        ChildNodes[1].ToEPL(writer, Precedence);
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.EQUALS; }
	    }

	    public override bool EqualsNode(ExprNode node)
	    {
	        var other = node as ExprEqualsNodeImpl;
	        return other != null && other.vIsNotEquals == vIsNotEquals;
	    }

	    private ExprEvaluator GetEvaluator(ExprEvaluator lhs, ExprEvaluator rhs) {
	        if (vIsIs) {
	            return new ExprEqualsEvaluatorIs(this, lhs, rhs);
	        }
	        else {
	            return new ExprEqualsEvaluatorEquals(this, lhs, rhs);
	        }
	    }

	    [Serializable]
	    public class ExprEqualsEvaluatorCoercingArray : ExprEvaluator
	    {
	        [NonSerialized] private readonly ExprEqualsNodeImpl _parent;
	        [NonSerialized] private readonly ExprEvaluator _lhs;
	        [NonSerialized] private readonly ExprEvaluator _rhs;
	        [NonSerialized] private readonly Coercer _coercerLHS;
	        [NonSerialized] private readonly Coercer _coercerRHS;

            public ExprEqualsEvaluatorCoercingArray(ExprEqualsNodeImpl parent, ExprEvaluator lhs, ExprEvaluator rhs, Coercer coercerLHS, Coercer coercerRHS)
            {
	            _parent = parent;
	            _lhs = lhs;
	            _rhs = rhs;
	            _coercerLHS = coercerLHS;
	            _coercerRHS = coercerRHS;
	        }

            public object Evaluate(EvaluateParams evaluateParams)
            {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprEquals(_parent);}
	            var result = EvaluateInternal(evaluateParams);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprEquals(result);}
	            return result;
	        }

            private bool? EvaluateInternal(EvaluateParams evaluateParams)
            {
	            var leftResult = _lhs.Evaluate(evaluateParams);
	            var rightResult = _rhs.Evaluate(evaluateParams);

                if (!_parent.IsIs)
                {
                    if (leftResult == null || rightResult == null) // null comparison
                    {
                        return null;
                    }
                }
                else if (leftResult == null)
                {
                    return rightResult == null;
                }
                if (rightResult == null)
                {
                    return false;
                }

                var leftArray = (Array) leftResult;
                var rightArray = (Array) rightResult;

                if (leftArray.Length != rightArray.Length)
                {
                    return !_parent.vIsNotEquals;
                }

                var isEquals = true;

                for (int ii = 0; isEquals && ii < leftArray.Length; ii++)
                {
                    var valueL = _coercerLHS.Invoke(leftArray.GetValue(ii));
                    var valueR = _coercerRHS.Invoke(rightArray.GetValue(ii));
                    isEquals &= Equals(valueL, valueR);
                }

                return isEquals ^ _parent.vIsNotEquals;
	        }

            public Type ReturnType
            {
                get { return typeof (bool?); }
            }
	    }

	    [Serializable]
	    public class ExprEqualsEvaluatorCoercing : ExprEvaluator
        {
	        [NonSerialized] private readonly ExprEqualsNodeImpl _parent;
	        [NonSerialized] private readonly ExprEvaluator _lhs;
	        [NonSerialized] private readonly ExprEvaluator _rhs;
	        [NonSerialized] private readonly Coercer _numberCoercerLHS;
	        [NonSerialized] private readonly Coercer _numberCoercerRHS;

	        public ExprEqualsEvaluatorCoercing(ExprEqualsNodeImpl parent, ExprEvaluator lhs, ExprEvaluator rhs, Coercer numberCoercerLHS, Coercer numberCoercerRHS)
            {
	            _parent = parent;
	            _lhs = lhs;
	            _rhs = rhs;
	            _numberCoercerLHS = numberCoercerLHS;
	            _numberCoercerRHS = numberCoercerRHS;
	        }

            public object Evaluate(EvaluateParams evaluateParams)
            {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprEquals(_parent);}
	            var result = EvaluateInternal(evaluateParams);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprEquals(result);}
	            return result;
	        }

            private bool? EvaluateInternal(EvaluateParams evaluateParams)
            {
	            var leftResult = _lhs.Evaluate(evaluateParams);
	            var rightResult = _rhs.Evaluate(evaluateParams);

	            if (!_parent.vIsIs)
                {
	                if (leftResult == null || rightResult == null)  // null comparison
	                {
	                    return null;
	                }
	            }
	            else {
	                if (leftResult == null) {
	                    return rightResult == null;
	                }
	                if (rightResult == null) {
	                    return false;
	                }
	            }

	            var left = _numberCoercerLHS.Invoke(leftResult);
	            var right = _numberCoercerRHS.Invoke(rightResult);
	            return left.Equals(right) ^ _parent.vIsNotEquals;
	        }

            public Type ReturnType
            {
                get { return typeof (bool?); }
            }
        }

	    [Serializable] 
        public class ExprEqualsEvaluatorEquals : ExprEvaluator
        {
	        [NonSerialized] private readonly ExprEqualsNodeImpl _parent;
	        [NonSerialized] private readonly ExprEvaluator _lhs;
	        [NonSerialized] private readonly ExprEvaluator _rhs;

	        public ExprEqualsEvaluatorEquals(ExprEqualsNodeImpl parent, ExprEvaluator lhs, ExprEvaluator rhs) {
	            _parent = parent;
	            _lhs = lhs;
	            _rhs = rhs;
	        }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                if (InstrumentationHelper.ENABLED)
                {
	                InstrumentationHelper.Get().QExprEquals(_parent);
                    var leftResultX = _lhs.Evaluate(evaluateParams);
                    var rightResultX = _rhs.Evaluate(evaluateParams);
	                if (leftResultX == null || rightResultX == null) { // null comparison
	                    InstrumentationHelper.Get().AExprEquals(null);
	                    return null;
	                }
	                var result = leftResultX.Equals(rightResultX) ^ _parent.IsNotEquals;
	                InstrumentationHelper.Get().AExprEquals(result);
	                return result;
	            }

                var leftResult = _lhs.Evaluate(evaluateParams);
                if (leftResult == null) { // null comparison
                    return null;
                }

                var rightResult = _rhs.Evaluate(evaluateParams);
	            if (rightResult == null) { // null comparison
	                return null;
	            }

	            return leftResult.Equals(rightResult) ^ _parent.vIsNotEquals;
	        }

	        public Type ReturnType
	        {
	            get { return typeof (bool?); }
	        }
        }

	    [Serializable]
        public class ExprEqualsEvaluatorIs : ExprEvaluator
        {
	        [NonSerialized] private readonly ExprEqualsNodeImpl _parent;
	        [NonSerialized] private readonly ExprEvaluator _lhs;
	        [NonSerialized] private readonly ExprEvaluator _rhs;

	        public ExprEqualsEvaluatorIs(ExprEqualsNodeImpl parent, ExprEvaluator lhs, ExprEvaluator rhs)
            {
	            _parent = parent;
	            _lhs = lhs;
	            _rhs = rhs;
	        }

	        public object Evaluate(EvaluateParams evaluateParams)
	        {
	            if (InstrumentationHelper.ENABLED) {
	                InstrumentationHelper.Get().QExprIs(_parent);
                    var leftResultX = _lhs.Evaluate(evaluateParams);
                    var rightResultX = _rhs.Evaluate(evaluateParams);

	                bool result;
	                if (leftResultX == null) {
	                    result = rightResultX == null ^ _parent.IsNotEquals;
	                }
	                else {
                        result = (rightResultX != null && leftResultX.Equals(rightResultX)) ^ _parent.IsNotEquals;
	                }
	                InstrumentationHelper.Get().AExprIs(result);
	                return result;
	            }

                var leftResult = _lhs.Evaluate(evaluateParams);
                var rightResult = _rhs.Evaluate(evaluateParams);

	            if (leftResult == null) {
                    return rightResult == null ^ _parent.vIsNotEquals;
	            }
                return (rightResult != null && leftResult.Equals(rightResult)) ^ _parent.vIsNotEquals;
	        }

	        public Type ReturnType
	        {
	            get { return typeof (bool?); }
	        }
        }
	}
} // end of namespace
