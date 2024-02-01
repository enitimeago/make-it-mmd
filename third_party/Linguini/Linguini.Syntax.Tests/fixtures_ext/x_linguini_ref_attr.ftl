-creature-fairy = fairy
-creature-elf = elf
    .StartsWith = vowel

you-see = You see { $$object.StartsWith ->
    [vowel] an { $$object }
    *[consonant] a { $$object }
}.
