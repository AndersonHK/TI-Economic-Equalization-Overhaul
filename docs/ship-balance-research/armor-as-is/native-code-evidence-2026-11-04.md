# Native armor code evidence — 2026-11-04

Fresh read-only ILSpy 9.1 type decompilations from `D:/Games/SteamLibrary/steamapps/common/Terra Invicta/TerraInvicta_Data/Managed/Assembly-CSharp.dll`, SHA-256 `4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`. These are selected excerpts, not a modified game source tree. Line numbers refer to the named temporary individual type file. Full decompilations stay in `.tmp/armor-as-is/`. Interpret them with the [as-is report](../armor-system-as-is-2026-11-04.md).

## Armor state and volume

`TISpaceShipState.cs:20-114`

```csharp

	public class ArmorData
	{
		public int maxArmor;

		public int armorValue;

		public float chippedPct;

		public bool damaged
		{
			get
			{
				if (maxArmor > 0)
				{
					if (!(chippedPct > 0f))
					{
						return armorValue < maxArmor;
					}
					return true;
				}
				return false;
			}
		}

		public ArmorData(int armorValue)
		{
			this.armorValue = armorValue;
			maxArmor = armorValue;
			chippedPct = 0f;
		}

		public void RepairArmor()
		{
			armorValue = maxArmor;
			chippedPct = 0f;
		}

		public int ShredArmor(int armorValueToShred)
		{
			int num = armorValue;
			armorValue -= armorValueToShred;
			int num2 = armorValueToShred;
			if (armorValue < 0)
			{
				num2 = num;
				armorValue = 0;
			}
			return Mathf.Max(0, armorValueToShred - num2);
		}

		public float ChipArmor(float chip)
		{
			float num = 1f - chippedPct;
			chippedPct += chip;
			float num2 = chip;
			if (chippedPct >= 1f)
			{
				chippedPct = 1f;
				num2 = num;
			}
			return Mathf.Max(0f, chip - num2);
		}

		public float GetArmorIntegrity()
		{
			if (maxArmor != 0)
			{
				return (float)armorValue * (1f - chippedPct) / (float)maxArmor;
			}
			return 0f;
		}

		public static float GetArmorFacingVolume_m3(TISpaceShipState ship, ArmorFacing facing)
		{
			return facing switch
			{
				ArmorFacing.Nose => ship.noseArmorTemplate.armor_section_volume(ship.template.noseArmorValue, ship.hull.length_m, ship.hull.width_m, ship.lateralArmorThickness_m, lateral: false),
				ArmorFacing.Tail => ship.tailArmorTemplate.armor_section_volume(ship.template.tailArmorValue, ship.hull.length_m, ship.hull.width_m, ship.lateralArmorThickness_m, lateral: false),
				ArmorFacing.Core => ship.hull.volume_m3,
				_ => ship.lateralArmorTemplate.armor_section_volume(ship.template.lateralArmorValue, ship.hull.length_m, ship.hull.width_m, ship.lateralArmorThickness_m, lateral: true) / 2f,
			};
		}

		public static TIShipArmorTemplate GetArmorTemplate(TISpaceShipState ship, ArmorFacing facing)
		{
			return facing switch
			{
				ArmorFacing.Nose => ship.noseArmorTemplate,
				ArmorFacing.Tail => ship.tailArmorTemplate,
				ArmorFacing.Core => null,
				_ => ship.lateralArmorTemplate,
			};
		}
	}


```

## Facing setup

`TISpaceShipState.cs:1774-1788`

```csharp

	public static List<IHullSection> SetUpArmorSections(TISpaceShipState ship)
	{
		float num = ((!TIGlobalValuesState.GlobalValues.scenarioCustomizations.cinematicCombatRealismScale) ? (ship.hull.baseArmorCapAngleCoverage_deg_realisticScaling * 2f) : 30f);
		float bounds = 90f - num;
		List<IHullSection> list = new List<IHullSection>(3);
		HullSection item = new HullSection(ship, new Facing(0f, num, ArmorFacing.Nose));
		HullSection hullSection = new HullSection(ship);
		hullSection.AddFacing(new Facing(90f, bounds, ArmorFacing.Right));
		hullSection.AddFacing(new Facing(-90f, bounds, ArmorFacing.Left));
		HullSection item2 = new HullSection(ship, new Facing(180f, num, ArmorFacing.Tail));
		list.Add(item);
		list.Add(hullSection);
		list.Add(item2);
		return list;
	}

```

