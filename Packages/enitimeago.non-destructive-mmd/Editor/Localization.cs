using System.Linq;
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
            public LocalizationAsset Asset;
        }

        public static Localizer Localizer { get; private set; }
        private static Locale[] _locales = new Locale[]
        {
            new Locale { IsoCode = "en-us" },
            new Locale { IsoCode = "ja-jp" },
            new Locale { IsoCode = "zh-hant" },
        };
        private static int _currentLocaleIndex = 0;

        static Localization()
        {
            Localizer = new Localizer("en-us", () =>
                _locales.Select(locale =>
                {
                    if (locale.Asset == null)
                    {
                        string localizationDir = AssetDatabase.GUIDToAssetPath("28b1340a11795724997e5bcda57804da");
                        locale.Asset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>($"{localizationDir}/{locale.IsoCode}.po");
                        if (locale.Asset != null)
                        {
                            Debug.Log($"Make It MMD successfully loaded LocalizationAsset for {locale.IsoCode}");
                        }
                        else
                        {
                            Debug.LogError($"Make It MMD failed to load LocalizationAsset for {locale.IsoCode}");
                        }
                    }
                    return locale.Asset;
                })
                .Where(asset => asset != null)
                .ToList());
            LanguagePrefs.RegisterLanguageChangeCallback(typeof(Localization), _ => OnLanguageChange());
            OnLanguageChange();
        }

        public static void DrawLanguagePicker()
        {
            int newLocaleIndex = EditorGUILayout.Popup(
                _currentLocaleIndex,
                _locales
                    .Select(locale => locale.Asset?.GetLocalizedString($"locale:{locale.IsoCode}") ?? locale.IsoCode)
                    .ToArray());
            if (newLocaleIndex != _currentLocaleIndex)
            {
                LanguagePrefs.Language = _locales[newLocaleIndex].IsoCode;
                _currentLocaleIndex = newLocaleIndex;
            }
        }

        public static string Tr(string key)
        {
            return Tr(key, key);
        }

        public static string Tr(string key, string fallback)
        {
            if (Localizer != null && Localizer.TryGetLocalizedString(key, out var value))
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
