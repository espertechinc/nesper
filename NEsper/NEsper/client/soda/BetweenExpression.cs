///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Between checks that a given value is in a range between a low endpoint and a high endpoint.
    /// <para/>
    /// Closed and open ranges (endpoint included or excluded) are supported by this class, as is not-between.
    /// </summary>
    [Serializable]
    public class BetweenExpression : ExpressionBase
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="BetweenExpression"/> class.
        /// </summary>
        public BetweenExpression()
        {
        }

        /// <summary>Ctor, creates a between range check.</summary>
	    /// <param name="datapoint">provides the datapoint</param>
	    /// <param name="lower">provides lower boundary</param>
	    /// <param name="higher">provides upper boundary</param>
	    public BetweenExpression(Expression datapoint, Expression lower, Expression higher)
	    	: this(datapoint, lower, higher, true, true, false)
	    {
	    }

	    /// <summary>
	    /// Ctor - for use to create an expression tree, without child expression.
	    /// <para/>
	    /// Use add methods to add child expressions to acts upon.
	    /// </summary>
	    /// <param name="lowEndpointIncluded">
	    /// true if the low endpoint is included, false if not
	    /// </param>
	    /// <param name="highEndpointIncluded">
	    /// true if the high endpoint is included, false if not
	    /// </param>
	    /// <param name="notBetween">true for not-between, false for between</param>
	    public BetweenExpression(bool lowEndpointIncluded, bool highEndpointIncluded, bool notBetween)
	    {
	        IsLowEndpointIncluded = lowEndpointIncluded;
	        IsHighEndpointIncluded = highEndpointIncluded;
	        IsNotBetween = notBetween;
	    }

	    /// <summary>Ctor.</summary>
	    /// <param name="datapoint">provides the datapoint</param>
	    /// <param name="lower">provides lower boundary</param>
	    /// <param name="higher">provides upper boundary</param>
	    /// <param name="lowEndpointIncluded">
	    /// true if the low endpoint is included, false if not
	    /// </param>
	    /// <param name="highEndpointIncluded">
	    /// true if the high endpoint is included, false if not
	    /// </param>
	    /// <param name="notBetween">true for not-between, false for between</param>
	    public BetweenExpression(Expression datapoint, Expression lower, Expression higher, bool lowEndpointIncluded, bool highEndpointIncluded, bool notBetween)
	    {
	        Children.Add(datapoint);
	        Children.Add(lower);
	        Children.Add(higher);

	        IsLowEndpointIncluded = lowEndpointIncluded;
	        IsHighEndpointIncluded = highEndpointIncluded;
	        IsNotBetween = notBetween;
	    }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if ((IsLowEndpointIncluded) && (IsHighEndpointIncluded))
            {
                Children[0].ToEPL(writer, Precedence);
                writer.Write(" between ");
                Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" and ");
                Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            else
            {
                Children[0].ToEPL(writer, Precedence);
                writer.Write(" in ");
                if (IsLowEndpointIncluded)
                {
                    writer.Write('[');
                }
                else
                {
                    writer.Write('(');
                }
                Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(':');
                Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                if (IsHighEndpointIncluded)
                {
                    writer.Write(']');
                }
                else
                {
                    writer.Write(')');
                }
            }
        }

        /// <summary>True if the low endpoint is included.</summary>
        /// <returns>true for inclusive range.</returns>
        public bool IsLowEndpointIncluded { get; set; }

        /// <summary>True if the high endpoint is included.</summary>
        /// <returns>true for inclusive range.</returns>
        public bool IsHighEndpointIncluded { get; set; }

        /// <summary>True for not-between, or false for between range.</summary>
        /// <returns>
        /// false is the default range check, true checks if the value is outside of the range
        /// </returns>
        public bool IsNotBetween { get; set; }
	}
} // End of namespace
