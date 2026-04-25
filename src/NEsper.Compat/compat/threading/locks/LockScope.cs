// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;

namespace com.espertech.esper.compat.threading.locks
{
    /// <summary>
    /// Zero-allocation lock scope returned by <see cref="ILockable.AcquireScope()"/>.
    /// As a readonly struct it is stack-allocated; no heap object is created per acquisition.
    /// </summary>
    public readonly struct LockScope : IDisposable
    {
        private readonly ILockable _lock;

        internal LockScope(ILockable lk) => _lock = lk;

        public void Dispose() => _lock?.Release();
    }
}
