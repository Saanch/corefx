// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Validation;
using Xunit;

namespace System.Collections.Immutable.Test
{
    public class ImmutableStackTest : SimpleElementImmutablesTestBase
    {
        /// <summary>
        /// A test for Empty
        /// </summary>
        /// <typeparam name="T">The type of elements held in the stack.</typeparam>
        private void EmptyTestHelper<T>() where T : new()
        {
            IImmutableStack<T> actual = ImmutableStack<T>.Empty;
            Assert.NotNull(actual);
            Assert.True(actual.IsEmpty);
            AssertAreSame(ImmutableStack<T>.Empty, actual.Clear());
            AssertAreSame(ImmutableStack<T>.Empty, actual.Push(new T()).Clear());
        }

        private ImmutableStack<T> InitStackHelper<T>(params T[] values)
        {
            Contract.Requires(values != null);

            var result = ImmutableStack<T>.Empty;
            foreach (var value in values)
            {
                result = result.Push(value);
            }

            return result;
        }

        private void PushAndCountTestHelper<T>() where T : new()
        {
            var actual0 = ImmutableStack<T>.Empty;
            Assert.Equal(0, actual0.Count());
            var actual1 = actual0.Push(new T());
            Assert.Equal(1, actual1.Count());
            Assert.Equal(0, actual0.Count());
            var actual2 = actual1.Push(new T());
            Assert.Equal(2, actual2.Count());
            Assert.Equal(0, actual0.Count());
        }

        private void PopTestHelper<T>(params T[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length > 0);

            var full = this.InitStackHelper(values);
            var currentStack = full;

            // This loop tests the immutable properties of Pop.
            for (int expectedCount = values.Length; expectedCount > 0; expectedCount--)
            {
                Assert.Equal(expectedCount, currentStack.Count());
                currentStack.Pop();
                Assert.Equal(expectedCount, currentStack.Count());
                var nextStack = currentStack.Pop();
                Assert.Equal(expectedCount, currentStack.Count());
                Assert.NotSame(currentStack, nextStack);
                AssertAreSame(currentStack.Pop(), currentStack.Pop(), "Popping the stack 2X should yield the same shorter stack.");
                currentStack = nextStack;
            }
        }

        private void PeekTestHelper<T>(params T[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length > 0);

            var current = this.InitStackHelper(values);
            for (int i = values.Length - 1; i >= 0; i--)
            {
                AssertAreSame(values[i], current.Peek());
                T element;
                current.Pop(out element);
                AssertAreSame(current.Peek(), element);
                var next = current.Pop();
                AssertAreSame(values[i], current.Peek(), "Pop mutated the stack instance.");
                current = next;
            }
        }

        private void EnumeratorTestHelper<T>(params T[] values)
        {
            var full = this.InitStackHelper(values);

            int i = values.Length - 1;
            foreach (var element in full)
            {
                AssertAreSame(values[i--], element);
            }

            Assert.Equal(-1, i);

            i = values.Length - 1;
            foreach (T element in (System.Collections.IEnumerable)full)
            {
                AssertAreSame(values[i--], element);
            }

            Assert.Equal(-1, i);
        }

        [Fact]
        public void EmptyTest()
        {
            this.EmptyTestHelper<GenericParameterHelper>();
            this.EmptyTestHelper<int>();
        }

        [Fact]
        public void PushAndCountTest()
        {
            this.PushAndCountTestHelper<GenericParameterHelper>();
            this.PushAndCountTestHelper<int>();
        }

        [Fact]
        public void PopTest()
        {
            this.PopTestHelper(
                new GenericParameterHelper(1),
                new GenericParameterHelper(2),
                new GenericParameterHelper(3));
            this.PopTestHelper(1, 2, 3);
        }

        [Fact]
        public void PopOutValue()
        {
            var stack = ImmutableStack<int>.Empty.Push(5).Push(6);
            int top;
            stack = stack.Pop(out top);
            Assert.Equal(6, top);
            var empty = stack.Pop(out top);
            Assert.Equal(5, top);
            Assert.True(empty.IsEmpty);

            // Try again with the interface to verify extension method behavior.
            IImmutableStack<int> stackInterface = stack;
            Assert.Same(empty, stackInterface.Pop(out top));
            Assert.Equal(5, top);
        }

