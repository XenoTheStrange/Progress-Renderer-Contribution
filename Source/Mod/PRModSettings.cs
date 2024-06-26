﻿using ProgressRenderer.Source.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ProgressRenderer
{

    public class PRModSettings : ModSettings
    {
        private static RenderFeedback DefaultRenderFeedback = RenderFeedback.Window;
        private static bool DefaultRenderNonPlayerHomes = false;
        private static bool DefaultRenderDesignations = false;
        private static bool DefaultRenderThingIcons = false;
        private static bool DefaultRenderGameConditions = true;
        private static bool DefaultRenderWeather = true;
        private static bool DefaultRenderZones = true;
        private static bool DefaultRenderOverlays = false;
        private static int DefaultSmoothRenderAreaSteps = 0;
        private static int DefaultInterval = 24;
        private static int DefaultTimeOfDay = 8;
        private static EncodingType DefaultEncoding = EncodingType.UnityJPG;
        
        private static int DefaultJPGQuality = 93;
        private static int DefaultpixelsPerCell = 32;
        private static bool DefaultScaleOutputImage = false;
        private static int DefaultOutputImageFixedHeight = 1080;
        private static bool DefaultCreateSubdirs = false;
        private static FileNamePattern DefaultFileNamePattern = FileNamePattern.DateTime;
        private static bool DefaultJPGQualityInitialize = false;
        public static RenderFeedback renderFeedback = DefaultRenderFeedback;
        public static bool renderDesignations = DefaultRenderDesignations;
        public static bool renderThingIcons = DefaultRenderThingIcons;
        public static bool renderGameConditions = DefaultRenderGameConditions;
        public static bool renderWeather = DefaultRenderWeather;
        public static bool renderZones = DefaultRenderZones;
        public static bool renderOverlays = DefaultRenderOverlays;

        public static int smoothRenderAreaSteps = DefaultSmoothRenderAreaSteps;
        private static int whichInterval = RenderIntervalHelper.Intervals.IndexOf(DefaultInterval);
        public static int timeOfDay = DefaultTimeOfDay;
        public static EncodingType encoding = DefaultEncoding;
               
        public static int JPGQuality = DefaultJPGQuality;
        public static int pixelsPerCell = DefaultpixelsPerCell;
        public static bool JPGQualityInitialize = DefaultJPGQualityInitialize;
        public static bool scaleOutputImage = DefaultScaleOutputImage;
        public static int outputImageFixedHeight = DefaultOutputImageFixedHeight;
        public static string exportPath;
        public static bool createSubdirs = DefaultCreateSubdirs;
        public static bool useMapNameInstead = false; 
        public static FileNamePattern fileNamePattern = DefaultFileNamePattern;

        private static string outputImageFixedHeightBuffer;

        public static bool DoMigrations { get; internal set; } = true;
        public static bool migratedOutputImageSettings = false;
        public static bool migratedInterval = false;

        public static bool renderNonPlayerHomes = false;

        public PRModSettings() : base()
        {
            if (exportPath.NullOrEmpty())
            {
                exportPath = DesktopPath;
            }
        }

        public void DoWindowContents(Rect settingsRect)
        {
            if (DoMigrations)
            {
                if (!migratedOutputImageSettings)
                {
                    //Yes, I know for the people who used to use 1080 as scaling and have upgraded this will turn off scaling for them.
                    //Unfortunately I don't think there's a better way to handle this.
                    scaleOutputImage = outputImageFixedHeight > 0 && outputImageFixedHeight != DefaultOutputImageFixedHeight;
                    if (!scaleOutputImage) outputImageFixedHeight = DefaultOutputImageFixedHeight;
                    migratedOutputImageSettings = true;
                    Log.Warning("Migrated output image settings");
                }
                if (!migratedInterval)
                {
                    whichInterval = RenderIntervalHelper.Intervals.IndexOf(interval);
                    if (whichInterval < 0) whichInterval = RenderIntervalHelper.Intervals.IndexOf(DefaultInterval);
                    migratedInterval = true;
                    Log.Warning("Migrated interval settings");
                }
            }

            var ls = new Listing_Standard();
            var leftHalf = new Rect(settingsRect.x, settingsRect.y, settingsRect.width / 2 - 12f, settingsRect.height);
            var rightHalf = new Rect(settingsRect.x + settingsRect.width / 2 + 12f, settingsRect.y, settingsRect.width / 2 - 12f, settingsRect.height);

            ls.Begin(leftHalf);

            // Left half (general settings)
            ls.CheckboxLabeled("LPR_SettingsEnabledLabel".Translate(), ref GameComponentProgressManager.enabled, "LPR_SettingsEnabledDescription".Translate());
            var backupAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (ls.ButtonTextLabeled("LPR_SettingsRenderFeedbackLabel".Translate(), ("LPR_RenderFeedback_" + renderFeedback).Translate()))
            {
                var menuEntries = new List<FloatMenuOption>();
                var feedbackTypes = (RenderFeedback[])Enum.GetValues(typeof(RenderFeedback));
                foreach (var type in feedbackTypes)
                {
                    menuEntries.Add(new FloatMenuOption(("LPR_RenderFeedback_" + EnumUtils.ToFriendlyString(type)).Translate(), delegate
                    {
                        renderFeedback = type;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(menuEntries));
            }
            Text.Anchor = backupAnchor;

            ls.Gap();
            ls.Label("LPR_SettingsRenderSettingsLabel".Translate(), -1, "LPR_SettingsRenderSettingsDescription".Translate());
            ls.GapLine();
            ls.CheckboxLabeled("LPR_SettingsRenderDesignationsLabel".Translate(), ref renderDesignations, "LPR_SettingsRenderDesignationsDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsRenderThingIconsLabel".Translate(), ref renderThingIcons, "LPR_SettingsRenderThingIconsDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsRenderGameConditionsLabel".Translate(), ref renderGameConditions, "LPR_SettingsRenderGameConditionsDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsRenderWeatherLabel".Translate(), ref renderWeather, "LPR_SettingsRenderWeatherDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsRenderZonesLabel".Translate(), ref renderZones, "LPR_SettingsRenderZonesDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsRenderOverlaysLabel".Translate(), ref renderOverlays, "LPR_SettingsRenderOverlaysDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsRenderNonPlayerHomes".Translate(), ref renderNonPlayerHomes, "LPR_SettingsRenderNonPlayerHomesDescription".Translate());
            ls.GapLine();

            ls.Gap();
            ls.Label("LPR_SettingsSmoothRenderAreaStepsLabel".Translate() + smoothRenderAreaSteps.ToString(": #0"), -1, "LPR_SettingsSmoothRenderAreaStepsDescription".Translate());
            smoothRenderAreaSteps = (int)ls.Slider(smoothRenderAreaSteps, 0, 30);

            ls.Label($"{"LPR_SettingsIntervalLabel".Translate()} {RenderIntervalHelper.GetLabel(interval)}", -1, "LPR_SettingsIntervalDescription".Translate());
            whichInterval = (int)ls.Slider(whichInterval, 0, RenderIntervalHelper.Intervals.Count - 1);
            ls.Label("LPR_SettingsTimeOfDayLabel".Translate() + timeOfDay.ToString(" 00H"), -1, "LPR_SettingsTimeOfDayDescription".Translate());
            timeOfDay = (int)ls.Slider(timeOfDay, 0, 23);

            ls.End();

            // Right half (file settings)
            ls.Begin(rightHalf);

            backupAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;


            if (ls.ButtonTextLabeled("LPR_SettingsEncodingLabel".Translate(), ("LPR_ImgEncoding_" + EnumUtils.ToFriendlyString(encoding)).Translate()))
            {
                var menuEntries = new List<FloatMenuOption>();
                var encodingTypes = (EncodingType[])Enum.GetValues(typeof(EncodingType));
                foreach (var encodingType in encodingTypes)
                {
                    menuEntries.Add(new FloatMenuOption(("LPR_ImgEncoding_" + EnumUtils.ToFriendlyString(encodingType)).Translate(), delegate
                    {
                        encoding = encodingType;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(menuEntries));
            }
            Text.Anchor = backupAnchor;

            if (encoding == EncodingType.UnityJPG)
            {
                if (ls.ButtonTextLabeled("LPR_SettingsJPGQualityAdjustment".Translate(), ("LPR_JPGQualityAdjustment_" + EnumUtils.ToFriendlyString(GameComponentProgressManager.qualityAdjustment)).Translate()))
                {
                    var menuEntries = new List<FloatMenuOption>();
                    var JPGQualityAdjustmentSettings = (JPGQualityAdjustmentSetting[])Enum.GetValues(typeof(JPGQualityAdjustmentSetting));
                    foreach (var JPGQualityAdjustmentSetting in JPGQualityAdjustmentSettings)
                    {
                        menuEntries.Add(new FloatMenuOption(("LPR_JPGQualityAdjustment_" + EnumUtils.ToFriendlyString(JPGQualityAdjustmentSetting)).Translate(), delegate
                        {
                            GameComponentProgressManager.qualityAdjustment = JPGQualityAdjustmentSetting;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(menuEntries));
                }
                Text.Anchor = backupAnchor;

                if (GameComponentProgressManager.qualityAdjustment == JPGQualityAdjustmentSetting.Manual)
                {
                    ls.Label("LPR_JPGQualityLabel".Translate() + JPGQuality.ToString(": ##0") + "%", -1, "LPR_JPGQualityDescription".Translate());
                    JPGQuality = (int)ls.Slider(JPGQuality, 1, 100);
                    ls.Label("LPR_SettingspixelsPerCellLabel".Translate() + pixelsPerCell.ToString(": ##0 ppc"), -1, "LPR_SettingspixelsPerCellDescription".Translate());
                    pixelsPerCell = (int)ls.Slider(pixelsPerCell, 1, 64);
                }
                else
                {
                    ls.Label("LPR_RenderSizeLabel".Translate() + GameComponentProgressManager.renderSize.ToString(": ##0") + "MB (Current JPG quality" + GameComponentProgressManager.JPGQuality_WORLD.ToString(": ##0)"), -1, "LPR_RenderSizeDescription".Translate());
                    GameComponentProgressManager.renderSize = (int)ls.Slider(GameComponentProgressManager.renderSize, 5, 30);
                    ls.Label("LPR_SettingspixelsPerCell_WORLDLabel".Translate() + GameComponentProgressManager.pixelsPerCell_WORLD.ToString(": ##0 ppc"), -1, "LPR_SettingspixelsPerCell_WORLDDescription".Translate());
                    GameComponentProgressManager.pixelsPerCell_WORLD = (int)ls.Slider(GameComponentProgressManager.pixelsPerCell_WORLD, 1, 64);
                    ls.CheckboxLabeled("LPR_SettingsInitializeLabel".Translate(), ref JPGQualityInitialize, "LPR_SettingsInitializeDescription".Translate());
                    ls.Gap();
                }
            }
            else
            {
                ls.Label("LPR_SettingspixelsPerCellLabel".Translate() + pixelsPerCell.ToString(": ##0 ppc"), -1, "LPR_SettingspixelsPerCellDescription".Translate());
                pixelsPerCell = (int)ls.Slider(pixelsPerCell, 1, 64);
            }

            ls.Gap();
            ls.CheckboxLabeled("LPR_SettingsScaleOutputImageLabel".Translate(), ref scaleOutputImage, "LPR_SettingsScaleOutputImageDescription".Translate());
            if (scaleOutputImage)
            {
                ls.Label("LPR_SettingsOutputImageFixedHeightLabel".Translate());
                ls.TextFieldNumeric(ref outputImageFixedHeight, ref outputImageFixedHeightBuffer, 1);
                ls.Gap();
            }

            ls.GapLine();
            if (scaleOutputImage)
            {
                ls.Gap(); // All about that visual balance
            }
            ls.Label("LPR_SettingsExportPathLabel".Translate(), -1, "LPR_SettingsExportPathDescription".Translate());
            exportPath = ls.TextEntry($"{exportPath}");
            if(exportPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ls.Label("LPR_SettingsExportPathInvalid".Translate());
            }

            ls.Gap();
            ls.CheckboxLabeled("LPR_SettingsCreateSubdirsLabel".Translate(), ref createSubdirs, "LPR_SettingsCreateSubdirsDescription".Translate());
            ls.CheckboxLabeled("LPR_SettingsUseMapNameInstead".Translate(), ref useMapNameInstead, "LPR_SettingsUseMapNameInsteadDescription".Translate());
            backupAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (ls.ButtonTextLabeled("LPR_SettingsFileNamePatternLabel".Translate(), ("LPR_FileNamePattern_" + fileNamePattern).Translate()))
            {
                var menuEntries = new List<FloatMenuOption>();
                var patterns = (FileNamePattern[])Enum.GetValues(typeof(FileNamePattern));
                foreach (var pattern in patterns)
                {
                    menuEntries.Add(new FloatMenuOption(("LPR_FileNamePattern_" + EnumUtils.ToFriendlyString(pattern)).Translate(), delegate
                    {
                        fileNamePattern = pattern;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(menuEntries));
            }
            Text.Anchor = backupAnchor;

            ls.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref renderFeedback, "renderFeedback", DefaultRenderFeedback);
            Scribe_Values.Look(ref renderDesignations, "renderDesignations", DefaultRenderDesignations);
            Scribe_Values.Look(ref renderNonPlayerHomes, "renderNonPlayerHomes", DefaultRenderNonPlayerHomes);
            Scribe_Values.Look(ref renderThingIcons, "renderThingIcons", DefaultRenderThingIcons);
            Scribe_Values.Look(ref renderGameConditions, "renderGameConditions", DefaultRenderGameConditions);
            Scribe_Values.Look(ref renderWeather, "renderWeather", DefaultRenderWeather);
            Scribe_Values.Look(ref renderZones, "renderZones", DefaultRenderZones);
            Scribe_Values.Look(ref renderOverlays, "renderOverlays", DefaultRenderOverlays);
            Scribe_Values.Look(ref smoothRenderAreaSteps, "smoothRenderAreaSteps", DefaultSmoothRenderAreaSteps);
            Scribe_Values.Look(ref whichInterval, "whichInterval", RenderIntervalHelper.Intervals.IndexOf(DefaultInterval));
            Scribe_Values.Look(ref timeOfDay, "timeOfDay", DefaultTimeOfDay);
            Scribe_Values.Look(ref encoding, "encodingFormat", DefaultEncoding);
            Scribe_Values.Look(ref JPGQuality, "JPGQuality", DefaultJPGQuality);
            Scribe_Values.Look(ref pixelsPerCell, "pixelsPerCell", DefaultpixelsPerCell);
            Scribe_Values.Look(ref scaleOutputImage, "scaleOutputImage", DefaultScaleOutputImage);
            Scribe_Values.Look(ref outputImageFixedHeight, "outputImageFixedHeight", DefaultOutputImageFixedHeight);
            Scribe_Values.Look(ref exportPath, "exportPath", DesktopPath);
            Scribe_Values.Look(ref createSubdirs, "createSubdirs", DefaultCreateSubdirs);
            Scribe_Values.Look(ref useMapNameInstead, "useMapNameInstead", false);
            Scribe_Values.Look(ref fileNamePattern, "fileNamePattern", DefaultFileNamePattern);
            Scribe_Values.Look(ref migratedOutputImageSettings, "migratedOutputImageSettings", false, true);
            Scribe_Values.Look(ref migratedInterval, "migratedInterval", false, true);
        }

        public static int interval
        {
            get
            {
                return RenderIntervalHelper.Intervals[whichInterval];
            }
        }

        private static string DesktopPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        private static class RenderIntervalHelper
        {
            public static readonly List<int> Intervals = new List<int>() { 15 * 24, 10 * 24, 6 * 24, 5 * 24, 4 * 24, 3 * 24, 2 * 24, 24, 12, 8, 6, 4, 3, 2, 1 };
            public static readonly List<int> WhichLabelsForInterval = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2, 2, 2, 3 };
            public static readonly List<string> Labels = new List<string>() { "LPR_RenderEveryDays", "LPR_RenderEveryDay", "LPR_RenderEveryHours", "LPR_RenderEveryHour" };

            public static string GetLabel(int interval)
            {
                var labelIndex = Intervals.IndexOf(interval);
                if (labelIndex < 0)
                {
                    Log.Error("Wrong configuration found for ProgressRenderer.PRModSettings.interval. Using default value.");
                    labelIndex = Intervals.IndexOf(DefaultInterval);
                }

                var whichLabel = WhichLabelsForInterval[labelIndex];
                float labelVal = interval;
                if (whichLabel == 0)
                {
                    labelVal /= 24f;
                }

                return Labels[whichLabel].Translate(labelVal.ToString("#0"));
            }
        }

    }

}