## Armor resolution

`TISpaceShipState.cs:4003-4222`

```csharp

	public float AbsorbAndApplyArmorDamage(TIShipWeaponTemplate attackingWeapon, ArmorFacing facing, float range_km, float damageAmount, float chippingAmount, DamageType damageType, float angle, TIFactionState attackingFaction, out float internalDamageAssessedHere, out float appliedRadiationDamage, int shreddingAmount = 0)
	{
		ModuleDataEntry partToDamage = GetPartToDamage(ShipSystem.Radiators);
		internalDamageAssessedHere = 0f;
		appliedRadiationDamage = 0f;
		bool flag = fleet?.bombarding ?? false;
		if ((radiatorsExtended || radiatorsExtending || radiatorsRetracting) && (!flag || !canEverRetractRadiators) && !PartDestroyed(partToDamage) && attackingWeapon != null && TIUtilities.RandomFloatValue() * 100f < (float)template.radiatorTemplate.vulnerability * ((radiatorsExtending || radiatorsRetracting) ? 0.5f : 1f))
		{
			float num = damageAmount + chippingAmount;
			if ((attackingWeapon == null || attackingWeapon.isParticleWeapon) && attackingWeapon?.ref_particleWeapon != null)
			{
				num *= attackingWeapon.ref_particleWeapon.heatFraction;
				damageAmount -= num;
			}
			else
			{
				damageAmount = num * attackingWeapon.chipping(range_km);
			}
			ApplyDamageToPart(partToDamage, damageAmount, out var internalDamageApplied);
			internalDamageAssessedHere += internalDamageApplied;
			spaceCombatValueDataDirty = true;
			if (shreddingAmount == 0 && !attackingWeapon.isParticleWeapon && (facing == ArmorFacing.Nose || facing == ArmorFacing.Tail || TIUtilities.RandomFloatValue() < 0.25f))
			{
				return internalDamageApplied;
			}
		}
		if (facing == ArmorFacing.Tail && 180f - Mathf.Abs(angle) < 5f && TIUtilities.RandomFloatValue() < 0.25f)
		{
			ApplyDamageToPart(GetPartToDamage(ShipSystem.Drive), damageAmount, out var internalDamageApplied2);
			internalDamageAssessedHere += internalDamageApplied2;
		}
		if (shreddingAmount > 0)
		{
			ApplyDamageToSystem(ShipSystem.Sensors, 0.5f, out var damagePointsToApply);
			internalDamageAssessedHere += damagePointsToApply;
		}
		else if (attackingWeapon != null && attackingWeapon.isBeamWeapon && TIUtilities.RandomFloatValue() < GetSystemFunction(ShipSystem.Sensors) * 0.1f)
		{
			ApplyDamageToSystem(ShipSystem.Sensors, TIUtilities.RandomFloatValue() * 0.1f, out var damagePointsToApply2);
			internalDamageAssessedHere += damagePointsToApply2;
		}
		else if (TIUtilities.RandomFloatValue() < GetSystemFunction(ShipSystem.Sensors) * 0.01f)
		{
			ApplyDamageToSystem(ShipSystem.Sensors, TIUtilities.RandomFloatValue() * 0.1f, out var damagePointsToApply3);
			internalDamageAssessedHere += damagePointsToApply3;
		}
		TIShipArmorTemplate armorTemplate = ArmorData.GetArmorTemplate(this, facing);
		int num2 = 0;
		int num3 = 1;
		bool flag2 = attackingWeapon != null && attackingWeapon.isMissileWeapon && attackingWeapon.ref_missileWeapon.warheadClass == WarheadClass.Fragmentation;
		if (flag2)
		{
			num3 = Mathf.RoundToInt(attackingWeapon.ref_missileWeapon.warheadMass_kg / 5f);
		}
		bool flag3 = false;
		float num4 = num3;
		float num5 = 0f;
		for (int i = 0; i < num3; i++)
		{
			if (TIUtilities.RandomFloatValue() < armor[facing].chippedPct)
			{
				num2++;
				flag3 = true;
				if (num3 == 1)
				{
					damageAmount += chippingAmount;
				}
				else
				{
					num5 += chippingAmount / (float)num3;
				}
				num4 -= 1f;
			}
		}
		chippingAmount *= num4 / (float)num3;
		if (chippingAmount > 0f)
		{
			float armorFacingVolume_m = ArmorData.GetArmorFacingVolume_m3(this, facing);
			if (armorFacingVolume_m > 0f && armor[facing].chippedPct < 1f && armor[facing].armorValue > 0)
			{
				chippingAmount *= armorTemplate.GetSpecialtyModifiers(ArmorSpecialty.ChippingResistance);
				float chip = chippingAmount / armorFacingVolume_m;
				chippingAmount = armor[facing].ChipArmor(chip);
				if (armor[facing].chippedPct >= 1f)
				{
					flag3 = true;
					num2 = ((!flag2) ? 1 : num3);
				}
			}
			else
			{
				damageAmount += chippingAmount;
				chippingAmount = 0f;
				flag3 = true;
				num2 = ((!flag2) ? 1 : num3);
			}
		}
		float num6 = 0f;
		if (shreddingAmount > 0)
		{
			spaceCombatValueDataDirty = true;
			damageAmount = armor[facing].ShredArmor(shreddingAmount);
			if (armor[facing].armorValue <= 0)
			{
				flag3 = true;
				num2 = ((!flag2) ? 1 : num3);
			}
			else
			{
				num6 = (float)shreddingAmount * armor[facing].chippedPct;
				if (num6 > 0f)
				{
					flag3 = true;
					num2 = ((!flag2) ? 1 : num3);
				}
			}
		}
		float num7 = armor[facing].armorValue;
		if (attackingWeapon != null && !flag3 && num7 > 0f && armor[facing].chippedPct < 1f)
		{
			if (!TIGlobalValuesState.Customizations.cinematicCombatRealismScale)
			{
				switch (facing)
				{
				case ArmorFacing.Right:
					num7 /= Mathf.Cos((float)Math.PI / 180f * Mathf.Abs(90f - angle));
					break;
				case ArmorFacing.Left:
					num7 /= Mathf.Cos((float)Math.PI / 180f * Mathf.Abs(-90f - angle));
					break;
				}
			}
			if (attackingWeapon.weaponClass == WeaponClass.Laser)
			{
				num7 = attackingWeapon.ref_laserWeapon.ModifyArmorValueForLaserShot(range_km, num7);
				damageAmount *= armorTemplate.GetSpecialtyModifiers(ArmorSpecialty.LaserResistance);
			}
			if (damageType == DamageType.Kinetic)
			{
				damageAmount *= armorTemplate.GetSpecialtyModifiers(ArmorSpecialty.KineticsResistance);
			}
		}
		damageAmount += TIEffectsState.SumEffectsModifiers(Context.DamageReductionAgainstAllShips, faction, damageAmount);
		if ((object)attackingFaction != null && attackingFaction.IsAlienFaction)
		{
			damageAmount += TIEffectsState.SumEffectsModifiers(Context.DamageReductionAgainstAlienShips, faction, damageAmount);
		}
		if (flag3)
		{
			spaceCombatValueDataDirty = true;
			if (flag2)
			{
				spaceCombatValueDataDirty = true;
				float num8 = (float)num2 / (float)num3;
				float num9 = num8 * damageAmount + num5;
				if (num4 > 0f)
				{
					float num10 = (1f - num8) * damageAmount / num4 - num7;
					if (num10 > 0f)
					{
						num9 += num10 * num4;
					}
				}
				return Mathf.Max(0f, num9);
			}
			if (num6 > 0f)
			{
				return num6;
			}
		}
		else if (flag2)
		{
			float num11 = damageAmount / num4 - num7;
			damageAmount = 0f;
			if (num11 > 0f)
			{
				damageAmount += num11 * num4;
				spaceCombatValueDataDirty = true;
			}
			return damageAmount;
		}
		if (damageType == DamageType.ParticleBeam && attackingWeapon != null)
		{
			float num12 = damageAmount * attackingWeapon.ref_particleWeapon.heatFraction;
			float num13 = damageAmount * attackingWeapon.ref_particleWeapon.xRayFraction;
			float num14 = damageAmount * attackingWeapon.ref_particleWeapon.baryonFraction;
			if (!flag3)
			{
				num13 = Mathf.Max(0f, num13 * Mathf.Min(0.0625f, Mathf.Pow(0.5f, armorTemplate.armor_section_thickness_m(armor[facing].armorValue) * 100f / armorTemplate.xRayHalfValue_cm)));
				num14 = Mathf.Max(0f, num14 * Mathf.Min(0.0625f, Mathf.Pow(0.5f, armorTemplate.armor_section_thickness_m(armor[facing].armorValue) * 100f / armorTemplate.baryonicHalfValue_cm)));
			}
			num13 += SumOfficerEffectsModifiers(OfficerEffectType.RadiationDamageReduction, num13);
			num14 += SumOfficerEffectsModifiers(OfficerEffectType.RadiationDamageReduction, num14);
			appliedRadiationDamage = ApplyInternalRadiationDamage(num13, num14, facing, facing, angle, Mathf.Min(1f, attackingWeapon.ref_particleWeapon.SpotSurfaceArea_m2(range_km) / GetCrossSectionalArea_m2(angle)));
			internalDamageAssessedHere += appliedRadiationDamage;
			damageAmount = num12;
		}
		float num15 = 0f;
		if (damageAmount > 0f)
		{
			num15 = Mathf.Max(0f, damageAmount + (flag3 ? chippingAmount : 0f) - (flag3 ? 0f : num7));
		}
		if (num15 > 0f || internalDamageAssessedHere > 0f)
		{
			spaceCombatValueDataDirty = true;
		}
		return num15;
	}

	public void ApplyDamage(TIShipWeaponTemplate attackingWeapon, ArmorFacing facing, float range_km, float damageAmount, float chippingAmount, DamageType damageType, float angle, TIFactionState attackingFaction, out float internalDamageAssessedHere, out float appliedRadiationDamage, int shreddingAmount = 0)
	{
		float num = AbsorbAndApplyArmorDamage(attackingWeapon, facing, range_km, damageAmount, chippingAmount, damageType, angle, attackingFaction, out internalDamageAssessedHere, out appliedRadiationDamage, shreddingAmount);
		if (num > 0f)
		{
			float num2 = num * InternalDamageModifier();
			bool explosion = damageType == DamageType.Explosive || damageType == DamageType.Nuclear || shreddingAmount > 0;
			ApplyInternalDamage(facing, facing, num2, explosion, chippingAmount, angle);
			internalDamageAssessedHere += num2;
		}
	}

```

