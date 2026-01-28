using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace CS2_Clearance
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using Colossal;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(CS2_Clearance)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        /// <summary>
        /// Gets the mod's settings configuration.
        /// </summary>
        public Setting Settings;

        /// <summary>
        /// Gets the instance reference.
        /// </summary>
        public static Mod Instance {
            get; private set;
        }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Instance = this;

            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            Settings = new Setting(this);
            Settings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            AssetDatabase.global.LoadSettings(nameof(CS2_Clearance), Settings, new Setting(this));
            Settings.RegisterKeyBindings();

            updateSystem.UpdateAt<RenderSystem>(SystemUpdatePhase.Rendering);

#if IS_DEBUG && EXPORT_EN_US
            GenerateLanguageFile();
#endif
        }

        /// <summary>
        /// Generates an en-US localization JSON file from the current localization dictionary.
        /// Only executed in debug builds with EXPORT_EN_US compiler directive.
        /// </summary>
        private void GenerateLanguageFile() {
            var localeDict = new LocaleEN(Settings).ReadEntries(new List<IDictionaryEntryError>(), new Dictionary<string, int>())
                                                   .ToDictionary(pair => pair.Key, pair => pair.Value);
            var str = JsonConvert.SerializeObject(localeDict, Formatting.Indented);
            try {
                var path       = GetThisFilePath();
                var directory  = Path.GetDirectoryName(path);
                var exportPath = $@"{directory}/lang/en-US.json";
                File.WriteAllText(exportPath, str);
            } catch (Exception ex) {
                log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the file path of the calling source file using the CallerFilePath attribute.
        /// </summary>
        /// <param name="path">The path provided by the compiler via CallerFilePath attribute.</param>
        /// <returns>The file path of the calling code.</returns>
        private static string GetThisFilePath([CallerFilePath] string path = null) {
            return path;
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
