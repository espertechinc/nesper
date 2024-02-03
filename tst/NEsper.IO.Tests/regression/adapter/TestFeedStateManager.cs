///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esperio.regression.adapter
{
	[TestFixture]
	public class TestFeedStateManager : AbstractIOTest
	{
		private AdapterStateManager _stateManager;

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
			Assert.Throws<IllegalStateTransitionException>(() => _stateManager.Destroy());
		}

		private void FailOnStart()
		{
			Assert.Throws<IllegalStateTransitionException>(() => _stateManager.Start());
		}

		private void FailOnResume()
		{
			Assert.Throws<IllegalStateTransitionException>(() => _stateManager.Resume());
		}

		private void FailOnPause()
		{
			Assert.Throws<IllegalStateTransitionException>(() => _stateManager.Pause());
		}

		private void FailOnStop()
		{
			Assert.Throws<IllegalStateTransitionException>(() => _stateManager.Stop());
		}

		private void Start()
		{
			_stateManager.Start();
			ClassicAssert.AreEqual(AdapterState.STARTED, _stateManager.State);
		}

		private void Open()
		{
			_stateManager = new AdapterStateManager();
			ClassicAssert.AreEqual(AdapterState.OPENED, _stateManager.State);
		}

		private void Destroy()
		{
			_stateManager.Destroy();
			ClassicAssert.AreEqual(AdapterState.DESTROYED, _stateManager.State);
		}

		private void Stop()
		{
			_stateManager.Stop();
			ClassicAssert.AreEqual(AdapterState.OPENED, _stateManager.State);
		}

		private void Pause()
		{
			_stateManager.Pause();
			ClassicAssert.AreEqual(AdapterState.PAUSED, _stateManager.State);
		}

		private void Resume()
		{
			_stateManager.Resume();
			ClassicAssert.AreEqual(AdapterState.STARTED, _stateManager.State);
		}
	}
} // end of namespace
