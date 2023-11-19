///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter expression tree.
    /// </summary>
    public class ExprRelationalOpAllAnyNode : ExprNodeBase
    {
        private readonly RelationalOpEnum relationalOpEnum;
        private readonly bool isAll;
        [JsonIgnore]
        [NonSerialized]
        private ExprRelationalOpAllAnyNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
        /// <param name = "isAll">true if all, false for any</param>
        public ExprRelationalOpAllAnyNode(
            RelationalOpEnum relationalOpEnum,
            bool isAll)
        {
            this.relationalOpEnum = relationalOpEnum;
            this.isAll = isAll;
        }

        public bool IsConstantResult => false;

        /// <summary>
        /// Returns true for ALL, false for ANY.
        /// </summary>
        /// <returns>indicator all or any</returns>
        public bool IsAll => isAll;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (ChildNodes.Length < 1) {
                throw new IllegalStateException("Group relational op node must have 1 or more parameters");
            }

            var typeOne = ChildNodes[0].Forge.EvaluationType.GetBoxedType();
            ExprNodeUtilityValidate.ValidateLHSTypeAnyAllSomeIn(typeOne);
            IList<Type> comparedTypes = new List<Type>();
            comparedTypes.Add(typeOne);
            var hasCollectionOrArray = false;
            for (var i = 0; i < ChildNodes.Length - 1; i++) {
                var propType = ChildNodes[i + 1].Forge.EvaluationType;
                if (propType == null) {
                    comparedTypes.Add(null);
                }
                else {
                    var propClass = propType;
                    if (propClass.IsArray) {
                        hasCollectionOrArray = true;
                        if (propClass.GetComponentType() != typeof(object)) {
                            comparedTypes.Add(propClass.GetComponentType());
                        }
                    }
                    else if (propType.IsGenericDictionary()) {
                        hasCollectionOrArray = true;
                    }
                    else if (propType.IsGenericCollection()) {
                        hasCollectionOrArray = true;
                    }
                    else {
                        comparedTypes.Add(propType);
                    }
                }
            }

            // Determine common denominator type
            Type coercionType;
            try {
                coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
            }
            catch (CoercionException ex) {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }

            // Must be either numeric or string
            if (coercionType == null) {
                throw new ExprValidationException(
                    "Implicit conversion from null-type to numeric or string is not allowed");
            }

            var coercionClass = coercionType;
            if (coercionClass != typeof(string)) {
                if (!coercionClass.IsTypeNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" + coercionClass.CleanName() + "' to numeric is not allowed");
                }
            }

            RelationalOpEnumComputer computer = relationalOpEnum.GetComputer(
                coercionClass,
                coercionClass,
                coercionClass);
            forge = new ExprRelationalOpAllAnyNodeForge(this, computer, hasCollectionOrArray);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            writer.Write(relationalOpEnum.GetExpressionText());
            if (isAll) {
                writer.Write("all");
            }
            else {
                writer.Write("any");
            }

            writer.Write("(");
            var delimiter = "";
            for (var i = 0; i < ChildNodes.Length - 1; i++) {
                writer.Write(delimiter);
                ChildNodes[i + 1].ToEPL(writer, Precedence, flags);
                delimiter = ",";
            }

            writer.Write(")");
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprRelationalOpAllAnyNode other)) {
                return false;
            }

            if (other.relationalOpEnum != relationalOpEnum || other.isAll != isAll) {
                return false;
            }

            return true;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge => forge;

        public RelationalOpEnum RelationalOpEnum => relationalOpEnum;
    }
} // end of namespace