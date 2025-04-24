using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Builder;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using UnityEditor;
using UnityEngine;

namespace enitimeago.NonDestructiveMMD
{
    internal static class Localization
    {
        private class Locale
        {
            public string IsoCode;
            public FluentBundle Bundle;
        }

        /// <summary>
        /// Wrapper around NDMF's <see cref="SimpleError"> for using Project Fluent strings and args.
        /// This is necessary because NDMF errors at time of writing only supports basic param substitutions
        /// using <c>{0}, {1} .. {n}</c>, and isn't capable of passing through typed args.
        /// </summary>
        internal class LocalizedError : SimpleError
        {
            public override Localizer Localizer { get; }
            public override ErrorSeverity Severity { get; }
            public override string TitleKey { get; }
            public IReadOnlyDictionary<string, IFluentType> Args { get; }
            private Dictionary<string, IFluentType> _args;

            public LocalizedError(ErrorSeverity severity, string titleKey, params (string, IFluentType)[] args)
            {
                _args = new Dictionary<string, IFluentType>(args.Length);
                foreach (var (k, v) in args)
                {
                    _args.Add(k, v);
                }
                Severity = severity;
                TitleKey = titleKey;
                Localizer = new Localizer("en-us", () => _locales.Select<Locale, (string, Func<string, string>)>(
                    // TODO: support DetailsKey and HintKey so the : hack for :description and :hint isn't needed
                    locale => (locale.IsoCode, key => key.Contains(":") ? "" : locale.Bundle.GetMessage(key, null, _args))).ToList());
            }
        }

#if NDMMD_DEBUG
        private static FileSystemWatcher _fileSystemWatcher;
#endif
        // This isn't a dictionary because OrderedDictionary isn't generic ¯\_(ツ)_/¯
        // Need ordering to always render a consistently-ordered EditorGUILayout.Popup
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
            if (string.IsNullOrEmpty(localizationDir))
            {
                Debug.LogError($"Make It MMD failed to find localization files!");
                return;
            }

            foreach (var locale in _locales.Where(l => l.Bundle == null))
            {
                LoadLocale(locale, $"{localizationDir}/{locale.IsoCode}.ftl");
            }
            LanguagePrefs.RegisterLanguageChangeCallback(typeof(Localization), _ => OnLanguageChange());
            OnLanguageChange();

#if NDMMD_DEBUG
            _fileSystemWatcher = new FileSystemWatcher(localizationDir);
            _fileSystemWatcher.Filter = "*.ftl";
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += (sender, e) =>
            {
                string isoCode = Path.GetFileNameWithoutExtension(e.FullPath);
                var locale = _locales.FirstOrDefault(l => l.IsoCode == isoCode);
                if (locale != null)
                {
                    Debug.Log($"Reloading from {e.FullPath}");
                    LoadLocale(locale, e.FullPath);
                }
            };
#endif
        }

        private static void LoadLocale(Locale locale, string path)
        {
            string text = File.ReadAllText(path);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError($"Make It MMD failed to read LocalizationAsset for {locale.IsoCode}");
                return;
            }
            Debug.Log($"Make It MMD successfully read LocalizationAsset for {locale.IsoCode}");
            locale.Bundle = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo(locale.IsoCode))
                .AddResource(text)
                .UncheckedBuild();
        }

        private static void OnLanguageChange()
        {
            _currentLocaleIndex = _locales.TakeWhile(locale => locale.IsoCode != LanguagePrefs.Language.ToLower()).Count();
#if NDMMD_DEBUG
            Debug.Log($"currentLocale {_currentLocaleIndex}");
#endif
        }

        public static void DrawLanguagePicker()
        {
            int newLocaleIndex = EditorGUILayout.Popup(
                _currentLocaleIndex,
                _locales
                    .Select(locale => locale.Bundle.GetMessage($"locale-{locale.IsoCode}"))
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
    }
}
