///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Qualifies a join by providing the outer join type (full/left/right) and joined-on properties.
    /// </summary>
    public class OuterJoinQualifier
    {
        private IList<PropertyValueExpressionPair> additionalProperties;
        private Expression left;
        private Expression right;
        private OuterJoinType type;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public OuterJoinQualifier()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">is a property providing joined-on values</param>
        /// <param name="type">is the type of outer join</param>
        /// <param name="right">is a property providing joined-on values</param>
        public OuterJoinQualifier(
            OuterJoinType type,
            PropertyValueExpression left,
            PropertyValueExpression right)
            :
            this(type, left, right, new List<PropertyValueExpressionPair>())
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="left">is a property providing joined-on values</param>
        /// <param name="type">is the type of outer join</param>
        /// <param name="right">is a property providing joined-on values</param>
        /// <param name="additionalProperties">for any pairs of additional on-clause properties</param>
        public OuterJoinQualifier(
            OuterJoinType type,
            PropertyValueExpression left,
            PropertyValueExpression right,
            List<PropertyValueExpressionPair> additionalProperties)
        {
            this.type = type;
            this.left = left;
            this.right = right;
            this.additionalProperties = additionalProperties;
        }

        /// <summary>
        ///     Returns the type of outer join.
        /// </summary>
        /// <returns>outer join type</returns>
        public OuterJoinType Type {
            get => type;
            set => type = value;
        }

        /// <summary>
        ///     Returns property value expression to join on.
        /// </summary>
        /// <returns>expression providing joined-on values</returns>
        public Expression Left {
            get => left;
            set => left = value;
        }

        /// <summary>
        ///     Returns property value expression to join on.
        /// </summary>
        /// <returns>expression providing joined-on values</returns>
        public Expression Right {
            get => right;
            set => right = value;
        }

        /// <summary>
        ///     Returns optional additional properties in the on-clause of the outer join.
        /// </summary>
        /// <returns>pairs of properties connected via logical-and in an on-clause</returns>
        public IList<PropertyValueExpressionPair> AdditionalProperties {
            get => additionalProperties;
            set => additionalProperties = value;
        }

        /// <summary>
        ///     Creates qualifier.
        /// </summary>
        /// <param name="propertyLeft">is a property name providing joined-on values</param>
        /// <param name="type">is the type of outer join</param>
        /// <param name="propertyRight">is a property name providing joined-on values</param>
        /// <returns>qualifier</returns>
        public static OuterJoinQualifier Create(
            string propertyLeft,
            OuterJoinType type,
            string propertyRight)
        {
            return new OuterJoinQualifier(
                type,
                new PropertyValueExpression(propertyLeft),
                new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        ///     Add additional properties to the on-clause, which are logical-and to existing properties
        /// </summary>
        /// <param name="propertyLeft">property providing joined-on value</param>
        /// <param name="propertyRight">property providing joined-on value</param>
        /// <returns>outer join qualifier</returns>
        public OuterJoinQualifier Add(
            string propertyLeft,
            string propertyRight)
        {
            additionalProperties.Add(
                new PropertyValueExpressionPair(
                    new PropertyValueExpression(propertyLeft),
                    new PropertyValueExpression(propertyRight)));
            return this;
        }
    }
} // end of namespace