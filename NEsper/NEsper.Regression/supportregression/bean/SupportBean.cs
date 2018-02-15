///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBean
    {
        private decimal? _decimalBoxed;
        private bool? _boolBoxed;
        private bool _boolPrimitive;
        private byte? _byteBoxed;
        private byte _bytePrimitive;
        private char? _charBoxed;
        private char _charPrimitive;
        private double? _doubleBoxed;
        private double _doublePrimitive;
        private SupportEnum _enumValue;
        private float? _floatBoxed;
        private float _floatPrimitive;

        private int? _intBoxed;
        private int _intPrimitive;
        private long? _longBoxed;
        private long _longPrimitive;
        private short? _shortBoxed;
        private short _shortPrimitive;
        private String _theString;

        public SupportBean()
        {
        }

        public SupportBean(String theString, int intPrimitive)
        {
            _theString = theString;
            _intPrimitive = intPrimitive;
        }

        public string TheString
        {
            get => _theString;
            set => _theString = value;
        }

        public bool BoolPrimitive
        {
            get => _boolPrimitive;
            set => _boolPrimitive = value;
        }

        public int IntPrimitive
        {
            get => _intPrimitive;
            set => _intPrimitive = value;
        }

        public long LongPrimitive
        {
            get => _longPrimitive;
            set => _longPrimitive = value;
        }

        public char CharPrimitive
        {
            get => _charPrimitive;
            set => _charPrimitive = value;
        }

        public short ShortPrimitive
        {
            get => _shortPrimitive;
            set => _shortPrimitive = value;
        }

        public byte BytePrimitive
        {
            get => _bytePrimitive;
            set => _bytePrimitive = value;
        }

        public float FloatPrimitive
        {
            get => _floatPrimitive;
            set => _floatPrimitive = value;
        }

        public double DoublePrimitive
        {
            get => _doublePrimitive;
            set => _doublePrimitive = value;
        }

        public bool? BoolBoxed
        {
            get => _boolBoxed;
            set => _boolBoxed = value;
        }

        public int? IntBoxed
        {
            get => _intBoxed;
            set => _intBoxed = value;
        }

        public long? LongBoxed
        {
            get => _longBoxed;
            set => _longBoxed = value;
        }

        public char? CharBoxed
        {
            get => _charBoxed;
            set => _charBoxed = value;
        }

        public short? ShortBoxed
        {
            get => _shortBoxed;
            set => _shortBoxed = value;
        }

        public byte? ByteBoxed
        {
            get => _byteBoxed;
            set => _byteBoxed = value;
        }

        public float? FloatBoxed
        {
            get => _floatBoxed;
            set => _floatBoxed = value;
        }

        public double? DoubleBoxed
        {
            get => _doubleBoxed;
            set => _doubleBoxed = value;
        }

        public SupportEnum EnumValue
        {
            get => _enumValue;
            set => _enumValue = value;
        }

        public SupportBean This
        {
            get { return this; }
        }

        public decimal? DecimalBoxed
        {
            get => _decimalBoxed;
            set => _decimalBoxed = value;
        }

        public int? GetIntBoxed() { return _intBoxed; }
        public int GetIntPrimitive() { return _intPrimitive; }
        public long? GetLongBoxed() { return _longBoxed; }
        public long GetLongPrimitive() { return _longPrimitive; }
        public double? GetDoubleBoxed() { return _doubleBoxed; }
        public double GetDoublePrimitive() { return _doublePrimitive; }
        public float? GetFloatBoxed() { return _floatBoxed; }
        public float GetFloatPrimitive() { return _floatPrimitive; }
        public string GetTheString() { return _theString; }

#if false
        public void SetIntBoxed(int? value) { _intBoxed = value; }
        public void SetIntPrimitive(int value) { _intPrimitive = value; }
        public void SetLongBoxed(long? value) { _longBoxed = value; }
        public void SetLongPrimitive(long value) { _longPrimitive = value; }
        public void SetDoubleBoxed(double? value) { _doubleBoxed = value; }
        public void SetDoublePrimitive(double value) { _doublePrimitive = value; }
        public void SetFloatBoxed(float? value) { _floatBoxed = value; }
        public void SetFloatPrimitive(float value) { _floatPrimitive = value; }
        public void SetTheString(string value) { _theString = value; }
#endif

        public override String ToString()
        {
            return string.Format("{0}({1}, {2})",
                GetType().Name, _theString, _intPrimitive);
        }

        public static SupportBean[] GetBeansPerIndex(SupportBean[] beans, int[] indexes)
        {
            if (indexes == null)
            {
                return null;
            }

            return indexes
                .Select(index => beans[index])
                .ToArray();
        }

        public static object[] GetOAStringAndIntPerIndex(SupportBean[] beans, int[] indexes)
        {
            SupportBean[] arr = GetBeansPerIndex(beans, indexes);
            return arr == null ? null : ToOAStringAndInt(arr);
        }

        private static object[] ToOAStringAndInt(SupportBean[] arr)
        {
            return arr
                .Select(v => new object[] { v.TheString, v.IntPrimitive })
                .ToArray();
        }
    }
}
