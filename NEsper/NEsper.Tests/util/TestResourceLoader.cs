///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
	public class TestResourceLoader
    {
	    private const string TEST_RESOURCE = "regression/esper.test.readconfig.cfg.xml";

        [Test]
	    public void TestResolveResourceAsURL()
        {
	        URL url = ResourceLoader.GetClasspathResourceAsURL("somefile", TEST_RESOURCE, Thread.CurrentThread().ContextClassLoader);
	        Assert.IsNotNull(url);

	        try {
	            ResourceLoader.GetClasspathResourceAsURL("somefile", "xxx", Thread.CurrentThread().ContextClassLoader);
	            Fail();
	        } catch (EPException ex) {
	            // expected
	        }
	    }

        [Test]
	    public void TestClasspathOrURL()
        {
	        URL url = this.GetType().ClassLoader.GetResource(TEST_RESOURCE);
	        URL urlAfterResolve = ResourceLoader.ResolveClassPathOrURLResource("a", url.ToString(), Thread.CurrentThread().ContextClassLoader);
	        Assert.AreEqual(url, urlAfterResolve);

	        URL url3 = ResourceLoader.ResolveClassPathOrURLResource("a", "file:///xxx/a.b", Thread.CurrentThread().ContextClassLoader);
	        Assert.AreEqual("file:/xxx/a.b", url3.ToString());

	        try {
	            ResourceLoader.ResolveClassPathOrURLResource("a", "b", Thread.CurrentThread().ContextClassLoader);
	            Fail();
	        } catch (EPException ex) {
	            // expected
	        }
	    }
	}
} // end of namespace
