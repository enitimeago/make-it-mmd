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
            Debug.Log($"currentLocale {_currentLocaleIndex}");
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
    }
}
