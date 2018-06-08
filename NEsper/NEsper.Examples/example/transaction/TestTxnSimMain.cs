///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NEsper.Examples.Transaction.sim;

using NUnit.Framework;

namespace NEsper.Examples.Transaction
{
	[TestFixture]
	public class TestTxnSimMain
	{
	    [Test]
	    public void TestTiny()
	    {
	        TxnGenMain main = new TxnGenMain(20, 200, "TransactionExample", false);
	        main.Run();
	    }

	    [Test]
	    public void TestSmall()
	    {
            TxnGenMain main = new TxnGenMain(1000, 3000, "TransactionExample", false);
	        main.Run();
	    }
	}
} // End of namespace
