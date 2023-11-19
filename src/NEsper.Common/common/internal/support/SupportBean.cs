///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportBean
    {
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

        private decimal _decimalPrimitive;
        private decimal? _decimalBoxed;

        private BigInteger? _bigInteger;

        private int? _intBoxed;
        private int _intPrimitive;
        private long? _longBoxed;
        private long _longPrimitive;
        private short? _shortBoxed;
        private short _shortPrimitive;
        private string _theString;

        public SupportBean()
        {
        }

        public SupportBean(
            string theString,
            int intPrimitive)
        {
            _theString = theString;
            _intPrimitive = intPrimitive;
        }

        public string TheString {
            get => _theString;
            set => _theString = value;
        }

        public bool BoolPrimitive {
            get => _boolPrimitive;
            set => _boolPrimitive = value;
        }

        public int IntPrimitive {
            get => _intPrimitive;
            set => _intPrimitive = value;
        }

        public long LongPrimitive {
            get => _longPrimitive;
            set => _longPrimitive = value;
        }

        public char CharPrimitive {
            get => _charPrimitive;
            set => _charPrimitive = value;
        }

        public short ShortPrimitive {
            get => _shortPrimitive;
            set => _shortPrimitive = value;
        }

        public byte BytePrimitive {
            get => _bytePrimitive;
            set => _bytePrimitive = value;
        }

        public float FloatPrimitive {
            get => _floatPrimitive;
            set => _floatPrimitive = value;
        }

        public double DoublePrimitive {
            get => _doublePrimitive;
            set => _doublePrimitive = value;
        }

        public bool? BoolBoxed {
            get => _boolBoxed;
            set => _boolBoxed = value;
        }

        public int? IntBoxed {
            get => _intBoxed;
            set => _intBoxed = value;
        }

        public long? LongBoxed {
            get => _longBoxed;
            set => _longBoxed = value;
        }

        public char? CharBoxed {
            get => _charBoxed;
            set => _charBoxed = value;
        }

        public short? ShortBoxed {
            get => _shortBoxed;
            set => _shortBoxed = value;
        }

        public byte? ByteBoxed {
            get => _byteBoxed;
            set => _byteBoxed = value;
        }

        public float? FloatBoxed {
            get => _floatBoxed;
            set => _floatBoxed = value;
        }

        public double? DoubleBoxed {
            get => _doubleBoxed;
            set => _doubleBoxed = value;
        }

        public SupportEnum EnumValue {
            get => _enumValue;
            set => _enumValue = value;
        }

        [JsonIgnore]
        public SupportBean This => this;

        public decimal? DecimalBoxed {
            get => _decimalBoxed;
            set => _decimalBoxed = value;
        }

        public decimal DecimalPrimitive {
            get => _decimalPrimitive;
            set => _decimalPrimitive = value;
        }

        public BigInteger? BigInteger {
            get => _bigInteger;
            set => _bigInteger = value;
        }

        public int? GetIntBoxed()
        {
            return _intBoxed;
        }

        public int GetIntPrimitive()
        {
            return _intPrimitive;
        }

        public long? GetLongBoxed()
        {
            return _longBoxed;
        }

        public long GetLongPrimitive()
        {
            return _longPrimitive;
        }

        public double? GetDoubleBoxed()
        {
            return _doubleBoxed;
        }

        public double GetDoublePrimitive()
        {
            return _doublePrimitive;
        }

        public float? GetFloatBoxed()
        {
            return _floatBoxed;
        }

        public float GetFloatPrimitive()
        {
            return _floatPrimitive;
        }

        public string GetTheString()
        {
            return _theString;
        }

        public void SetIntBoxed(int? value)
        {
            _intBoxed = value;
        }

        public void SetIntPrimitive(int value)
        {
            _intPrimitive = value;
        }

        public void SetLongBoxed(long? value)
        {
            _longBoxed = value;
        }

        public void SetLongPrimitive(long value)
        {
            _longPrimitive = value;
        }

        public void SetDoubleBoxed(double? value)
        {
            _doubleBoxed = value;
        }

        public void SetDoublePrimitive(double value)
        {
            _doublePrimitive = value;
        }

        public void SetFloatBoxed(float? value)
        {
            _floatBoxed = value;
        }

        public void SetFloatPrimitive(float value)
        {
            _floatPrimitive = value;
        }

        public void SetTheString(string value)
        {
            _theString = value;
        }

        public override string ToString()
        {
            return $"{GetType().Name}({_theString.RenderAny()}, {_intPrimitive})";
        }

        protected bool Equals(SupportBean other)
        {
            return _boolBoxed == other._boolBoxed &&
                   _boolPrimitive == other._boolPrimitive &&
                   _byteBoxed == other._byteBoxed &&
                   _bytePrimitive == other._bytePrimitive &&
                   _charBoxed == other._charBoxed &&
                   _charPrimitive == other._charPrimitive &&
                   Nullable.Equals(_doubleBoxed, other._doubleBoxed) &&
                   _doublePrimitive.Equals(other._doublePrimitive) &&
                   _enumValue == other._enumValue &&
                   Nullable.Equals(_floatBoxed, other._floatBoxed) &&
                   _floatPrimitive.Equals(other._floatPrimitive) &&
                   _decimalPrimitive == other._decimalPrimitive &&
                   _decimalBoxed == other._decimalBoxed &&
                   Nullable.Equals(_bigInteger, other._bigInteger) &&
                   _intBoxed == other._intBoxed &&
                   _intPrimitive == other._intPrimitive &&
                   _longBoxed == other._longBoxed &&
                   _longPrimitive == other._longPrimitive &&
                   _shortBoxed == other._shortBoxed &&
                   _shortPrimitive == other._shortPrimitive &&
                   _theString == other._theString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((SupportBean)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = _boolBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _boolPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ _byteBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _bytePrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ _charBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _charPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ _doubleBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _doublePrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_enumValue;
                hashCode = (hashCode * 397) ^ _floatBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _floatPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ _decimalPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ _decimalBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _bigInteger.GetHashCode();
                hashCode = (hashCode * 397) ^ _intBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _intPrimitive;
                hashCode = (hashCode * 397) ^ _longBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _longPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ _shortBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ _shortPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ (_theString != null ? _theString.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static SupportBean[] GetBeansPerIndex(
            SupportBean[] beans,
            int[] indexes)
        {
            return indexes?.Select(index => beans[index])
                .ToArray();
        }

        public static object[] GetOAStringAndIntPerIndex(
            SupportBean[] beans,
            int[] indexes)
        {
            var arr = GetBeansPerIndex(beans, indexes);
            return arr == null ? null : ToOAStringAndInt(arr);
        }

        private static object[] ToOAStringAndInt(SupportBean[] arr)
        {
            return arr
                .Select(v => new object[] { v.TheString, v.IntPrimitive })
                .ToArray();
        }

        public static SupportBean MakeBean(
            string @string,
            int intPrimitive,
            long longPrimitive)
        {
            return MakeBean(@string, intPrimitive, longPrimitive, 0);
        }

        public static SupportBean MakeBean(
            string @string,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            return MakeBean(@string, intPrimitive, longPrimitive, doublePrimitive, false);
        }

        public static SupportBean MakeBean(
            string @string,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive,
            bool boolPrimitive)
        {
            var @event = new SupportBean(@string, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            @event.DoublePrimitive = doublePrimitive;
            @event.BoolPrimitive = boolPrimitive;
            return @event;
        }

        public static SupportBean MakeBean(string @string)
        {
            return new SupportBean(@string, -1);
        }

        public static SupportBean MakeBean(
            string @string,
            int intPrimitive)
        {
            return new SupportBean(@string, intPrimitive);
        }

        public static SupportBean MakeBeanWBoxed(
            string @string,
            int intPrimitive,
            double? doubleBoxed,
            long? longBoxed)
        {
            var @event = new SupportBean(@string, intPrimitive);
            @event.DoubleBoxed = doubleBoxed;
            @event.LongBoxed = longBoxed;
            return @event;
        }

        public static void Compare(
            object[] others,
            string[] split,
            object[][] objects)
        {
            ScopeTestHelper.AssertEquals(others.Length, objects.Length);
            for (var i = 0; i < others.Length; i++) {
                Compare((SupportBean)others[i], split, objects[i]);
            }
        }

        public static void Compare(
            object other,
            string theString,
            int intPrimitive)
        {
            var that = (SupportBean)other;
            ScopeTestHelper.AssertEquals(that.TheString, theString);
            ScopeTestHelper.AssertEquals(that.IntPrimitive, intPrimitive);
        }

        public static void Compare(
            SupportBean received,
            string[] split,
            object[] objects)
        {
            ScopeTestHelper.AssertEquals(split.Length, objects.Length);
            for (var i = 0; i < split.Length; i++) {
                Compare(received, split[i], objects[i]);
            }
        }

        public static void Compare(
            SupportBean received,
            string property,
            object expected)
        {
            switch (property) {
                case "IntPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.IntPrimitive);
                    break;

                case "IntBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.IntBoxed);
                    break;

                case "BoolPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.BoolPrimitive);
                    break;

                case "BoolBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.BoolBoxed);
                    break;

                case "ShortPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.ShortPrimitive);
                    break;

                case "ShortBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.ShortBoxed);
                    break;

                case "LongPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.LongPrimitive);
                    break;

                case "LongBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.LongBoxed);
                    break;

                case "CharPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.CharPrimitive);
                    break;

                case "CharBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.CharBoxed);
                    break;

                case "BytePrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.BytePrimitive);
                    break;

                case "ByteBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.ByteBoxed);
                    break;

                case "FloatPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.FloatPrimitive);
                    break;

                case "FloatBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.FloatBoxed);
                    break;

                case "DoublePrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.DoublePrimitive);
                    break;

                case "DoubleBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.DoubleBoxed);
                    break;

                case "DecimalPrimitive":
                    ScopeTestHelper.AssertEquals(expected, received.DecimalPrimitive);
                    break;

                case "DecimalBoxed":
                    ScopeTestHelper.AssertEquals(expected, received.DecimalBoxed);
                    break;

                case "EnumValue":
                    ScopeTestHelper.AssertEquals(expected, received.EnumValue);
                    break;

                case "TheString":
                    ScopeTestHelper.AssertEquals(expected, received.TheString);
                    break;

                default:
                    ScopeTestHelper.Fail("Assertion not found for '" + property + "'");
                    break;
            }
        }
    }
}