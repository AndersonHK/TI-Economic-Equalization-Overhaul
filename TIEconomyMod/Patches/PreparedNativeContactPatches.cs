using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(SpaceCombatManager),"Initialize")]
    public static class PreparedContactWarmupPatch
    {
        [HarmonyPostfix,HarmonyPriority(Priority.Last)]
        public static void Postfix(){if(BattleScaleRuntime.Enabled)ContactJobRuntime.Warmup();}
    }
    [HarmonyPatch(typeof(MissileController),"HasHitTarget")]
    public static class PreparedMissileContactPatch
    {
        [HarmonyPrefix]
        public static void Prefix(){if(BattleScaleRuntime.Enabled)CompatibilityContactWorld.EnsureSnapshot();}
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result=new List<CodeInstruction>();int count=0;
            foreach(var i in instructions)
            {
                MethodInfo m=i.operand as MethodInfo;
                if(m!=null&&m.DeclaringType==typeof(Physics)&&m.Name=="RaycastAll")
                {
                    if(m.GetParameters().Length!=4)throw new InvalidOperationException("Missile ray signature changed.");
                    var owner=new CodeInstruction(OpCodes.Ldarg_0);owner.labels.AddRange(i.labels);i.labels.Clear();result.Add(owner);
                    i.operand=AccessTools.Method(typeof(PreparedContactWorld),nameof(PreparedContactWorld.RaycastMissile));count++;
                }
                result.Add(i);
            }
            if(count!=1)throw new InvalidOperationException("Missile contact call site changed.");return result;
        }
    }
    [HarmonyPatch(typeof(TIUtilities),"SimpleConeCastAll")]
    public static class PreparedConeContactPatch
    {
        [HarmonyPrefix]
        public static void Prefix(){if(BattleScaleRuntime.Enabled){CompatibilityContactWorld.Invalidate();CompatibilityContactWorld.EnsureSnapshot();}}
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {return PreparedNativeContact.Rewrite(instructions,2);}
    }
    internal static class PreparedNativeContact
    {
        internal static IEnumerable<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> instructions,int expected)
        {
            var result=new List<CodeInstruction>(instructions);int count=0;
            foreach(var i in result)
            {
                MethodInfo m=i.operand as MethodInfo;
                if(m==null||m.DeclaringType!=typeof(Physics)||m.Name!="RaycastAll")continue;
                if(m.GetParameters().Length!=4)throw new InvalidOperationException("Unsupported native ray signature.");
                i.operand=AccessTools.Method(typeof(PreparedContactWorld),nameof(PreparedContactWorld.RaycastAll));count++;
            }
            if(count!=expected)throw new InvalidOperationException("Native contact ray sites changed: "+count+" expected "+expected);
            return result;
        }
    }
    [HarmonyPatch(typeof(BeamWeapon.Beam),MethodType.Constructor,new Type[]{typeof(IDamageable),typeof(Vector3),typeof(Vector3),typeof(TIBeamWeaponTemplate),typeof(CombatWeaponCarrierState)})]
    public static class PreparedBeamContactPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result=new List<CodeInstruction>(instructions);int count=0;
            foreach(var i in result)
            {
                MethodInfo m=i.operand as MethodInfo;
                if(m==null||m.DeclaringType!=typeof(Collider)||m.Name!="Raycast")continue;
                i.opcode=OpCodes.Call;i.operand=AccessTools.Method(typeof(PreparedContactWorld),nameof(PreparedContactWorld.RaycastCollider));count++;
            }
            if(count!=1)throw new InvalidOperationException("Beam collider call site changed.");return result;
        }
    }
    [HarmonyPatch]
    public static class PreparedVisibilityPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(RadiatorVisController),"OnRadiatorDestroyed");
            yield return AccessTools.Method(typeof(RadiatorVisController),"OnRadiatorRepaired");
            yield return AccessTools.Method(typeof(ShipModelController),"OnDestructionStart");
            yield return AccessTools.Method(typeof(ShipWeaponVisController),"OnWeaponDestroyedExplosion");
        }
        [HarmonyPostfix]
        public static void Postfix(Component __instance)
        {var geometry=__instance.GetComponentInParent<CombatMeshGeometry>();if(geometry!=null)geometry.RefreshWorkerVisibility();}
    }
}
