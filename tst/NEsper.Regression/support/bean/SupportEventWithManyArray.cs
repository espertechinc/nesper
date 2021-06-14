///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.bean
{
	[Serializable]
	public class SupportEventWithManyArray {
	    private string id;
	    private int[] intOne;
	    private int[] intTwo;
	    private int?[] intBoxedOne;
	    private int?[] intBoxedTwo;
	    private int[][] int2DimOne;
	    private int[][] int2DimTwo;
	    private object[] objectOne;
	    private object[] objectTwo;
	    private bool[] booleanOne;
	    private bool[] booleanTwo;
	    private short[] shortOne;
	    private short[] shortTwo;
	    private float[] floatOne;
	    private float[] floatTwo;
	    private double[] doubleOne;
	    private double[] doubleTwo;
	    private char[] charOne;
	    private char[] charTwo;
	    private byte[] byteOne;
	    private byte[] byteTwo;
	    private long[] longOne;
	    private long[] longTwo;
	    private string[] stringOne;
	    private string[] stringTwo;
	    private int value;
	    private ICollection<int[]> intArrayCollection;

	    public SupportEventWithManyArray() {
	    }

	    public SupportEventWithManyArray(string id) {
	        this.id = id;
	    }

	    public string Id {
		    get => id;
		    set => id = value;
	    }

	    public int[] IntOne {
		    get => intOne;
		    set => intOne = value;
	    }

	    public int[] IntTwo {
		    get => intTwo;
		    set => intTwo = value;
	    }

	    public int?[] IntBoxedOne {
		    get => intBoxedOne;
		    set => intBoxedOne = value;
	    }

	    public int?[] IntBoxedTwo {
		    get => intBoxedTwo;
		    set => intBoxedTwo = value;
	    }

	    public int[][] Int2DimOne {
		    get => int2DimOne;
		    set => int2DimOne = value;
	    }

	    public int[][] Int2DimTwo {
		    get => int2DimTwo;
		    set => int2DimTwo = value;
	    }

	    public object[] ObjectOne {
		    get => objectOne;
		    set => objectOne = value;
	    }

	    public object[] ObjectTwo {
		    get => objectTwo;
		    set => objectTwo = value;
	    }

	    public bool[] BooleanOne {
		    get => booleanOne;
		    set => booleanOne = value;
	    }

	    public bool[] BooleanTwo {
		    get => booleanTwo;
		    set => booleanTwo = value;
	    }

	    public short[] ShortOne {
		    get => shortOne;
		    set => shortOne = value;
	    }

	    public short[] ShortTwo {
		    get => shortTwo;
		    set => shortTwo = value;
	    }

	    public float[] FloatOne {
		    get => floatOne;
		    set => floatOne = value;
	    }

	    public float[] FloatTwo {
		    get => floatTwo;
		    set => floatTwo = value;
	    }

	    public double[] DoubleOne {
		    get => doubleOne;
		    set => doubleOne = value;
	    }

	    public double[] DoubleTwo {
		    get => doubleTwo;
		    set => doubleTwo = value;
	    }

	    public char[] CharOne {
		    get => charOne;
		    set => charOne = value;
	    }

	    public char[] CharTwo {
		    get => charTwo;
		    set => charTwo = value;
	    }

	    public byte[] ByteOne {
		    get => byteOne;
		    set => byteOne = value;
	    }

	    public byte[] ByteTwo {
		    get => byteTwo;
		    set => byteTwo = value;
	    }

	    public long[] LongOne {
		    get => longOne;
		    set => longOne = value;
	    }

	    public long[] LongTwo {
		    get => longTwo;
		    set => longTwo = value;
	    }

	    public string[] StringOne {
		    get => stringOne;
		    set => stringOne = value;
	    }

	    public string[] StringTwo {
		    get => stringTwo;
		    set => stringTwo = value;
	    }

	    public int Value {
		    get => value;
		    set => this.value = value;
	    }

	    public ICollection<int[]> IntArrayCollection {
		    get => intArrayCollection;
		    set => intArrayCollection = value;
	    }

	    public SupportEventWithManyArray WithIntOne(int[] intOne) {
	        this.intOne = intOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithIntTwo(int[] intTwo) {
	        this.intTwo = intTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithValue(int value) {
	        this.value = value;
	        return this;
	    }

	    public SupportEventWithManyArray WithIntBoxedOne(int?[] intBoxedOne) {
	        this.intBoxedOne = intBoxedOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithIntBoxedTwo(int?[] intBoxedTwo) {
	        this.intBoxedTwo = intBoxedTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithInt2DimOne(int[][] int2DimOne) {
	        this.int2DimOne = int2DimOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithInt2DimTwo(int[][] int2DimTwo) {
	        this.int2DimTwo = int2DimTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithObjectOne(object[] objectOne) {
	        this.objectOne = objectOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithObjectTwo(object[] objectTwo) {
	        this.objectTwo = objectTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithBooleanOne(bool[] booleanOne) {
	        this.booleanOne = booleanOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithBooleanTwo(bool[] booleanTwo) {
	        this.booleanTwo = booleanTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithShortOne(short[] shortOne) {
	        this.shortOne = shortOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithShortTwo(short[] shortTwo) {
	        this.shortTwo = shortTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithFloatOne(float[] floatOne) {
	        this.floatOne = floatOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithFloatTwo(float[] floatTwo) {
	        this.floatTwo = floatTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithDoubleOne(double[] doubleOne) {
	        this.doubleOne = doubleOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithDoubleTwo(double[] doubleTwo) {
	        this.doubleTwo = doubleTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithCharOne(char[] charOne) {
	        this.charOne = charOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithCharTwo(char[] charTwo) {
	        this.charTwo = charTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithByteOne(byte[] byteOne) {
	        this.byteOne = byteOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithByteTwo(byte[] byteTwo) {
	        this.byteTwo = byteTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithLongOne(long[] longOne) {
	        this.longOne = longOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithLongTwo(long[] longTwo) {
	        this.longTwo = longTwo;
	        return this;
	    }
	    public SupportEventWithManyArray WithStringOne(string[] stringOne) {
	        this.stringOne = stringOne;
	        return this;
	    }

	    public SupportEventWithManyArray WithStringTwo(string[] stringTwo) {
	        this.stringTwo = stringTwo;
	        return this;
	    }

	    public SupportEventWithManyArray WithIntArrayCollection(ICollection<int[]> intArrays) {
	        this.intArrayCollection = intArrays;
	        return this;
	    }
	}
} // end of namespace
