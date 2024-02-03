///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestIntSeqKey : AbstractCommonTest
    {
        private readonly IntSeqKeyRoot _zero = IntSeqKeyRoot.INSTANCE;
        private readonly IntSeqKeyOne _one = new IntSeqKeyOne(1);
        private readonly IntSeqKeyTwo _two = new IntSeqKeyTwo(2, 3);
        private readonly IntSeqKeyThree _three = new IntSeqKeyThree(4, 5, 6);
        private readonly IntSeqKeyFour _four = new IntSeqKeyFour(7, 8, 9, 10);
        private readonly IntSeqKeyFive _five = new IntSeqKeyFive(11, 12, 13, 14, 15);
        private readonly IntSeqKeySix _six = new IntSeqKeySix(16, 17, 18, 19, 20, 21);
        private readonly IntSeqKeyMany _seven = new IntSeqKeyMany(new[] { 22, 23, 24, 25, 26, 27, 28 });
        private readonly IntSeqKeyMany _eight = new IntSeqKeyMany(new[] { 29, 30, 31, 32, 33, 34, 35, 36 });

        private void AssertReadWrite<T>(
            T key,
            Writer<T> write,
            Reader<T> read)
            where T : IntSeqKey
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream()) {
                using (var dataOutput = new BinaryDataOutput(memoryStream)) {
                    write.Invoke(dataOutput, key);
                    memoryStream.Flush();
                    memoryStream.Close();
                    bytes = memoryStream.ToArray();
                }
            }

            using (var memoryStream = new MemoryStream(bytes)) {
                using (var dataInput = new BinaryDataInput(memoryStream)) {
                    IntSeqKey deserialized = read.Invoke(dataInput);
                    ClassicAssert.AreEqual(key, deserialized);
                }
            }
        }

        private void AssertArray(
            int[] result,
            params int[] expected)
        {
            ClassicAssert.IsTrue(Arrays.AreEqual(expected, result));
        }

        [Test]
        public void TestAddToEnd()
        {
            AssertArray(_eight.AddToEnd(99).AsIntArray(), 29, 30, 31, 32, 33, 34, 35, 36, 99);
            AssertArray(_seven.AddToEnd(99).AsIntArray(), 22, 23, 24, 25, 26, 27, 28, 99);
            AssertArray(_six.AddToEnd(99).AsIntArray(), 16, 17, 18, 19, 20, 21, 99);
            AssertArray(_five.AddToEnd(99).AsIntArray(), 11, 12, 13, 14, 15, 99);
            AssertArray(_four.AddToEnd(99).AsIntArray(), 7, 8, 9, 10, 99);
            AssertArray(_three.AddToEnd(99).AsIntArray(), 4, 5, 6, 99);
            AssertArray(_two.AddToEnd(99).AsIntArray(), 2, 3, 99);
            AssertArray(_one.AddToEnd(99).AsIntArray(), 1, 99);
            AssertArray(_zero.AddToEnd(99).AsIntArray(), 99);
        }

        [Test]
        public void TestAsIntArray()
        {
            AssertArray(_eight.AsIntArray(), 29, 30, 31, 32, 33, 34, 35, 36);
            AssertArray(_seven.AsIntArray(), 22, 23, 24, 25, 26, 27, 28);
            AssertArray(_six.AsIntArray(), 16, 17, 18, 19, 20, 21);
            AssertArray(_five.AsIntArray(), 11, 12, 13, 14, 15);
            AssertArray(_four.AsIntArray(), 7, 8, 9, 10);
            AssertArray(_three.AsIntArray(), 4, 5, 6);
            AssertArray(_two.AsIntArray(), 2, 3);
            AssertArray(_one.AsIntArray(), 1);
            AssertArray(_zero.AsIntArray());
        }

        [Test]
        public void TestIsParent()
        {
            ClassicAssert.IsTrue(_zero.IsParentTo(_one));
            ClassicAssert.IsTrue(_zero.IsParentTo(new IntSeqKeyOne(99)));
            ClassicAssert.IsFalse(_zero.IsParentTo(_zero));
            ClassicAssert.IsFalse(_zero.IsParentTo(_two));

            ClassicAssert.IsTrue(_one.IsParentTo(_one.AddToEnd(99)));
            ClassicAssert.IsTrue(_one.IsParentTo(new IntSeqKeyTwo(1, 2)));
            ClassicAssert.IsFalse(_one.IsParentTo(new IntSeqKeyTwo(2, 2)));
            ClassicAssert.IsFalse(_one.IsParentTo(_zero));
            ClassicAssert.IsFalse(_one.IsParentTo(_one));
            ClassicAssert.IsFalse(_one.IsParentTo(_two));
            ClassicAssert.IsFalse(_one.IsParentTo(new IntSeqKeyThree(1, 2, 3)));

            ClassicAssert.IsTrue(_two.IsParentTo(_two.AddToEnd(99)));
            ClassicAssert.IsTrue(_two.IsParentTo(new IntSeqKeyThree(2, 3, 5)));
            ClassicAssert.IsFalse(_two.IsParentTo(new IntSeqKeyThree(1, 3, 5)));
            ClassicAssert.IsFalse(_two.IsParentTo(new IntSeqKeyThree(2, 4, 5)));
            ClassicAssert.IsFalse(_two.IsParentTo(_zero));
            ClassicAssert.IsFalse(_two.IsParentTo(_one));
            ClassicAssert.IsFalse(_two.IsParentTo(_two));
            ClassicAssert.IsFalse(_two.IsParentTo(_three));
            ClassicAssert.IsFalse(_two.IsParentTo(new IntSeqKeyFour(2, 3, 4, 5)));

            ClassicAssert.IsTrue(_three.IsParentTo(_three.AddToEnd(99)));
            ClassicAssert.IsTrue(_three.IsParentTo(new IntSeqKeyFour(4, 5, 6, 0)));
            ClassicAssert.IsFalse(_three.IsParentTo(new IntSeqKeyFour(3, 5, 6, 0)));
            ClassicAssert.IsFalse(_three.IsParentTo(new IntSeqKeyFour(4, 4, 6, 0)));
            ClassicAssert.IsFalse(_three.IsParentTo(new IntSeqKeyFour(4, 5, 5, 0)));
            ClassicAssert.IsFalse(_three.IsParentTo(_zero));
            ClassicAssert.IsFalse(_three.IsParentTo(_one));
            ClassicAssert.IsFalse(_three.IsParentTo(_two));
            ClassicAssert.IsFalse(_three.IsParentTo(_four));
            ClassicAssert.IsFalse(_three.IsParentTo(new IntSeqKeyFive(4, 5, 6, 7, 5)));

            ClassicAssert.IsTrue(_four.IsParentTo(_four.AddToEnd(99)));
            ClassicAssert.IsTrue(_four.IsParentTo(new IntSeqKeyFive(7, 8, 9, 10, 0)));
            ClassicAssert.IsFalse(_four.IsParentTo(new IntSeqKeyFive(6, 8, 9, 10, 0)));
            ClassicAssert.IsFalse(_four.IsParentTo(new IntSeqKeyFive(7, 7, 9, 10, 0)));
            ClassicAssert.IsFalse(_four.IsParentTo(new IntSeqKeyFive(7, 8, 8, 10, 0)));
            ClassicAssert.IsFalse(_four.IsParentTo(new IntSeqKeyFive(7, 8, 9, 9, 0)));
            ClassicAssert.IsFalse(_four.IsParentTo(_zero));
            ClassicAssert.IsFalse(_four.IsParentTo(_one));
            ClassicAssert.IsFalse(_four.IsParentTo(_two));
            ClassicAssert.IsFalse(_four.IsParentTo(_four));
            ClassicAssert.IsFalse(_four.IsParentTo(_five));
            ClassicAssert.IsFalse(_four.IsParentTo(_six));
            ClassicAssert.IsFalse(_four.IsParentTo(new IntSeqKeySix(7, 8, 9, 10, 11, 12)));

            ClassicAssert.IsTrue(_five.IsParentTo(_five.AddToEnd(99)));
            ClassicAssert.IsTrue(_five.IsParentTo(new IntSeqKeySix(11, 12, 13, 14, 15, 0)));
            ClassicAssert.IsFalse(_five.IsParentTo(new IntSeqKeySix(0, 12, 13, 14, 15, 0)));
            ClassicAssert.IsFalse(_five.IsParentTo(new IntSeqKeySix(11, 0, 13, 14, 15, 0)));
            ClassicAssert.IsFalse(_five.IsParentTo(new IntSeqKeySix(11, 12, 0, 14, 15, 0)));
            ClassicAssert.IsFalse(_five.IsParentTo(new IntSeqKeySix(11, 12, 13, 0, 15, 0)));
            ClassicAssert.IsFalse(_five.IsParentTo(new IntSeqKeySix(11, 12, 13, 14, 0, 0)));
            ClassicAssert.IsFalse(_five.IsParentTo(_five));
            ClassicAssert.IsFalse(_five.IsParentTo(_six));
            ClassicAssert.IsFalse(_five.IsParentTo(_seven));
            ClassicAssert.IsFalse(_five.IsParentTo(new IntSeqKeyMany(new[] { 11, 12, 13, 14, 15, 0, 0 })));

            ClassicAssert.IsTrue(_six.IsParentTo(_six.AddToEnd(99)));
            ClassicAssert.IsTrue(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 17, 18, 19, 20, 21, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 0, 17, 18, 19, 20, 21, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 0, 18, 19, 20, 21, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 17, 0, 19, 20, 21, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 17, 18, 0, 20, 21, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 17, 18, 19, 0, 21, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 17, 18, 19, 20, 0, 0 })));
            ClassicAssert.IsFalse(_six.IsParentTo(_five));
            ClassicAssert.IsFalse(_six.IsParentTo(_six));
            ClassicAssert.IsFalse(_six.IsParentTo(_seven));
            ClassicAssert.IsFalse(_six.IsParentTo(_eight));
            ClassicAssert.IsFalse(_six.IsParentTo(new IntSeqKeyMany(new[] { 16, 17, 18, 19, 20, 21, 0, 0 })));

            ClassicAssert.IsTrue(_seven.IsParentTo(_seven.AddToEnd(99)));
            ClassicAssert.IsTrue(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 24, 25, 26, 27, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 0, 23, 24, 25, 26, 27, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 0, 24, 25, 26, 27, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 0, 25, 26, 27, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 24, 0, 26, 27, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 24, 25, 0, 27, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 24, 25, 26, 0, 28, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 24, 25, 26, 27, 0, 0 })));
            ClassicAssert.IsFalse(_seven.IsParentTo(_five));
            ClassicAssert.IsFalse(_seven.IsParentTo(_six));
            ClassicAssert.IsFalse(_seven.IsParentTo(_seven));
            ClassicAssert.IsFalse(_seven.IsParentTo(_eight));
            ClassicAssert.IsFalse(_seven.IsParentTo(new IntSeqKeyMany(new[] { 22, 23, 24, 25, 26, 27, 28, 0, 0 })));
        }

        [Test]
        public void TestLast()
        {
            ClassicAssert.AreEqual(36, _eight.Last);
            ClassicAssert.AreEqual(28, _seven.Last);
            ClassicAssert.AreEqual(21, _six.Last);
            ClassicAssert.AreEqual(15, _five.Last);
            ClassicAssert.AreEqual(10, _four.Last);
            ClassicAssert.AreEqual(6, _three.Last);
            ClassicAssert.AreEqual(3, _two.Last);
            ClassicAssert.AreEqual(1, _one.Last);
            Assert.That(() => _zero.Last, Throws.InstanceOf<UnsupportedOperationException>());
        }

        [Test]
        public void TestLength()
        {
            ClassicAssert.AreEqual(8, _eight.Length);
            ClassicAssert.AreEqual(7, _seven.Length);
            ClassicAssert.AreEqual(6, _six.Length);
            ClassicAssert.AreEqual(5, _five.Length);
            ClassicAssert.AreEqual(4, _four.Length);
            ClassicAssert.AreEqual(3, _three.Length);
            ClassicAssert.AreEqual(2, _two.Length);
            ClassicAssert.AreEqual(1, _one.Length);
            ClassicAssert.AreEqual(0, _zero.Length);
        }

        [Test]
        public void TestReadWrite()
        {
            Writer<IntSeqKeyOne> writerOne = IntSeqKeyOne.Write;
            AssertReadWrite(_one, writerOne, IntSeqKeyOne.Read);

            Writer<IntSeqKeyTwo> writerTwo = IntSeqKeyTwo.Write;
            AssertReadWrite(_two, writerTwo, IntSeqKeyTwo.Read);

            Writer<IntSeqKeyThree> writerThree = IntSeqKeyThree.Write;
            AssertReadWrite(_three, writerThree, IntSeqKeyThree.Read);

            Writer<IntSeqKeyFour> writerFour = IntSeqKeyFour.Write;
            AssertReadWrite(_four, writerFour, IntSeqKeyFour.Read);

            Writer<IntSeqKeyFive> writerFive = IntSeqKeyFive.Write;
            AssertReadWrite(_five, writerFive, IntSeqKeyFive.Read);

            Writer<IntSeqKeySix> writerSix = IntSeqKeySix.Write;
            AssertReadWrite(_six, writerSix, IntSeqKeySix.Read);

            Writer<IntSeqKeyMany> writerMany = IntSeqKeyMany.Write;
            AssertReadWrite(_seven, writerMany, IntSeqKeyMany.Read);
            AssertReadWrite(_eight, writerMany, IntSeqKeyMany.Read);
        }

        [Test]
        public void TestRemoveFromEnd()
        {
            AssertArray(_eight.RemoveFromEnd().AsIntArray(), 29, 30, 31, 32, 33, 34, 35);
            AssertArray(_seven.RemoveFromEnd().AsIntArray(), 22, 23, 24, 25, 26, 27);
            AssertArray(_six.RemoveFromEnd().AsIntArray(), 16, 17, 18, 19, 20);
            AssertArray(_five.RemoveFromEnd().AsIntArray(), 11, 12, 13, 14);
            AssertArray(_four.RemoveFromEnd().AsIntArray(), 7, 8, 9);
            AssertArray(_three.RemoveFromEnd().AsIntArray(), 4, 5);
            AssertArray(_two.RemoveFromEnd().AsIntArray(), 2);
            AssertArray(_one.RemoveFromEnd().AsIntArray());
            Assert.That(() => _zero.RemoveFromEnd(), Throws.InstanceOf<UnsupportedOperationException>());
        }

        public delegate void Writer<T>(DataOutput output, T t);
        public delegate T Reader<T>(DataInput input);
    }
} // end of namespace
