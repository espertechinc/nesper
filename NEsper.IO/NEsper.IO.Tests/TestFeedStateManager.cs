///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esperio
{
    [TestFixture]
    public class TestFeedStateManager 
    {
    	private AdapterStateManager stateManager;
    
        [Test]
    	public void TestValidTransitionsFromOpened()
    	{
            Open();
            Start();

            Open();
            Destroy();
    	}
    
        [Test]
    	public void TestValidTransitionsFromStarted()
    	{
            Open();
            Start();
    		Stop();

            Open();
            Start();
            Pause();

            Open();
            Start();
            Destroy();
    	}
    
        [Test]
    	public void TestValidTransitionsFromPaused()
    	{
            Open();
            Start();
            Pause();
    		Stop();
    
    		Open();
    		Start();
    		Pause();
    		Destroy();
    
    		Open();
    		Start();
    		Pause();
    		Resume();
    	}
    
        [Test]
    	public void TestInvalidTransitionsFromOpened()
    	{
    		Open();
    
    		FailOnStop();
    		FailOnPause();
    		FailOnResume();
    	}
    
        [Test]
    	public void TestInvalidTransitionsFromStarted()
    	{
    		Open();
    		Start();
    
    		FailOnStart();
    		FailOnResume();
    	}
    
        [Test]
    	public void TestInvalidTransitionsFromPaused()
    	{
    		Open();
    		Start();
    		Pause();
    
    		FailOnStart();
    		FailOnPause();
    	}
    
        [Test]
    	public void TestInvalidTransitionsFromDestroyed()
    	{
    		Open();
    		Destroy();
    
    		FailOnStart();
    		FailOnStop();
    		FailOnPause();
    		FailOnResume();
    		FailOnDestroy();
    	}
    
    	private void FailOnDestroy()
    	{
    		try
    		{
    			stateManager.Destroy();
    			Assert.Fail();
    		}
    		catch(IllegalStateTransitionException)
    		{
    			// Expected
    		}
    	}
    
    	private void FailOnStart()
    	{
    		try
    		{
    			stateManager.Start();
    			Assert.Fail();
    		}
    		catch(IllegalStateTransitionException)
    		{
    			// Expected
    		}
    	}
    
    	private void FailOnResume()
    	{
    		try
    		{
    			stateManager.Resume();
    			Assert.Fail();
    		}
    		catch(IllegalStateTransitionException)
    		{
    			// Expected
    		}
    	}
    
    	private void FailOnPause()
    	{
    		try
    		{
    			stateManager.Pause();
    			Assert.Fail();
    		}
    		catch(IllegalStateTransitionException)
    		{
    			// Expected
    		}
    	}
    
    	private void FailOnStop()
    	{
    		try
    		{
    			stateManager.Stop();
    			Assert.Fail();
    		}
    		catch(IllegalStateTransitionException)
    		{
    			// Expected
    		}
    	}
    
    	private void Start()
    	{
    		stateManager.Start();
    		Assert.AreEqual(AdapterState.STARTED, stateManager.State);
    	}
    
    	private void Open()
    	{
    		stateManager = new AdapterStateManager();
    		Assert.AreEqual(AdapterState.OPENED, stateManager.State);
    	}
    
    	private void Destroy()
    	{
    		stateManager.Destroy();
    		Assert.AreEqual(AdapterState.DESTROYED, stateManager.State);
    	}
    
    	private void Stop()
    	{
    		stateManager.Stop();
    		Assert.AreEqual(AdapterState.OPENED, stateManager.State);
    	}
    
    	private void Pause()
    	{
    		stateManager.Pause();
    		Assert.AreEqual(AdapterState.PAUSED, stateManager.State);
    	}
    
    	private void Resume()
    	{
    		stateManager.Resume();
    		Assert.AreEqual(AdapterState.STARTED, stateManager.State);
    	}
    }
}
