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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

using DataCollection = System.Collections.Generic.ICollection<object>;
using DataMap = System.Collections.Generic.IDictionary<string, object>;
using AnyMap = System.Collections.Generic.IDictionary<object, object>;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents an equals-for-group (= ANY/ALL/SOME (expression list)) comparator in
    /// a expression tree.
    /// </summary>
    [Serializable]
    public class ExprEqualsAllAnyNode
        : ExprNodeBase
        , ExprEvaluator
    {
        [NonSerialized]
        private Coercer _coercer;
        private bool _hasCollectionOrArray;
        private bool _mustCoerce;

        private Func<object, object>[] _transformList;

        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isNotEquals">true if this is a (!=) not equals rather then equals, false if its a '=' equals</param>
        /// <param name="isAll">true if all, false for any</param>
        public ExprEqualsAllAnyNode(bool isNotEquals, bool isAll)
        {
            IsNot = isNotEquals;
            IsAll = isAll;
        }

        /// <summary>
        /// Gets the expression evaluator.
        /// </summary>
        /// <value>The expression evaluator.</value>
        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>
        /// Returns true if this is a NOT EQUALS node, false if this is a EQUALS node.
        /// </summary>
        /// <returns>
        /// true for !=, false for =
        /// </returns>
        public bool IsNot { get; private set; }

        /// <summary>
        /// True if all.
        /// </summary>
        /// <returns>
        /// all-flag
        /// </returns>
        public bool IsAll { get; private set; }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            var childNodes = ChildNodes;
            if (childNodes.Count < 1)
            {
                throw new IllegalStateException("Equals group node does not have 1 or more parameters");
            }

            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);

            // Must be the same boxed type returned by expressions under this
            Type typeOne = _evaluators[0].ReturnType.GetBoxedType();

            // collections, array or map not supported
            if ((typeOne.IsArray) ||
                (typeOne.IsGenericCollection()) ||
                (typeOne.IsGenericDictionary()))
            {
                throw new ExprValidationException(
                    "Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }

            _transformList = new Func<object, object>[childNodes.Count];

            var comparedTypes = new List<Type>();
            comparedTypes.Add(typeOne);
            _hasCollectionOrArray = false;
            for (int i = 1; i < childNodes.Count; i++)
            {
                Type propType = _evaluators[i].ReturnType;
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

            return null;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprEqualsAnyOrAll(this); }
            var result = (bool?)EvaluateInternal(evaluateParams);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprEqualsAnyOrAll(result); }
            return result;
        }

        public object EvaluateInternal(EvaluateParams evaluateParams)
        {
            Object leftResult = _evaluators[0].Evaluate(evaluateParams);
            if (_transformList[0] != null)
                leftResult = _transformList[0](leftResult);

            if (_hasCollectionOrArray)
            {
                if (IsAll)
                {
                    return CompareAllColl(leftResult, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
                }

                return CompareAnyColl(leftResult, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
            }

            // coerce early if testing without collections
            if ((_mustCoerce) && (leftResult != null))
            {
                leftResult = _coercer.Invoke(leftResult);
            }

            if (IsAll)
            {
                return CompareAll(leftResult, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
            }

            return CompareAny(leftResult, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }

        private Object CompareAll(Object leftResult, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var children = ChildNodes;
            if (IsNot)
            {
                int len = children.Count - 1;
                if ((len > 0) && (leftResult == null))
                {
                    return null;
                }
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                for (int i = 1; i <= len; i++)
                {
                    var rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult != null)
                    {
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (leftResult.Equals(rightResult))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            object right = _coercer.Invoke(rightResult);
                            if (leftResult.Equals(right))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        hasNullRow = true;
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return true;
            }
            else
            {
                int len = children.Count - 1;
                if ((len > 0) && (leftResult == null))
                {
                    return null;
                }
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                for (int i = 1; i <= len; i++)
                {
                    var rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult != null)
                    {
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (!leftResult.Equals(rightResult))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            object right = _coercer.Invoke(rightResult);
                            if (!leftResult.Equals(right))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        hasNullRow = true;
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return true;
            }
        }

        private Object CompareAllColl(Object leftResult, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var children = ChildNodes;
            if (IsNot)
            {
                int len = children.Count - 1;
                bool hasNonNullRow = false;
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

                    if (rightResult is AnyMap)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (AnyMap)rightResult;
                        if (coll.ContainsKey(leftResult))
                        {
                            return false;
                        }
                        hasNonNullRow = true;
                    }
                    else if (rightResult is DataCollection)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (DataCollection)rightResult;
                        if (coll.Contains(leftResult))
                        {
                            return false;
                        }
                        hasNonNullRow = true;
                    }
                    else if (rightResult.GetType().IsArray)
                    {
                        var array = (Array)rightResult;
                        int arrayLength = array.Length;
                        for (int index = 0; index < arrayLength; index++)
                        {
                            var item = array.GetValue(index);
                            if (item == null)
                            {
                                hasNullRow = true;
                                continue;
                            }
                            if (leftResult == null)
                            {
                                return null;
                            }
                            hasNonNullRow = true;
                            if (!_mustCoerce)
                            {
                                if (leftResult.Equals(item))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (!item.IsNumber())
                                {
                                    continue;
                                }

                                object left = _coercer.Invoke(leftResult);
                                object right = _coercer.Invoke(item);
                                if (Equals(left, right))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        if (!_mustCoerce)
                        {
                            if (leftResult.Equals(rightResult))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            object left = _coercer.Invoke(leftResult);
                            object right = _coercer.Invoke(rightResult);
                            if (Equals(left, right))
                            {
                                return false;
                            }
                        }
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return true;
            }
            else
            {
                int len = children.Count - 1;
                bool hasNonNullRow = false;
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

                    if (rightResult is AnyMap)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (AnyMap)rightResult;
                        if (!coll.ContainsKey(leftResult))
                        {
                            return false;
                        }
                        hasNonNullRow = true;
                    }
                    else if (rightResult is DataCollection)
                    {
                        hasNonNullRow = true;
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (DataCollection)rightResult;
                        if (!coll.Contains(leftResult))
                        {
                            return false;
                        }
                    }
                    else if (rightResult.GetType().IsArray)
                    {
                        var array = (Array)rightResult;
                        var arrayLength = array.Length;
                        for (int index = 0; index < arrayLength; index++)
                        {
                            Object item = array.GetValue(index);
                            if (item == null)
                            {
                                hasNullRow = true;
                                continue;
                            }
                            if (leftResult == null)
                            {
                                return null;
                            }
                            hasNonNullRow = true;
                            if (!_mustCoerce)
                            {
                                if (!leftResult.Equals(item))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (!item.IsNumber())
                                {
                                    continue;
                                }

                                object left = _coercer.Invoke(leftResult);
                                object right = _coercer.Invoke(item);
                                if (!Equals(left, right))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        if (!_mustCoerce)
                        {
                            if (!leftResult.Equals(rightResult))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            object left = _coercer.Invoke(leftResult);
                            object right = _coercer.Invoke(rightResult);
                            if (!Equals(left, right))
                            {
                                return false;
                            }
                        }
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return true;
            }
        }

        private Object CompareAny(Object leftResult, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Return true on the first not-equal.
            var children = ChildNodes;
            if (IsNot)
            {
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                int len = children.Count - 1;
                for (int i = 1; i <= len; i++)
                {
                    Object rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (leftResult == null)
                    {
                        return null;
                    }
                    if (rightResult == null)
                    {
                        hasNullRow = true;
                        continue;
                    }

                    hasNonNullRow = true;
                    if (!_mustCoerce)
                    {
                        if (!leftResult.Equals(rightResult))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        object right = _coercer.Invoke(rightResult);
                        if (!leftResult.Equals(right))
                        {
                            return true;
                        }
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return false;
            }
            // Return true on the first equal.
            else
            {
                int len = children.Count - 1;
                if ((len > 0) && (leftResult == null))
                {
                    return null;
                }
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                for (int i = 1; i <= len; i++)
                {
                    Object rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult == null)
                    {
                        hasNullRow = true;
                        continue;
                    }

                    hasNonNullRow = true;
                    if (!_mustCoerce)
                    {
                        if (leftResult.Equals(rightResult))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        object right = _coercer.Invoke(rightResult);
                        if (leftResult.Equals(right))
                        {
                            return true;
                        }
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return false;
            }
        }

        private Object CompareAnyColl(Object leftResult, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var childNodes = ChildNodes;

            // Return true on the first not-equal.
            if (IsNot)
            {
                int len = childNodes.Count - 1;
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                for (int i = 1; i <= len; i++)
                {
                    Object rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult == null)
                    {
                        hasNullRow = true;
                        continue;
                    }

                    if (rightResult is AnyMap)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (AnyMap)rightResult;
                        if (!coll.ContainsKey(leftResult))
                        {
                            return true;
                        }
                        hasNonNullRow = true;
                    }
                    else if (rightResult is DataCollection)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (DataCollection)rightResult;
                        if (!coll.Contains(leftResult))
                        {
                            return true;
                        }
                        hasNonNullRow = true;
                    }

                    else if (rightResult.GetType().IsArray)
                    {
                        var array = (Array)rightResult;
                        var arrayLength = array.Length;
                        if ((arrayLength > 0) && (leftResult == null))
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
                            hasNonNullRow = true;
                            if (!_mustCoerce)
                            {
                                if (!leftResult.Equals(item))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if (!item.IsNumber())
                                {
                                    continue;
                                }

                                object left = _coercer.Invoke(leftResult);
                                object right = _coercer.Invoke(item);
                                if (!Equals(left, right))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (!leftResult.Equals(rightResult))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            object left = _coercer.Invoke(leftResult);
                            object right = _coercer.Invoke(rightResult);
                            if (!Equals(left, right))
                            {
                                return true;
                            }
                        }
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return false;
            }
            // Return true on the first equal.
            else
            {
                int len = childNodes.Count - 1;
                bool hasNonNullRow = false;
                bool hasNullRow = false;
                for (int i = 1; i <= len; i++)
                {
                    Object rightResult = _evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    if (_transformList[i] != null)
                        rightResult = _transformList[i](rightResult);

                    if (rightResult == null)
                    {
                        hasNonNullRow = true;
                        continue;
                    }

                    if (rightResult is AnyMap)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        var coll = (AnyMap)rightResult;
                        if (coll.ContainsKey(leftResult))
                        {
                            return true;
                        }
                        hasNonNullRow = true;
                    }
                    else if (rightResult is DataCollection)
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        hasNonNullRow = true;
                        var coll = (DataCollection)rightResult;
                        if (coll.Contains(leftResult))
                        {
                            return true;
                        }
                    }
                    else if (rightResult.GetType().IsArray)
                    {
                        var array = (Array)rightResult;
                        var arrayLength = array.Length;
                        if ((arrayLength > 0) && (leftResult == null))
                        {
                            return null;
                        }
                        for (int index = 0; index < arrayLength; index++)
                        {
                            Object item = array.GetValue(index);
                            if (item == null)
                            {
                                hasNullRow = true;
                                continue;
                            }
                            hasNonNullRow = true;
                            if (!_mustCoerce)
                            {
                                if (leftResult.Equals(item))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if (!item.IsNumber())
                                {
                                    continue;
                                }

                                object left = _coercer.Invoke(leftResult);
                                object right = _coercer.Invoke(item);
                                if (Equals(left, right))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (leftResult == null)
                        {
                            return null;
                        }
                        hasNonNullRow = true;
                        if (!_mustCoerce)
                        {
                            if (leftResult.Equals(rightResult))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            object left = _coercer.Invoke(leftResult);
                            object right = _coercer.Invoke(rightResult);
                            if (Equals(left, right))
                            {
                                return true;
                            }
                        }
                    }
                }

                if ((!hasNonNullRow) || (hasNullRow))
                {
                    return null;
                }
                return false;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var children = ChildNodes;

            children[0].ToEPL(writer, Precedence);
            if (IsAll)
            {
                if (IsNot)
                {
                    writer.Write("!=all");
                }
                else
                {
                    writer.Write("=all");
                }
            }
            else
            {
                if (IsNot)
                {
                    writer.Write("!=any");
                }
                else
                {
                    writer.Write("=any");
                }
            }
            writer.Write("(");

            String delimiter = "";
            for (int i = 0; i < children.Count - 1; i++)
            {
                writer.Write(delimiter);
                children[i + 1].ToEPL(writer, Precedence);
                delimiter = ",";
            }
            writer.Write(")");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.EQUALS; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprEqualsAllAnyNode;
            if (other == null)
            {
                return false;
            }

            return (other.IsNot == IsNot) && (other.IsAll == IsAll);
        }
    }
}
