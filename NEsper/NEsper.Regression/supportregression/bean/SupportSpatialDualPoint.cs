///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportSpatialDualPoint  {
        private string id;
        private double x1;
        private double y1;
        private double x2;
        private double y2;
    
        public SupportSpatialDualPoint(string id, double x1, double y1, double x2, double y2) {
            this.id = id;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
    
        public string GetId() {
            return id;
        }
    
        public double GetX1() {
            return x1;
        }
    
        public double GetY1() {
            return y1;
        }
    
        public double GetX2() {
            return x2;
        }
    
        public double GetY2() {
            return y2;
        }
    }
} // end of namespace
