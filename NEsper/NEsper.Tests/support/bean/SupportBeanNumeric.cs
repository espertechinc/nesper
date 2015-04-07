///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.support.bean
{
    [Serializable]
	public class SupportBeanNumeric
	{
        public SupportBeanNumeric(int? intOne, int? intTwo, decimal? decimalOne, double doubleOne, double doubleTwo)
        {
            IntOne = intOne;
            IntTwo = intTwo;
            DecimalOne = decimalOne;
            DecimalTwo = null;
            DoubleOne = doubleOne;
            DoubleTwo = doubleTwo;
        }

        public SupportBeanNumeric(int? intOne, int? intTwo, decimal? decimalOne, decimal? decimalTwo, double doubleOne, double doubleTwo)
        {
            IntOne = intOne;
            IntTwo = intTwo;
            DecimalOne = decimalOne;
            DecimalTwo = decimalTwo;
            DoubleOne = doubleOne;
            DoubleTwo = doubleTwo;
        }

        public SupportBeanNumeric(int? intOne, int? intTwo)
        {
            IntOne = intOne;
            IntTwo = intTwo;
        }

        public SupportBeanNumeric(int? intOne, decimal? decimalOne)
        {
            IntOne = intOne;
            DecimalOne = decimalOne;
        }

        public SupportBeanNumeric(decimal? decimalOne)
        {
            DecimalOne = decimalOne;
        }

        public SupportBeanNumeric(decimal? decimalOne, decimal? decimalTwo)
        {
            DecimalOne = decimalOne;
            DecimalTwo = decimalTwo;
        }

        public SupportBeanNumeric(bool floatDummy, float floatOne, float floatTwo)
        {
            FloatOne = floatOne;
            FloatTwo = floatTwo;
        }

        public int? IntOne { set; get; }
        public int? IntTwo { set; get; }

        public decimal? DecimalOne { set; get; }
        public decimal? DecimalTwo { set; get; }

        public double DoubleOne { set; get; }
        public double DoubleTwo { set; get; }

        public float FloatOne { get; set; }
        public float FloatTwo { get; set; }
	}
} // End of namespace