## Internal modifier

`TISpaceShipState.cs:3859-3871`

```csharp

	public float InternalDamageModifier()
	{
		float num = 1f + SumOfficerEffectsModifiers(OfficerEffectType.InternalDamageTaken, 1f);
		foreach (ModuleDataEntry utilityModule in utilityModules)
		{
			TIUtilityModuleTemplate ref_utilityModule = utilityModule.moduleTemplate.ref_utilityModule;
			if (ref_utilityModule != null && ref_utilityModule.specialModuleRules.Contains(SpecialModuleRule.ComponentArmor))
			{
				return num * (1f - (1f - utilityModule.moduleTemplate.ref_utilityModule.specialModuleValue) * GetPartFunction(utilityModule));
			}
		}
		return num;
	}

```

## Internal routing

`TISpaceShipState.cs:4288-4359`

```csharp

	public void ApplyInternalDamage(ArmorFacing facing, ArmorFacing originalFacing, float damageAmount, bool explosion, float weaponChipValue, float angle)
	{
		if (internalDamageTables.Count == 0)
		{
			return;
		}
		bool secondaryExplosion = false;
		float num = damageAmount;
		IEnumerable<KeyValuePair<ShipSystem, float>> enumerable = internalDamageTables[facing].Where((KeyValuePair<ShipSystem, float> x) => !SystemAsAWholeCanBeDamaged(x.Key) || GetSystemFunction(x.Key) > 0f);
		if (enumerable.Any())
		{
			ShipSystem key = enumerable.SelectRandomWeightedItem((KeyValuePair<ShipSystem, float> x) => x.Value).Key;
			if (key == ShipSystem.Propellant)
			{
				float num2 = propellant_tons / template.propellantMass_tons;
				_ = template.propellantTanks;
				int num3 = (int)(num / 1f);
				if (num3 > template.propellantTanks)
				{
					num3 = template.propellantTanks;
				}
				for (int num4 = 0; num4 < num3; num4++)
				{
					if (TIUtilities.RandomFloatValue() < num2)
					{
						ChangePropellant_tons(-100f);
						num -= 1f;
						if (num < 0f)
						{
							num = 0f;
						}
					}
				}
				SetPropulsionValuesDirty(immediate: true);
			}
			else if (DirectDamageableSystems.Contains(key))
			{
				num = ApplyDamageToSystem(key, num, out var _);
			}
			else
			{
				ModuleDataEntry partToDamage = GetPartToDamage(key);
				num = ApplyDamageToPart(partToDamage, num, out secondaryExplosion, out var _);
			}
		}
		if (num > 0f && !ShipDestroyed())
		{
			bool outTheOtherSide;
			ArmorFacing nextInteralDamageLocation = GetNextInteralDamageLocation(facing, originalFacing, angle, explosion || secondaryExplosion, weaponChipValue, out outTheOtherSide);
			if (!outTheOtherSide)
			{
				ApplyInternalDamage(nextInteralDamageLocation, originalFacing, num, explosion || secondaryExplosion, weaponChipValue, angle);
			}
			else
			{
				float chip = num * weaponChipValue * ArmorData.GetArmorTemplate(this, facing).GetSpecialtyModifiers(ArmorSpecialty.ChippingResistance) / ArmorData.GetArmorFacingVolume_m3(this, facing);
				armor[facing].ChipArmor(chip);
				num = 0f;
				GameControl.eventManager.TriggerEvent(new ShipDamageGoesOutTheOtherSide(this, facing), null, this);
			}
		}
		if (damagedSystems.Count > 0)
		{
			_ = template.size;
			float maxExclusive = (SystemDestroyed(ShipSystem.NoseStructure) ? (50 + (int)template.size * 50) : (20 + (int)template.size * 20));
			float num5 = (SystemDestroyed(ShipSystem.TailStructure) ? (50 + (int)template.size * 50) : (20 + (int)template.size * 20));
			Mathf.Clamp(2f + (float)damagedSystems.Count * 2f, 2f, 50f);
			damagePoints.Add(DamageLayer.AddDamagePointInternal(new Vector3(0f, 0f, TIUtilities.RandomRange(0f - num5, maxExclusive)), Mathf.Clamp((float)damagedSystems.Count * 5f, 1f, 50f), DamageType.Thermal));
			damageVisualizationDirty = true;
		}
		spaceCombatValueDataDirty = true;
	}

```

