///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the "new Class(...)" operator in an expression tree.
    /// </summary>
    public class ExprNewInstanceNode : ExprNodeBase
    {
        private readonly ClassDescriptor _classIdentNoDimensions;
        private readonly int _numArrayDimensions;
        private bool _arrayInitializedByExpr;
        [NonSerialized] private ExprForge _forge;

        public ExprNewInstanceNode(
            ClassDescriptor classIdentNoDimensions,
            int numArrayDimensions)
        {
            this._classIdentNoDimensions = classIdentNoDimensions;
            this._numArrayDimensions = numArrayDimensions;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Resolve target class
            Type targetClass = null;
            if (_numArrayDimensions > 0 && _classIdentNoDimensions.TypeParameters.IsEmpty()) {
                // the "double[]" does become "double[]" and not "Double[]"
                var primitive = TypeHelper.GetPrimitiveTypeForName(_classIdentNoDimensions.ClassIdentifier);
                if (primitive != null) {
                    targetClass = primitive;
                }
            }

            if (targetClass == null) {
                targetClass = ImportTypeUtil.ResolveClassIdentifierToType(
                    _classIdentNoDimensions,
                    false,
                    validationContext.ImportService,
                    validationContext.ClassProvidedExtension);
            }

            if (targetClass == null) {
                throw new ExprValidationException(
                    "Failed to resolve type parameter '" + _classIdentNoDimensions.ToEPL() + "'");
            }

            // handle non-array
            if (_numArrayDimensions == 0) {
                var manufacturerFactory = InstanceManufacturerFactoryFactory.GetManufacturer(
                    targetClass,
                    validationContext.ImportService,
                    ChildNodes);
                _forge = new ExprNewInstanceNodeNonArrayForge(this, targetClass, manufacturerFactory);
                return null;
            }

            // determine array initialized or not
            var targetClassArray = TypeHelper.GetArrayType(targetClass, _numArrayDimensions);
            if (ChildNodes.Length == 1 && ChildNodes[0] is ExprArrayNode) {
                _arrayInitializedByExpr = true;
            }
            else {
                foreach (var child in ChildNodes) {
                    var evalType = child.Forge.EvaluationType;
                    if (!evalType.IsTypeInteger()) {
                        var message =
                            "New-keyword with an array-type result requires an Integer-typed dimension but received type '" +
                            (evalType == null ? "null" : evalType.CleanName()) +
                            "'";
                        throw new ExprValidationException(message);
                    }
                }
            }

            // handle array initialized by dimension only
            if (!_arrayInitializedByExpr) {
                _forge = new ExprNewInstanceNodeArrayForge(this, targetClass, targetClassArray);
                return null;
            }

            // handle array initialized by array expression
            if (_numArrayDimensions < 1 || _numArrayDimensions > 2) {
                throw new IllegalStateException("Num-array-dimensions unexpected at " + _numArrayDimensions);
            }

            var arrayNode = (ExprArrayNode)ChildNodes[0];
            // handle 2-dimensional array validation
            if (_numArrayDimensions == 2) {
                foreach (var inner in arrayNode.ChildNodes) {
                    if (!(inner is ExprArrayNode innerArray)) {
                        throw new ExprValidationException(
                            "Two-dimensional array element does not allow element expression '" +
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(inner) +
                            "'");
                    }

                    innerArray.OptionalRequiredType = targetClass;
                    innerArray.Validate(validationContext);
                }

                var component = targetClassArray.GetElementType();
                arrayNode.OptionalRequiredType = component;
            }
            else {
                arrayNode.OptionalRequiredType = targetClass;
            }

            arrayNode.Validate(validationContext);
            _forge = new ExprNewInstanceNodeArrayForge(this, targetClass, targetClassArray);
            return null;
        }

        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprNewInstanceNode other)) {
                return false;
            }

            return other._classIdentNoDimensions.Equals(_classIdentNoDimensions) &&
                   other._numArrayDimensions == _numArrayDimensions;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("new ");
            writer.Write(_classIdentNoDimensions.ToEPL());
            if (_numArrayDimensions == 0) {
                ExprNodeUtilityPrint.ToExpressionStringParams(writer, ChildNodes);
            }
            else {
                if (_arrayInitializedByExpr) {
                    writer.Write("[] ");
                    ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.UNARY, flags);
                }
                else {
                    foreach (var child in ChildNodes) {
                        writer.Write("[");
                        child.ToEPL(writer, ExprPrecedenceEnum.UNARY, flags);
                        writer.Write("]");
                    }
                }
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsArrayInitializedByExpr => _arrayInitializedByExpr;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public ClassDescriptor ClassIdentNoDimensions => _classIdentNoDimensions;

        public int NumArrayDimensions => _numArrayDimensions;
    }
} // end of namespace