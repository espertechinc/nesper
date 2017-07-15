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

namespace com.espertech.esper.util
{
    public class ObjectInputStreamWithTCCL : ObjectInputStream{
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public ObjectInputStreamWithTCCL(InputStream input) {
            Super(input);
        }
    
        public ObjectInputStreamWithTCCL() {
        }
    
        public override Type ResolveClass(ObjectStreamClass desc) {
    
            if (Log.IsDebugEnabled) {
                Log.Debug("Resolving class " + desc.Name + " id " + desc.SerialVersionUID + " classloader " + Thread.CurrentThread().ContextClassLoader.Class);
            }
    
            ClassLoader currentTccl = null;
            try {
                currentTccl = Thread.CurrentThread().ContextClassLoader;
                if (currentTccl != null) {
                    return CurrentTccl.LoadClass(desc.Name);
                }
            } catch (Exception e) {
            }
            return Base.ResolveClass(desc);
        }
    }
} // end of namespace
