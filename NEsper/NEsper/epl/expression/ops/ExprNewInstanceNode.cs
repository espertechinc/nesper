///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents the "new Class(...)" operator in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprNewInstanceNode
        : ExprNodeBase
        , ExprEvaluator
    {
        private readonly string _classIdent;
        [NonSerialized] private Type _targetClass;
        [NonSerialized] private InstanceManufacturer _manufacturer;

        public ExprNewInstanceNode(string classIdent)
        {
            _classIdent = classIdent;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            try
            {
                _targetClass = validationContext.EngineImportService.ResolveType(_classIdent, false);
            }
            catch (EngineImportException)
            {
                throw new ExprValidationException("Failed to resolve new-operator class name '" + _classIdent + "'");
            }
            _manufacturer = InstanceManufacturerFactory.GetManufacturer(
                _targetClass, validationContext.EngineImportService, this.ChildNodes);
            return null;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _manufacturer.Make(evaluateParams);
        }

        public Type ReturnType
        {
            get { return _targetClass; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public string ClassIdent
        {
            get { return _classIdent; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprNewInstanceNode;
            return other != null && other._classIdent.Equals(this._classIdent);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("new ");
            writer.Write(_classIdent);
            ExprNodeUtility.ToExpressionStringParams(writer, this.ChildNodes);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }
    }
} // end of namespace
