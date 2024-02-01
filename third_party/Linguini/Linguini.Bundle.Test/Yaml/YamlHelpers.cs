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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;
using YamlDotNet.RepresentationModel;

#pragma warning disable 8600

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Test.Yaml
{
    public static class YamlHelpers
    {
        /**
         * MIT License

            Copyright (c) 2017 Space Wizards Federation

            Permission is hereby granted, free of charge, to any person obtaining a copy
            of this software and associated documentation files (the "Software"), to deal
            in the Software without restriction, including without limitation the rights
            to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
            copies of the Software, and to permit persons to whom the Software is
            furnished to do so, subject to the following conditions:

            The above copyright notice and this permission notice shall be included in all
            copies or substantial portions of the Software.

            THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
            OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
            SOFTWARE.
         */

        #region ROBUSTLY_STOLEN

        // To fetch nodes by key name with YAML, we NEED a YamlScalarNode.
        // We use a thread local one to avoid allocating one every fetch, since we just replace the inner value.
        // Obviously thread local to avoid threading issues.
        private static readonly ThreadLocal<YamlScalarNode> FetchNode =
            new(() => new YamlScalarNode());

        [Pure]
        public static bool TryGetNode<T>(this YamlMappingNode mapping, string key,
            [NotNullWhen(true)] out T? returnNode) where T : YamlNode
        {
            if (mapping.Children.TryGetValue(_getFetchNode(key), out var node))
            {
                returnNode = (T)node;
                return true;
            }

            returnNode = null;
            return false;
        }

        private static YamlScalarNode _getFetchNode(string key)
        {
            var node = FetchNode.Value!;
            node.Value = key;
            return node;
        }

        [Pure]
        public static bool AsBool(this YamlNode node)
        {
            return bool.Parse(node.AsString());
        }

        [Pure]
        public static string AsString(this YamlNode node)
        {
            return ((YamlScalarNode)node).Value ?? "";
        }

        #endregion

        #region TEST_PARSING

        public static List<ResolverTestSuite> ParseResolverTests(this YamlDocument doc)
        {
            var suiteMapNode = (YamlMappingNode)doc.RootNode["suites"][0];
            var testSuites = new List<ResolverTestSuite>();
            if (suiteMapNode.TryGetNode<YamlSequenceNode>("tests", out _))
            {
                var topLevel = new ResolverTestSuite();
                ProcessTestSuite(suiteMapNode, topLevel);
                testSuites.Add(topLevel);
            }

            if (suiteMapNode.TryGetNode("suites", out YamlSequenceNode suites))
            {
                foreach (var suiteProp in suites.Children)
                {
                    var testSuite = new ResolverTestSuite();
                    if (suiteProp is YamlMappingNode mapNode)
                    {
                        ProcessTestSuite(mapNode, testSuite);
                    }

                    testSuites.Add(testSuite);
                }
            }

            return testSuites;
        }

        private static void ProcessTestSuite(YamlMappingNode mapNode, ResolverTestSuite testSuite)
        {
            if (mapNode.TryGetNode<YamlScalarNode>("name", out var name))
            {
                testSuite.Name = name.Value!;
            }

            if (mapNode.TryGetNode<YamlSequenceNode>("resources", out var resources))
            {
                var (res, errs) = ProcessResources(resources);
                testSuite.Resources.AddRange(res);
                if (testSuite.Bundle == null)
                {
                    testSuite.Bundle = new();
                }

                testSuite.Bundle.Errors.AddRange(errs);
            }

            if (mapNode.TryGetNode<YamlSequenceNode>("bundles", out var bundles))
            {
                ProcessBundles(bundles, out testSuite.Bundle);
            }

            if (mapNode.TryGetNode<YamlSequenceNode>("tests", out var tests))
            {
                testSuite.Tests = ProcessTests(tests);
            }
        }

        private static List<ResolverTestSuite.ResolverTest> ProcessTests(YamlSequenceNode testsNode)
        {
            var testCollection = new List<ResolverTestSuite.ResolverTest>();
            foreach (var test in testsNode.Children)
            {
                testCollection.Add(ProcessTest((YamlMappingNode)test));
            }

            return testCollection;
        }

        private static ResolverTestSuite.ResolverTest ProcessTest(YamlMappingNode test)
        {
            var resolverTest = new ResolverTestSuite.ResolverTest();
            if (test.TryGetNode("name", out YamlScalarNode nameProp))
            {
                resolverTest.TestName = nameProp.Value!;
            }

            if (test.TryGetNode("asserts", out YamlSequenceNode asserts))
            {
                resolverTest.Asserts = ProcessAsserts(asserts);
            }

            if (test.TryGetNode("bundles", out YamlSequenceNode bundleNodes))
            {
                ProcessBundles(bundleNodes, out resolverTest.Bundle);
            }

            if (test.TryGetNode("resources", out YamlSequenceNode resourceNode))
            {
                var (res, errors) = ProcessResources(resourceNode);
                resolverTest.Resources.AddRange(res);
                resolverTest.ExpectedErrors.AddRange(errors);
            }

            return resolverTest;
        }

        private static List<ResolverTestSuite.ResolverAssert> ProcessAsserts(YamlSequenceNode assertsNodes)
        {
            var retVal = new List<ResolverTestSuite.ResolverAssert>();
            foreach (var assertNode in assertsNodes.Children)
            {
                if (assertNode is YamlMappingNode assertMap)
                {
                    var resolverAssert = new ResolverTestSuite.ResolverAssert();

                    if (assertMap.TryGetNode("id", out YamlScalarNode idNode))
                    {
                        resolverAssert.Id = idNode.Value!;
                    }

                    if (assertMap.TryGetNode("attribute", out YamlScalarNode attrNode))
                    {
                        resolverAssert.Attribute = attrNode.Value;
                    }

                    if (assertMap.TryGetNode("value", out YamlScalarNode valNode))
                    {
                        resolverAssert.ExpectedValue = valNode.AsString();
                    }

                    if (assertMap.TryGetNode("args", out YamlMappingNode argsNode))
                    {
                        resolverAssert.Args = ProcessArgs(argsNode);
                    }

                    if (assertMap.TryGetNode("missing", out YamlScalarNode args))
                    {
                        resolverAssert.Missing = args.AsBool();
                    }

                    if (assertMap.TryGetNode("errors", out YamlSequenceNode errorsNode))
                    {
                        resolverAssert.ExpectedErrors = ProcessErrors(errorsNode);
                    }


                    retVal.Add(resolverAssert);
                }
            }

            return retVal;
        }

        private static Dictionary<string, IFluentType> ProcessArgs(YamlMappingNode argsNode)
        {
            var processArgs = new Dictionary<string, IFluentType>();
            foreach (var arg in argsNode.Children)
            {
                var key = (YamlScalarNode)arg.Key;
                var val = (YamlScalarNode)arg.Value;
                IFluentType fluentVal;
                if (Double.TryParse(val.AsString(),
                        NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo,
                        out var result))
                {
                    fluentVal = (FluentNumber)result;
                }
                else
                {
                    fluentVal = (FluentString)val.AsString();
                }

                processArgs.Add(key.Value!, fluentVal);
            }

            return processArgs;
        }

        private static void ProcessBundles(YamlSequenceNode bundles,
            out ResolverTestSuite.ResolverTestBundle? testBundle)
        {
            testBundle = new ResolverTestSuite.ResolverTestBundle();
            foreach (var bundleNode in bundles.Children)
            {
                if (bundleNode is YamlMappingNode bundleMap)
                {
                    foreach (var keyValueNode in bundleMap.Children)
                    {
                        if (keyValueNode.Key.ToString().Equals("functions"))
                        {
                            ProcessFunctions((YamlSequenceNode)keyValueNode.Value, out testBundle.Functions);
                        }
                        else if (keyValueNode.Key.ToString().Equals("errors"))
                        {
                            testBundle.Errors = ProcessErrors((YamlSequenceNode)bundleMap["errors"]);
                        }
                        else if (keyValueNode.Key.ToString().Equals("transform"))
                        {
                            testBundle.TransformFunc = keyValueNode.Value.AsString();
                        }
                        else if (keyValueNode.Key.ToString().Equals("useIsolating"))
                        {
                            testBundle.UseIsolating = keyValueNode.Value.AsBool();
                        }
                        else if (keyValueNode.Key.ToString().Equals("override"))
                        {
                            testBundle.Override = keyValueNode.Value.AsBool();
                        }
                    }
                }
            }
        }

        private static void ProcessFunctions(YamlSequenceNode functionsNode, out List<string> bundle)
        {
            bundle = new List<string>(functionsNode.Children.Count);
            foreach (var function in functionsNode)
            {
                if (function is YamlScalarNode funcName)
                {
                    bundle.Add(funcName.Value!);
                }
            }
        }

        private static (List<string>, List<ResolverTestSuite.ResolverTestError>)
            ProcessResources(YamlSequenceNode returnNode)
        {
            List<string> resource = new();
            List<ResolverTestSuite.ResolverTestError> errors = new();
            foreach (var resNode in returnNode.Children)
            {
                if (resNode is YamlMappingNode map)
                {
                    if (map.TryGetNode("source", out YamlScalarNode sourceValue))
                    {
                        resource.Add(sourceValue.Value!);
                    }

                    if (map.TryGetNode("errors", out YamlSequenceNode errorNode))
                    {
                        errors = ProcessErrors(errorNode);
                    }
                }
            }

            return (resource, errors);
        }

        private static List<ResolverTestSuite.ResolverTestError> ProcessErrors(YamlSequenceNode errorNode)
        {
            List<ResolverTestSuite.ResolverTestError> resolverTestErrors = new();
            foreach (var error in errorNode.Children)
            {
                if (error is YamlMappingNode errMap)
                {
                    var err = new ResolverTestSuite.ResolverTestError();
                    if (errMap.TryGetNode("type", out YamlScalarNode errType))
                    {
                        Enum.TryParse(errType.Value, out err.Type);
                    }

                    if (errMap.TryGetNode("desc", out YamlScalarNode errDesc))
                    {
                        err.Description = errDesc.Value;
                    }

                    resolverTestErrors.Add(err);
                }
            }

            return resolverTestErrors;
        }

        #endregion
    }
}