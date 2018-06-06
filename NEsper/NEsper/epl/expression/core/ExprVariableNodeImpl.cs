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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents a variable in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprVariableNodeImpl
        : ExprNodeBase
        , ExprEvaluator
        , ExprVariableNode
    {
        private readonly String _variableName;
        private readonly String _optSubPropName;
        private readonly bool _isConstant;
        private readonly Object _valueIfConstant;

        private Type _variableType;
        private bool _isPrimitive;
        [NonSerialized]
        private EventPropertyGetter _eventTypeGetter;
        [NonSerialized]
        private IDictionary<int, VariableReader> _readersPerCp;
        [NonSerialized]
        private VariableReader _readerNonCP;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variableMetaData">The variable meta data.</param>
        /// <param name="optSubPropName">Name of the opt sub property.</param>
        /// <exception cref="System.ArgumentException">Variables metadata is null</exception>
        public ExprVariableNodeImpl(VariableMetaData variableMetaData, String optSubPropName)
        {
            if (variableMetaData == null)
            {
                throw new ArgumentException("Variables metadata is null");
            }
            _variableName = variableMetaData.VariableName;
            _optSubPropName = optSubPropName;
            _isConstant = variableMetaData.IsConstant;
            _valueIfConstant = _isConstant ? variableMetaData.VariableStateFactory.InitialState : null;
        }

        public bool IsConstantValue
        {
            get { return _isConstant; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns the name of the variable. </summary>
        /// <value>variable name</value>
        public string VariableName
        {
            get { return _variableName; }
        }

        public object GetConstantValue(ExprEvaluatorContext context)
        {
            return _isConstant ? _valueIfConstant : null;
        }

        public override bool IsConstantResult
        {
            get { return _isConstant; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // determine if any types are property agnostic; If yes, resolve to variable
            var hasPropertyAgnosticType = false;
            var types = validationContext.StreamTypeService.EventTypes;
            for (var i = 0; i < validationContext.StreamTypeService.EventTypes.Length; i++)
            {
                if (types[i] is EventTypeSPI)
                {
                    hasPropertyAgnosticType |= ((EventTypeSPI)types[i]).Metadata.IsPropertyAgnostic;
                }
            }

            if (!hasPropertyAgnosticType)
            {
                // the variable name should not overlap with a property name
                try
                {
                    validationContext.StreamTypeService.ResolveByPropertyName(_variableName, false);
                    throw new ExprValidationException("The variable by name '" + _variableName + "' is ambigous to a property of the same name");
                }
                catch (DuplicatePropertyException)
                {
                    throw new ExprValidationException("The variable by name '" + _variableName + "' is ambigous to a property of the same name");
                }
                catch (PropertyNotFoundException)
                {
                    // expected
                }
            }

            VariableMetaData variableMetadata = validationContext.VariableService.GetVariableMetaData(_variableName);
            if (variableMetadata == null)
            {
                throw new ExprValidationException("Failed to find variable by name '" + _variableName + "'");
            }
            _isPrimitive = variableMetadata.EventType == null;
            _variableType = variableMetadata.VariableType;

            if (_optSubPropName != null)
            {
                if (variableMetadata.EventType == null)
                {
                    throw new ExprValidationException("Property '" + _optSubPropName + "' is not valid for variable '" + _variableName + "'");
                }
                _eventTypeGetter = variableMetadata.EventType.GetGetter(_optSubPropName);
                if (_eventTypeGetter == null)
                {
                    throw new ExprValidationException("Property '" + _optSubPropName + "' is not valid for variable '" + _variableName + "'");
                }
                _variableType = variableMetadata.EventType.GetPropertyType(_optSubPropName);
            }

            _readersPerCp = validationContext.VariableService.GetReadersPerCP(_variableName);
            if (variableMetadata.ContextPartitionName == null)
            {
                _readerNonCP = _readersPerCp.Get(EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            }

            return null;
        }

        public Type ConstantType
        {
            get { return _variableType; }
        }

        public Type ReturnType
        {
            get { return _variableType; }
        }

        public override String ToString()
        {
            return "variableName=" + _variableName;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            VariableReader reader;
            if (_readerNonCP != null)
            {
                reader = _readerNonCP;
            }
            else
            {
                reader = _readersPerCp.Get(evaluateParams.ExprEvaluatorContext.AgentInstanceId);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprVariable(this); }
            var value = reader.Value;
            if (_isPrimitive || value == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprVariable(value); }
                return value;
            }
            var theEvent = (EventBean)value;
            if (_optSubPropName == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprVariable(theEvent.Underlying); }
                return theEvent.Underlying;
            }
            var result = _eventTypeGetter.Get(theEvent);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprVariable(result); }
            return result;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_variableName);
            if (_optSubPropName != null)
            {
                writer.Write(".");
                writer.Write(_optSubPropName);
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprVariableNodeImpl))
            {
                return false;
            }

            var that = (ExprVariableNodeImpl)node;

            if (_optSubPropName != null ? !_optSubPropName.Equals(that._optSubPropName) : that._optSubPropName != null)
            {
                return false;
            }
            return that._variableName.Equals(_variableName);
        }

        public string VariableNameWithSubProp
        {
            get
            {
                if (_optSubPropName == null)
                {
                    return _variableName;
                }
                return _variableName + "." + _optSubPropName;
            }
        }
    }
}
