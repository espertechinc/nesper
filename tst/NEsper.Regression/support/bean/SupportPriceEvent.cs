///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportPriceEvent
    {
        private int _price;
        private string _sym;

        public SupportPriceEvent(
            int price,
            string sym)
        {
            _price = price;
            _sym = sym;
        }

        public int Price {
            get => _price;
            set => _price = value;
        }

        public string Sym {
            get => _sym;
            set => _sym = value;
        }
    }
} // end of namespace