## Repair accounting

`TISpaceShipState.cs:5432-5456`

```csharp

	public TIResourcesCost ArmorFacingRepairCost(ArmorFacing facing, TIFactionState designingFaction, TIHabState repairingHab, bool testBoost)
	{
		if (armor[facing].maxArmor > 0 && (armor[facing].chippedPct > 0f || armor[facing].armorValue < armor[facing].maxArmor))
		{
			TIResourcesCost tIResourcesCost = new TIResourcesCost();
			switch (facing)
			{
			case ArmorFacing.Nose:
				tIResourcesCost = template.noseArmorBuildCost;
				break;
			case ArmorFacing.Right:
			case ArmorFacing.Left:
				tIResourcesCost = template.lateralArmorBuildCost;
				break;
			case ArmorFacing.Tail:
				tIResourcesCost = template.tailArmorBuildCost;
				break;
			}
			int num = (armor[facing].maxArmor - armor[facing].armorValue) / armor[facing].maxArmor;
			TIResourcesCost tIResourcesCost2 = tIResourcesCost.MultiplyCost(num);
			tIResourcesCost2.SetCompletionTime_Days(1f * (float)num);
			TIResourcesCost tIResourcesCost3 = tIResourcesCost.MultiplyCost(armor[facing].chippedPct * ArmorData.GetArmorTemplate(this, facing).repairCostMultipler);
			tIResourcesCost3.SetCompletionTime_Days(1f * armor[facing].chippedPct);
			tIResourcesCost2.SumCosts_NoDuration(tIResourcesCost3);
			tIResourcesCost2.SetCompletionTime_Days(tIResourcesCost2.completionTime_days + tIResourcesCost3.completionTime_days);

```

## Laser optics and chipping

`TILaserWeaponTemplate.cs:41-59`

```csharp

	private float mirrorDiameter_m => mirrorRadius_m * 2f;

	private float mirrorRadius_m => (float)mirrorRadius_cm / 100f;

	private float wavelength_m => (float)wavelength_nm / 1E+09f;

	public float ModifyArmorValueForLaserShot(float range_km, float baseArmorValue, float armorEffectiveness = -1f)
	{
		if (armorEffectiveness < 0f)
		{
			armorEffectiveness = ArmorEffectivenessAtRange(range_km);
		}
		return Mathf.Pow(baseArmorValue, 1.5f) * armorEffectiveness;
	}

	public float ArmorEffectivenessAtRange(float range_km)
	{
		return SpotAreaPrecise_m2(range_km) / 0.005f;
	}

```

`TILaserWeaponTemplate.cs:122-161`

```csharp

	public override float DamageAtRange_MJ(float range_km, float targetCrossSectionalArea_m2, CombatWeaponCarrierState attackingShip, float finalVelocity_kps = 0f, float warheadMass_kg = 0f, bool applyChipping = true)
	{
		float num = (float)shotPower_MJ + (attackingShip?.ref_shipCarrier()?.GetBonusPowerForWeapon_MJ(this)).GetValueOrDefault();
		num += TIEffectsState.SumEffectsModifiers(Context.ShipLaserDamage, attackingShip?.GetFaction(), num) + (attackingShip?.ref_shipCarrier()?.SumOfficerEffectsModifiers(OfficerEffectType.BeamDamage, num)).GetValueOrDefault() + (attackingShip?.ref_shipCarrier()?.SumOfficerEffectsModifiers(OfficerEffectType.GlobalDamage, num)).GetValueOrDefault();
		float num2 = SpotAreaPrecise_m2(range_km);
		if (num2 > targetCrossSectionalArea_m2)
		{
			num *= targetCrossSectionalArea_m2 / num2;
		}
		return num;
	}

	public override float BaseDamageAtRange_MJ(float range_km, bool applyChipping = true)
	{
		return shotPower_MJ;
	}

	public override float chipping(float range_km)
	{
		return Mathf.Clamp(ArmorEffectivenessAtRange(range_km) / 1000f, 0f, 0.25f);
	}

	public override float EffectiveRangeAgainstProjectiles_km()
	{
		return Mathf.Max(200f, attackMode ? (targetingRange_km / 3f) : targetingRange_km);
	}

	private float SpotDiameterPrecise_m(float range_km)
	{
		if (_SpotDiameterPreciseFactor_m == -1f)
		{
			_SpotDiameterPreciseFactor_m = 1000f * Mathf.Sqrt(Mathf.Pow(1.22f * wavelength_m * beam_quality, 2f) + Mathf.Pow(2f * jitter_Rad * mirrorDiameter_m, 2f)) / mirrorDiameter_m;
		}
		return range_km * _SpotDiameterPreciseFactor_m;
	}

	private float SpotAreaPrecise_m2(float range_km)
	{
		return (float)Math.PI / 4f * SpotDiameterPrecise_m(range_km) * SpotDiameterPrecise_m(range_km);
	}

```

## Damage split and randomization

`TIShipWeaponTemplate.cs:405-414`

```csharp

	public virtual DamageBreakdown DamageAtRange_points(float range_km, float defenderCrossSectionalArea_m2, CombatWeaponCarrierState attacker = null, float finalVelocity_kps = 0f, float warheadMass_kg = 0f, bool applyChipping = true)
	{
		float num = DamageAtRange_MJ(range_km, defenderCrossSectionalArea_m2, attacker, finalVelocity_kps, warheadMass_kg, applyChipping: false) / 20f;
		if (applyChipping)
		{
			float num2 = chipping(range_km);
			return new DamageBreakdown(num * (1f - num2), num * num2);
		}
		return new DamageBreakdown(num, 0f);
	}

```

## Damage randomization

`Damage.cs:25-41`

```csharp

	private float RandomizedDamageAmount(float damageValue)
	{
		return damageValue * 0.8f + damageValue * TIUtilities.RandomRange(0f, 0.4f);
	}

	public Damage(TIShipWeaponTemplate weapon, float range_km, DamageType type, float amount, float chippingAmount, int shreddingAmount, TIFactionState applyingFaction)
	{
		this.weapon = weapon;
		this.range_km = range_km;
		this.type = type;
		this.amount = amount;
		this.chippingAmount = chippingAmount;
		this.shreddingAmount = shreddingAmount;
		this.applyingFaction = applyingFaction;
		this.amount = RandomizedDamageAmount(this.amount);
		this.chippingAmount = RandomizedDamageAmount(this.chippingAmount);
	}

```

