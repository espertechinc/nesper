///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.type;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Qualifies a join by providing the outer join type (full/left/right) and joined-on properties.
    /// </summary>
    [Serializable]
    public class OuterJoinQualifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OuterJoinQualifier"/> class.
        /// </summary>
        public OuterJoinQualifier()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">is the type of outer join</param>
        /// <param name="left">is a property providing joined-on values</param>
        /// <param name="right">is a property providing joined-on values</param>
        public OuterJoinQualifier(OuterJoinType type, PropertyValueExpression left, PropertyValueExpression right)
            : this(type, left, right, new List<PropertyValueExpressionPair>())
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">is the type of outer join</param>
        /// <param name="left">is a property providing joined-on values</param>
        /// <param name="right">is a property providing joined-on values</param>
        /// <param name="additionalProperties">for any pairs of additional on-clause properties</param>
        public OuterJoinQualifier(OuterJoinType type, Expression left, Expression right,
                                  IList<PropertyValueExpressionPair> additionalProperties)
        {
            JoinType = type;
            Left = left;
            Right = right;
            AdditionalProperties = additionalProperties;
        }

        /// <summary>Gets or sets the type of outer join.</summary>
        /// <returns>outer join type</returns>
        public OuterJoinType JoinType { get; set; }

        /// <summary>Gets or sets the property value expression to join on.</summary>
        /// <returns>expression providing joined-on values</returns>
        public Expression Left { get; set; }

        /// <summary>Gets or sets the property value expression to join on.</summary>
        /// <returns>expression providing joined-on values</returns>
        public Expression Right { get; set; }

        /// <summary>
        /// Gets the optional additional properties in the on-clause of the outer join.
        /// </summary>
        /// <value>pairs of properties connected via logical-and in an on-clause</value>
        public IList<PropertyValueExpressionPair> AdditionalProperties { get; set; }

        /// <summary>Creates qualifier.</summary>
        /// <param name="propertyLeft">is a property name providing joined-on values</param>
        /// <param name="type">is the type of outer join</param>
        /// <param name="propertyRight">is a property name providing joined-on values</param>
        /// <returns>qualifier</returns>
        public static OuterJoinQualifier Create(String propertyLeft, OuterJoinType type, String propertyRight)
        {
            return new OuterJoinQualifier(type, new PropertyValueExpression(propertyLeft),
                                          new PropertyValueExpression(propertyRight));
        }

        /// <summary>
        /// Add additional properties to the on-clause, which are logical-and to existing properties
        /// </summary>
        /// <param name="propertyLeft">property providing joined-on value</param>
        /// <param name="propertyRight">property providing joined-on value</param>
        /// <returns>outer join qualifier</returns>
        public OuterJoinQualifier Add(String propertyLeft, String propertyRight)
        {
            AdditionalProperties.Add(new PropertyValueExpressionPair(
                                         new PropertyValueExpression(propertyLeft),
                                         new PropertyValueExpression(propertyRight)));
            return this;
        }
    }
} // End of namespace
