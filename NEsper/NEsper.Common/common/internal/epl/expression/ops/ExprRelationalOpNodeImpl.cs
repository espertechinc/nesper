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
    [Serializable]
    public class ExprRelationalOpNodeImpl : ExprNodeBase,
        ExprRelationalOpNode
    {
        private readonly RelationalOpEnum relationalOpEnum;

        [NonSerialized] private ExprRelationalOpNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
        public ExprRelationalOpNodeImpl(RelationalOpEnum relationalOpEnum)
        {
            this.relationalOpEnum = relationalOpEnum;
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

        public bool IsConstantResult {
            get => false;
        }

        /// <summary>
        /// Returns the type of relational op used.
        /// </summary>
        /// <returns>enum with relational op type</returns>
        public RelationalOpEnum RelationalOpEnum {
            get => relationalOpEnum;
        }

        public override void AddChildNode(ExprNode childNode)
        {
            base.AddChildNode(childNode);
        }

        public override void AddChildNodes(ICollection<ExprNode> childNodeColl)
        {
            base.AddChildNodes(childNodeColl);
        }
        
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (this.ChildNodes.Length != 2) {
                throw new IllegalStateException("Relational op node does not have exactly 2 parameters");
            }

            // Must be either numeric or string
            var typeOne = ChildNodes[0].Forge.EvaluationType.GetBoxedType();
            var typeTwo = ChildNodes[1].Forge.EvaluationType.GetBoxedType();

            if ((typeOne != typeof(string)) || (typeTwo != typeof(string))) {
                if (!typeOne.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" +
                        (typeOne == null ? "null" : typeOne.CleanName()) +
                        "' to numeric is not allowed");
                }

                if (!typeTwo.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" +
                        (typeTwo == null ? "null" : typeTwo.CleanName()) +
                        "' to numeric is not allowed");
                }
            }

            var coercionType = typeOne.GetCompareToCoercionType(typeTwo);
            var computer = relationalOpEnum.GetComputer(coercionType, typeOne, typeTwo);
            forge = new ExprRelationalOpNodeForge(this, computer, coercionType);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            this.ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(relationalOpEnum.GetExpressionText());
            this.ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprRelationalOpNodeImpl)) {
                return false;
            }

            ExprRelationalOpNodeImpl other = (ExprRelationalOpNodeImpl) node;

            if (other.relationalOpEnum != this.relationalOpEnum) {
                return false;
            }

            return true;
        }
    }
} // end of namespace