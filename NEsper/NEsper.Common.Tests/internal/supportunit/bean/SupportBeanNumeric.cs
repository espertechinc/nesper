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
        private decimal _decimalOne;
        private decimal _decimalTwo;
        private BigInteger _bigint;
        private double _doubleOne;
        private double _doubleTwo;
        private float _floatOne;
        private float _floatTwo;
        private int? _intOne;
        private int? _intTwo;

        public SupportBeanNumeric(
            int? intOne,
            int? intTwo,
            BigInteger bigint,
            decimal decimalOne,
            double doubleOne,
            double doubleTwo)
        {
            _intOne = intOne;
            _intTwo = intTwo;
            _bigint = bigint;
            _decimalOne = decimalOne;
            _doubleOne = doubleOne;
            _doubleTwo = doubleTwo;
        }

        public SupportBeanNumeric(
            int? intOne,
            int? intTwo)
        {
            _intOne = intOne;
            _intTwo = intTwo;
        }

        public SupportBeanNumeric(
            BigInteger bigint,
            decimal decimalOne)
        {
            _bigint = bigint;
            _decimalOne = decimalOne;
        }

        public SupportBeanNumeric(
            bool floatDummy,
            float floatOne,
            float floatTwo)
        {
            _floatOne = floatOne;
            _floatTwo = floatTwo;
        }

        public int? IntOne {
            get => _intOne;
            set => _intOne = value;
        }

        public int? IntTwo {
            get => _intTwo;
            set => _intTwo = value;
        }

        public BigInteger Bigint {
            get => _bigint;
            set => _bigint = value;
        }

        public decimal DecimalOne {
            get => _decimalOne;
            set => _decimalOne = value;
        }

        public double DoubleOne {
            get => _doubleOne;
            set => _doubleOne = value;
        }

        public double DoubleTwo {
            get => _doubleTwo;
            set => _doubleTwo = value;
        }

        public decimal DecimalTwo {
            get => _decimalTwo;
            set => _decimalTwo = value;
        }

        public float FloatOne {
            get => _floatOne;
            set => _floatOne = value;
        }

        public float FloatTwo {
            get => _floatTwo;
            set => _floatTwo = value;
        }
    }
} // end of namespace