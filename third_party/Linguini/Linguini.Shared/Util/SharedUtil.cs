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
using System.Diagnostics.CodeAnalysis;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Util
{
    public static class SharedUtil
    {
        /// <summary>
        /// Method for matching Fluent types.
        /// 
        /// Should be overriden if the type has custom value matching. E.g. for datetime
        /// one could compare by the long value of their timestamp or by their representations.
        /// </summary>
        /// <param name="self">Currently called value</param>
        /// <param name="other">The other FluentType to compare this value with</param>
        /// <param name="scope">Current scope of the fluent bundle</param>
        /// <returns><c>true</c> if the values match and <c>false</c> otherwise</returns>
        public static bool Matches(IFluentType self, IFluentType other, IScope scope)
        {
            return (self, other) switch
            {
                (FluentString fs1, FluentString fs2) => fs1.Equals(fs2),
                (FluentNumber fn1, FluentNumber fn2) => fn1.Equals(fn2),
                (FluentReference fn1, FluentReference fn2) => fn1.Equals(fn2),
                (FluentString fs1, FluentNumber fn2) => scope.MatchByPluralCategory(fs1, fn2),
                _ => false,
            };
        }
        
        public static bool InRange(this int self, int start, int end)
        {
            return self >= start && self <= end;
        }

        public static bool InRange(this ulong self, int start, int end)
        {
            return self >= (ulong) start && self <= (ulong) end;
        }
        
        public static bool InRange(this long self, int start, int end)
        {
            return self >= start && self <= end;
        }

        public static bool InRange(this double value, int start, int end)
        {
            for (var x = Convert.ToDouble(start); x <= end; x++)
            {
                if (value.Equals(x))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
