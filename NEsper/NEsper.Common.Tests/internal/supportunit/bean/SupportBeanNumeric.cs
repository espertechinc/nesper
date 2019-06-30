///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    [Serializable]
    public class SupportBeanNumeric
    {
        private decimal _decOne;
        private decimal _decTwo;
        private BigInteger bigint;
        private double doubleOne;
        private double doubleTwo;
        private float floatOne;
        private float floatTwo;
        private int? intOne;
        private int? intTwo;

        public SupportBeanNumeric(
            int? intOne,
            int? intTwo,
            BigInteger bigint,
            decimal decOne,
            double doubleOne,
            double doubleTwo)
        {
            this.intOne = intOne;
            this.intTwo = intTwo;
            this.bigint = bigint;
            _decOne = decOne;
            this.doubleOne = doubleOne;
            this.doubleTwo = doubleTwo;
        }

        public SupportBeanNumeric(
            int? intOne,
            int? intTwo)
        {
            this.intOne = intOne;
            this.intTwo = intTwo;
        }

        public SupportBeanNumeric(
            BigInteger bigint,
            decimal decOne)
        {
            this.bigint = bigint;
            _decOne = decOne;
        }

        public SupportBeanNumeric(
            bool floatDummy,
            float floatOne,
            float floatTwo)
        {
            this.floatOne = floatOne;
            this.floatTwo = floatTwo;
        }

        public int? IntOne
        {
            get => intOne;
            set => intOne = value;
        }

        public int? IntTwo
        {
            get => intTwo;
            set => intTwo = value;
        }

        public BigInteger Bigint
        {
            get => bigint;
            set => bigint = value;
        }

        public decimal DecOne
        {
            get => _decOne;
            set => _decOne = value;
        }

        public double DoubleOne
        {
            get => doubleOne;
            set => doubleOne = value;
        }

        public double DoubleTwo
        {
            get => doubleTwo;
            set => doubleTwo = value;
        }

        public decimal DecTwo
        {
            get => _decTwo;
            set => _decTwo = value;
        }

        public float FloatOne
        {
            get => floatOne;
            set => floatOne = value;
        }

        public float FloatTwo
        {
            get => floatTwo;
            set => floatTwo = value;
        }
    }
} // end of namespace