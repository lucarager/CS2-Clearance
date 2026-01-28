using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

namespace CS2_Clearance
{
    [FileLocation(nameof(CS2_Clearance))]
    [SettingsUIGroupOrder(kGroup)]
    [SettingsUIShowGroupName(kGroup)]
    public class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kGroup   = "Options";

        public Setting(IMod mod) : base(mod)
        {

        }

        [SettingsUISection(kSection, kGroup)]
        [SettingsUIKeyboardBinding(BindingKeyboard.C, "ToggleOverlay", alt: true)]
        public ProxyBinding ToggleOverlay {
            get; set;
        }

        [SettingsUISlider(min = 0, max = 10, step = 0.1f, scalarMultiplier = 1, unit = Unit.kLength)]
        [SettingsUISection(kSection, kGroup)]
        public float RoadHeight {
            get;
            set;
        } = 3.8f;

        [SettingsUISlider(min = 0, max = 10, step = 0.1f, scalarMultiplier = 1, unit = Unit.kLength)]
        [SettingsUISection(kSection, kGroup)]
        public float TrainTrackHeight {
            get;
            set;
        } = 4f;

        [SettingsUISlider(min = 0, max = 10, step = 0.1f, scalarMultiplier = 1, unit = Unit.kLength)]
        [SettingsUISection(kSection, kGroup)]
        public float TramTrackHeight {
            get; set;
        } = 4f;

        [SettingsUISlider(min = 0, max = 10, step = 0.1f, scalarMultiplier = 1, unit = Unit.kLength)]
        [SettingsUISection(kSection, kGroup)]
        public float SubwayTrackHeight {
            get; set;
        } = 4f;

        [SettingsUISlider(min = 0, max = 10, step = 0.1f, scalarMultiplier = 1, unit = Unit.kLength)]
        [SettingsUISection(kSection, kGroup)]
        public float WaterwayHeight {
            get; set;
        } = 4f;

        public override void SetDefaults() {
            RoadHeight = 3.8f;
            TrainTrackHeight = 3.8f;
            TramTrackHeight = 3.8f;
            SubwayTrackHeight = 3.8f;
            WaterwayHeight = 3.8f;
        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Clearance Helper" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kGroup), "Sliders" },

                { m_Setting.GetBindingKeyLocaleID(nameof(Setting.ToggleOverlay)), "Toggle Clearance Overlay" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ToggleOverlay)), "Toggle Clearance Overlay" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ToggleOverlay)), "Shortcut to toggle the rendering of the clearance overlay on or off." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RoadHeight)), "Road Height" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TrainTrackHeight)), "Train Track Height" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TramTrackHeight)), "Tram Track Height" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SubwayTrackHeight)), "Subway Track Height" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WaterwayHeight)), "Waterway Height" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RoadHeight)), """
                                                                               Sets the height of the overlay above roads. 
                                                                               
                                                                               Common heights: 
                                                                               - Truck: 3.8m 
                                                                               - Dump truck: 3.6m 
                                                                               - Average car: 1.5m
                                                                               """},
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.TrainTrackHeight)), """
                                                                               Sets the height of the overlay above train tracks. 
                                                                               
                                                                               Common heights: 
                                                                               - Truck: 3.8m 
                                                                               - Dump truck: 3.6m 
                                                                               - Average car: 1.5m
                                                                               """ },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.TramTrackHeight)), """
                                                                               Sets the height of the overlay above tram tracks. 
                                                                               
                                                                               Common heights: 
                                                                               - Truck: 3.8m 
                                                                               - Dump truck: 3.6m 
                                                                               - Average car: 1.5m
                                                                               """},
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.SubwayTrackHeight)), """
                                                                               Sets the height of the overlay above subway tracks 
                                                                               
                                                                               Common heights: 
                                                                               - Truck: 3.8m 
                                                                               - Dump truck: 3.6m 
                                                                               - Average car: 1.5m
                                                                               """},
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WaterwayHeight)), """
                                                                               Sets the height of the overlay above waterways. 
                                                                               
                                                                               Common heights: 
                                                                               - Truck: 3.8m 
                                                                               - Dump truck: 3.6m 
                                                                               - Average car: 1.5m
                                                                               """},
            };
        }

        public void Unload()
        {

        }
    }
}
