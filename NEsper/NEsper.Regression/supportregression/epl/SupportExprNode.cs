///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.supportregression.epl
{
    [Serializable]
    public class SupportExprNode : ExprNodeBase, ExprEvaluator
    {
        private static int _validateCount;
    
        private readonly Type _type;
        private Object _value;
        private int _validateCountSnapshot;
    
        public static void SetValidateCount(int validateCount)
        {
            _validateCount = validateCount;
        }

        public SupportExprNode(Type type)
        {
            _type = type.GetBoxedType();
            _value = null;
        }
    
        public SupportExprNode(Object value)
        {
            _type = value.GetType();
            _value = value;
        }
    
        public SupportExprNode(Object value, Type type)
        {
            _value = value;
            _type = type;
        }

        public override ExprEvaluator ExprEvaluator => this;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Keep a count for if and when this was validated
            _validateCount++;
            _validateCountSnapshot = _validateCount;
            return null;
        }

        public override bool IsConstantResult => false;

        public Type ReturnType => _type;

        public int ValidateCountSnapshot => _validateCountSnapshot;

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _value;
        }

        public object Value
        {
            get => _value;
            set => _value = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (_value is String)
            {
                writer.Write("\"" + _value + "\"");
            }
            else
            {
                if (_value == null)
                {
                    writer.Write("null");
                }
                else {
                    writer.Write(_value.ToString());
                }
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is SupportExprNode other && Equals(_value, other._value);
        }
    }
}
