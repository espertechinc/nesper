///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.manufacturer;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the "new Class(...)" operator in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprNewInstanceNode : ExprNodeBase
    {
        private readonly string classIdent;

        [NonSerialized] private ExprNewInstanceNodeForge forge;

        public ExprNewInstanceNode(string classIdent)
        {
            this.classIdent = classIdent;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            Type targetClass;
            try {
                targetClass = validationContext.ImportService.ResolveClass(classIdent, false);
            }
            catch (ImportException) {
                throw new ExprValidationException("Failed to resolve new-operator class name '" + classIdent + "'");
            }

            InstanceManufacturerFactory manufacturerFactory =
                InstanceManufacturerFactoryFactory.GetManufacturer(
                    targetClass,
                    validationContext.ImportService,
                    ChildNodes);
            forge = new ExprNewInstanceNodeForge(this, targetClass, manufacturerFactory);
            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

        public string ClassIdent {
            get => classIdent;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprNewInstanceNode)) {
                return false;
            }

            ExprNewInstanceNode other = (ExprNewInstanceNode) node;
            return other.classIdent.Equals(classIdent);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("new ");
            writer.Write(classIdent);
            ExprNodeUtilityPrint.ToExpressionStringParams(writer, ChildNodes);
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }
    }
} // end of namespace