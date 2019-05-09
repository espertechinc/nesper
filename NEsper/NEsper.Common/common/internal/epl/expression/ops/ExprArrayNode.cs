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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents an array in a filter expressiun tree.
    /// </summary>
    [Serializable]
    public class ExprArrayNode : ExprNodeBase
    {
        [NonSerialized] private ExprArrayNodeForge forge;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public bool IsConstantResult {
            get {
                CheckValidated(forge);
                return forge.ConstantResult != null;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public Type ComponentTypeCollection {
            get {
                CheckValidated(forge);
                return forge.ArrayReturnType;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var length = ChildNodes.Length;

            // Can be an empty array with no content
            if (ChildNodes.Length == 0) {
                forge = new ExprArrayNodeForge(this, typeof(object), CollectionUtil.OBJECTARRAY_EMPTY);
                return null;
            }

            IList<Type> comparedTypes = new List<Type>();
            for (var i = 0; i < length; i++) {
                comparedTypes.Add(ChildNodes[i].Forge.EvaluationType);
            }

            // Determine common denominator type
            Type arrayReturnType = null;
            var mustCoerce = false;
            SimpleNumberCoercer coercer = null;
            try {
                arrayReturnType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());

                // Determine if we need to coerce numbers when one type doesn't match any other type
                if (arrayReturnType.IsNumeric()) {
                    mustCoerce = false;
                    foreach (var comparedType in comparedTypes) {
                        if (comparedType != arrayReturnType) {
                            mustCoerce = true;
                        }
                    }

                    if (mustCoerce) {
                        coercer = SimpleNumberCoercerFactory.GetCoercer(null, arrayReturnType);
                    }
                }
            }
            catch (CoercionException) {
                // expected, such as mixing String and int values, or Java classes (not boxed) and primitives
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
                constantResult = Array.CreateInstance(arrayReturnType, length);
                for (var i = 0; i < length; i++) {
                    if (mustCoerce) {
                        var boxed = results[i];
                        if (boxed != null) {
                            object coercedResult = coercer.CoerceBoxed(boxed);
                            constantResult.SetValue(coercedResult, i);
                        }
                    }
                    else {
                        constantResult.SetValue(results[i], i);
                    }
                }
            }

            forge = new ExprArrayNodeForge(this, arrayReturnType, mustCoerce, coercer, constantResult);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";
            writer.Write("{");
            foreach (var expr in ChildNodes) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
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