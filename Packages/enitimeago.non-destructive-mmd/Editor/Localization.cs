using System.Linq;
using nadena.dev.ndmf.localization;
using UnityEditor;
using UnityEngine;
using Linguini.Bundle.Builder;
using System.Globalization;
using Linguini.Bundle;
using System.IO;
using Linguini.Shared.Types.Bundle;
using System.Collections.Generic;

namespace enitimeago.NonDestructiveMMD
{
    internal static class Localization
    {
        private class Locale
        {
            public string IsoCode;
            public FluentBundle Bundle;
        }

        private static Locale[] _locales = new Locale[]
        {
            new Locale { IsoCode = "en-us" },
            new Locale { IsoCode = "ja-jp" },
            new Locale { IsoCode = "zh-hant" },
        };
        private static int _currentLocaleIndex = 0;

        static Localization()
        {
            string localizationDir = AssetDatabase.GUIDToAssetPath("28b1340a11795724997e5bcda57804da");
            foreach (var locale in _locales)
            {
                if (locale.Bundle == null)
                {
                    string text = File.ReadAllText($"{localizationDir}/{locale.IsoCode}.ftl");
                    if (string.IsNullOrEmpty(text))
                    {
                        Debug.LogError($"Make It MMD failed to read LocalizationAsset for {locale.IsoCode}");
                        continue;
                    }
                    Debug.Log($"Make It MMD successfully read LocalizationAsset for {locale.IsoCode}");
                    locale.Bundle = LinguiniBuilder.Builder()
                        .CultureInfo(new CultureInfo(locale.IsoCode))
                        .AddResource(text)
                        .UncheckedBuild();
                }
            }
            LanguagePrefs.RegisterLanguageChangeCallback(typeof(Localization), _ => OnLanguageChange());
            OnLanguageChange();
        }

        public static void DrawLanguagePicker()
        {
            int newLocaleIndex = EditorGUILayout.Popup(
                _currentLocaleIndex,
                _locales
                    .Select(locale => /*locale.Asset?.GetLocalizedString($"locale:{locale.IsoCode}") ??*/ locale.IsoCode)
                    .ToArray());
            if (newLocaleIndex != _currentLocaleIndex)
            {
                LanguagePrefs.Language = _locales[newLocaleIndex].IsoCode;
                _currentLocaleIndex = newLocaleIndex;
            }
        }

        public static string Tr(string key, params (string, IFluentType)[] args)
        {
            return Tr(key, key, args);
        }

        public static string Tr(string key, string fallback, params (string, IFluentType)[] args)
        {
            key = key.Replace(':', '-');
            var dictionary = new Dictionary<string, IFluentType>(args.Length);
            foreach (var (k, v) in args)
            {
                dictionary.Add(k, v);
            }
            if (_locales[_currentLocaleIndex].Bundle.TryGetAttrMessage(key, dictionary, out _, out var value))
            {
                return value;
            }
            return fallback;
        }

        private static void OnLanguageChange()
        {
            _currentLocaleIndex = _locales.TakeWhile(locale => locale.IsoCode != LanguagePrefs.Language.ToLower()).Count();
            Debug.Log($"currentLocale {_currentLocaleIndex}");
        }
    }
}
