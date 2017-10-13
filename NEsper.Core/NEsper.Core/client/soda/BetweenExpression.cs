///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Between checks that a given value is in a range between a low endpoint and a high endpoint.
    /// <para>
    /// Closed and open ranges (endpoint included or excluded) are supported by this class, as is not-between.
    /// </para>
    /// </summary>
    public class BetweenExpression : ExpressionBase
    {
        private bool _isLowEndpointIncluded;
        private bool _isHighEndpointIncluded;
        private bool _isNotBetween;
    
        /// <summary>Ctor.</summary>
        public BetweenExpression()
        {
        }
    
        /// <summary>
        /// Ctor, creates a between range check.
        /// </summary>
        /// <param name="datapoint">provides the datapoint</param>
        /// <param name="lower">provides lower boundary</param>
        /// <param name="higher">provides upper boundary</param>
        public BetweenExpression(Expression datapoint, Expression lower, Expression higher)
            : this(datapoint, lower, higher, true, true, false)
        {
        }
    
        /// <summary>
        /// Ctor - for use to create an expression tree, without child expression.
        /// <para>
        /// Use add methods to add child expressions to acts upon.
        /// </para>
        /// </summary>
        /// <param name="lowEndpointIncluded">true if the low endpoint is included, false if not</param>
        /// <param name="highEndpointIncluded">true if the high endpoint is included, false if not</param>
        /// <param name="notBetween">true for not-between, false for between</param>
        public BetweenExpression(bool lowEndpointIncluded, bool highEndpointIncluded, bool notBetween) {
            _isLowEndpointIncluded = lowEndpointIncluded;
            _isHighEndpointIncluded = highEndpointIncluded;
            _isNotBetween = notBetween;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="datapoint">provides the datapoint</param>
        /// <param name="lower">provides lower boundary</param>
        /// <param name="higher">provides upper boundary</param>
        /// <param name="lowEndpointIncluded">true if the low endpoint is included, false if not</param>
        /// <param name="highEndpointIncluded">true if the high endpoint is included, false if not</param>
        /// <param name="notBetween">true for not-between, false for between</param>
        public BetweenExpression(Expression datapoint, Expression lower, Expression higher, bool lowEndpointIncluded, bool highEndpointIncluded, bool notBetween) {
            Children.Add(datapoint);
            Children.Add(lower);
            Children.Add(higher);
    
            _isLowEndpointIncluded = lowEndpointIncluded;
            _isHighEndpointIncluded = highEndpointIncluded;
            _isNotBetween = notBetween;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            if (_isLowEndpointIncluded && _isHighEndpointIncluded) {
                Children[0].ToEPL(writer, Precedence);
                writer.Write(" between ");
                Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(" and ");
                Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            } else {
                Children[0].ToEPL(writer, Precedence);
                writer.Write(" in ");
                if (_isLowEndpointIncluded) {
                    writer.Write('[');
                } else {
                    writer.Write('(');
                }
                Children[1].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(':');
                Children[2].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                if (_isHighEndpointIncluded) {
                    writer.Write(']');
                } else {
                    writer.Write(')');
                }
            }
        }

        /// <summary>
        /// True if the low endpoint is included.
        /// </summary>
        /// <value>true for inclusive range.</value>
        public bool IsLowEndpointIncluded
        {
            get { return _isLowEndpointIncluded; }
            set { _isLowEndpointIncluded = value; }
        }

        /// <summary>
        /// True if the high endpoint is included.
        /// </summary>
        /// <value>true for inclusive range.</value>
        public bool IsHighEndpointIncluded
        {
            get { return _isHighEndpointIncluded; }
            set { _isHighEndpointIncluded = value; }
        }

        /// <summary>
        /// Returns true for not-between, or false for between range.
        /// </summary>
        /// <value>
        ///   false is the default range check, true checks if the value is outside of the range
        /// </value>
        public bool IsNotBetween
        {
            get { return _isNotBetween; }
            set { _isNotBetween = value; }
        }
    }
} // end of namespace
