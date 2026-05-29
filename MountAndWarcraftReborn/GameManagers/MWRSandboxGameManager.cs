using SandBox;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ModuleManager;
using HarmonyLib;

namespace MountAndWarcraftReborn.GameManagers
{
    public class MWRSandboxGameManager : SandBoxGameManager
    {
        private const string ModuleId = "MountAndWarcraftReborn";
        private const string CampaignIntroFolder = "Videos\\CampaignIntro";
        private const string CampaignIntroBaseName = "campaign_intro";
        public MWRSandboxGameManager()
            : base(CreateCampaign)
        {
        }

        public override void OnLoadFinished()
        {
            if (LoadingSavedGame)
            {
                base.OnLoadFinished();
                return;
            }

            if (TryLaunchCustomIntro())
            {
                IsLoaded = true;
                return;
            }

            base.OnLoadFinished();
        }

        private bool TryLaunchCustomIntro()
        {
            try
            {
                string introFolder = Path.Combine(GetModuleRootPath(), CampaignIntroFolder);
                string mp4Path = Path.Combine(introFolder, CampaignIntroBaseName + ".mp4");
                string ivfPath = Path.Combine(introFolder, CampaignIntroBaseName + ".ivf");
                string oggPath = Path.Combine(introFolder, CampaignIntroBaseName + ".ogg");
                string subtitleBasePath = Path.Combine(introFolder, CampaignIntroBaseName);

                string videoPath = null;
                string audioPath = string.Empty;
                if (File.Exists(ivfPath))
                {
                    videoPath = ivfPath;
                    audioPath = File.Exists(oggPath) ? oggPath : string.Empty;
                }
                else if (File.Exists(mp4Path))
                {
                    videoPath = mp4Path;
                }

                if (string.IsNullOrWhiteSpace(videoPath))
                {
                    return false;
                }

                VideoPlaybackState state = Game.Current.GameStateManager.CreateState<VideoPlaybackState>();
                float frameRate = GetIvfFrameRate(videoPath);
                state.SetStartingParameters(videoPath, audioPath, subtitleBasePath, frameRate, true);
                state.SetOnVideoFinisedDelegate(OnIntroFinished);
                Game.Current.GameStateManager.CleanAndPushState(state);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string GetModuleRootPath()
        {
            try
            {
                string modulePath = ModuleHelper.GetModuleFullPath(ModuleId);
                if (!string.IsNullOrWhiteSpace(modulePath))
                {
                    return modulePath;
                }
            }
            catch (Exception)
            {
            }

            return Path.Combine(BasePath.Name, "Modules", ModuleId);
        }

        private static float GetIvfFrameRate(string videoPath)
        {
            try
            {
                byte[] header = File.ReadAllBytes(videoPath);
                if (header.Length < 24 || header[0] != (byte)'D' || header[1] != (byte)'K' || header[2] != (byte)'I' || header[3] != (byte)'F')
                {
                    return 30f;
                }

                uint rateNumerator = BitConverter.ToUInt32(header, 16);
                uint rateDenominator = BitConverter.ToUInt32(header, 20);
                if (rateNumerator == 0 || rateDenominator == 0)
                {
                    return 30f;
                }

                return (float)rateNumerator / rateDenominator;
            }
            catch (Exception ex)
            {
                return 30f;
            }
        }

        private void OnIntroFinished()
        {
            AccessTools.Method(typeof(SandBoxGameManager), "LaunchSandboxCharacterCreation")
                ?.Invoke(this, Array.Empty<object>());
        }

        private static Campaign CreateCampaign()
        {
            return new Campaign(CampaignGameMode.Campaign);
        }
    }
}
