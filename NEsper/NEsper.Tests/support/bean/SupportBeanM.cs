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
    /// <summary>
    /// SupportBean structure exposed through methods.
    /// </summary>
    [Serializable]
    public class SupportBeanM
    {
        private bool? _boolBoxed;
        private bool _boolPrimitive;
        private byte? _byteBoxed;
        private byte _bytePrimitive;
        private char? _charBoxed;
        private char _charPrimitive;
        private double? _doubleBoxed;
        private double _doublePrimitive;
        private SupportEnum? _enumValue;
        private float? _floatBoxed;
        private float _floatPrimitive;
        private int? _intBoxed;
        private int _intPrimitive;
        private long? _longBoxed;
        private long _longPrimitive;
        private short? _shortBoxed;
        private short _shortPrimitive;
        private string _string;

        public SupportBeanM()
        {
        }

        public SupportBeanM(String s, int intPrimitive)
        {
            SetString(s);
            SetIntPrimitive(intPrimitive);
        }

        public void SetString(string value)
        {
            _string = value;
        }

        public string GetString()
        {
            return _string;
        }

        public void SetEnumValue(SupportEnum? value)
        {
            _enumValue = value;
        }

        public SupportEnum? GetEnumValue()
        {
            return _enumValue;
        }

        public void SetBoolPrimitive(bool value)
        {
            _boolPrimitive = value;
        }

        public bool GetBoolPrimitive()
        {
            return _boolPrimitive;
        }

        public void SetIntPrimitive(int value)
        {
            _intPrimitive = value;
        }

        public int GetIntPrimitive()
        {
            return _intPrimitive;
        }

        public void SetLongPrimitive(long value)
        {
            _longPrimitive = value;
        }

        public long GetLongPrimitive()
        {
            return _longPrimitive;
        }

        public void SetCharPrimitive(char value)
        {
            _charPrimitive = value;
        }

        public char GetCharPrimitive()
        {
            return _charPrimitive;
        }

        public void SetShortPrimitive(short value)
        {
            _shortPrimitive = value;
        }

        public short GetShortPrimitive()
        {
            return _shortPrimitive;
        }

        public void SetBytePrimitive(byte value)
        {
            _bytePrimitive = value;
        }

        public byte GetBytePrimitive()
        {
            return _bytePrimitive;
        }

        public void SetFloatPrimitive(float value)
        {
            _floatPrimitive = value;
        }

        public float GetFloatPrimitive()
        {
            return _floatPrimitive;
        }

        public void SetDoublePrimitive(double value)
        {
            _doublePrimitive = value;
        }

        public double GetDoublePrimitive()
        {
            return _doublePrimitive;
        }

        public void SetBoolBoxed(bool? value)
        {
            _boolBoxed = value;
        }

        public bool? GetBoolBoxed()
        {
            return _boolBoxed;
        }

        public void SetIntBoxed(int? value)
        {
            _intBoxed = value;
        }

        public int? GetIntBoxed()
        {
            return _intBoxed;
        }

        public void SetLongBoxed(long? value)
        {
            _longBoxed = value;
        }

        public long? GetLongBoxed()
        {
            return _longBoxed;
        }

        public void SetCharBoxed(char? value)
        {
            _charBoxed = value;
        }

        public char? GetCharBoxed()
        {
            return _charBoxed;
        }

        public void SetShortBoxed(short? value)
        {
            _shortBoxed = value;
        }

        public short? GetShortBoxed()
        {
            return _shortBoxed;
        }

        public void SetByteBoxed(byte? value)
        {
            _byteBoxed = value;
        }

        public byte? GetByteBoxed()
        {
            return _byteBoxed;
        }

        public void SetFloatBoxed(float? value)
        {
            _floatBoxed = value;
        }

        public float? GetFloatBoxed()
        {
            return _floatBoxed;
        }

        public void SetDoubleBoxed(double? value)
        {
            _doubleBoxed = value;
        }

        public double? GetDoubleBoxed()
        {
            return _doubleBoxed;
        }

        public SupportBeanM GetThis()
        {
            return this;
        }

        public override String ToString()
        {
            return GetType().Name + "(" + GetString() + ", " + GetIntPrimitive() + ")";
        }
    }
} // End of namespace
