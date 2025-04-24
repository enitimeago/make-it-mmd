// Linguini
//
// MIT License
//
// Copyright 2021 Daniel Fath
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Util;
using NUnit.Framework;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Tests.IO
{
    [TestFixture]
    [TestOf(typeof(ZeroCopyUtil))]
    public class NonValidCharInputTest
    {
        [TestCase("string")]
        [TestCase("clear")]
        [TestCase("漢字")]
        [TestCase("단편")]
        [TestCase("かんじ")]
        [TestCase("Северный поток")]
        public void OperationOnNonCalleeReturnFalse(string input)
        {
            var span = input.AsSpan();
            Assert.That(span.IsCallee(), Is.False);
        }
    }
}
