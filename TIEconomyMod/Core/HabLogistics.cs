using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TIEconomyMod
{
    internal enum LogisticsDeliveryKind
    {
        HabModule,
        Probe
    }

    internal sealed class HabLogisticsRoute
    {
        internal TIHabState OriginHab;
        internal bool IsEarthFallback;
        internal int FactoryTier;
        internal int DockTier;
        internal int ExportTier;
        internal double LaunchDeltaV_kps;
        internal double TransferDeltaV_kps;
        internal double LandingDeltaV_kps;
        internal double TotalDeltaV_kps;
        // Includes trajectory physics and Solar Steamers' off-window effect,
        // but not payload-specific module or probe transfer-time effects.
        internal float TrajectoryTime_days;

        internal string OriginName
        {
            get
            {
                return IsEarthFallback || OriginHab == null
                    ? "Earth"
                    : OriginHab.displayName;
            }
        }
    }

    internal sealed class HabFreightQuote
    {
        internal readonly float[] StockpileCosts;
        internal readonly float[] EarthPurchaseCosts;
        internal HabLogisticsRoute Route;
        internal float MaterialMass_tons;
        internal float EarthMaterialMass_tons;
        internal float SpacePayloadMass_tons;
        internal float PropellantMass_tons;
        internal float EarthFreightMass_tons;

        internal HabFreightQuote(int resourceCount)
        {
            StockpileCosts = new float[resourceCount];
            EarthPurchaseCosts = new float[resourceCount];
        }
    }

    internal static class HabLogistics
    {
        internal static readonly FactionResource[] MaterialResources =
        {
            FactionResource.Water,
            FactionResource.Volatiles,
            FactionResource.Metals,
            FactionResource.NobleMetals,
            FactionResource.Fissiles,
            FactionResource.Antimatter,
            FactionResource.Exotics
        };

        private sealed class SourceSummary
        {
            internal TIHabState Hab;
            internal int FactoryTier;
            internal int DockTier;
            internal int ExportTier;
        }

        private sealed class SourceRegistry
        {
            internal int Generation;
            internal readonly List<SourceSummary> Sources =
                new List<SourceSummary>();
        }

        private sealed class RouteEndpoint
        {
            internal TIGameState State;
            internal double ExtraDeltaV_kps;
        }

        private struct LogisticsEffectSnapshot
        {
            internal float GenericTransferEV_kps;
            internal int GenericTransferEVBits;
            internal int OffDateModifierBits;

            internal static LogisticsEffectSnapshot Capture(
                TIFactionState faction)
            {
                float genericTransferEV =
                    TISpaceObjectState.ModifiedGenericTransferEV_kps(faction);
                float offDateModifier = TIEffectsState.SumEffectsModifiers(
                    Context.GenericTransfer_OffDate_PCT,
                    faction,
                    1f,
                    null);
                return new LogisticsEffectSnapshot
                {
                    GenericTransferEV_kps = genericTransferEV,
                    GenericTransferEVBits = FloatBits(genericTransferEV),
                    OffDateModifierBits = FloatBits(offDateModifier)
                };
            }
        }

        private struct RouteKey : IEquatable<RouteKey>
        {
            private readonly TIFactionState faction;
            private readonly TIGameState destination;
            private readonly int tier;
            private readonly int offDateModifierBits;

            internal RouteKey(
                TIFactionState faction,
                TIGameState destination,
                int tier,
                int offDateModifierBits)
            {
                this.faction = faction;
                this.destination = destination;
                this.tier = tier;
                this.offDateModifierBits = offDateModifierBits;
            }

            public bool Equals(RouteKey other)
            {
                return ReferenceEquals(faction, other.faction) &&
                    ReferenceEquals(destination, other.destination) &&
                    tier == other.tier &&
                    offDateModifierBits == other.offDateModifierBits;
            }

            public override bool Equals(object obj)
            {
                return obj is RouteKey && Equals((RouteKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = RuntimeHelpers.GetHashCode(faction);
                    hash = hash * 397 + RuntimeHelpers.GetHashCode(destination);
                    hash = hash * 397 + tier;
                    return hash * 397 + offDateModifierBits;
                }
            }
        }

        private struct FreightKey : IEquatable<FreightKey>
        {
            private readonly TIFactionState faction;
            private readonly TIGameState destination;
            private readonly int tier;
            private readonly int materialsHash;
            private readonly int resourcesHash;
            private readonly int genericTransferEVBits;
            private readonly int offDateModifierBits;
            private readonly bool fullPayload;
            private readonly bool allowEarthFallback;
            private readonly bool substitute;

            internal FreightKey(
                TIFactionState faction,
                TIGameState destination,
                int tier,
                int materialsHash,
                int resourcesHash,
                int genericTransferEVBits,
                int offDateModifierBits,
                bool fullPayload,
                bool allowEarthFallback,
                bool substitute)
            {
                this.faction = faction;
                this.destination = destination;
                this.tier = tier;
                this.materialsHash = materialsHash;
                this.resourcesHash = resourcesHash;
                this.genericTransferEVBits = genericTransferEVBits;
                this.offDateModifierBits = offDateModifierBits;
                this.fullPayload = fullPayload;
                this.allowEarthFallback = allowEarthFallback;
                this.substitute = substitute;
            }

            public bool Equals(FreightKey other)
            {
                return ReferenceEquals(faction, other.faction) &&
                    ReferenceEquals(destination, other.destination) &&
                    tier == other.tier &&
                    materialsHash == other.materialsHash &&
                    resourcesHash == other.resourcesHash &&
                    genericTransferEVBits == other.genericTransferEVBits &&
                    offDateModifierBits == other.offDateModifierBits &&
                    fullPayload == other.fullPayload &&
                    allowEarthFallback == other.allowEarthFallback &&
                    substitute == other.substitute;
            }

            public override bool Equals(object obj)
            {
                return obj is FreightKey && Equals((FreightKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = RuntimeHelpers.GetHashCode(faction);
                    hash = hash * 397 + RuntimeHelpers.GetHashCode(destination);
                    hash = hash * 397 + tier;
                    hash = hash * 397 + materialsHash;
                    hash = hash * 397 + resourcesHash;
                    hash = hash * 397 + genericTransferEVBits;
                    hash = hash * 397 + offDateModifierBits;
                    hash = hash * 397 + (fullPayload ? 1 : 0);
                    hash = hash * 397 + (allowEarthFallback ? 1 : 0);
                    return hash * 397 + (substitute ? 1 : 0);
                }
            }
        }

        private static readonly Dictionary<TIFactionState, SourceRegistry>
            SourceRegistries = new Dictionary<TIFactionState, SourceRegistry>(
                ReferenceIdentityComparer<TIFactionState>.Instance);

        private static readonly Dictionary<RouteKey, HabLogisticsRoute>
            RouteCache = new Dictionary<RouteKey, HabLogisticsRoute>();

        private static readonly Dictionary<FreightKey, HabFreightQuote>
            FreightCache = new Dictionary<FreightKey, HabFreightQuote>();

        private static readonly Dictionary<TIFactionState, int>
            ResourceBalanceHashes = new Dictionary<TIFactionState, int>(
                ReferenceIdentityComparer<TIFactionState>.Instance);

        private static int topologyGeneration = 1;
        private static int cacheTopologyGeneration;
        private static long cacheTimeTicks = long.MinValue;
        private static int routeGeneration = 1;

        internal static int RouteGeneration
        {
            get { return routeGeneration; }
        }

        internal static void InvalidateTopology()
        {
            unchecked
            {
                topologyGeneration++;
            }
        }

        internal static void Clear()
        {
            SourceRegistries.Clear();
            RouteCache.Clear();
            FreightCache.Clear();
            ResourceBalanceHashes.Clear();
            cacheTopologyGeneration = 0;
            cacheTimeTicks = long.MinValue;
            unchecked
            {
                topologyGeneration++;
                routeGeneration++;
            }
        }

        internal static HabLogisticsRoute ResolveRoute(
            TIFactionState faction,
            TIGameState destination,
            int requiredTier,
            bool allowEarthFallback)
        {
            EnsureRouteCacheCurrent();
            LogisticsEffectSnapshot effects =
                LogisticsEffectSnapshot.Capture(faction);
            RouteKey key = new RouteKey(
                faction,
                destination,
                Math.Max(1, requiredTier),
                effects.OffDateModifierBits);
            HabLogisticsRoute cached;
            if (RouteCache.TryGetValue(key, out cached))
            {
                if (cached.IsEarthFallback && !allowEarthFallback)
                {
                    return null;
                }

                return cached;
            }

            SourceRegistry registry = GetSourceRegistry(faction);
            TIHabState destinationHab = destination != null && destination.isHabState
                ? destination.ref_hab
                : null;
            HabLogisticsRoute best = null;

            foreach (SourceSummary source in registry.Sources)
            {
                bool local = destinationHab != null &&
                    ReferenceEquals(source.Hab, destinationHab);
                int usableTier = local
                    ? source.FactoryTier
                    : source.ExportTier;
                if (usableTier < requiredTier)
                {
                    continue;
                }

                HabLogisticsRoute candidate = CalculateRoute(
                    faction,
                    source,
                    destination,
                    local);
                if (candidate == null)
                {
                    continue;
                }

                if (best == null || IsBetter(candidate, best))
                {
                    best = candidate;
                }
            }

            if (best == null)
            {
                best = new HabLogisticsRoute
                {
                    IsEarthFallback = true,
                    FactoryTier = 3,
                    DockTier = 3,
                    ExportTier = 3,
                    TrajectoryTime_days = TISpaceObjectState
                        .GenericTransferTimeFromEarthsSurface_d(
                            faction,
                            destination)
                };
            }

            RouteCache[key] = best;
            if (best.IsEarthFallback && !allowEarthFallback)
            {
                return null;
            }

            return best;
        }

        internal static HabFreightQuote Quote(
            TIFactionState faction,
            TIGameState destination,
            int requiredTier,
            ResourceCostBuilder materials,
            bool fullPayload,
            bool allowEarthFallback,
            bool substitute)
        {
            LogisticsEffectSnapshot effects =
                LogisticsEffectSnapshot.Capture(faction);
            HabLogisticsRoute route = ResolveRoute(
                faction,
                destination,
                requiredTier,
                allowEarthFallback);
            if (route == null)
            {
                return null;
            }

            int materialsHash = MaterialsHash(materials);
            int resourcesHash = ResourceBalancesHash(faction);
            int priorResourcesHash;
            if (!ResourceBalanceHashes.TryGetValue(
                    faction,
                    out priorResourcesHash) ||
                priorResourcesHash != resourcesHash)
            {
                FreightCache.Clear();
                ResourceBalanceHashes[faction] = resourcesHash;
            }

            FreightKey key = new FreightKey(
                faction,
                destination,
                requiredTier,
                materialsHash,
                resourcesHash,
                effects.GenericTransferEVBits,
                effects.OffDateModifierBits,
                fullPayload,
                allowEarthFallback,
                substitute);
            HabFreightQuote cached;
            if (FreightCache.TryGetValue(key, out cached))
            {
                return cached;
            }

            HabFreightQuote quote = CalculateQuote(
                faction,
                destination,
                materials,
                route,
                fullPayload,
                substitute,
                effects.GenericTransferEV_kps);
            FreightCache[key] = quote;
            return quote;
        }

        internal static float EarthTransferTime(
            TIFactionState faction,
            TIGameState destination)
        {
            return EarthDeliveryTime(
                faction,
                destination,
                LogisticsDeliveryKind.HabModule);
        }

        internal static float EarthDeliveryTime(
            TIFactionState faction,
            TIGameState destination,
            LogisticsDeliveryKind deliveryKind)
        {
            float trajectoryDays = TISpaceObjectState
                .GenericTransferTimeFromEarthsSurface_d(
                    faction,
                    destination);
            return EffectiveDeliveryTime(
                faction,
                trajectoryDays,
                deliveryKind);
        }

        internal static float EffectiveDeliveryTime(
            TIFactionState faction,
            float trajectoryDays,
            LogisticsDeliveryKind deliveryKind)
        {
            Context context = deliveryKind == LogisticsDeliveryKind.Probe
                ? Context.ProbeTransferTime
                : Context.GenericModuleTransferTime;
            return trajectoryDays + TIEffectsState.SumEffectsModifiers(
                context,
                faction,
                trajectoryDays,
                null);
        }

        private static void EnsureRouteCacheCurrent()
        {
            long currentTicks = CurrentTimeTicks();
            if (cacheTopologyGeneration == topologyGeneration &&
                cacheTimeTicks == currentTicks)
            {
                return;
            }

            RouteCache.Clear();
            FreightCache.Clear();
            cacheTopologyGeneration = topologyGeneration;
            cacheTimeTicks = currentTicks;
            unchecked
            {
                routeGeneration++;
            }
        }

        private static long CurrentTimeTicks()
        {
            try
            {
                TIDateTime now = TITimeState.Now();
                return now == null ? 0L : now.ExportTime().Ticks;
            }
            catch (InvalidOperationException)
            {
                return 0L;
            }
            catch (NullReferenceException)
            {
                return 0L;
            }
        }

        private static SourceRegistry GetSourceRegistry(TIFactionState faction)
        {
            SourceRegistry registry;
            if (SourceRegistries.TryGetValue(faction, out registry) &&
                registry.Generation == topologyGeneration)
            {
                return registry;
            }

            registry = new SourceRegistry
            {
                Generation = topologyGeneration
            };
            if (faction != null && faction.habs != null)
            {
                foreach (TIHabState hab in faction.habs)
                {
                    if (hab == null || hab.coreFaction != faction)
                    {
                        continue;
                    }

                    int factoryTier = 0;
                    int dockTier = 0;
                    foreach (TIHabModuleState module in hab.ActiveModules())
                    {
                        TIHabModuleTemplate template = module.moduleTemplate;
                        if (template == null || template.alienModule)
                        {
                            continue;
                        }

                        if (template.constructionModule)
                        {
                            factoryTier = Math.Max(factoryTier, template.tier);
                        }

                        if (template.allowsShipConstruction)
                        {
                            dockTier = Math.Max(dockTier, template.tier);
                        }
                    }

                    if (factoryTier > 0)
                    {
                        registry.Sources.Add(new SourceSummary
                        {
                            Hab = hab,
                            FactoryTier = factoryTier,
                            DockTier = dockTier,
                            ExportTier = HabRebalanceMath.EffectiveExportTier(
                                factoryTier,
                                dockTier)
                        });
                    }
                }
            }

            SourceRegistries[faction] = registry;
            return registry;
        }

        private static HabLogisticsRoute CalculateRoute(
            TIFactionState faction,
            SourceSummary source,
            TIGameState destination,
            bool local)
        {
            if (local)
            {
                return new HabLogisticsRoute
                {
                    OriginHab = source.Hab,
                    FactoryTier = source.FactoryTier,
                    DockTier = source.DockTier,
                    ExportTier = source.FactoryTier
                };
            }

            List<RouteEndpoint> origins = new List<RouteEndpoint>();
            if (source.Hab.IsBase)
            {
                TIHabSiteState sourceSite = source.Hab.habSite;
                if (sourceSite.parentBody.interfaceOrbits != null)
                {
                    foreach (TIOrbitState sourceInterface in
                        sourceSite.parentBody.interfaceOrbits)
                    {
                        origins.Add(new RouteEndpoint
                        {
                            State = sourceInterface,
                            ExtraDeltaV_kps = sourceInterface
                                .DeltaVToReachFromSurface_kps(
                                    sourceSite.latitude)
                        });
                    }
                }
            }
            else
            {
                origins.Add(new RouteEndpoint
                {
                    State = source.Hab
                });
            }

            List<RouteEndpoint> destinations = new List<RouteEndpoint>();
            TIHabSiteState destinationSite = null;
            if (destination.isHabSiteState)
            {
                destinationSite = destination.ref_habSite;
            }
            else if (destination.isHabState && destination.ref_hab.IsBase)
            {
                destinationSite = destination.ref_hab.habSite;
            }

            if (destinationSite != null)
            {
                if (destinationSite.parentBody.interfaceOrbits != null)
                {
                    foreach (TIOrbitState destinationInterface in
                        destinationSite.parentBody.interfaceOrbits)
                    {
                        destinations.Add(new RouteEndpoint
                        {
                            State = destinationInterface,
                            ExtraDeltaV_kps = destinationSite
                                .DeltaVToLandFromInterface_kps(
                                    null,
                                    9.8d,
                                    true,
                                    true)
                        });
                    }
                }
            }
            else if (destination.isSpaceBodyState)
            {
                if (destination.ref_spaceBody.interfaceOrbits != null)
                {
                    foreach (TIOrbitState destinationInterface in
                        destination.ref_spaceBody.interfaceOrbits)
                    {
                        destinations.Add(new RouteEndpoint
                        {
                            State = destinationInterface
                        });
                    }
                }
            }
            else
            {
                destinations.Add(new RouteEndpoint
                {
                    State = destination
                });
            }

            if (origins.Count == 0 || destinations.Count == 0)
            {
                return null;
            }

            double bestTotal = double.PositiveInfinity;
            double bestLaunch = 0d;
            double bestTransfer = 0d;
            double bestLanding = 0d;
            float bestTime = float.PositiveInfinity;
            foreach (RouteEndpoint origin in origins)
            {
                foreach (RouteEndpoint routeDestination in destinations)
                {
                    double transferDeltaV = ReferenceEquals(
                        origin.State,
                        routeDestination.State)
                        ? 0d
                        : TISpaceObjectState.GenericTransferDeltaV_mps(
                            origin.State,
                            routeDestination.State) / 1000d;
                    double totalDeltaV = origin.ExtraDeltaV_kps +
                        transferDeltaV +
                        routeDestination.ExtraDeltaV_kps;
                    float transferTime = TISpaceObjectState
                        .GenericTransferTime_d(
                            faction,
                            origin.State,
                            routeDestination.State);
                    if (totalDeltaV < bestTotal ||
                        Math.Abs(totalDeltaV - bestTotal) < 0.000001d &&
                        transferTime < bestTime)
                    {
                        bestTotal = totalDeltaV;
                        bestLaunch = origin.ExtraDeltaV_kps;
                        bestTransfer = transferDeltaV;
                        bestLanding = routeDestination.ExtraDeltaV_kps;
                        bestTime = transferTime;
                    }
                }
            }

            return new HabLogisticsRoute
            {
                OriginHab = source.Hab,
                FactoryTier = source.FactoryTier,
                DockTier = source.DockTier,
                ExportTier = source.ExportTier,
                LaunchDeltaV_kps = bestLaunch,
                TransferDeltaV_kps = bestTransfer,
                LandingDeltaV_kps = bestLanding,
                TotalDeltaV_kps = bestTotal,
                TrajectoryTime_days = bestTime
            };
        }

        private static bool IsBetter(
            HabLogisticsRoute candidate,
            HabLogisticsRoute current)
        {
            const double tolerance = 0.000001d;
            if (candidate.TotalDeltaV_kps + tolerance < current.TotalDeltaV_kps)
            {
                return true;
            }

            if (Math.Abs(candidate.TotalDeltaV_kps - current.TotalDeltaV_kps) >
                tolerance)
            {
                return false;
            }

            if (candidate.TrajectoryTime_days < current.TrajectoryTime_days)
            {
                return true;
            }

            if (candidate.TrajectoryTime_days > current.TrajectoryTime_days)
            {
                return false;
            }

            return candidate.OriginHab.ID.CompareTo(
                current.OriginHab.ID) < 0;
        }

        private static HabFreightQuote CalculateQuote(
            TIFactionState faction,
            TIGameState destination,
            ResourceCostBuilder materials,
            HabLogisticsRoute route,
            bool fullPayload,
            bool substitute,
            float genericTransferEV_kps)
        {
            float conversion = TemplateManager.global.spaceResourceToTons;
            HabFreightQuote quote = new HabFreightQuote(
                MaterialResources.Length)
            {
                Route = route
            };
            float[] required = MaterialValues(materials);
            float[] remaining = new float[required.Length];
            float earthMaterialUnits = 0f;
            float totalMaterialUnits = 0f;

            for (int index = 0; index < required.Length; index++)
            {
                float amount = Math.Max(0f, required[index]);
                totalMaterialUnits += amount;
                FactionResource resource = MaterialResources[index];
                bool replaceable = !TIResourcesCost.irreplaceableSpaceResources
                    .Contains(resource);
                float available = Math.Max(
                    0f,
                    faction.GetCurrentResourceAmount(resource));
                if (!substitute || !replaceable || available >= amount)
                {
                    quote.StockpileCosts[index] = amount;
                    remaining[index] = Math.Max(0f, available - amount);
                }
                else
                {
                    quote.StockpileCosts[index] = available;
                    quote.EarthPurchaseCosts[index] = amount - available;
                    earthMaterialUnits += amount - available;
                }
            }

            quote.MaterialMass_tons = totalMaterialUnits / conversion;
            quote.EarthMaterialMass_tons = earthMaterialUnits / conversion;

            if (route.IsEarthFallback)
            {
                float targetEarthUnits = HabRebalanceMath.EarthFallbackMass(
                    quote.MaterialMass_tons,
                    quote.EarthMaterialMass_tons) * conversion;
                float additionalEarthUnits = Math.Max(
                    0f,
                    targetEarthUnits - earthMaterialUnits);
                ShiftStockpileToEarth(quote, additionalEarthUnits);
                quote.EarthMaterialMass_tons = Sum(
                    quote.EarthPurchaseCosts) / conversion;
                quote.SpacePayloadMass_tons = 0f;
            }
            else
            {
                quote.SpacePayloadMass_tons = fullPayload
                    ? quote.MaterialMass_tons
                    : HabRebalanceMath.MandatoryTransportMass(
                        quote.MaterialMass_tons,
                        quote.EarthMaterialMass_tons);
                quote.PropellantMass_tons = HabRebalanceMath.PropellantMass(
                    quote.SpacePayloadMass_tons,
                    route.TotalDeltaV_kps,
                    genericTransferEV_kps);
                AddPropellant(
                    faction,
                    quote,
                    remaining,
                    quote.PropellantMass_tons * conversion,
                    substitute);
            }

            quote.EarthFreightMass_tons = Sum(
                quote.EarthPurchaseCosts) / conversion;
            return quote;
        }

        private static void ShiftStockpileToEarth(
            HabFreightQuote quote,
            float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float replaceableStock = 0f;
            for (int index = 0; index < MaterialResources.Length; index++)
            {
                if (!TIResourcesCost.irreplaceableSpaceResources.Contains(
                    MaterialResources[index]))
                {
                    replaceableStock += quote.StockpileCosts[index];
                }
            }

            if (replaceableStock <= 0f)
            {
                return;
            }

            float remainingShift = Math.Min(amount, replaceableStock);
            for (int index = 0; index < MaterialResources.Length; index++)
            {
                if (TIResourcesCost.irreplaceableSpaceResources.Contains(
                    MaterialResources[index]))
                {
                    continue;
                }

                float share = index == MaterialResources.Length - 1
                    ? remainingShift
                    : Math.Min(
                        quote.StockpileCosts[index],
                        amount * quote.StockpileCosts[index] /
                        replaceableStock);
                share = Math.Min(share, remainingShift);
                quote.StockpileCosts[index] -= share;
                quote.EarthPurchaseCosts[index] += share;
                remainingShift -= share;
                if (remainingShift <= HabRebalanceMath.FractionTolerance)
                {
                    break;
                }
            }

            if (remainingShift > HabRebalanceMath.FractionTolerance)
            {
                for (int index = 0;
                    index < MaterialResources.Length;
                    index++)
                {
                    if (TIResourcesCost.irreplaceableSpaceResources.Contains(
                            MaterialResources[index]) ||
                        quote.StockpileCosts[index] <= 0f)
                    {
                        continue;
                    }

                    float share = Math.Min(
                        quote.StockpileCosts[index],
                        remainingShift);
                    quote.StockpileCosts[index] -= share;
                    quote.EarthPurchaseCosts[index] += share;
                    remainingShift -= share;
                    if (remainingShift <= HabRebalanceMath.FractionTolerance)
                    {
                        break;
                    }
                }
            }
        }

        private static void AddPropellant(
            TIFactionState faction,
            HabFreightQuote quote,
            float[] remaining,
            float propellantUnits,
            bool substitute)
        {
            if (propellantUnits <= 0f)
            {
                return;
            }

            float water = propellantUnits *
                TemplateManager.global.probeWaterPropellantMassFraction;
            float volatiles = propellantUnits *
                TemplateManager.global.probeVolatilesPropellantMassFraction;
            AddPropellantResource(
                faction,
                quote,
                remaining,
                0,
                water,
                substitute);
            AddPropellantResource(
                faction,
                quote,
                remaining,
                1,
                volatiles,
                substitute);
        }

        private static void AddPropellantResource(
            TIFactionState faction,
            HabFreightQuote quote,
            float[] remaining,
            int index,
            float amount,
            bool substitute)
        {
            if (!substitute)
            {
                quote.StockpileCosts[index] += amount;
                return;
            }

            float available = Math.Max(0f, remaining[index]);
            float fromStockpile = Math.Min(available, amount);
            quote.StockpileCosts[index] += fromStockpile;
            quote.EarthPurchaseCosts[index] += amount - fromStockpile;
        }

        private static int MaterialsHash(ResourceCostBuilder materials)
        {
            unchecked
            {
                int hash = 17;
                foreach (float value in MaterialValues(materials))
                {
                    hash = hash * 31 + value.GetHashCode();
                }

                return hash;
            }
        }

        private static int FloatBits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        private static int ResourceBalancesHash(TIFactionState faction)
        {
            unchecked
            {
                int hash = 17;
                foreach (FactionResource resource in MaterialResources)
                {
                    hash = hash * 31 + faction
                        .GetCurrentResourceAmount(resource)
                        .GetHashCode();
                }

                return hash;
            }
        }

        private static float[] MaterialValues(ResourceCostBuilder materials)
        {
            return new[]
            {
                materials.water,
                materials.volatiles,
                materials.metals,
                materials.nobleMetals,
                materials.fissiles,
                materials.antimatter,
                materials.exotics
            };
        }

        private static float Sum(float[] values)
        {
            float total = 0f;
            for (int index = 0; index < values.Length; index++)
            {
                total += Math.Max(0f, values[index]);
            }

            return total;
        }
    }

}
