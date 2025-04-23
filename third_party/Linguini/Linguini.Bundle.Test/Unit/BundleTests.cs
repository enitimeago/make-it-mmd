using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [TestOf(typeof(FluentBundle))]
    [Parallelizable]
    public class BundleTests
    {
        private readonly ExternalFunction _zeroFunc = (_, _) => FluentNone.None;
        private readonly ExternalFunction _idFunc = (args, _) => args[0];
        private readonly Func<IFluentType, string> _formatter = _ => "";
        private readonly Func<string, string> _transform = str => str.ToUpper(CultureInfo.InvariantCulture);

        private const string Res1 = @"
term = term
    .attr = 3";

        private const string Wrong = @"
    term = 1";

        private const string Multi = @"
term1 = val1
term2 = val2
    .attr = 6";

        private const string Replace1 = @"
term1 = val1
term2 = val2";

        private const string Replace2 = @"
term1 = xxx
new1  = new
    .attr = 6";


        [Test]
        public void TestDefaultBundleOptions()
        {
            var defaultBundleOpt = new FluentBundleOption();
            var bundle = FluentBundle.MakeUnchecked(defaultBundleOpt);
            Assert.That(CultureInfo.CurrentCulture, Is.EqualTo(bundle.Culture));
            Assert.That(bundle.FormatterFunc, Is.Null);
            Assert.That(bundle.TransformFunc, Is.Null);
            Assert.That(bundle.UseIsolating);
            Assert.That(100, Is.EqualTo(bundle.MaxPlaceable));
        }

        [Test]
        public void TestNonDefaultBundleOptions()
        {
            var defaultBundleOpt = new FluentBundleOption()
            {
                Locales = { "en" },
                MaxPlaceable = 123,
                UseIsolating = false,
                TransformFunc = _transform,
                FormatterFunc = _formatter,
                Functions = new Dictionary<string, ExternalFunction>()
                {
                    ["zero"] = _zeroFunc,
                    ["id"] = _idFunc,
                }
            };
            var bundle = FluentBundle.MakeUnchecked(defaultBundleOpt);
            Assert.That(new CultureInfo("en"), Is.EqualTo(bundle.Culture));
            Assert.That(defaultBundleOpt.FormatterFunc, Is.EqualTo(bundle.FormatterFunc));
            Assert.That(defaultBundleOpt.TransformFunc, Is.EqualTo(bundle.TransformFunc));
            Assert.That(defaultBundleOpt.UseIsolating, Is.EqualTo(bundle.UseIsolating));
            Assert.That(defaultBundleOpt.MaxPlaceable, Is.EqualTo(bundle.MaxPlaceable));
            Assert.That(bundle.TryGetFunction("zero", out var zero));
            Assert.That(_zeroFunc, Is.EqualTo(zero?.Function));
            Assert.That(bundle.TryGetFunction("id", out var id));
            Assert.That(_idFunc, Is.EqualTo(id?.Function));
        }

        [Test]
        [Parallelizable]
        public void TestReplaceMessage()
        {
            var bundler = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource(Replace1);

            var bundle = bundler.UncheckedBuild();
            Assert.That(bundle.TryGetAttrMessage("term1", null, out _, out var termMsg));
            Assert.That("val1", Is.EqualTo(termMsg));
            Assert.That(bundle.TryGetAttrMessage("term2", null, out _, out var termMsg2));
            Assert.That("val2", Is.EqualTo(termMsg2));

            bundle.AddResourceOverriding(Replace2);
            Assert.That(bundle.TryGetAttrMessage("term2", null, out _, out _));
            Assert.That(bundle.TryGetAttrMessage("term1", null, out _, out termMsg));
            Assert.That("xxx", Is.EqualTo(termMsg));
            Assert.That(bundle.TryGetAttrMessage("new1.attr", null, out _, out var newMsg));
            Assert.That("6", Is.EqualTo(newMsg));
        }

        [Test]
        [Parallelizable]
        public void TestExceptions()
        {
            var bundler = LinguiniBuilder.Builder()
                .Locales("en-US", "sr-RS")
                .AddResources(Wrong, Res1)
                .SetFormatterFunc(_formatter)
                .SetTransformFunc(_transform)
                .AddFunction("id", _idFunc)
                .AddFunction("zero", _zeroFunc);

            Assert.Throws(typeof(LinguiniException), () => bundler.UncheckedBuild());
        }

        [Test]
        [Parallelizable]
        public void TestReadme()
        {
            var bundle = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource("hello-user =  Hello, { $username }!")
                .UncheckedBuild();

            var message = bundle.GetAttrMessage("hello-user", ("username", (FluentString)"Test"));
            Assert.That("Hello, Test!", Is.EqualTo(message));
        }

        [Test]
        [Parallelizable]
        public void TestEnumeration()
        {
            var bundler = LinguiniBuilder.Builder()
                .Locale("en-US")
                .AddResource(Multi)
                .AddFunction("id", _idFunc)
                .AddFunction("zero", _zeroFunc)
                .UncheckedBuild();
            var messages = bundler.GetMessageEnumerable().ToArray();
            var functions = bundler.GetFuncEnumerable().ToArray();
            CollectionAssert.AreEquivalent(new[] { "term1", "term2" }, messages);
            CollectionAssert.AreEquivalent(new[] { "id", "zero" }, functions);
        }

        [Test]
        [Parallelizable]
        public void TestConcurrencyBundler()
        {
            var bundler = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en-US"))
                .SkipResources()
                .UseConcurrent()
                .UncheckedBuild();

            Parallel.For(0, 10, i => bundler.AddResource($"term-1 = {i}", out _));
            Parallel.For(0, 10, i => bundler.AddResource($"term-2= {i}", out _));
            Parallel.For(0, 10, i => bundler.TryGetAttrMessage("term-1", null, out _, out _));
            Parallel.For(0, 10, i => bundler.AddResourceOverriding($"term-2= {i + 1}"));
            Assert.That(bundler.HasMessage("term-1"));
        }

        [Test]
        public void TestConcurrencyOption()
        {
            var bundleOpt = new FluentBundleOption()
            {
                Locales = { "en-US" },
                UseConcurrent = true,
            };
            var optBundle = FluentBundle.MakeUnchecked(bundleOpt);
            
            Parallel.For(0, 10, i => optBundle.AddResource($"term-1 = {i}", out _));
            Parallel.For(0, 10, i => optBundle.AddResource($"term-2= {i}", out _));
            Parallel.For(0, 10, i => optBundle.TryGetAttrMessage("term-1", null, out _, out _));
            Parallel.For(0, 10, i => optBundle.AddResourceOverriding($"term-2= {i + 1}"));
            Assert.That(optBundle.HasMessage("term-1"));
            
            // Frozen bundle are read only and should be thread-safe
            var frozenBundle = optBundle.ToFrozenBundle();
            Parallel.For(0, 10, i => frozenBundle.TryGetAttrMessage("term-1", null, out _, out _));
        }

        [Test]
        [Parallelizable]
        public void TestExample()
        {
            var bundler = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource("hello-user =  Hello, { $username }!")
                .UncheckedBuild();

            var message = bundler.GetAttrMessage("hello-user",  ("username", (FluentString)"Test"));
            Assert.That("Hello, Test!", Is.EqualTo(message));
        }

        [Test]
        [Parallelizable]
        public void TestFuncAddBehavior()
        {
            var bundle = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource("x = y")
                .UncheckedBuild();

            bundle.TryAddFunction("id", _idFunc);
            
            Assert.That(bundle.TryAddFunction("id", _zeroFunc), Is.False);
            Assert.Throws<ArgumentException>(() => bundle.AddFunctionUnchecked("id", _zeroFunc));
        }
        
        [Test]
        [Parallelizable]
        [TestCase("new1.attr", true)]
        [TestCase("new1", true)]
        [TestCase("new1.", false)]
        [TestCase("new2", false)]
        public void TestBehavior(string idWithAttr, bool found)
        {
            var bundle = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource(Replace2)
                .UncheckedBuild();

            Assert.That(bundle.TryGetAttrMessage(idWithAttr, null, out _, out _),
                Is.EqualTo(bundle.HasAttrMessage(idWithAttr)));
        }

        [Test]
        [Parallelizable]
        [TestCase("new1.attr", true)]
        [TestCase("new1", true)]
        [TestCase("new1.", false)]
        [TestCase("new2", false)]
        public void TestHasAttrMessage(string idWithAttr, bool found)
        {
            var bundle = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource(Replace2)
                .UncheckedBuild();

            Assert.That(bundle.TryGetAttrMessage(idWithAttr, null, out _, out _),
                Is.EqualTo(bundle.HasAttrMessage(idWithAttr)));
        }

        public static IEnumerable<TestCaseData> TestBundleErrors
        {
            get
            {
                yield return new TestCaseData("### Comment\r\nterm1")
                    .Returns(new List<ErrorSpan?>
                    {
                        new(2, 13, 18, 18, 19)
                    });
            }
        }

        [Test]
        [TestCaseSource(nameof(TestBundleErrors))]
        public List<ErrorSpan?> TestBundleErrorsSpan(string input)
        {
            var (_, error) = LinguiniBuilder.Builder().Locale("en-US")
                .AddResource(input)
                .Build();
            Debug.Assert(error != null, nameof(error) + " != null");
            Assert.That(error, Is.Not.Empty);
            return error.Select(e => e.GetSpan()).ToList();
        }

        private const string DynRef = @"
cat = {$number ->
  *[one] Cat
  [other] Cats
}
dog = {$number ->
  *[one] Dog
  [other] Dogs
}
attack-log = { $$attacker } attacked {$$defender}.
"; 
        
        
        [Test]
        [TestCase(DynRef)]
        public void TestDynamicReference(string input)
        {
            var (bundle, err) =  LinguiniBuilder.Builder(useExperimental: true).Locale("en-US")
                .AddResource(input)
                .Build();
            Assert.That(err, Is.Null);
            var args = new Dictionary<string, IFluentType>()
            {
                ["attacker"] = (FluentReference)"cat",
                ["defender"] = (FluentReference)"dog",
            };
            Assert.That(bundle.TryGetMessage("attack-log", args, out _, out var message));
            Assert.That("Cat attacked Dog.", Is.EqualTo(message));
        }

        private const string Macros = @"
-ship = Ship
    .gender = {$style ->
        *[traditional] neuter
          [chicago] feminine
    }
call-attr-no-args = {-ship.gender() ->
    *[masculine] He
      [feminine] She
      [neuter] It
}
"; 
        
        [Test]
        [Parallelizable]
        public void TestExtensionsWork()
        {
            var (bundle, err) =  LinguiniBuilder.Builder(useExperimental: true).Locale("en-US")
                .AddResource(Macros)
                .Build();
            Assert.That(err, Is.Null);
            var args = new Dictionary<string, IFluentType>
            {
                ["style"] = (FluentString)"chicago",
            };
            Assert.That(bundle.TryGetMessage("call-attr-no-args", args, out _, out var message));
            Assert.That("It", Is.EqualTo(message));
            
            // Check Frozen bundle behaves similarly
            var frozenBundle = bundle.ToFrozenBundle();
            Assert.That(frozenBundle.TryGetMessage("call-attr-no-args", args, out _, out var frozenMessage));
            Assert.That("It", Is.EqualTo(frozenMessage));
        }
        private const string DynamicSelectors = @"
-creature-fairy = fairy
-creature-elf = elf
    .StartsWith = vowel

you-see = You see { $$object.StartsWith ->
    [vowel] an { $$object }
    *[consonant] a { $$object }
}.
"; 
        
        [Test]
        [Parallelizable]
        public void TestDynamicSelectors()
        {
            var (bundle, err) = LinguiniBuilder.Builder(useExperimental: true)
                .Locale("en-US")
                .AddResource(DynamicSelectors)
                .Build();
            Assert.That(err, Is.Null);
            var args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-elf",
            };
            Assert.That(bundle.TryGetMessage("you-see", args, out _, out var message1));
            Assert.That("You see an elf.", Is.EqualTo(message1));
            args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-fairy",
            };
            Assert.That(bundle.TryGetMessage("you-see", args, out _, out var message2));
            Assert.That("You see a fairy.", Is.EqualTo(message2));
            
            // Check Frozen bundle behaves similarly
            var frozenBundle = bundle.ToFrozenBundle();
            args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-elf",
            };
            Assert.That(frozenBundle.TryGetMessage("you-see", args, out _, out var frozenMessage1));
            Assert.That("You see an elf.", Is.EqualTo(frozenMessage1));
            args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-fairy",
            };
            Assert.That(frozenBundle.TryGetMessage("you-see", args, out _, out var frozenMessage2));
            Assert.That("You see a fairy.", Is.EqualTo(frozenMessage2));
        }

        [Test]
        public void TestDeepClone()
        {
            var originalBundleOption = new FluentBundleOption
            {
                Locales = { "en-US" },
                MaxPlaceable = 123,
                UseIsolating = false,
                TransformFunc = _transform,
                FormatterFunc = _formatter,
                Functions = new Dictionary<string, ExternalFunction>()
                {
                    ["zero"] = _zeroFunc,
                    ["id"] = _idFunc,
                }
            };

            // Assume FluentBundle object has DeepClone method
            FluentBundle originalBundle = FluentBundle.MakeUnchecked(originalBundleOption);
            FluentBundle clonedBundle = originalBundle.DeepClone();

            // Assert that the original and cloned objects are not the same reference
            Assert.That(originalBundle, Is.Not.SameAs(clonedBundle));

            // Assert that the properties are copied properly
            Assert.That(originalBundle, Is.EqualTo(clonedBundle));
            
            // Assert that if original property is changed, new property isn't.
            originalBundle.AddFunctionOverriding("zero", _idFunc);
            clonedBundle.TryGetFunction("zero", out var clonedZero);
            Assert.That((FluentFunction) _zeroFunc, Is.EqualTo(clonedZero));
            originalBundle.TryGetFunction("zero", out var originalZero);
            Assert.That((FluentFunction) _idFunc, Is.EqualTo(originalZero));
        }
    }
}