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
using com.espertech.esper.compat.magic;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    using DataCollection = ICollection<object>;
    using AnyMap = IDictionary<object, object>;

    /// <summary>
    /// Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprInNodeImpl
        : ExprNodeBase
        , ExprEvaluator
        , ExprInNode
    {
        private readonly bool _isNotIn;

        private bool _mustCoerce;
        private bool _hasCollectionOrArray;

        [NonSerialized]
        private Coercer _coercer;
        [NonSerialized]
        private ExprEvaluator[] _evaluators;
        [NonSerialized]
        private Func<object, object>[] _transformList;

        /// <summary>Ctor. </summary>
        /// <param name="isNotIn">is true for "not in" and false for "in"</param>
        public ExprInNodeImpl(bool isNotIn)
        {
            _isNotIn = isNotIn;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns true for not-in, false for regular in </summary>
        /// <value>false for &quot;val in (a,b,c)&quot; or true for &quot;val not in (a,b,c)&quot;</value>
        public bool IsNotIn
        {
            get { return _isNotIn; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            ValidateWithoutContext();

            return null;
        }

        public void ValidateWithoutContext()
        {
            if (ChildNodes.Count < 2)
            {
                throw new ExprValidationException("The IN operator requires at least 2 child expressions");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);

            // Must be the same boxed type returned by expressions under this
            var typeOne = _evaluators[0].ReturnType.GetBoxedType();

            // collections, array or map not supported
            if ((typeOne.IsArray) ||
                (typeOne.IsGenericCollection()) ||
                (typeOne.IsGenericDictionary()))
            {
                throw new ExprValidationException("Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }

            _transformList = new Func<object, object>[_evaluators.Length];

            var comparedTypes = new List<Type> { typeOne };
            _hasCollectionOrArray = false;

            var length = ChildNodes.Count - 1;
            for (int i = 1; i <= length; i++)
            {
                var propType = _evaluators[i].ReturnType;
                if (propType == null)
                {
                    continue;
                }
                if (propType.IsArray)
                {
                    _hasCollectionOrArray = true;
                    if (propType.GetElementType() != typeof(Object))
                    {
                        comparedTypes.Add(propType.GetElementType());
                    }
                }
                else if (propType.IsGenericDictionary())
                {
                    var baseTransform = MagicMarker.GetDictionaryFactory(propType);
                    _transformList[i] = o => baseTransform(o);
                    _hasCollectionOrArray = true;
                }
                else if (propType.IsGenericCollection())
                {
                    var baseTransform = MagicMarker.GetCollectionFactory(propType);
                    _transformList[i] = o => baseTransform(o);
                    _hasCollectionOrArray = true;
                }
                else
                {
                    comparedTypes.Add(propType);
                }
            }

            // Determine common denominator type
            Type coercionType;
            try
            {
                coercionType = TypeHelper.GetCommonCoercionType(comparedTypes);
            }
            catch (CoercionException ex)
            {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }

            // Check if we need to coerce
            _mustCoerce = false;
            if (coercionType.IsNumeric())
            {
                foreach (Type compareType in comparedTypes)
                {
                    if (coercionType != compareType.GetBoxedType())
                    {
                        _mustCoerce = true;
                    }
                }
                if (_mustCoerce)
                {
                    _coercer = CoercerFactory.GetCoercer(null, coercionType.GetBoxedType());
                }
            }
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprIn(this); }
            var result = EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprIn(result); }
            return result;
        }

        private bool? EvaluateInternal(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var inPropResult = _evaluators[0].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));

            if (!_hasCollectionOrArray)
            {
                if ((_mustCoerce) && (inPropResult != null))
                {
                    inPropResult = _coercer.Invoke(inPropResult);
                }

                int len = this.ChildNodes.Count - 1;
                if ((len > 0) && (inPropResult == null))
                {
                    return null;
                }
                bool hasNullRow = false;
                for (int i = 1; i <= len; i++)
                {
                    var rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult == null)
                    {
                        hasNullRow = true;
                        continue;
                    }

                    if (!_mustCoerce)
                    {
                        if (rightResult.Equals(inPropResult))
                        {
                            return !_isNotIn;
                        }
                    }
                    else
                    {
                        var right = _coercer.Invoke(rightResult);
                        if (right.Equals(inPropResult))
                        {
                            return !_isNotIn;
                        }
                    }
                }

                if (hasNullRow)
                {
                    return null;
                }
                return _isNotIn;
            }
            else
            {
                var len = ChildNodes.Count - 1;
                var hasNullRow = false;
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                for (int i = 1; i <= len; i++)
                {
                    var rightResult = _evaluators[i].Evaluate(evaluateParams);
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult == null)
                    {
                        continue;
                    }

                    if (rightResult is AnyMap)
                    {
                        if (inPropResult == null)
                        {
                            return null;
                        }
                        var coll = (AnyMap)rightResult;
                        if (coll.ContainsKey(inPropResult))
                        {
                            return !_isNotIn;
                        }
                    }
                    else if (rightResult.GetType().IsArray)
                    {
                        var array = (Array)rightResult;
                        int arrayLength = array.Length;
                        if ((arrayLength > 0) && (inPropResult == null))
                        {
                            return null;
                        }
                        for (int index = 0; index < arrayLength; index++)
                        {
                            var item = array.GetValue(index);
                            if (item == null)
                            {
                                hasNullRow = true;
                                continue;
                            }
                            if (!_mustCoerce)
                            {
                                if (inPropResult.Equals(item))
                                {
                                    return !_isNotIn;
                                }
                            }
                            else
                            {
                                if (!item.IsNumber())
                                {
                                    continue;
                                }
                                var left = _coercer.Invoke(inPropResult);
                                var right = _coercer.Invoke(item);
                                if (left.Equals(right))
                                {
                                    return !_isNotIn;
                                }
                            }
                        }
                    }
                    else if (rightResult is DataCollection)
                    {
                        if (inPropResult == null)
                        {
                            return null;
                        }
                        var coll = (DataCollection)rightResult;
                        if (coll.Contains(inPropResult))
                        {
                            return !_isNotIn;
                        }
                    }
                    else
                    {
                        if (inPropResult == null)
                        {
                            return null;
                        }
                        if (!_mustCoerce)
                        {
                            if (inPropResult.Equals(rightResult))
                            {
                                return !_isNotIn;
                            }
                        }
                        else
                        {
                            var left = _coercer.Invoke(inPropResult);
                            var right = _coercer.Invoke(rightResult);
                            if (left.Equals(right))
                            {
                                return !_isNotIn;
                            }
                        }
                    }
                }

                if (hasNullRow)
                {
                    return null;
                }
                return _isNotIn;
            }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprInNodeImpl))
            {
                return false;
            }

            var other = (ExprInNodeImpl)node;
            return other._isNotIn == _isNotIn;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";

            IEnumerator<ExprNode> it = ChildNodes.Cast<ExprNode>().GetEnumerator();
            it.MoveNext();
            it.Current.ToEPL(writer, Precedence);
            writer.Write(_isNotIn ? " not in (" : " in (");

            while (it.MoveNext())
            {
                ExprNode inSetValueExpr = it.Current;
                writer.Write(delimiter);
                inSetValueExpr.ToEPL(writer, Precedence);
                delimiter = ",";
            }

            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }
    }
}
