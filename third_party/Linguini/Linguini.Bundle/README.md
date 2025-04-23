Linguini Bundle
===

Construct a bundle
---

Linguini Bundle is the main API for accessing Fluent templates. It takes input, locale and useful localization
functions.

First way is to construct is using a fluent/chainable API to create a bundle.

```csharp
var bundle = LinguiniBuilder.Builder()
    .CultureInfo(new CultureInfo("en"))
    .AddResource("loc = Localization value")
    .AddFunction("idfunc", (args, _) => args[0]);
    .UncheckedBuild();
```

Here we set `loc` value for English language and pass an `idfunc` that just returns the first argument passed.
By using `UncheckedBuild()` we get a guaranteed bundle, but any error will become an Exception. If we used `Build()`
the result would be a tuple of type `(FluentBundle, List<FluentError>)`.

Another way to construct a bundle is to provide a `FluentBundleOption`.

```csharp
var defaultBundle = new FluentBundleOption
{
    CultureInfo = new CultureInfo("en"),
    Functions =
    {
        ["idfunc"] = (args, _) => args[0]
    },
    // Note no resource added!
};
var bundle = FluentBundle.FromBundleOptions(defaultBundleOpt);
bundle.AddResource("loc = Localization value", out _);
```

Both methods are nearly the same, but `FluentBundleOption` doesn't provide Resources.
They can be added vie `BundleOption`s `AddResource`.