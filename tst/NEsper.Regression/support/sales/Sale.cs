///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.sales
{
    [Serializable]
    public class Sale
    {
        public Sale(
            Person buyer,
            Person seller,
            double cost)
        {
            Buyer = buyer;
            Seller = seller;
            Cost = cost;
        }

        public Person Buyer { get; }

        public Person Seller { get; }

        public double Cost { get; }
    }
} // end of namespace