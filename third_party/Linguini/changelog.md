version 0.1.0 (yanked)
======================
- Initial release 
    
version 0.1.1 
=============

- Move PluralRules to netstandard 2.1
- Fix .ftl tests treated by Git as binary
    
version 0.1.2
=============

- Fix escaping 
- Add 3rd party licenses
- Nit - remove annoying warning
    
version 0.1.3
=============

- Fix row counting
- Fix error reports
- Make builds reproducible
    
version 0.1.4
=============

- Fix issue with number formatting using `CultureInfo.CurrentCulture` instead of `CultureInfo.InvariantCulture`. 
 Big thanks to @Mailaender

version 0.2.0
=============

- Linguini supports netstandard2.1 for use in Mono (this changes no API but introduces 
polyfills for netstandard2.1)
- Fix some stale dependencies
    
version 0.2.1
=============

- Fix issue with `System.Text.Json` leaking analyzers, causing issues on Mono
- Minor tweak of versions of `System.Text.Json` and enabling tests

version 0.2.2
=======

- Fix issue with `System.Text.Json` causing problems with Mono see https://github.com/mono/mono/issues/15833
> Chatted with @danroth27 and he mentioned that this could be due to mono lagging in netstandard2.1 support.
 I can imagine that if they were missing a typeforward in netstandard.dll for IAsyncDisposable this could happen.
 
 
version 0.3.0
========

- Extracted `Linguini.Syntax.Serializers` to a separate package
- Changed the way `InsertBehavior` works
- Add support for net461 @mtkennerly

version 0.3.1
========

- Fixed error in `Linguini.Bundle` that prevented term and scope arguments from coexisting. @adcdefg30

version 0.3.2
========

- Adds `LinguiniBundle.HasAttrMessage` method
- Obsoletes `LinguiniBundle.TryGetAttrMsg` for `LinguiniBundle.TryGetAttrMessage`
- Obsoletes `LinguiniBundle.TryGetMsg` for `LinguiniBundle.TryGetMessage`

version 0.4.0
========

- Changes default on `LinguiniBundle.SetIsolating` from `true` to `false`
- Adds method `GetAttrMessage(string msgWithAttr, params (string, IFluentType)[] args)` for ease of use.
- Removes `enum InsertBehavior` in favor of three separate functions (`TryAddFunction`, `AddFunctionOverriding`, `AddFunctionUnchecked`)
- Removes previously obsolete methods.

version 0.5.0
========

- Improves parsing performance by eliminating bounds check on `ReadOnlySpan<char>` for `char` @RoosterDragon
  Breaking changes:
  - ZeroCopyUtil
    - `TryReadCharSpan` replaced with `TryReadChar`
    - Methods `IsIdentifier`/`IsNumberStart`/`IsAsciiDigit`/`IsAsciiUppercase`/`IsAsciiHexdigit`/`IsAsciiAlphabetic`
      take char rather than `ReadOnlySpan<char>`
    - `EqualSpans` method removed
  - ZeroCopyReader
    - method signature `ReadOnlySpan<char> PeekCharSpan(int offset = 0)` changed to `char? PeekChar(int offset = 0)`
    - method `SeekEol` added.
    - methods `TryPeekChar`, `TryPeekCharAt`,`CurrentChar`, and  `IndexOfAnyChar` added.
  - ParserError
    - factory method for `ExpectedTokens` arguments changed
  - LinguiniParser
    - changed to use new ZeroCopyUtil internally.
  Non-breaking changes:
  - Fluent bundle private bundle method separated into `AddEntry` and `AddEntryOverriding`

version 0.6.0
========
- Fixes errors when reading line numbers due to interaction
  with ZeroCopy Parser (thanks to @PJB3005)
- Moves project to minimal dotnet version to 6

version 0.6.1
========
- Fixes errors when reading an empty line on Windows (reported by @JosefNemec)

version 0.7.0
========
- Experimental features when `UseExperimental` flag is true:
  - Dynamic Reference - ability to reference terms/message using `$$term_ref`.
    After defining it in file like so:
      ```fluent
      # example.ftl
      cat = {$number ->
          *[one] Cat
          [other] Cats
      }
      dog = {$number ->
          *[one] Dog
          [other] Dogs
      }
      attack-log = { $$attacker(number: $atk_num) } attacked {$$defender(number: $def_num)}.
      ```
      It can be called like following:
      ```csharp
      var args = new Dictionary<string, IFluentType>
      {
          ["attacker"] = (FluentReference)"cat",
          ["defender"] = (FluentReference)"dog",
      };
      Assert.True(bundle.TryGetMessage("attack-log", args, out _, out var message));
      Assert.AreEqual("Cat attacked Dog.", message);
      ```
  - Dynamic Reference attributes - You can call an attribute of a dynamic reference. It will be resolved at runtime, so
    make sure your term/message has the associated attribute.
    Example:
    ```fluent
    # dyn_attr.ftl
    -creature-elf = elf
      .StartsWith = vowel

    you-see = You see { $$object.StartsWith ->
      [vowel] an { $$object }
      *[consonant] a { $$object }
    }.
    ```
    ```csharp
    var args = new Dictionary<string, IFluentType>
    {
      ["object"] = (FluentReference)"creature-elf",
    };
    Assert.True(bundle.TryGetMessage("you-see", args, out _, out var message));
    Assert.AreEqual("You see an elf.", message);
    ```
    
  - Term passing - experimental feature allows users to override term arguments.
    ```fluent
    # ship_gender.ftl
    -ship = Ship
        .gender =  { $style ->
            *[traditional] neuter
            [chicago] feminine
        }
    ship-gender = { -ship.gender(style: $style) ->
        *[masculine] He
        [feminine] She
        [neuter] It
    }
    ```
    Usually when style isn't passed, it would to default `-ship.gender()` i.e. `neuter`, which would set `ship-gender` selector to neuter i.e. `It`.
    In above example if we set style variable to `chicago`, `-ship.gender()` it would evaluate `feminine`, so `ship-gender` would would evaluate to `She`.

version 0.8.0
========

## What's Changed
* Remove `net5` or greater by 
* Move to `net6` and/or `net8`.
* Move to `NUnit 4.0.1`
* Fix issue with Windows test not being fully run
* `[Breaking change]` Refactor to use consistent naming
* Remove unnecessary `ContainsKey()` calls and split dictionaries by @ElectroJr

* `[Breaking change]` Make `FluentBundle` abstract and do some API refactoring
  * To fully resolve the issue reported by @ElectroJr  a common base for bundle is added `FluentBundle`.
  * Extract read-only methods to `IReadBundle`
  * Adds `FrozenBundle` as a read-only version of `FluentBundle`
  * Most fields are read only.
* `[Major change]` Refactor `Ast*` API 
  * Adds builder for most `Ast*` types (`AstMessage`, `AstTerm` and `Junk`). E.g.
    ```csharp
    SelectExpressionBuilder(new VariableReference("x"))
       .AddVariant("one", new PatternBuilder("select 1"))
       .AddVariant("other", new PatternBuilder("select other"))
       .SetDefault(1)
       .Build();
    ```
  * Adds `Equals` to most `Linguini.Syntax.Ast` types.
  * All serializers now have a `Read` method implementation.

version 0.8.1
========

## What's changed 
* Add `AddResourceOverriding(Resource res)`.

version 0.8.2
========

## What's changed
* `TryGetMessage` returns error if no message was found.
* Adds methods `FormatPatternErrRef`, `TryGetMessageErrRef`, `TryGetAttrMessageErrRef`, `TryGetMessageErrRef` in `IReadBundle` and
  associated classes.