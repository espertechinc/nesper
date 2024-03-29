///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Configuration;
using System.Text;

namespace NEsper.Benchmark.Common
{
    /// <summary>
    /// Holds the list of symbols. Defaults to 1000
    /// Use -Desper.benchmark.symbol=1000 to configure the number of symbols to use (hence the number of EPL statements)
    /// <para/>
    /// Each symbol is prefixed with &quot;S&quot; and suffixed with &quot;A&quot; to have all symbols have the same length
    /// (f.e. S1AA S2AA ... S99A for 100 symbols)
    /// </summary>
    /// <author>Alexandre Vasseur http://avasseur.blogspot.com</author>
	public class Symbols
    {
	    private static readonly Random RAND = new Random();
	    public static readonly int SIZE;
	    public static readonly int LENGTH;

	    static Symbols()
        {
	        var symbolcount = 1000;
	        var symbolTest = symbolcount.ToString();

	        LENGTH = symbolTest.Length;
	        var symbols = new string[symbolcount];
	        for (var i = 0; i < symbols.Length; i++) {
	            symbols[i] = "S" + i;
	            while (symbols[i].Length < LENGTH) {
	                symbols[i] += "A";
	            }
	        }

	        SYMBOLS = symbols;
	        SIZE = Encoding.Unicode.GetByteCount(symbols[0]);
	    }

	    public static readonly string[] SYMBOLS;

        public static double NextPrice(double baseVal)
        {
	        var percentVar = RAND.Next(9) + 1;
	        var trend = RAND.Next(3);
	        var result = baseVal;
	        switch (trend) {
	            case 0:
	                result *= 1.0D - percentVar * 0.01D;
	                break;
	            case 2:
	                result *= 1.0D + percentVar * 0.01D;
	                break;
	        }
	        return result;
	    }

	    public static int NextVolume(int max)
        {
	        return RAND.Next(max - 1) + 1;
	    }
	}
} // End of namespace
