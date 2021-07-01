///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportBeanNumeric
    {
        public SupportBeanNumeric(
            int? intOne,
            int? intTwo,
            BigInteger? bigint,
            decimal? decimalOne,
            double doubleOne,
            double doubleTwo)
        {
            IntOne = intOne;
            IntTwo = intTwo;
            Bigint = bigint;
            DecimalOne = decimalOne;
            DoubleOne = doubleOne;
            DoubleTwo = doubleTwo;
        }

        public SupportBeanNumeric(
            int? intOne,
            int? intTwo)
        {
            IntOne = intOne;
            IntTwo = intTwo;
        }

        public SupportBeanNumeric(
            BigInteger? bigint,
            decimal? decimalOne)
        {
            Bigint = bigint;
            DecimalOne = decimalOne;
        }

        public SupportBeanNumeric(
            bool floatDummy,
            float floatOne,
            float floatTwo)
        {
            FloatOne = floatOne;
            FloatTwo = floatTwo;
        }

        public int? IntOne { get; set; }

        public int? IntTwo { get; set; }

        public BigInteger? Bigint { get; set; }

        public double DoubleOne { get; set; }

        public double DoubleTwo { get; set; }

        public decimal? DecimalOne { get; set; }

        public decimal? DecimalTwo { get; set; }

        public float FloatOne { get; set; }

        public float FloatTwo { get; set; }
    }
} // end of namespace