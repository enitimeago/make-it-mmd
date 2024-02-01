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
#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Types;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Function
{
    public static class LinguiniFluentFunctions
    {
        public static IFluentType Number(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            var num = args[0].ToFluentNumber();
            if (num != null)
            {
                // TODO merge named arguments
                return num;
            }

            return new FluentErrType();
        }

        public static IFluentType Sum(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            var sum = 0.0;
            for (var i = 0; i < args.Count; i++)
            {
                var fluentType = args[i].ToFluentNumber();
                if (fluentType == null)
                {
                    return new FluentErrType();
                }

                sum += fluentType.Value;
            }

            return (FluentNumber)sum;
        }

        public static IFluentType Identity(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            try
            {
                var id = args[0];
                return id;
            }
            catch (Exception)
            {
                return new FluentErrType();
            }
        }

        public static IFluentType Concat(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            var stringConcat = new StringBuilder();
            for (var i = 0; i < args.Count; i++)
            {
                var str = args[i];
                if (str is FluentString fs)
                {
                    stringConcat.Append(fs);
                }
                else if (str is FluentNumber fn)
                {
                    stringConcat.Append(fn);
                }
            }

            return (FluentString)stringConcat.ToString();
        }
    }
}
