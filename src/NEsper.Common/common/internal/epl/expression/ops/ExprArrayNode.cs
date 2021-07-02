///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents an array in a filter expressiun tree.
    /// </summary>
    [Serializable]
    public class ExprArrayNode : ExprNodeBase
    {
        [NonSerialized] private ExprArrayNodeForge _forge;
        private Type _optionalRequiredType;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public bool IsConstantResult {
            get {
                CheckValidated(_forge);
                return _forge.ConstantResult != null;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public Type ComponentTypeCollection {
            get {
                CheckValidated(_forge);
                return _forge.ArrayReturnType;
            }
        }

        public Type OptionalRequiredType {
            get => _optionalRequiredType;
            set => _optionalRequiredType = value;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var length = ChildNodes.Length;

            // Can be an empty array with no content
            if (ChildNodes.Length == 0) {
                if (_optionalRequiredType == null) {
                    _forge = new ExprArrayNodeForge(this, typeof(object), CollectionUtil.OBJECTARRAY_EMPTY);
                }
                else {
                    _forge = new ExprArrayNodeForge(this, _optionalRequiredType, Arrays.CreateInstanceChecked(_optionalRequiredType, 0));
                }

                return null;
            }

            IList<Type> comparedTypes = new List<Type>();
            for (var i = 0; i < length; i++) {
                comparedTypes.Add(ChildNodes[i].Forge.EvaluationType);
            }

            // Determine common denominator type
            Type arrayReturnType = null;
            var mustCoerce = false;
            Coercer coercer = null;
            try {
                if (_optionalRequiredType == null) {
                    var coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
                    arrayReturnType = coercionType.IsNullTypeSafe() ? null : coercionType;

                    // Determine if we need to coerce numbers when one type doesn't match any other type
                    if (arrayReturnType.IsNumeric()) {
                        foreach (var comparedType in comparedTypes) {
                            if (comparedType != arrayReturnType) {
                                mustCoerce = true;
                                break;
                            }
                        }

                        if (mustCoerce) {
                            coercer = SimpleNumberCoercerFactory.GetCoercer(null, arrayReturnType);
                        }
                    }
                }
                else {
                    arrayReturnType = _optionalRequiredType;
                    var arrayBoxedType = _optionalRequiredType.GetBoxedType();
                    foreach (var comparedType in comparedTypes) {
                        if (!comparedType.GetBoxedType().IsAssignmentCompatible(arrayBoxedType)) {
                            throw new ExprValidationException(
                                "Array element type mismatch: Expecting type " + arrayReturnType.TypeSafeName() + " but received type " + comparedType.TypeSafeName());
                        }
                    }
                }
            }
            catch (CoercionException) {
                // expected, such as mixing String and int values, or types (not boxed) and primitives
                // use Object[] in such cases
            }

            if (arrayReturnType == null) {
                arrayReturnType = typeof(object);
            }

            // Determine if we are dealing with constants only
            var results = new object[length];
            var index = 0;
            foreach (var child in ChildNodes) {
                if (!child.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    results = null; // not using a constant result
                    break;
                }

                results[index] = ChildNodes[index].Forge.ExprEvaluator.Evaluate(null, false, null);
                index++;
            }

            // Copy constants into array and coerce, if required
            Array constantResult = null;
            if (results != null) {
                constantResult = Arrays.CreateInstanceChecked(arrayReturnType, length);
                for (var i = 0; i < length; i++) {
                    if (mustCoerce) {
                        var boxed = results[i];
                        if (boxed != null) {
                            var coercedResult = coercer.CoerceBoxed(boxed);
                            constantResult.SetValue(coercedResult, i);
                        }
                    }
                    else {
                        if (arrayReturnType.IsPrimitive && results[i] == null) {
                            throw new ExprValidationException(
                                "Array element type mismatch: Expecting type " + arrayReturnType.TypeSafeName() + " but received null");
                        }

                        try {
                            constantResult.SetValue(results[i], i);
                        }
                        catch (ArgumentException) {
                            throw new ExprValidationException(
                                "Array element type mismatch: Expecting type " + arrayReturnType.TypeSafeName() +
                                " but received type " + results[i].GetType().TypeSafeName());
                        }
                    }
                }
            }

            _forge = new ExprArrayNodeForge(this, arrayReturnType, mustCoerce, coercer, constantResult);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            var delimiter = "";
            writer.Write("{");
            foreach (var expr in ChildNodes) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
                delimiter = ",";
            }

            writer.Write('}');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprArrayNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace