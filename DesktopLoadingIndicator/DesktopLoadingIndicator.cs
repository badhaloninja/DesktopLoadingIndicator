using BaseX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System;

namespace DesktopLoadingIndicator
{
    public class DesktopLoadingIndicator : NeosMod
    {
        public override string Name => "DesktopLoadingIndicator";
        public override string Author => "badhaloninja";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/badhaloninja/DesktopLoadingIndicator";


        // BaseX.float2 is not supported with the NeosModLoader config saving currently 
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<float> verticalDistanceFromCenter = new ModConfigurationKey<float>("verticalDistanceFromCenter", "Vertical Distance From Center", () => 0.4f);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<float> horizontalDistanceFromCenter = new ModConfigurationKey<float>("horizontalDistanceFromCenter", "Horizontal Distance From Center", () => 0.68f);


        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<Alignment> indicatorAlignment = new ModConfigurationKey<Alignment>("indicatorAlignment", "Indicator Alignment", () => Alignment.BottomLeft);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<float> indicatorSize = new ModConfigurationKey<float>("indicatorSize", "Indicator Size", () => 0.6f);

        private static ModConfiguration config;



        public override void OnEngineInit()
        {
            config = GetConfiguration();

            Harmony harmony = new Harmony("me.badhaloninja.DesktopLoadingIndicator");
            harmony.PatchAll();
        }


        [HarmonyPatch(typeof(LoadingIndicator), "OnCommonUpdate")]
        class LoadingIndicator_OnCommonUpdate_Patch
        {

            public static void Postfix(LoadingIndicator __instance)
            {
                if (__instance.World == Userspace.UserspaceWorld)
                {
                    OverlayManager overlayManager = __instance.World.GetGloballyRegisteredComponent<OverlayManager>();
                    if (overlayManager == null) return; // Exit if not found

                    if (!__instance.InputInterface.VR_Active)
                    { // Desktop mode
                        if (__instance.Slot.Parent != overlayManager.OverlayRoot)
                        {
                            __instance.Slot.Parent = overlayManager.OverlayRoot;
                        }

                        // Convert enum into a matrix to multiply the offet by
                        var alignmentInt = (int) config.GetValue(indicatorAlignment); // Convert to int for funny math
                        var alignmentMul = new float3(alignmentInt % 3 - 1, (alignmentInt / 3 - 1) * -1, 0f); //mmhm fun stuff
                        /* X
                         * Left | Center | Right
                         *  -1  |   0    |   1
                         * ----------------------
                         * Y
                         * Top  | Middle | Bottom
                         *  1   |   0    |   -1
                         */

                        var calculatedOffset = new float3(config.GetValue(horizontalDistanceFromCenter), config.GetValue(verticalDistanceFromCenter)) * alignmentMul; // *:*)

                        __instance.Slot.LocalPosition = calculatedOffset;
                        __instance.Slot.LocalRotation = floatQ.Identity;
                        __instance.Slot.LocalScale = float3.One * config.GetValue(indicatorSize);
                        return;
                    }
                    if (__instance.Slot.Parent == overlayManager.OverlayRoot)
                    {
                        __instance.Slot.Parent = null;
                        __instance.Slot.LocalScale = float3.One;
                    }
                }
            }
        }
    }
}