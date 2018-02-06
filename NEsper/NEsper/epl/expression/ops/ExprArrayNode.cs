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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents an array in a filter expressiun tree.
    /// </summary>
    [Serializable]
    public class ExprArrayNode 
        : ExprNodeBase
        , ExprEvaluator
        , ExprEvaluatorEnumeration
    {
        private Type _arrayReturnType;
        private bool _mustCoerce;
        private int _length;
    
        [NonSerialized] private Coercer _coercer;
        [NonSerialized] private Object _constantResult;
        [NonSerialized] private ExprEvaluator[] _evaluators;
        [NonSerialized] private volatile ICollection<object> _constantResultList;
    
        /// <summary>Ctor. </summary>
        public ExprArrayNode()
        {
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _length = ChildNodes.Count;
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
    
            // Can be an empty array with no content
            if (ChildNodes.Count == 0)
            {
                _arrayReturnType = typeof(Object);
                _constantResult = new Object[0];
                return null;
            }
    
            var comparedTypes = new List<Type>();
            for (int i = 0; i < _length; i++)
            {
                comparedTypes.Add(_evaluators[i].ReturnType);
            }
    
            // Determine common denominator type
            try {
                _arrayReturnType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
    
                // Determine if we need to coerce numbers when one type doesn't match any other type
                if (_arrayReturnType.IsNumeric())
                {
                    _mustCoerce = false;
                    foreach (var comparedType in comparedTypes)
                    {
                        if (comparedType != _arrayReturnType)
                        {
                            _mustCoerce = true;
                        }
                    }
                    if (_mustCoerce)
                    {
                        _coercer = CoercerFactory.GetCoercer(null, _arrayReturnType);
                    }
                }
            }
            catch (CoercionException)
            {
                // expected, such as mixing String and int values, or classes (not boxed) and primitives
                // use Object[] in such cases
            }
            if (_arrayReturnType == null)
            {
                _arrayReturnType = typeof(Object);
            }
    
            // Determine if we are dealing with constants only
            var results = new Object[_length];
            int index = 0;
            foreach (ExprNode child in ChildNodes)
            {
                if (!child.IsConstantResult)
                {
                    results = null;  // not using a constant result
                    break;
                }
                results[index] = _evaluators[index].Evaluate(new EvaluateParams(null, false, validationContext.ExprEvaluatorContext));
                index++;
            }
    
            // Copy constants into array and coerce, if required
            if (results != null)
            {
                var asArray = Array.CreateInstance(_arrayReturnType, _length);
                _constantResult = asArray;
                
                for (int i = 0; i < _length; i++)
                {
                    if (_mustCoerce)
                    {
                        var boxed = results[i];
                        if (boxed != null)
                        {
                            Object coercedResult = _coercer.Invoke(boxed);
                            asArray.SetValue(coercedResult, i);
                        }
                    }
                    else
                    {
                        asArray.SetValue(results[i], i);
                    }
                }
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return _constantResult != null; }
        }

        public Type ReturnType
        {
            get { return Array.CreateInstance(_arrayReturnType, 0).GetType(); }
        }

        public Object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprArray(this); }
            if (_constantResult != null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprArray(_constantResult); }
                return _constantResult;
            }
    
            Array array = Array.CreateInstance(_arrayReturnType, _length);
    
            if (_length == 0)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprArray(array); }
                return array;
            }
    
            int index = 0;
            foreach (ExprEvaluator child in _evaluators)
            {
                var result = child.Evaluate(evaluateParams);
                if (result != null)
                {
                    if (_mustCoerce)
                    {
                        Object coercedResult = _coercer.Invoke(result);
                        array.SetValue(coercedResult, index);
                    }
                    else
                    {
                        array.SetValue(result, index);
                    }
                }
                index++;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprArray(array); }
            return array;
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";
            writer.Write("{");
            foreach (ExprNode expr in ChildNodes)
            {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
            writer.Write('}');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public Type ComponentTypeCollection
        {
            get { return _arrayReturnType; }
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            if (_constantResult != null)
            {
                if (_constantResultList != null) {
                    return _constantResultList;
                }
                var list = new List<object>();
                var array = (Array)_constantResult;
                for (int i = 0; i < _length; i++)
                {
                    list.Add(array.GetValue(i));
                }
                _constantResultList = list;
                return list;
            }
    
            if (_length == 0)
            {
                return new List<object>();
            }

            var resultList = new List<object>();
    
            int index = 0;
            foreach (ExprEvaluator child in _evaluators)
            {
                var result = child.Evaluate(evaluateParams);
                if (result != null)
                {
                    if (_mustCoerce)
                    {
                        Object coercedResult = _coercer.Invoke(result);
                        resultList.Add(coercedResult);
                    }
                    else
                    {
                        resultList.Add(result);
                    }
                }
                index++;
            }
    
            return resultList;
        }
    
        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return null;
        }
    
        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }
    
        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return null;
        }
    
        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprArrayNode;
        }
    }
}