        [Fact]
        public void PeekTest()
        {
            this.PeekTestHelper(
                new GenericParameterHelper(1),
                new GenericParameterHelper(2),
                new GenericParameterHelper(3));
            this.PeekTestHelper(1, 2, 3);
        }

        [Fact]
        public void EnumeratorTest()
        {
            this.EnumeratorTestHelper(new GenericParameterHelper(1), new GenericParameterHelper(2));
            this.EnumeratorTestHelper<GenericParameterHelper>();

            this.EnumeratorTestHelper(1, 2);
            this.EnumeratorTestHelper<int>();

            var stack = ImmutableStack.Create<int>(5);
            var enumeratorStruct = stack.GetEnumerator();
            Assert.Throws<InvalidOperationException>(() => enumeratorStruct.Current);
            Assert.True(enumeratorStruct.MoveNext());
            Assert.Equal(5, enumeratorStruct.Current);
            Assert.False(enumeratorStruct.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumeratorStruct.Current);

            var enumerator = ((IEnumerable<int>)stack).GetEnumerator();
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(5, enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);

            enumerator.Reset();
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(5, enumerator.Current);
            Assert.False(enumerator.MoveNext());
            enumerator.Dispose();

            Assert.Throws<ObjectDisposedException>(() => enumerator.Reset());
            Assert.Throws<ObjectDisposedException>(() => enumerator.MoveNext());
            Assert.Throws<ObjectDisposedException>(() => enumerator.Current);
        }

        [Fact]
        public void EqualityTest()
        {
            Assert.False(ImmutableStack<int>.Empty.Equals(null));
            Assert.False(ImmutableStack<int>.Empty.Equals("hi"));
            Assert.Equal(ImmutableStack<int>.Empty, ImmutableStack<int>.Empty);
            Assert.Equal(ImmutableStack<int>.Empty.Push(3), ImmutableStack<int>.Empty.Push(3));
            Assert.NotEqual(ImmutableStack<int>.Empty.Push(5), ImmutableStack<int>.Empty.Push(3));
            Assert.NotEqual(ImmutableStack<int>.Empty.Push(3).Push(5), ImmutableStack<int>.Empty.Push(3));
            Assert.NotEqual(ImmutableStack<int>.Empty.Push(3), ImmutableStack<int>.Empty.Push(3).Push(5));
        }

        [Fact]
        public void EmptyPeekThrows()
        {
            Assert.Throws<InvalidOperationException>(() => ImmutableStack<GenericParameterHelper>.Empty.Peek());
        }

        [Fact]
        public void EmptyPopThrows()
        {
            Assert.Throws<InvalidOperationException>(() => ImmutableStack<GenericParameterHelper>.Empty.Pop());
        }

        [Fact]
        public void Create()
        {
            ImmutableStack<int> queue = ImmutableStack.Create<int>();
            Assert.True(queue.IsEmpty);

            queue = ImmutableStack.Create(1);
            Assert.False(queue.IsEmpty);
            Assert.Equal(new[] { 1 }, queue);

            queue = ImmutableStack.Create(1, 2);
            Assert.False(queue.IsEmpty);
            Assert.Equal(new[] { 2, 1 }, queue);

            queue = ImmutableStack.CreateRange((IEnumerable<int>)new[] { 1, 2 });
            Assert.False(queue.IsEmpty);
            Assert.Equal(new[] { 2, 1 }, queue);

            Assert.Throws<ArgumentNullException>(() => ImmutableStack.CreateRange((IEnumerable<int>)null));
            Assert.Throws<ArgumentNullException>(() => ImmutableStack.Create((int[])null));
        }

        protected override IEnumerable<T> GetEnumerableOf<T>(params T[] contents)
        {
            var stack = ImmutableStack<T>.Empty;
            foreach (var value in contents.Reverse())
            {
                stack = stack.Push(value);
            }

            return stack;
        }
    }
}
