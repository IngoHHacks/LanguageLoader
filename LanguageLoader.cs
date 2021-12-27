using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Localization;

namespace LanguageLoader
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "IngoH.inscryption.LanguageLoader";
        private const string PluginName = "LanguageLoader";
        private const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log;

        public static Plugin p;

		public static List<LanguageInfo> newLangs;
		public static bool langsLoaded;

		public static int origLangCount;

		private void Awake()
        {
			origLangCount = (int) Language.NUM_LANGUAGES;
			newLangs = new List<LanguageInfo>();
			langsLoaded = false;
            p = this;
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
        }

		// TODO: Make transpiler
        [HarmonyPatch(typeof(Localization), "ReadCSVFileIntoTranslationData", typeof(Language))]
        public class LanguagePatch
        {
            public static bool Prefix(Language language)
            {
				if (!langsLoaded)
				{
					LoadLanguages();
					langsLoaded = true;
				}
				if (language >= Language.NUM_LANGUAGES)
                {
					if ((int) language - origLangCount >= newLangs.Count)
                    {
						return false;
                    }
					TextAsset textAsset = new TextAsset(newLangs[(int) language - origLangCount].translations);
					try
					{
						List<Translation> translations = Translations;
						StringReader aReader = new StringReader(textAsset.text);
						CSVParser cSVParser = new CSVParser(aReader, ';');
						List<string> list = new List<string>();
						int num = 0;
						while (cSVParser.NextLine(list))
						{
							if (list.Count < 3)
							{
								continue;
							}
							string text = list[0];
							bool num2 = text.EndsWith("_F");
							Translation translation;
							if (num2)
							{
								translation = translations[num - 1];
							}
							else if (num < translations.Count)
							{
								translation = translations[num];
							}
							else
							{
								translation = new Translation();
								translations.Add(translation);
							}
							if (string.IsNullOrEmpty(translation.id))
							{
								translation.id = text;
								translation.englishString = list[1];
								translation.englishStringFormatted = list[1].Replace("\n", "").Replace(" ", "").Replace("\"", "").ToLowerInvariant();
							}
							if (num2)
							{
								if (!string.IsNullOrEmpty(list[2]) && translation.values[language] != list[2])
								{
									translation.femaleGenderValues.Add(language, list[2]);
								}
								continue;
							}
							if (text != translation.id)
							{
								Debug.Log("Mismatched Translation! Starting at: " + translation.id);
							}
							translation.values.Add(language, list[2]);
							num++;
						}
					}
					catch
					{
						Debug.Log("No Translation File for " + language);
					}
					return false;
				}
                else
                {
                    return true;
                }
            }
        }

		[HarmonyPatch(typeof(OptionsUI), "InitializeLanguageField")]
		public class OptionsPatch
		{
            public static bool Prefix(OptionsUI __instance, IncrementalField ___languageField)
			{
				if (!langsLoaded)
                {
					LoadLanguages();
					langsLoaded = true;
                }
				___languageField.AssignTextItems(new List<string>(LocalizedLanguageNames.NAMES.AddRangeToArray(newLangs.Select(l => l.name).ToArray())));
				___languageField.ShowValue((int) CurrentLanguage, immediate: true);
				return false;
			}
        }

		[HarmonyPatch(typeof(OptionsUI), "OnLanguageChanged")]
		public class LanguageButtonPatch
		{
			public static bool Prefix(OptionsUI __instance, int newValue, GenericUIButton ___setLanguageButton, IncrementalField ___languageField, PixelText ___languageButtonText)
			{
				if (newValue >= (int) Language.NUM_LANGUAGES)
				{
					LanguageInfo lang = newLangs[newValue - (int) Language.NUM_LANGUAGES];
					___setLanguageButton.gameObject.SetActive(newValue != (int) CurrentLanguage);
					LocalizeFont[] componentsInChildren = ___languageField.GetComponentsInChildren<LocalizeFont>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].ChangeFontForLanguage((Language) newValue);
					}
					componentsInChildren = ___languageButtonText.GetComponentsInChildren<LocalizeFont>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].ChangeFontForLanguage((Language) newValue);
					}
					Language old = CurrentLanguage;
					CurrentLanguage = (Language) newValue;
					string confirmText = Translate("REBOOT WITH ENGLISH");
					CurrentLanguage = old;
					___languageButtonText.SetText(confirmText);                
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(StartScreenController), "StartSequence")]
		public class StartScreenPatch
		{
			public static bool Prefix(StartScreenController __instance, Animator ___titleAnimation)
			{
				if (!langsLoaded)
				{
					LoadLanguages();
					langsLoaded = true;
				}
				if ((int) CurrentLanguage >= (int) Language.NUM_LANGUAGES && newLangs[(int) CurrentLanguage - origLangCount].special.Contains("CustomTitle1")) {
					SpriteRenderer[] srs = ___titleAnimation.gameObject.GetComponentsInChildren<SpriteRenderer>();
					byte[] imgBytes = File.ReadAllBytes(Path.Combine(p.Info.Location.Replace("LanguageLoader.dll", ""), "Artwork/inscryption_title_hide.png"));
					Texture2D rep = new Texture2D(2, 2);
					rep.LoadImage(imgBytes);
					int i = 0;
					foreach (SpriteRenderer sr in srs)
					{
						if (i >= 5 && i <= 9)
						{
							sr.transform.Translate(new Vector2(0.13f, 0));
						}
						else if (i >= 11 && i <= 15)
						{
							sr.transform.Translate(new Vector2(-0.13f, 0));
						}
						if (sr.sprite.texture.name == "startscreen_title")
						{
							sr.sprite = Sprite.Create(rep, sr.sprite.rect, new Vector2(0.5f, 0.5f));
							sr.sprite.name = sr.name + "_sprite";
							sr.material.mainTexture = rep;
						}
						i++;
					}
				}
				return true;
			}
		}

		public static void LoadLanguages()
		{
			IEnumerable<string> files = Directory.EnumerateFiles(Paths.PluginPath, "*.ilang", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				string filename = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1);
				string name = filename.Substring(0, filename.Length - 4);
				string all = File.ReadAllText(file);
				string meta = "";
				string translations;
				if (all.Contains("#ENDMETA#"))
                {
					string[] arr = all.Split(new string[] { "#ENDMETA#" }, 2, System.StringSplitOptions.None);
					meta = arr[0];
					translations = arr[1];
				}
				else
                {
					translations = all;
                }
				newLangs.Add(new LanguageInfo(name, meta, translations.Trim()));
			}
			Log.LogInfo($"Loaded {files.Count()} languages");
		}

		public static void ExportLanguage(Language language)
        {
			TextAsset textAsset = Resources.Load("Data/Localization/Translations/" + language) as TextAsset;
			File.WriteAllText(Path.Combine(p.Info.Location.Replace("LanguageLoader.dll", ""), "Translations", language.ToString() + ".ilang"), textAsset.text);
		}
	}
}
