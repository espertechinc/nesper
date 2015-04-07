///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.util;

namespace com.espertech.esper.schedule
{
	/// <summary>
    /// This class is a slot in a <see cref="ScheduleBucket"/> for sorting schedule service callbacks.
    /// </summary>

    public class ScheduleSlot
		: IComparable<ScheduleSlot>
		, MetaDefItem
	{
	    /// <summary>
	    /// Returns the bucket number.
	    /// </summary>
	    public int BucketNum { get; private set; }

	    /// <summary>
	    /// Returns the slot number.
	    /// </summary>
	    public int SlotNum { get; private set; }

	    /// <summary> Ctor.</summary>
		/// <param name="bucketNum">is the number of the bucket the slot belongs to
		/// </param>
		/// <param name="slotNum">is the slot number for ordering within the bucket
		/// </param>
		public ScheduleSlot(int bucketNum, int slotNum)
		{
			BucketNum = bucketNum;
			SlotNum = slotNum;
		}

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="scheduleCallbackSlot">The schedule callback slot.</param>
        /// <returns></returns>
		public virtual int CompareTo(ScheduleSlot scheduleCallbackSlot)
		{
			if (BucketNum > scheduleCallbackSlot.BucketNum)
			{
				return 1;
			}
			if (BucketNum < scheduleCallbackSlot.BucketNum)
			{
				return - 1;
			}
			if (SlotNum > scheduleCallbackSlot.SlotNum)
			{
				return 1;
			}
			if (SlotNum < scheduleCallbackSlot.SlotNum)
			{
				return - 1;
			}
			
			return 0;
		}

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public virtual int CompareTo(Object obj)
		{
            return CompareTo(obj as ScheduleSlot);
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return "bucket/slot=" + BucketNum + "/" + SlotNum;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ScheduleSlot that = (ScheduleSlot)obj;

            if (BucketNum != that.BucketNum)
            {
                return false;
            }
            if (SlotNum != that.SlotNum)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return BucketNum*31 + SlotNum;
        }
	}
}