## Critical display

`CombatantListItemController.cs:265-287`

```csharp

	private IEnumerator ShowHitData(float rawDamage, float penetratedDamage, float radiationDamage)
	{
		float num = rawDamage - penetratedDamage;
		rawDamageTxt.SetText(TIUtilities.FormatSmallNumber(rawDamage, 1));
		absorbedDamageTxt.SetText(TIUtilities.FormatSmallNumber(num, 1));
		penetratedDamageTxt.SetText(TIUtilities.FormatSmallNumber(penetratedDamage), 1f);
		if (num < 0f)
		{
			absorbedDamageTxt.color = redDamageColor;
			critText.gameObject.SetActive(value: true);
		}
		else
		{
			absorbedDamageTxt.color = normalDamageColor;
			critText.gameObject.SetActive(value: false);
		}
		if (radiationDamage > 0f)
		{
			radiationDamageText.SetText(TIUtilities.FormatSmallNumber(radiationDamage), 0f);
			radiationDamageImage.gameObject.SetActive(value: true);
			radiationDamageText.gameObject.SetActive(value: true);
			critText.gameObject.SetActive(value: false);
		}

```

## Spatial information loss

`Hull.cs:168-187`

```csharp

	public float ApplyDamage(DamageSource source, Transform transform)
	{
		Vector3 normalized = (source.hitPosition - transform.position).normalized;
		float angle = Vector3.SignedAngle(transform.forward, normalized, Vector3.up);
		float num = 0f;
		foreach (IHullSection section in sections)
		{
			if (section.Contains(angle))
			{
				Damage damage = source.damage;
				if ((damage.amount > 0f || damage.chippingAmount > 0f || (float)damage.shreddingAmount > 0f) && !IsDestroyed())
				{
					damage = section.ApplyDamage(damage, angle, out var internalDamageApplied);
					num += internalDamageApplied;
				}
				break;
			}
		}
		return num;
	}

```

## Facing to ship state

`HullSection.cs:46-59`

