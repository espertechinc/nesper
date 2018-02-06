///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents an stream property identifier in a filter expressiun tree.
    /// </summary>
    [Serializable]
    public class ExprContextPropertyNode : ExprNodeBase, ExprEvaluator
    {
        private readonly string _propertyName;
        private Type _returnType;
        [NonSerialized] private EventPropertyGetter _getter;

        public ExprContextPropertyNode(String propertyName)
        {
            _propertyName = propertyName;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (validationContext.ContextDescriptor == null)
            {
                throw new ExprValidationException(
                    "Context property '" + _propertyName + "' cannot be used in the expression as provided");
            }
            EventType eventType = validationContext.ContextDescriptor.ContextPropertyRegistry.ContextEventType;
            if (eventType == null)
            {
                throw new ExprValidationException(
                    "Context property '" + _propertyName + "' cannot be used in the expression as provided");
            }
            _getter = eventType.GetGetter(_propertyName);
            if (_getter == null)
            {
                throw new ExprValidationException(
                    "Context property '" + _propertyName + "' is not a known property, known properties are " +
                    eventType.PropertyNames.Render());
            }
            _returnType = eventType.GetPropertyType(_propertyName);

            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var context = evaluateParams.ExprEvaluatorContext;

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QExprContextProp(this);

                Object result = null;
                if (context.ContextProperties != null)
                {
                    result = _getter.Get(context.ContextProperties);
                }
                InstrumentationHelper.Get().AExprContextProp(result);
                return result;
            }

            if (context.ContextProperties != null)
            {
                return _getter.Get(context.ContextProperties);
            }
            return null;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_propertyName);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public EventPropertyGetter Getter
        {
            get { return _getter; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (this == node) return true;
            if (node == null || GetType() != node.GetType()) return false;
    
            var that = (ExprContextPropertyNode) node;
            return _propertyName.Equals(that._propertyName);
        }
    }
}
