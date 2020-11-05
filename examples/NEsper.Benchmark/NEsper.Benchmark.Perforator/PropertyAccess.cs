///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq.Expressions;
using com.espertech.esper.compat;
using com.espertech.esper.compat.magic;

namespace NEsper.Benchmark.Perforator
{
    public class PropertyAccess
    {
        public static void MeasureNative()
        {
            Console.WriteLine("Native");

            var bidData = new BidData("IBM", 0L, 0.50);
            var bidDataObject = (Object)bidData;

            for (int nn = 0; nn < 10; nn++)
            {
                var timeItem = PerformanceObserver.TimeMicro(
                    delegate
                    {
                        for (int ii = 1000000; ii >= 0; ii--)
                        {
                            var bidPrice1 = (object)(((BidData)bidDataObject).BidPrice);
                        }
                    });

                Console.WriteLine("Native:         {0,8} {1,8:N3} {2,8:N3}", timeItem, timeItem / 1000000.0m, 1000000.0m / timeItem);
            }
        }

        public static void MeasureNativeLambda()
        {
            Console.WriteLine("Lambda(Native)");

            var bidData = new BidData("IBM", 0L, 0.50);
            var bidDataObject = (Object)bidData;

            Expression<Func<object, object>> lambdaExpression =
                arg => (object) ((BidData) arg).BidPrice;

            Func<object, object> lambda = lambdaExpression.Compile();

            for (int nn = 0; nn < 10; nn++)
            {
                var timeItem = PerformanceObserver.TimeMicro(
                    delegate
                    {
                        for (int ii = 1000000; ii >= 0; ii--)
                        {
                            lambda.Invoke(bidDataObject);
                        }
                    });

                Console.WriteLine("Lambda(Native): {0,8} {1,8:N3} {2,8:N3}", timeItem, timeItem / 1000000.0m, 1000000.0m / timeItem);
            }
        }

        public static void MeasureMagic()
        {
            Console.WriteLine("Lambda(Magic)");

            var bidData = new BidData("IBM", 0L, 0.50);
            var bidDataObject = (Object)bidData;

            var propInfo = typeof(BidData).GetProperty("BidPrice");
            var methodInfo = propInfo.GetGetMethod();

            var lambda = MagicType.GetLambdaAccessor(methodInfo);

            for (int nn = 0; nn < 10; nn++)
            {
                var timeItem = PerformanceObserver.TimeMicro(
                    delegate
                    {
                        for (int ii = 1000000; ii >= 0; ii--)
                        {
                            lambda.Invoke(bidDataObject);
                        }
                    });

                Console.WriteLine("Lambda(Magic):  {0,8} {1,8:N3} {2,8:N3}", timeItem, timeItem / 1000000.0m, 1000000.0m / timeItem);
            }
        }
    }
}
