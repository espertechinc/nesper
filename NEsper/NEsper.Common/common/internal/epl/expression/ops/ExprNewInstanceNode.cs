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
using com.espertech.esper.common.@internal.util;
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
        private readonly string _classIdent;
        private readonly int _numArrayDimensions;
        private bool _arrayInitializedByExpr;

        [NonSerialized] private ExprForge _forge;


        public ExprNewInstanceNode(string classIdent, int numArrayDimensions)
        {
            this._classIdent = classIdent;
            this._numArrayDimensions = numArrayDimensions;
        }

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

        public int NumArrayDimensions => _numArrayDimensions;

        public bool IsArrayInitializedByExpr => _arrayInitializedByExpr;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Resolve target class
            Type targetClass = null;
            if (_numArrayDimensions != 0) {
                targetClass = TypeHelper.GetPrimitiveTypeForName(_classIdent);
            }

            if (targetClass == null) {
                try {
                    targetClass = validationContext.ImportService
                        .ResolveClass(_classIdent, false, validationContext.ClassProvidedExtension);
                }
                catch (ImportException e) {
                    throw new ExprValidationException("Failed to resolve new-operator class name '" + _classIdent + "'");
                }
            }

            // handle non-array
            if (_numArrayDimensions == 0) {
                InstanceManufacturerFactory manufacturerFactory = InstanceManufacturerFactoryFactory.GetManufacturer(
                    targetClass,
                    validationContext.ImportService,
                    this.ChildNodes);
                _forge = new ExprNewInstanceNodeNonArrayForge(this, targetClass, manufacturerFactory);
                return null;
            }

            // determine array initialized or not
            var targetClassArray = TypeHelper.GetArrayType(targetClass, _numArrayDimensions);
            if (ChildNodes.Length == 1 && ChildNodes[0] is ExprArrayNode) {
                _arrayInitializedByExpr = true;
            } else {
                foreach (ExprNode child  in ChildNodes) {
                    var evalType = child.Forge.EvaluationType;
                    if (!TypeHelper.IsInt32(evalType)) {
                        String message = "New-keyword with an array-type result requires an Integer-typed dimension but received type '" +
                                         evalType.CleanName() +
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

            ExprArrayNode arrayNode = (ExprArrayNode) ChildNodes[0];

            // handle 2-dimensional array validation
            if (_numArrayDimensions == 2) {
                foreach (ExprNode inner in arrayNode.ChildNodes) {
                    if (!(inner is ExprArrayNode)) {
                        throw new ExprValidationException(
                            "Two-dimensional array element does not allow element expression '" +
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(inner) +
                            "'");
                    }

                    ExprArrayNode innerArray = (ExprArrayNode) inner;
                    innerArray.OptionalRequiredType = targetClass;
                    innerArray.Validate(validationContext);
                }

                arrayNode.OptionalRequiredType = targetClassArray.GetElementType();
            }
            else {
                arrayNode.OptionalRequiredType = targetClass;
            }

            arrayNode.Validate(validationContext);

            _forge = new ExprNewInstanceNodeArrayForge(this, targetClass, targetClassArray);
            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

        public string ClassIdent {
            get => _classIdent;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprNewInstanceNode)) {
                return false;
            }

            ExprNewInstanceNode other = (ExprNewInstanceNode) node;
            return other._classIdent.Equals(_classIdent) && (other._numArrayDimensions == this._numArrayDimensions);
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("new ");
            writer.Write(_classIdent);
            if (_numArrayDimensions == 0) {
                ExprNodeUtilityPrint.ToExpressionStringParams(writer, ChildNodes);
            }
            else {
                if (_arrayInitializedByExpr) {
                    writer.Write("[] ");
                    this.ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.UNARY, flags);
                } else {
                    foreach (ExprNode child in this.ChildNodes) {
                        writer.Write("[");
                        child.ToEPL(writer, ExprPrecedenceEnum.UNARY, flags);
                        writer.Write("]");
                    }
                }
            }
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }
    }
} // end of namespace