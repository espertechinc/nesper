///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
	/// <summary>
	/// Enum representing relational types of operation.
	/// </summary>
	public class RelationalOpEnum {
	    /// <summary>
	    /// Greater then.
	    /// </summary>
	    public static readonly RelationalOpEnum GT = new RelationalOpEnum(">");

	    /// <summary>
	    /// Greater equals.
	    /// </summary>
	    public static readonly RelationalOpEnum GE = new RelationalOpEnum(">=");

	    /// <summary>
	    /// Lesser then.
	    /// </summary>
	    public static readonly RelationalOpEnum LT = new RelationalOpEnum("<");

	    /// <summary>
	    /// Lesser equals.
	    /// </summary>
	    public static readonly RelationalOpEnum LE = new RelationalOpEnum("<=");

	    private static IDictionary<HashableMultiKey, RelationalOpEnum.Computer> computers;

	    private string expressionText;

	    private RelationalOpEnum(string expressionText) {
	        this.expressionText = expressionText;
	    }

	    public RelationalOpEnum Reversed() {
	        if (GT == this) {
	            return LT;
	        } else if (GE == this) {
	            return LE;
	        } else if (LE == this) {
	            return GE;
	        }
	        return GT;
	    }

	    /// <summary>
	    /// Parses the operator and returns an enum for the operator.
	    /// </summary>
	    /// <param name="op">to parse</param>
	    /// <returns>enum representing relational operation</returns>
	    public static RelationalOpEnum Parse(string op) {
	        if (op.Equals("<")) {
	            return LT;
	        } else if (op.Equals(">")) {
	            return GT;
	        } else if ((op.Equals(">=")) || op.Equals("=>")) {
	            return GE;
	        } else if ((op.Equals("<=")) || op.Equals("=<")) {
	            return LE;
	        } else throw new ArgumentException("Invalid relational operator '" + op + "'");
	    }

	    static RelationalOpEnum()
	    {
	        computers = new Dictionary<HashableMultiKey, Computer>();
	        computers.Put(new HashableMultiKey(new object[]{typeof(string), GT}), new GTStringComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(string), GE}), new GEStringComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(string), LT}), new LTStringComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(string), LE}), new LEStringComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(int?), GT}), new GTIntegerComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(int?), GE}), new GEIntegerComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(int?), LT}), new LTIntegerComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(int?), LE}), new LEIntegerComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(long?), GT}), new GTLongComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(long?), GE}), new GELongComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(long?), LT}), new LTLongComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(long?), LE}), new LELongComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(double?), GT}), new GTDoubleComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(double?), GE}), new GEDoubleComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(double?), LT}), new LTDoubleComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(double?), LE}), new LEDoubleComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(float?), GT}), new GTFloatComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(float?), GE}), new GEFloatComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(float?), LT}), new LTFloatComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(float?), LE}), new LEFloatComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(decimal?), GT}), new GTDecimalComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(decimal?), GE}), new GEDecimalComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(decimal?), LT}), new LTDecimalComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(decimal?), LE}), new LEDecimalComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(BigInteger), GT}), new GTBigIntComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(BigInteger), GE}), new GEBigIntComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(BigInteger), LT}), new LTBigIntComputer());
	        computers.Put(new HashableMultiKey(new object[]{typeof(BigInteger), LE}), new LEBigIntComputer());
	    }

	    /// <summary>
	    /// Returns the computer to use for the relational operation based on the coercion type.
	    /// </summary>
	    /// <param name="coercedType">is the object type</param>
	    /// <param name="typeOne">the compare-to type on the LHS</param>
	    /// <param name="typeTwo">the compare-to type on the RHS</param>
	    /// <returns>computer for performing the relational op</returns>
	    public RelationalOpEnum.Computer GetComputer(Type coercedType, Type typeOne, Type typeTwo) {
	        if ((coercedType != typeof(double?)) &&
	                (coercedType != typeof(float?)) &&
	                (coercedType != typeof(int?)) &&
	                (coercedType != typeof(long?)) &&
	                (coercedType != typeof(string)) &&
	                (coercedType != typeof(decimal?)) &&
	                (coercedType != typeof(BigInteger))) {
	            throw new ArgumentException("Unsupported type for relational op compare, type " + coercedType);
	        }

	        if (coercedType == typeof(decimal?)) {
	            return MakedecimalComputer(typeOne, typeTwo);
	        }
	        if (coercedType == typeof(BigInteger)) {
	            return MakeBigIntegerComputer(typeOne, typeTwo);
	        }

	        HashableMultiKey key = new HashableMultiKey(new object[]{coercedType, this});
	        return computers.Get(key);
	    }

	    private Computer MakedecimalComputer(Type typeOne, Type typeTwo) {
	        if ((typeOne == typeof(decimal?)) && (typeTwo == typeof(decimal?))) {
	            return computers.Get(new HashableMultiKey(new object[]{typeof(decimal?), this}));
	        }
            
	        SimpleNumberDecimalCoercer convertorOne = SimpleNumberCoercerFactory.GetCoercerdecimal(typeOne);
	        SimpleNumberDecimalCoercer convertorTwo = SimpleNumberCoercerFactory.GetCoercerdecimal(typeTwo);
	        if (this == GT) {
	            return new GTDecimalConvComputer(convertorOne, convertorTwo);
	        }
	        if (this == LT) {
	            return new LTDecimalConvComputer(convertorOne, convertorTwo);
	        }
	        if (this == GE) {
	            return new GEDecimalConvComputer(convertorOne, convertorTwo);
	        }
	        return new LEDecimalConvComputer(convertorOne, convertorTwo);
	    }

	    private Computer MakeBigIntegerComputer(Type typeOne, Type typeTwo) {
	        if ((typeOne == typeof(BigInteger)) && (typeTwo == typeof(BigInteger))) {
	            return computers.Get(new HashableMultiKey(new object[]{typeof(BigInteger), this}));
	        }
	        SimpleNumberBigIntegerCoercer convertorOne = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeOne);
	        SimpleNumberBigIntegerCoercer convertorTwo = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeTwo);
	        if (this == GT) {
	            return new GTBigIntConvComputer(convertorOne, convertorTwo);
	        }
	        if (this == LT) {
	            return new LTBigIntConvComputer(convertorOne, convertorTwo);
	        }
	        if (this == GE) {
	            return new GEBigIntConvComputer(convertorOne, convertorTwo);
	        }
	        return new LEBigIntConvComputer(convertorOne, convertorTwo);
	    }

	    /// <summary>
	    /// Computer for relational op.
	    /// </summary>
	    public interface Computer {
	        /// <summary>
	        /// Compares objects and returns boolean indicating larger (true) or smaller (false).
	        /// </summary>
	        /// <param name="objOne">object to compare</param>
	        /// <param name="objTwo">object to compare</param>
	        /// <returns>true if larger, false if smaller</returns>
	        bool Compare(object objOne, object objTwo);

	        CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType);
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTStringComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            string s1 = (string) objOne;
	            string s2 = (string) objTwo;
	            int result = s1.CompareTo(s2);
	            return result > 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenStringCompare(lhs, lhsType, rhs, rhsType, CodegenExpressionRelational.CodegenRelational.GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEStringComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            string s1 = (string) objOne;
	            string s2 = (string) objTwo;
	            return s1.CompareTo(s2) >= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenStringCompare(lhs, lhsType, rhs, rhsType, CodegenExpressionRelational.CodegenRelational.GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEStringComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            string s1 = (string) objOne;
	            string s2 = (string) objTwo;
	            return s1.CompareTo(s2) <= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenStringCompare(lhs, lhsType, rhs, rhsType, CodegenExpressionRelational.CodegenRelational.LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTStringComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            string s1 = (string) objOne;
	            string s2 = (string) objTwo;
	            return s1.CompareTo(s2) < 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenStringCompare(lhs, lhsType, rhs, rhsType, CodegenExpressionRelational.CodegenRelational.LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTLongComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsLong() > s2.AsLong();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenLong(lhs, lhsType, rhs, rhsType, GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GELongComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsLong() >= s2.AsLong();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenLong(lhs, lhsType, rhs, rhsType, GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTLongComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsLong() < s2.AsLong();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenLong(lhs, lhsType, rhs, rhsType, LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LELongComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsLong() <= s2.AsLong();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenLong(lhs, lhsType, rhs, rhsType, LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTIntegerComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsInt() > s2.AsInt();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenInt(lhs, lhsType, rhs, rhsType, GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEIntegerComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsInt() >= s2.AsInt();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenInt(lhs, lhsType, rhs, rhsType, GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTIntegerComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsInt() < s2.AsInt();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenInt(lhs, lhsType, rhs, rhsType, LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEIntegerComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsInt() <= s2.AsInt();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenInt(lhs, lhsType, rhs, rhsType, LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTDoubleComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsDouble() > s2.AsDouble();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDouble(lhs, lhsType, rhs, rhsType, GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEDoubleComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsDouble() >= s2.AsDouble();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDouble(lhs, lhsType, rhs, rhsType, GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTDoubleComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsDouble() < s2.AsDouble();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDouble(lhs, lhsType, rhs, rhsType, LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEDoubleComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsDouble() <= s2.AsDouble();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDouble(lhs, lhsType, rhs, rhsType, LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTFloatComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsFloat() > s2.AsFloat();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenFloat(lhs, lhsType, rhs, rhsType, GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEFloatComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsFloat() >= s2.AsFloat();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenFloat(lhs, lhsType, rhs, rhsType, GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTFloatComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsFloat() < s2.AsFloat();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenFloat(lhs, lhsType, rhs, rhsType, LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEFloatComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            object s1 = (object) objOne;
	            object s2 = (object) objTwo;
	            return s1.AsFloat() <= s2.AsFloat();
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenFloat(lhs, lhsType, rhs, rhsType, LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTDecimalComputer : Computer {
	        public bool Compare(object objOne, object objTwo)
	        {
	            var s1 = objOne.AsDecimal();
	            var s2 = objTwo.AsDecimal();
	            int result = s1.CompareTo(s2);
	            return result > 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEDecimalComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            var s1 = objOne.AsDecimal();
	            var s2 = objTwo.AsDecimal();
	            return s1.CompareTo(s2) >= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEDecimalComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            var s1 = objOne.AsDecimal();
	            var s2 = objTwo.AsDecimal();
	            return s1.CompareTo(s2) <= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTDecimalComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            var s1 = objOne.AsDecimal();
	            var s2 = objTwo.AsDecimal();
	            return s1.CompareTo(s2) < 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTBigIntComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = (BigInteger) objOne;
	            BigInteger s2 = (BigInteger) objTwo;
	            int result = s1.CompareTo(s2);
	            return result > 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEBigIntComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = (BigInteger) objOne;
	            BigInteger s2 = (BigInteger) objTwo;
	            return s1.CompareTo(s2) >= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEBigIntComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = (BigInteger) objOne;
	            BigInteger s2 = (BigInteger) objTwo;
	            return s1.CompareTo(s2) <= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTBigIntComputer : Computer {
	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = (BigInteger) objOne;
	            BigInteger s2 = (BigInteger) objTwo;
	            return s1.CompareTo(s2) < 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntDecimalPlain(lhs, rhs, CodegenExpressionRelational.CodegenRelational.LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTBigIntConvComputer : Computer {
	        private readonly SimpleNumberBigIntegerCoercer convOne;
	        private readonly SimpleNumberBigIntegerCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public GTBigIntConvComputer(SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = convOne.CoerceBoxedBigInt((object) objOne);
	            BigInteger s2 = convTwo.CoerceBoxedBigInt((object) objTwo);
	            int result = s1.CompareTo(s2);
	            return result > 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEBigIntConvComputer : Computer {
	        private readonly SimpleNumberBigIntegerCoercer convOne;
	        private readonly SimpleNumberBigIntegerCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public GEBigIntConvComputer(SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = convOne.CoerceBoxedBigInt((object) objOne);
	            BigInteger s2 = convTwo.CoerceBoxedBigInt((object) objTwo);
	            return s1.CompareTo(s2) >= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEBigIntConvComputer : Computer {
	        private readonly SimpleNumberBigIntegerCoercer convOne;
	        private readonly SimpleNumberBigIntegerCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public LEBigIntConvComputer(SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = convOne.CoerceBoxedBigInt((object) objOne);
	            BigInteger s2 = convTwo.CoerceBoxedBigInt((object) objTwo);
	            return s1.CompareTo(s2) <= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTBigIntConvComputer : Computer {
	        private readonly SimpleNumberBigIntegerCoercer convOne;
	        private readonly SimpleNumberBigIntegerCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public LTBigIntConvComputer(SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            BigInteger s1 = convOne.CoerceBoxedBigInt((object) objOne);
	            BigInteger s2 = convTwo.CoerceBoxedBigInt((object) objTwo);
	            return s1.CompareTo(s2) < 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenBigIntConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.LT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GTDecimalConvComputer : Computer {
	        private readonly SimpleNumberDecimalCoercer convOne;
	        private readonly SimpleNumberDecimalCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public GTDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            var s1 = convOne.CoerceBoxedDecimal((object) objOne);
	            var s2 = convTwo.CoerceBoxedDecimal((object) objTwo);
	            int result = s1.CompareTo(s2);
	            return result > 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDecimalConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.GT);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class GEDecimalConvComputer : Computer {
	        private readonly SimpleNumberDecimalCoercer convOne;
	        private readonly SimpleNumberDecimalCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public GEDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            var s1 = convOne.CoerceBoxedDecimal((object) objOne);
	            var s2 = convTwo.CoerceBoxedDecimal((object) objTwo);
	            return s1.CompareTo(s2) >= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDecimalConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.GE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LEDecimalConvComputer : Computer {
	        private readonly SimpleNumberDecimalCoercer convOne;
	        private readonly SimpleNumberDecimalCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public LEDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            var s1 = convOne.CoerceBoxedDecimal((object) objOne);
	            var s2 = convTwo.CoerceBoxedDecimal((object) objTwo);
	            return s1.CompareTo(s2) <= 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDecimalConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.LE);
	        }
	    }

	    /// <summary>
	    /// Computer for relational op compare.
	    /// </summary>
	    public class LTDecimalConvComputer : Computer {
	        private readonly SimpleNumberDecimalCoercer convOne;
	        private readonly SimpleNumberDecimalCoercer convTwo;

	        /// <summary>
	        /// Ctor.
	        /// </summary>
	        /// <param name="convOne">convertor for LHS</param>
	        /// <param name="convTwo">convertor for RHS</param>
	        public LTDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo) {
	            this.convOne = convOne;
	            this.convTwo = convTwo;
	        }

	        public bool Compare(object objOne, object objTwo) {
	            var s1 = convOne.CoerceBoxedDecimal((object) objOne);
	            var s2 = convTwo.CoerceBoxedDecimal((object) objTwo);
	            return s1.CompareTo(s2) < 0;
	        }

	        public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
	            return CodegenDecimalConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.LT);
	        }
	    }

	    /// <summary>
	    /// Returns string rendering of enum.
	    /// </summary>
	    /// <returns>relational op string</returns>
	    public string ExpressionText
	    {
	        get => expressionText;
	    }

	    private static CodegenExpression CodegenLong(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, RelationalOpEnum op) {
	        return Op(MathArithTypeEnum.CodegenAsLong(lhs, lhsType), op.ExpressionText, MathArithTypeEnum.CodegenAsLong(rhs, rhsType));
	    }

	    private static CodegenExpression CodegenDouble(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, RelationalOpEnum op) {
	        return Op(MathArithTypeEnum.CodegenAsDouble(lhs, lhsType), op.ExpressionText, MathArithTypeEnum.CodegenAsDouble(rhs, rhsType));
	    }

	    private static CodegenExpression CodegenFloat(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, RelationalOpEnum op) {
	        return Op(MathArithTypeEnum.CodegenAsFloat(lhs, lhsType), op.ExpressionText, MathArithTypeEnum.CodegenAsFloat(rhs, rhsType));
	    }

	    private static CodegenExpression CodegenInt(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, RelationalOpEnum op) {
	        return Op(MathArithTypeEnum.CodegenAsInt(lhs, lhsType), op.ExpressionText, MathArithTypeEnum.CodegenAsInt(rhs, rhsType));
	    }

	    private static CodegenExpression CodegenStringCompare(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, CodegenExpressionRelational.CodegenRelational rel) {
	        return Relational(ExprDotMethod(CodegenAsString(lhs, lhsType), "compareTo", CodegenAsString(rhs, rhsType)), rel, Constant(0));
	    }

	    private static CodegenExpression CodegenAsString(CodegenExpression @ref, Type type) {
	        if (type == typeof(string)) {
	            return @ref;
	        }
	        return Cast(typeof(string), @ref);
	    }

	    private static CodegenExpression CodegenBigIntDecimalPlain(CodegenExpression lhs, CodegenExpression rhs, CodegenExpressionRelational.CodegenRelational rel) {
	        return Relational(ExprDotMethod(lhs, "compareTo", rhs), rel, Constant(0));
	    }

	    private static CodegenExpression CodegenDecimalConv(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, SimpleNumberDecimalCoercer convLeft, SimpleNumberDecimalCoercer convRight, CodegenExpressionRelational.CodegenRelational rel) {
	        CodegenExpression leftConv = convLeft.CoerceBoxedDecimalCodegen(lhs, lhsType);
	        CodegenExpression rightConv = convRight.CoerceBoxedDecimalCodegen(rhs, rhsType);
	        return Relational(ExprDotMethod(leftConv, "compareTo", rightConv), rel, Constant(0));
	    }

	    private static CodegenExpression CodegenBigIntConv(CodegenExpression lhs, Type lhsType, CodegenExpression rhs, Type rhsType, SimpleNumberBigIntegerCoercer convLeft, SimpleNumberBigIntegerCoercer convRight, CodegenExpressionRelational.CodegenRelational rel) {
	        CodegenExpression leftConv = convLeft.CoerceBoxedBigIntCodegen(lhs, lhsType);
	        CodegenExpression rightConv = convRight.CoerceBoxedBigIntCodegen(rhs, rhsType);
	        return Relational(ExprDotMethod(leftConv, "compareTo", rightConv), rel, Constant(0));
	    }
	}
} // end of namespace