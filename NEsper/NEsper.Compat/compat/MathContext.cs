///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.RegularExpressions;

namespace com.espertech.esper.compat
{
    [Serializable]
    public class MathContext
    {
        public static readonly MathContext DECIMAL32;

        static MathContext()
        {
            DECIMAL32 = new MathContext(MidpointRounding.AwayFromZero, 7);
        }

        /// <summary>
        /// Gets the rounding mode.
        /// </summary>
        /// <value>The rounding mode.</value>
        public MidpointRounding RoundingMode { get; set; }

        /// <summary>
        /// Gets the precision.
        /// </summary>
        /// <value>The precision.</value>
        public int Precision { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MathContext"/> class.
        /// </summary>
        public MathContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MathContext"/> class.
        /// </summary>
        /// <param name="roundingMode">The rounding mode.</param>
        /// <param name="precision">The precision.</param>
        public MathContext(MidpointRounding roundingMode, int precision)
        {
            RoundingMode = roundingMode;
            Precision = precision;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MathContext"/> class.
        /// </summary>
        /// <param name="val">The val.</param>
        public MathContext(string val)
        {
            var matchPrecision = Regex.Match(val, @"Precision=[ ]*(\d+)");
            if (matchPrecision == Match.Empty)
            {
                throw new ArgumentException("Precision missing");
            }

            var matchRounding = Regex.Match(val, @"RoundingMode=[ ]*([\.\w-]+)[,]*");
            if (matchRounding == Match.Empty)
            {
                throw new ArgumentException("RoundingMode missing");
            }

            Precision = int.Parse(matchPrecision.Groups[1].Value);
            RoundingMode = EnumHelper.Parse<MidpointRounding>(matchRounding.Groups[1].Value, true);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("RoundingMode= {0}, Precision= {1}", RoundingMode, Precision);
        }
    }
}
