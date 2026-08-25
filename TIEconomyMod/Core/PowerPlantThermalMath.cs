using System;

namespace TIEconomyMod
{
    public struct ShipPowerDemandSnapshot
    {
        public float DriveDemand_GW;
        public bool DriveDemandIsThermal;
        public float OpenCycleReactorOutput_GWth;
        public float UsefulElectricalDemand_GWe;
        public float ElectricalReactorInput_GWth;
        public float TotalReactorOutput_GWth;
        public float MassRatedOutput_GW;
        public float CapRatedDriveDemand_GW;
        public float OpenCycleWasteHeat_GW;
        public float ElectricalWasteHeat_GW;
        public float PlantWasteHeat_GW;
        public float OpenCycleThermalMassMultiplier;
    }

    public static class PowerPlantThermalMath
    {
        private const float MinimumEfficiency = 0.0001f;

        public static float WasteHeatFromUsefulPower_GW(
            float usefulPower_GW, float efficiency)
        {
            if (!IsFinite(usefulPower_GW) || usefulPower_GW <= 0f)
            {
                return 0f;
            }

            // No shipped plant has zero efficiency. Keep a malformed template finite
            // so it cannot poison radiator and ship-mass calculations with infinity.
            float boundedEfficiency = BoundedEfficiency(efficiency);
            if (boundedEfficiency >= 1f)
            {
                return 0f;
            }

            return usefulPower_GW * (1f / boundedEfficiency - 1f);
        }

        public static float OpenCycleEffectiveCoupling(
            float efficiency, float retainedHeatFraction)
        {
            float boundedEfficiency = BoundedEfficiency(efficiency);
            float boundedFraction = BoundedFraction(retainedHeatFraction);
            return Math.Max(
                MinimumEfficiency,
                1f - boundedFraction * (1f - boundedEfficiency));
        }

        public static float OpenCycleReactorOutput_GW(
            float usefulDrivePower_GW,
            float efficiency,
            float retainedHeatFraction)
        {
            if (!IsFinite(usefulDrivePower_GW) || usefulDrivePower_GW <= 0f)
            {
                return 0f;
            }

            float output_GW = usefulDrivePower_GW /
                OpenCycleEffectiveCoupling(efficiency, retainedHeatFraction);
            return IsFinite(output_GW) ? output_GW : usefulDrivePower_GW;
        }

        public static float OpenCycleResidualHeat_GW(
            float reactorOutput_GW,
            float efficiency,
            float retainedHeatFraction)
        {
            if (!IsFinite(reactorOutput_GW) || reactorOutput_GW <= 0f)
            {
                return 0f;
            }

            float heat_GW = reactorOutput_GW *
                BoundedFraction(retainedHeatFraction) *
                (1f - BoundedEfficiency(efficiency));
            return IsFinite(heat_GW) ? heat_GW : 0f;
        }

        public static float CapRatedDriveDemand_GW(
            bool openCycleDriveCooling,
            float drivePowerRequirement_GW,
            float efficiency,
            float openCycleDriveHeatFraction,
            float openCycleThermalMultiplier)
        {
            float driveDemand_GW = NonNegativeFinite(
                drivePowerRequirement_GW);
            if (!openCycleDriveCooling)
            {
                return driveDemand_GW;
            }

            return OpenCycleReactorOutput_GW(
                    driveDemand_GW,
                    efficiency,
                    openCycleDriveHeatFraction) *
                BoundedMassMultiplier(openCycleThermalMultiplier);
        }

        public static float PlantWasteHeat_GW(
            bool openCycleDriveCooling,
            float drivePowerRequirement_GW,
            float systemsAndWeaponsRequirement_GW,
            float efficiency,
            float openCycleDriveHeatFraction)
        {
            return CalculateShipDemand(
                openCycleDriveCooling,
                drivePowerRequirement_GW,
                systemsAndWeaponsRequirement_GW,
                efficiency,
                openCycleDriveHeatFraction,
                1f).PlantWasteHeat_GW;
        }