```csharp


	public Damage ApplyDamage(Damage damage, float angle, out float internalDamageApplied)
	{
		Facing obj = facings.Single((Facing f) => f.Contains(angle));
		internalDamageApplied = 0f;
		if (Error.IsNull(obj))
		{
			return Damage.None;
		}
		float amount = damage.amount;
		shipState.ApplyDamage(damage.weapon, obj.armorFacing, damage.range_km, amount, damage.chippingAmount, damage.type, angle, damage.applyingFaction, out internalDamageApplied, out var appliedRadiationDamage, damage.shreddingAmount);
		GameControl.eventManager.TriggerEvent(new ShipArmorFacingStruckInCombat(shipState, obj.armorFacing, damage.weapon, amount, internalDamageApplied, appliedRadiationDamage), null, shipState);
		return Damage.None;
	}

```

## Station armor

`CombatHabModuleController.cs:292-334`

```csharp


	public static float ApplyDamage(TIHabModuleTemplate moduleTemplate, DamageSource source, TIShipArmorTemplate armorTemplate, float baseHitPoints, float irradiatedMultiplier, ref float armor, ref float hitPoints, out float absorbedDamage, IEnumerable<IWeapon> weapons = null)
	{
		float num = source.damage.amount;
		absorbedDamage = 0f;
		float num2 = armor;
		TIShipWeaponTemplate weapon = source.damage.weapon;
		if (weapon.isLaserWeapon)
		{
			num2 = weapon.ref_laserWeapon.ModifyArmorValueForLaserShot(source.damage.range_km, num2);
		}
		if (source.damage.type == DamageType.ParticleBeam)
		{
			float num3 = num * weapon.ref_particleWeapon.heatFraction;
			float num4 = num * weapon.ref_particleWeapon.xRayFraction;
			float num5 = num * weapon.ref_particleWeapon.baryonFraction;
			float a = 0.0625f * irradiatedMultiplier;
			num4 = Mathf.Max(0f, num4 * Mathf.Min(a, Mathf.Pow(0.5f, armorTemplate.armor_section_thickness_m(armor) * 100f / armorTemplate.xRayHalfValue_cm)));
			num5 = Mathf.Max(0f, num5 * Mathf.Min(a, Mathf.Pow(0.5f, armorTemplate.armor_section_thickness_m(armor) * 100f / armorTemplate.baryonicHalfValue_cm))) * 5f;
			float num6 = num4 + num5;
			if (weapons != null && num6 >= TIUtilities.RandomFloatValue() && weapons.SelectRandomItem() is Weapon weapon2)
			{
				weapon2.EnterCooldown(downFired: false, cooldownByBollix: false, cooldownByDisable: true, 2 + (int)num6);
			}
			num = num3;
		}
		if (TIUtilities.RandomFloatValue() < hitPoints / baseHitPoints)
		{
			if (num2 > num)
			{
				num2 = num;
			}
			num -= num2;
			absorbedDamage = num2;
		}
		hitPoints -= num;
		return num;
	}

	public void UpdateHab()
	{
		UpdateIsMissileSaturated();
	}

```

## Burst damage conversion

`MissileController.cs:37-57`

```csharp

	}

	public class BurstDamage : DamageSource
	{
		public BurstDamage(Vector3 origin, Vector3 hitPosition, CombatWeaponCarrierState attacker, IDamageable target, TIMissileTemplate missileTemplate, TIFactionState launchingFaction, float damageValue)
		{
			base.attacker = attacker;
			base.hitPosition = hitPosition;
			if (target.damageableType == IDamageableType.StationModule)
			{
				base.damage = new Damage(missileTemplate, 0f, DamageType.Thermal, damageValue * 3f, 0f, 0, launchingFaction);
			}
			else if (missileTemplate.warheadClass == WarheadClass.ShapedNuclear && missileTemplate.shapedChargeAngle < 1f)
			{
				base.damage = new Damage(missileTemplate, 0f, DamageType.Thermal, damageValue, 0f, 0, launchingFaction);
			}
			else
			{
				base.damage = new Damage(missileTemplate, 0f, DamageType.Thermal, 0f, 0f, (int)damageValue, launchingFaction);
			}
		}

```
