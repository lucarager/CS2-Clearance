using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace CS2_Clearance
{
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
