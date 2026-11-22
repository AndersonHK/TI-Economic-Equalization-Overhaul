using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(NotificationScreenController), "SetCustomNotificationButtons")]
    internal static class FleetTransferNotificationPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var orbitFleets = AccessTools.PropertyGetter(typeof(TIOrbitState), nameof(TIOrbitState.fleetsInOrbit));
            var shipCompletion = Enumerable.Range(0, code.Count).Where(i => code[i].Calls(orbitFleets)).ToArray();
            var sortie = Enumerable.Range(0, code.Count).Where(i => code[i].LoadsConstant("UI.Notifications.Intercept")).ToArray();
            if (shipCompletion.Length != 3 || sortie.Length != 2)
                throw new InvalidOperationException("Fleet notification call sites changed.");

            // In 1.0.65 the first orbit lookup belongs to ship completion.
            code[shipCompletion[0]].opcode = OpCodes.Call;
            code[shipCompletion[0]].operand = AccessTools.Method(typeof(FleetTransferNotificationPatch), nameof(ShipCompletionTargets));

            // The first Intercept label belongs to OnFleetAlarm. The alarm's
            // Select Fleet delegate consumes button 0 before any sortie.
            var getItem = AccessTools.PropertyGetter(typeof(List<TISpaceFleetState>), "Item");
            var getCount = AccessTools.PropertyGetter(typeof(List<TISpaceFleetState>), "Count");
            int index = FindBefore(code, sortie[0], getItem);
            int count = FindBefore(code, index, getCount);
            if (sortie[0] - index > 8 || index - count > 25 ||
                !Equals(code[index - 1].operand, code[count + 1].operand) ||
                !code[index - 1].IsLdloc() || !code[count + 1].IsLdloc())
                throw new InvalidOperationException("Fleet alarm candidate indexing changed.");
            code.InsertRange(index, AlarmIndexOffset());
            code.InsertRange(count + 2, AlarmIndexOffset());
            return code;
        }

        private static int FindBefore(List<CodeInstruction> code, int start, System.Reflection.MethodInfo method)
        {
            for (int i = start - 1; i > 0; i--)
                if (code[i].Calls(method))
                    return i;
            throw new InvalidOperationException("Fleet alarm candidate list access is missing.");
        }

        private static CodeInstruction[] AlarmIndexOffset() =>
            new[] { new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Sub) };

        private static List<TISpaceFleetState> ShipCompletionTargets(TIOrbitState shipyardOrbit)
        {
            return shipyardOrbit.fleetsInOrbit
                .Concat(shipyardOrbit.barycenter.fleetsInOrbit)
                .Concat(shipyardOrbit.barycenter.stationsInOrbit.SelectMany(station => station.dockedFleets))
                .Where(fleet => fleet.exists && !fleet.archived)
                .Distinct()
                .ToList();
        }
    }
}
