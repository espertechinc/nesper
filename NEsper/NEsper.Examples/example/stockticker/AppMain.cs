///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////



namespace NEsper.Examples.StockTicker
{
    public class AppMain
    {
        public static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            new StockTickerMain("StockTicker", false).Run();

#if false
            using (TestStockTickerGenerator testStockTickerGenerator = new TestStockTickerGenerator()) {
                testStockTickerGenerator.TestFlow();
                testStockTickerGenerator.TestMakeStream();
            }

            using (TestStockTickerSimple testStockTickerSimple = new TestStockTickerSimple()) {
                testStockTickerSimple.SetUp();
                testStockTickerSimple.TestStockTicker();
            }

            using (TestStockTickerMultithreaded testStockTickerMultithreaded = new TestStockTickerMultithreaded()) {
                testStockTickerMultithreaded.SetUp();
                testStockTickerMultithreaded.TestMultithreaded();
            }
#endif
        }
    }
}
