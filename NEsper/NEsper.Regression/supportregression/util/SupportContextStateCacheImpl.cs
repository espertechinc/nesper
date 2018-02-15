///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.epl.spec;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportContextStateCacheImpl : ContextStateCache
    {
        private static readonly IDictionary<ContextStatePathKey, ContextStatePathValue> state = new Dictionary<ContextStatePathKey, ContextStatePathValue>();
        private static readonly ICollection<ContextStatePathKey> removedState = new HashSet<ContextStatePathKey>();
    
        public static void Reset() {
            state.Clear();
            removedState.Clear();
        }
    
        public static void AssertState(params ContextState[] descs) {
            Assert.AreEqual(descs.Length, state.Count);
            int count = -1;
            foreach (ContextState desc in descs) {
                count++;
                String text = "failed at descriptor " + count;
                ContextStatePathValue value = state.Get(new ContextStatePathKey(desc.Level, desc.ParentPath, desc.Subpath));
                Assert.AreEqual(desc.AgentInstanceId, (int) value.OptionalContextPartitionId, text);
                Assert.AreEqual(desc.IsStarted ? ContextPartitionState.STARTED : ContextPartitionState.STOPPED, value.State, text);
    
                Object payloadReceived = ContextStateCacheNoSave.DEFAULT_SPI_TEST_BINDING.ByteArrayToObject(value.Blob, null);
                if (desc.Payload == null) {
                    Assert.NotNull(payloadReceived);
                }
                else {
                    EPAssertionUtil.AssertEqualsAllowArray(text, desc.Payload, payloadReceived);
                }
            }
        }
    
        public static void AssertRemovedState(params ContextStatePathKey[] keys) {
            Assert.AreEqual(keys.Length, removedState.Count);
            int count = -1;
            foreach (ContextStatePathKey key in keys) {
                count++;
                String text = "failed at descriptor " + count;
                Assert.IsTrue(removedState.Contains(key), text);
            }
        }
    
        public ContextStatePathValueBinding GetBinding(Object bindingInfo) {
            if (bindingInfo is ContextDetailInitiatedTerminated) {
                return new ContextStateCacheNoSave.ContextStateCacheNoSaveInitTermBinding();
            }
            return ContextStateCacheNoSave.DEFAULT_SPI_TEST_BINDING;
        }
    
        public void AddContextPath(String contextName, int level, int parentPath, int subPath, int? optionalContextPartitionId, Object additionalInfo, ContextStatePathValueBinding binding) {
            state.Put(new ContextStatePathKey(level, parentPath, subPath), new ContextStatePathValue(optionalContextPartitionId, binding.ToByteArray(additionalInfo), ContextPartitionState.STARTED));
        }
    
        public void UpdateContextPath(String contextName, ContextStatePathKey key, ContextStatePathValue value) {
            state.Put(key, value);
        }
    
        public void RemoveContextParentPath(String contextName, int level, int parentPath) {
    
        }
    
        public void RemoveContextPath(String contextName, int level, int parentPath, int subPath) {
            ContextStatePathKey key = new ContextStatePathKey(level, parentPath, subPath);
            removedState.Add(key);
            state.Remove(key);
        }
    
        public void RemoveContext(String contextName) {
    
        }
    
        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> GetContextPaths(String contextName) {
            return null;
        }
    }
}
