///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the COALESCE(a,b,...) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprCoalesceNode : ExprNodeBase, ExprEvaluator
    {
        private Type _resultType;
        private bool[] _isNumericCoercion;
    
        [NonSerialized] private ExprEvaluator[] _evaluators;

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count < 2)
            {
                throw new ExprValidationException("Coalesce node must have at least 2 parameters");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
    
            // get child expression types
            var childTypes = new Type[ChildNodes.Count];
            for (var i = 0; i < _evaluators.Length; i++)
            {
                childTypes[i] = _evaluators[i].ReturnType;
            }
    
            // determine coercion type
            try {
                _resultType = TypeHelper.GetCommonCoercionType(childTypes);
            }
            catch (CoercionException ex)
            {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }
    
            // determine which child nodes need numeric coercion
            _isNumericCoercion = new bool[ChildNodes.Count];
            for (var i = 0; i < _evaluators.Length; i++)
            {
                if ((_evaluators[i].ReturnType.GetBoxedType() != _resultType) &&
                    (_evaluators[i].ReturnType != null) && (_resultType != null))
                {
                    if (!_resultType.IsNumeric())
                    {
                        throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to {1} is not allowed", Name.Clean(_resultType), Name.Clean(_evaluators[i].ReturnType)));
                    }
                    _isNumericCoercion[i] = true;
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

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprCoalesce(this);}
            Object value;
    
            // Look for the first non-null return value
            for (var i = 0; i < _evaluators.Length; i++)
            {
                value = _evaluators[i].Evaluate(evaluateParams);
    
                if (value != null)
                {
                    // Check if we need to coerce
                    if (_isNumericCoercion[i])
                    {
                        value = CoercerFactory.CoerceBoxed(value, _resultType);
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprCoalesce(value);}
                    return value;
                }
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprCoalesce(null);}
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ExprNodeUtility.ToExpressionStringWFunctionName("coalesce", ChildNodes, writer);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprCoalesceNode;
        }
    }
}
