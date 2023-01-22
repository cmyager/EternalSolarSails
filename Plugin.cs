using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace EternalSolarSails
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;
        private void Start()
        {
            harmony = new Harmony("com.cmyager.plugin.DSP.EternalSolarSails");
            try
            {
                harmony.PatchAll(typeof(Plugin.DysonSwarm_GameTick_Patch));
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"[EternalSolarSails] EXCEPTION: {ex}");
            }
        }

        internal void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        [HarmonyPatch(typeof(DysonSwarm), "GameTick")]
        public static class DysonSwarm_GameTick_Patch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Get FieldInfo for the eternal variable
                FieldInfo[] dysonSwarmFieldInfo = typeof(DysonSwarm).GetFields(BindingFlags.Instance | BindingFlags.Public);
                FieldInfo eternalVariableFieldInfo = null;
                for (int i = 0; i < dysonSwarmFieldInfo.Length; i++)
                {
                    if (dysonSwarmFieldInfo[i].Name == "eternal")
                    {
                        eternalVariableFieldInfo = dysonSwarmFieldInfo[i];
                        break;
                    }
                }

                // Exit early if eternal was not found
                if (eternalVariableFieldInfo == null)
                {
                    return instructions;
                }

                // Find the instruction that sets eternal to false if not in sandbox mode
                List<CodeInstruction> codeInstructionlist = new(instructions);
                for (int i = 0; i < codeInstructionlist.Count; i++)
                {
                    if (i > 0 && CodeInstructionExtensions.StoresField(codeInstructionlist[i], eternalVariableFieldInfo))
                    {
                        // change the value it is set to from 0 to 1

                        codeInstructionlist[i-1].opcode = OpCodes.Ldc_I4_1;
                        break;
                    }
                }
                return codeInstructionlist.AsEnumerable<CodeInstruction>();
            }
        }
    }
}
