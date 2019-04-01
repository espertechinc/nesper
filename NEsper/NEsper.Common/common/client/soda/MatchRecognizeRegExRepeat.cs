///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Match-recognize pattern descriptor for repetition
    /// </summary>
	[Serializable]
    public class MatchRecognizeRegExRepeat
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRecognizeRegExRepeat"/> class.
        /// </summary>
	    public MatchRecognizeRegExRepeat()
        {
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRecognizeRegExRepeat"/> class.
        /// </summary>
        /// <param name="low">The low.</param>
        /// <param name="high">The high.</param>
        /// <param name="single">The single.</param>
	    public MatchRecognizeRegExRepeat(Expression low, Expression high, Expression single)
        {
	        this.Low = low;
	        this.High = high;
	        this.Single = single;
	    }

        /// <summary>
        /// Gets or sets the low endpoint or null.
        /// </summary>
        /// <value>
        /// The low.
        /// </value>
	    public Expression Low { get; set; }

        /// <summary>
        /// Gets or sets the high endpoint or null.
        /// </summary>
        /// <value>
        /// The high.
        /// </value>
	    public Expression High { get; set; }

        /// <summary>
        /// Gets or sets the single exact-match repetition, should be null if low or high is provided.
        /// </summary>
        /// <value>
        /// The single.
        /// </value>
	    public Expression Single { get; set; }

        /// <summary>
        /// RenderAny as epl.
        /// </summary>
        /// <param name="writer">The writer.</param>
	    public void WriteEPL(TextWriter writer)
        {
	        writer.Write("{");
	        if (System.Single != null) {
	            System.Single.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	        }
	        else {
	            if (Low != null) {
	                Low.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	            }
	            writer.Write(",");
	            if (High != null) {
	                High.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
	            }
	        }
	        writer.Write("}");
	    }
	}
} // end of namespace
