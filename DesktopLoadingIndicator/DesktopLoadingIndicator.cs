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
        public override string Version => "1.1.0";
        public override string Link => "https://github.com/badhaloninja/DesktopLoadingIndicator";


        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<float2> distanceFromCenter = new ModConfigurationKey<float2>("distanceFromCenter", "Distance From Center", () => new float2(0.7f, 0.32f));


        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<Alignment> indicatorAlignment = new ModConfigurationKey<Alignment>("indicatorAlignment", "Indicator Alignment", () => Alignment.BottomLeft);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<float> indicatorSize = new ModConfigurationKey<float>("indicatorSize", "Indicator Size", () => 0.6f);

        private static ModConfiguration config;

        private static float3 offsetPosition
        {
            get
            {
                // Convert enum into a matrix to multiply the offet by
                var alignmentInt = (int)config.GetValue(indicatorAlignment); // Convert to int for funny math
                var alignmentMul = new float3(alignmentInt % 3 - 1, (alignmentInt / 3 - 1) * -1, 0f); //mmhm fun stuff
                /* X
                 * Left | Center | Right
                 *  -1  |   0    |   1
                 * ----------------------
                 * Y
                 * Top  | Middle | Bottom
                 *  1   |   0    |   -1
                 */

                return config.GetValue(distanceFromCenter).xy_ * alignmentMul; // :D
            }
        }

        public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
        { // Wipe previous version of the config due to enum saving differently
            builder.Version(new Version(1, 1, 0));
        }

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

                        // Over write the LoadingIndicator trying to position it's self
                        __instance.Slot.LocalPosition = offsetPosition;
                        __instance.Slot.LocalRotation = floatQ.Identity;
                        __instance.Slot.LocalScale = float3.One * config.GetValue(indicatorSize);
                        return;
                    }
                    if (__instance.Slot.Parent == overlayManager.OverlayRoot)
                    {
                        __instance.Slot.Parent = __instance.World.RootSlot; // Reset Parent
                        __instance.Slot.LocalScale = float3.One; // Reset scale
                    }
                }
            }
        }
    }
}