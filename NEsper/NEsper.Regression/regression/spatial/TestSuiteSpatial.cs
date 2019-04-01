///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.spatial
{
    [TestFixture]
    public class TestSuiteSpatial
    {
        [Test]
        public void TestExecSpatialMXCIFQuadTreeEventIndex() {
            RegressionRunner.Run(new ExecSpatialMXCIFQuadTreeEventIndex());
        }
    
        [Test]
        public void TestExecSpatialMXCIFQuadTreeFilterIndex() {
            RegressionRunner.Run(new ExecSpatialMXCIFQuadTreeFilterIndex());
        }
    
        [Test]
        public void TestExecSpatialMXCIFQuadTreeInvalid() {
            RegressionRunner.Run(new ExecSpatialMXCIFQuadTreeInvalid());
        }
    
        [Test]
        public void TestExecSpatialPointRegionQuadTreeEventIndex() {
            RegressionRunner.Run(new ExecSpatialPointRegionQuadTreeEventIndex());
        }
    
        [Test]
        public void TestExecSpatialPointRegionQuadTreeFilterIndex() {
            RegressionRunner.Run(new ExecSpatialPointRegionQuadTreeFilterIndex());
        }
    
        [Test]
        public void TestExecSpatialPointRegionQuadTreeInvalid() {
            RegressionRunner.Run(new ExecSpatialPointRegionQuadTreeInvalid());
        }
    
    }
} // end of namespace
