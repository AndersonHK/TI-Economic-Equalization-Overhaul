namespace TIEconomyMod
{
    public static class PowerPlantScalingMath
    {
        public static float DefaultOpenCycleThermalMassMultiplier(
            PowerPlantRequirement powerPlantClass)
        {
            switch (powerPlantClass)
            {
            case PowerPlantRequirement.Any_General:
            case PowerPlantRequirement.Fuel_Cell:
                return 1f;
            default:
                // Temporary gameplay calibration until reactor specific
                // masses are rebalanced. Restore differentiated technology
                // values from the implementation plan afterward.
                return 0.5f;
            }
        }
    }
}