        public static ShipPowerDemandSnapshot CalculateShipDemand(
            bool openCycleDriveCooling,
            float drivePowerRequirement_GW,
            float systemsAndWeaponsRequirement_GW,
            float efficiency,
            float openCycleDriveHeatFraction,
            float openCycleThermalMassMultiplier)
        {
            ShipPowerDemandSnapshot snapshot =
                default(ShipPowerDemandSnapshot);
            float driveDemand_GW = NonNegativeFinite(
                drivePowerRequirement_GW);
            float auxiliaryElectricalDemand_GWe = NonNegativeFinite(
                systemsAndWeaponsRequirement_GW);
            float boundedEfficiency = BoundedEfficiency(efficiency);
            float boundedMassMultiplier = BoundedMassMultiplier(
                openCycleThermalMassMultiplier);

            snapshot.DriveDemand_GW = driveDemand_GW;
            snapshot.DriveDemandIsThermal = openCycleDriveCooling;
            snapshot.OpenCycleThermalMassMultiplier =
                openCycleDriveCooling ? boundedMassMultiplier : 1f;

            if (openCycleDriveCooling)
            {
                snapshot.OpenCycleReactorOutput_GWth =
                    OpenCycleReactorOutput_GW(
                        driveDemand_GW,
                        boundedEfficiency,
                        openCycleDriveHeatFraction);
                snapshot.OpenCycleWasteHeat_GW =
                    OpenCycleResidualHeat_GW(
                        snapshot.OpenCycleReactorOutput_GWth,
                        boundedEfficiency,
                        openCycleDriveHeatFraction);
                snapshot.UsefulElectricalDemand_GWe =
                    auxiliaryElectricalDemand_GWe;
            }
            else
            {
                snapshot.UsefulElectricalDemand_GWe =
                    driveDemand_GW + auxiliaryElectricalDemand_GWe;
            }

            snapshot.ElectricalReactorInput_GWth =
                snapshot.UsefulElectricalDemand_GWe <= 0f
                    ? 0f
                    : snapshot.UsefulElectricalDemand_GWe /
                        boundedEfficiency;
            if (!IsFinite(snapshot.ElectricalReactorInput_GWth))
            {
                snapshot.ElectricalReactorInput_GWth =
                    snapshot.UsefulElectricalDemand_GWe;
            }

            snapshot.ElectricalWasteHeat_GW = Math.Max(
                0f,
                snapshot.ElectricalReactorInput_GWth -
                    snapshot.UsefulElectricalDemand_GWe);
            snapshot.TotalReactorOutput_GWth =
                snapshot.OpenCycleReactorOutput_GWth +
                snapshot.ElectricalReactorInput_GWth;
            snapshot.MassRatedOutput_GW =
                snapshot.OpenCycleReactorOutput_GWth *
                    snapshot.OpenCycleThermalMassMultiplier +
                snapshot.ElectricalReactorInput_GWth;
            snapshot.CapRatedDriveDemand_GW =
                openCycleDriveCooling
                    ? snapshot.OpenCycleReactorOutput_GWth *
                        snapshot.OpenCycleThermalMassMultiplier
                    : driveDemand_GW;
            snapshot.PlantWasteHeat_GW =
                snapshot.OpenCycleWasteHeat_GW +
                snapshot.ElectricalWasteHeat_GW;
            return snapshot;
        }

        private static float NonNegativeFinite(float value)
        {
            return IsFinite(value) ? Math.Max(0f, value) : 0f;
        }

        private static float BoundedMassMultiplier(float multiplier)
        {
            if (!IsFinite(multiplier) || multiplier <= 0f)
            {
                return 1f;
            }

            return Math.Min(1f, multiplier);
        }

        private static float BoundedEfficiency(float efficiency)
        {
            if (!IsFinite(efficiency))
            {
                return 1f;
            }

            return Math.Max(MinimumEfficiency, Math.Min(1f, efficiency));
        }

        private static float BoundedFraction(float fraction)
        {
            if (!IsFinite(fraction))
            {
                return 0f;
            }

            return Math.Max(0f, Math.Min(1f, fraction));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
