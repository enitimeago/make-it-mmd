using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nadena.dev.ndmf.localization;
using UnityEditor;
using UnityEngine;

namespace enitimeago.NonDestructiveMMD
{
    [InitializeOnLoad]
    internal static class Localization
    {
        private static List<LocalizationAsset> _localizationAssets;
        private static string[] _localeCodes;
        private static string[] _localeNames;
        private static int _currentLocaleIndex;
        public static Localizer Localizer { get; private set; }

        static Localization()
        {
            _localizationAssets = Directory.GetFiles(AssetDatabase.GUIDToAssetPath("85d7850cb8f79884ab5fe8d20e382df5"))
                .Select(AssetDatabase.LoadAssetAtPath<LocalizationAsset>)
                .Where(asset => asset != null)
                .OrderBy(asset => asset.localeIsoCode)
                .ToList();
            _localeCodes = _localizationAssets
                .Select(asset => asset.localeIsoCode.ToLower())
                .ToArray();
            _localeNames = _localizationAssets
                .Select(asset => asset.GetLocalizedString($"locale:{asset.localeIsoCode}"))
                .ToArray();
            Localizer = new Localizer("en-US", () => _localizationAssets);
            OnLanguageChange();
            LanguagePrefs.RegisterLanguageChangeCallback(typeof(Localization), _ => OnLanguageChange());
        }

        public static void DrawLanguagePicker()
        {
            int newLocaleIndex = EditorGUILayout.Popup(_currentLocaleIndex, _localeNames);
            if (newLocaleIndex != _currentLocaleIndex)
            {
                LanguagePrefs.Language = _localeCodes[newLocaleIndex];
                _currentLocaleIndex = newLocaleIndex;
            }
        }

        public static string Tr(string key)
        {
            return Tr(key, key);
        }

        public static string Tr(string key, string fallback)
        {
            if (Localizer.TryGetLocalizedString(key, out var value))
            {
                return value;
            }
            return fallback;
        }

        private static void OnLanguageChange()
        {
            _currentLocaleIndex = Array.IndexOf(_localeCodes, LanguagePrefs.Language);
        }
    }
}
