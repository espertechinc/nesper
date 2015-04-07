///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents a constant in an expressiun tree.
    /// </summary>
    [Serializable]
    public class ExprConstantNodeImpl
        : ExprNodeBase
        , ExprConstantNode
        , ExprEvaluator
    {
        private Object _constantValue;
        private readonly Type _clazz;

        /// <summary>Ctor. </summary>
        /// <param name="constantValue">is the constant's ConstantValue.</param>
        public ExprConstantNodeImpl(Object constantValue)
        {
            _constantValue = constantValue;
            if (constantValue == null)
            {
                _clazz = null;
            }
            else
            {
                _clazz = constantValue.GetType().GetBoxedType();
            }
        }

        /// <summary>Ctor. </summary>
        /// <param name="constantValue">is the constant's ConstantValue.</param>
        /// <param name="valueType">is the constant's ConstantValue type.</param>
        public ExprConstantNodeImpl(Object constantValue, Type valueType)
        {
            _constantValue = constantValue;
            if (constantValue == null)
            {
                _clazz = valueType;
            }
            else
            {
                _clazz = constantValue.GetType().GetBoxedType();
            }
        }
    
        /// <summary>Ctor - for use when the constant should return a given type and the actual ConstantValue is always null. </summary>
        /// <param name="clazz">the type of the constant null.</param>
        public ExprConstantNodeImpl(Type clazz)
        {
            _clazz = clazz;
            _constantValue = null;
        }

        public bool IsConstantValue
        {
            get { return true; }
        }
        
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public override bool IsConstantResult
        {
            get { return true; }
        }

        /// <summary>Returns the constant's ConstantValue. </summary>
        /// <ConstantValue>ConstantValue of constant</ConstantValue>
        public object ConstantValue
        {
            set { _constantValue = value; }
        }

        public object GetConstantValue(ExprEvaluatorContext context)
        {
            return _constantValue;
        }

        public Type ConstantType
        {
            get { return _clazz; }
        }

        public Type ReturnType
        {
            get { return _clazz; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QaExprConst(_constantValue); }
            return _constantValue;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(
                EPStatementObjectModelHelper.RenderValue(_constantValue));
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node)
        {
            var other = node as ExprConstantNodeImpl;

            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(_constantValue, other._constantValue);
        }
    }
}
