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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    using DataCollection = ICollection<object>;
    using DataMap = IDictionary<string, object>;
    using AnyMap = IDictionary<object, object>;
    using RelationalComputer = Func<object, object, bool>;

    /// <summary>
    /// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter
    /// expression tree.
    /// </summary>
    [Serializable]
    public class ExprRelationalOpAllAnyNode 
        : ExprNodeBase 
        , ExprEvaluator
    {
        private readonly bool _isAll;
        private readonly RelationalOpEnum _relationalOp;

        private bool _hasCollectionOrArray;

        [NonSerialized] private Func<object, object>[] _transformList;
        [NonSerialized] private RelationalComputer _computer;
        [NonSerialized] private ExprEvaluator[] _evaluators;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
        /// <param name="isAll">true if all, false for any</param>
        public ExprRelationalOpAllAnyNode(RelationalOpEnum relationalOpEnum, bool isAll)
        {
            _relationalOp = relationalOpEnum;
            _isAll = isAll;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true for ALL, false for ANY.
        /// </summary>
        /// <returns>
        /// indicator all or any
        /// </returns>
        public bool IsAll
        {
            get { return _isAll; }
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        /// <summary>
        /// Gets the expression evaluator.
        /// </summary>
        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(_relationalOp.GetExpressionText());
            if (_isAll)
            {
                writer.Write("all");
            }
            else
            {
                writer.Write("any");
            }

            writer.Write("(");
            String delimiter = "";

            for (int i = 0; i < ChildNodes.Count - 1; i++)
            {
                writer.Write(delimiter);
                ChildNodes[i + 1].ToEPL(writer, Precedence);
                delimiter = ",";
            }
            writer.Write(")");
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        /// <summary>
        /// Returns the type of relational op used.
        /// </summary>
        /// <returns>
        /// enum with relational op type
        /// </returns>
        public RelationalOpEnum RelationalOp
        {
            get { return _relationalOp; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            var childNodes = ChildNodes;
            if (childNodes.Count < 1)
            {
                throw new IllegalStateException("Group relational op node must have 1 or more parameters");
            }

            _evaluators = ExprNodeUtility.GetEvaluators(childNodes);

            var typeOne = _evaluators[0].ReturnType.GetBoxedType();

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
                var propType = _evaluators[i].ReturnType;
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
                coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
            }
            catch (CoercionException ex)
            {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }

            // Must be either numeric or string
            if (coercionType != typeof(String))
            {
                if (!coercionType.IsNumeric())
                {
                    throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(coercionType)));
                }
            }

            _computer = _relationalOp.GetComputer(coercionType, coercionType, coercionType);

            return null;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprRelOpAnyOrAll(this, _relationalOp.GetExpressionText()); }
            var result = EvaluateInternal(evaluateParams);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprRelOpAnyOrAll(result); }
            return result;
        }

        private bool? EvaluateInternal(EvaluateParams evaluateParams)
        {
            if (_evaluators.Length == 1)
            {
                return false;
            }

            Object valueLeft = _evaluators[0].Evaluate(evaluateParams);
            int len = _evaluators.Length - 1;

            if (_hasCollectionOrArray)
            {
                bool hasNonNullRow = false;
                bool hasRows = false;
                for (int i = 1; i <= len; i++)
                {
                    Object valueRight = _evaluators[i].Evaluate(evaluateParams);
                    if (_transformList[i] != null)
                        valueRight = _transformList[i](valueRight);

                    if (valueRight == null)
                    {
                        continue;
                    }

                    if (valueRight is AnyMap)
                    {
                        var coll = (AnyMap)valueRight;
                        hasRows = true;
                        foreach (var item in coll.Keys)
                        {
                            if (!item.IsNumber())
                            {
                                if (_isAll && item == null)
                                {
                                    return null;
                                }
                                continue;
                            }
                            hasNonNullRow = true;
                            if (valueLeft != null)
                            {
                                if (_isAll)
                                {
                                    if (!_computer.Invoke(valueLeft, item))
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (_computer.Invoke(valueLeft, item))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (valueRight is DataCollection)
                    {
                        var coll = (DataCollection)valueRight;
                        hasRows = true;
                        foreach (Object item in coll)
                        {
                            if (!item.IsNumber())
                            {
                                if (_isAll && item == null)
                                {
                                    return null;
                                }
                                continue;
                            }
                            hasNonNullRow = true;
                            if (valueLeft != null)
                            {
                                if (_isAll)
                                {
                                    if (!_computer.Invoke(valueLeft, item))
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (_computer.Invoke(valueLeft, item))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (valueRight.GetType().IsArray)
                    {
                        hasRows = true;
                        var array = (Array)valueRight;
                        var arrayLength = array.Length;
                        for (int index = 0; index < arrayLength; index++)
                        {
                            Object item = array.GetValue(index);
                            if (item == null)
                            {
                                if (_isAll)
                                {
                                    return null;
                                }
                                continue;
                            }
                            hasNonNullRow = true;
                            if (valueLeft != null)
                            {
                                if (_isAll)
                                {
                                    if (!_computer.Invoke(valueLeft, item))
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (_computer.Invoke(valueLeft, item))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!valueRight.IsNumber())
                    {
                        if (_isAll)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        hasNonNullRow = true;
                        if (_isAll)
                        {
                            if (!_computer.Invoke(valueLeft, valueRight))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (_computer.Invoke(valueLeft, valueRight))
                            {
                                return true;
                            }
                        }
                    }
                }

                if (_isAll)
                {
                    if (!hasRows)
                    {
                        return true;
                    }
                    if ((!hasNonNullRow) || (valueLeft == null))
                    {
                        return null;
                    }
                    return true;
                }

                if (!hasRows)
                {
                    return false;
                }
                if ((!hasNonNullRow) || (valueLeft == null))
                {
                    return null;
                }
                return false;
            }
            else
            {
                bool hasNonNullRow = false;
                bool hasRows = false;
                for (int i = 1; i <= len; i++)
                {
                    Object valueRight = _evaluators[i].Evaluate(evaluateParams);
                    if (_transformList[i] != null)
                        valueRight = _transformList[i](valueRight);

                    hasRows = true;

                    if (valueRight != null)
                    {
                        hasNonNullRow = true;
                    }
                    else
                    {
                        if (_isAll)
                        {
                            return null;
                        }
                    }

                    if ((valueRight != null) && (valueLeft != null))
                    {
                        if (_isAll)
                        {
                            if (!_computer.Invoke(valueLeft, valueRight))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (_computer.Invoke(valueLeft, valueRight))
                            {
                                return true;
                            }
                        }
                    }
                }

                if (_isAll)
                {
                    if (!hasRows)
                    {
                        return true;
                    }
                    if ((!hasNonNullRow) || (valueLeft == null))
                    {
                        return null;
                    }
                    return true;
                }
                if (!hasRows)
                {
                    return false;
                }
                if ((!hasNonNullRow) || (valueLeft == null))
                {
                    return null;
                }
                return false;
            }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprRelationalOpAllAnyNode))
            {
                return false;
            }

            var other = (ExprRelationalOpAllAnyNode)node;

            if ((other._relationalOp != _relationalOp) ||
                (other._isAll != _isAll))
            {
                return false;
            }

            return true;
        }
    }
}
