///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading
{
    public interface IReaderWriterLockManager
    {
        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="lockFactory">The lock factory.</param>
        void RegisterCategoryLock<T>(Func<IReaderWriterLock> lockFactory);

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        void RegisterCategoryLock(Type typeCategory, Func<IReaderWriterLock> lockFactory);

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        void RegisterCategoryLock(string category, Func<IReaderWriterLock> lockFactory);

        /// <summary>
        /// Creates a lock for the category defined by the type.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <returns></returns>
        IReaderWriterLock CreateLock(Type typeCategory);

        /// <summary>
        /// Creates a lock for the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        IReaderWriterLock CreateLock(string category);

        /// <summary>
        /// Creates the default lock.
        /// </summary>
        /// <returns></returns>
        IReaderWriterLock CreateDefaultLock();

        /// <summary>
        /// Creates the default lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        IReaderWriterLock CreateDefaultLock(string category);
    }
}
