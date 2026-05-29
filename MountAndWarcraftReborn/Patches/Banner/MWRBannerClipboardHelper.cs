#nullable enable
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using BannerType = TaleWorlds.Core.Banner;
using InformationManagerType = TaleWorlds.Library.InformationManager;
using InformationMessageType = TaleWorlds.Library.InformationMessage;

namespace MountAndWarcraftReborn.Patches.Banner
{
    internal static class MWRBannerClipboardHelper
    {
        public static void HandleInput(object? screen)
        {
            if (screen == null || !IsControlHeld())
            {
                return;
            }

            if (Input.IsKeyPressed(InputKey.C))
            {
                CopyBannerCode(screen);
            }
            else if (Input.IsKeyPressed(InputKey.V))
            {
                PasteBannerCode(screen);
            }
        }

        private static bool IsControlHeld()
        {
            return Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl);
        }

        private static void CopyBannerCode(object screen)
        {
            if (!TryGetBannerCode(screen, out string bannerCode) || string.IsNullOrWhiteSpace(bannerCode))
            {
                ShowMessage("Could not copy banner code.");
                return;
            }

            try
            {
                Input.SetClipboardText(bannerCode);
                ShowMessage("Banner code copied to clipboard.");
            }
            catch (Exception)
            {
                ShowMessage("Could not copy banner code.");
            }
        }

        private static void PasteBannerCode(object screen)
        {
            string bannerCode;
            try
            {
                bannerCode = Input.GetClipboardText()?.Trim() ?? string.Empty;
            }
            catch (Exception)
            {
                ShowMessage("Could not read banner code from clipboard.");
                return;
            }

            if (string.IsNullOrWhiteSpace(bannerCode) || !BannerType.IsValidBannerCode(bannerCode))
            {
                ShowMessage("Clipboard does not contain a valid banner code.");
                return;
            }

            if (TryApplyBannerCode(screen, bannerCode))
            {
                ShowMessage("Banner code pasted.");
                return;
            }

            ShowMessage("Could not paste banner code.");
        }

        private static bool TryGetBannerCode(object screen, out string bannerCode)
        {
            object? dataSource = GetPropertyValue(screen, "DataSource");

            if (dataSource != null)
            {
                MethodInfo? getBannerCode = AccessTools.Method(dataSource.GetType(), "GetBannerCode");
                if (getBannerCode != null)
                {
                    object? codeObject = getBannerCode.Invoke(dataSource, null);
                    if (codeObject is string code && !string.IsNullOrWhiteSpace(code))
                    {
                        bannerCode = code;
                        return true;
                    }
                }

                object? bannerViewModel = GetPropertyValue(dataSource, "BannerVM");
                if (bannerViewModel != null && GetPropertyValue(bannerViewModel, "BannerCode") is string vmBannerCode && !string.IsNullOrWhiteSpace(vmBannerCode))
                {
                    bannerCode = vmBannerCode;
                    return true;
                }
            }

            if (GetPropertyValue(screen, "Banner") is BannerType banner)
            {
                bannerCode = banner.Serialize();
                return true;
            }

            bannerCode = string.Empty;
            return false;
        }

        private static bool TryApplyBannerCode(object screen, string bannerCode)
        {
            object? dataSource = GetPropertyValue(screen, "DataSource");

            if (dataSource != null)
            {
                MethodInfo? setBannerCode = AccessTools.Method(dataSource.GetType(), "SetBannerCode", new[] { typeof(string) });
                if (setBannerCode != null)
                {
                    setBannerCode.Invoke(dataSource, new object[] { bannerCode });
                    InvokeNoArg(dataSource, "Refresh");
                    InvokeNoArg(dataSource, "RefreshValues");
                    InvokeNoArg(screen, "RefreshShieldAndCharacter");
                    InvokeNoArg(screen, "SetMapIconAsDirtyForAllPlayerClanParties");
                    return true;
                }
            }

            bool applied = false;

            if (GetPropertyValue(screen, "Banner") is BannerType screenBanner)
            {
                screenBanner.Deserialize(bannerCode);
                applied = true;
            }

            if (dataSource != null)
            {
                object? bannerViewModel = GetPropertyValue(dataSource, "BannerVM");
                if (bannerViewModel != null)
                {
                    if (GetPropertyValue(bannerViewModel, "Banner") is BannerType vmBanner)
                    {
                        vmBanner.Deserialize(bannerCode);
                        applied = true;
                    }

                    SetPropertyValue(bannerViewModel, "BannerCode", bannerCode);
                }

                InvokeNoArg(dataSource, "Refresh");
                InvokeNoArg(dataSource, "RefreshValues");
            }

            if (!applied)
            {
                return false;
            }

            InvokeNoArg(screen, "RefreshShieldAndCharacter");
            InvokeNoArg(screen, "UpdateBanners");
            InvokeNoArg(screen, "SetMapIconAsDirtyForAllPlayerClanParties");
            return true;
        }

        private static object? GetPropertyValue(object instance, string propertyName)
        {
            return AccessTools.Property(instance.GetType(), propertyName)?.GetValue(instance, null);
        }

        private static void SetPropertyValue(object instance, string propertyName, object? value)
        {
            AccessTools.Property(instance.GetType(), propertyName)?.SetValue(instance, value, null);
        }

        private static void InvokeNoArg(object instance, string methodName)
        {
            AccessTools.Method(instance.GetType(), methodName)?.Invoke(instance, null);
        }

        private static void ShowMessage(string message)
        {
            InformationManagerType.DisplayMessage(new InformationMessageType(message));
        }
    }
}
