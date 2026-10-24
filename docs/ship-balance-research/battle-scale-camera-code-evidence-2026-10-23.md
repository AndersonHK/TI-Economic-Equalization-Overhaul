# Battle scale and camera: code evidence

2026-10-23. Static inspection only. Native excerpts were freshly decompiled with ILSpy 9.1.0.7988 from the installed game DLL. Line numbers identify the individual type decompilations, not the older combined decompilation. Ellipses between ranges mean omitted code. No game, configuration, source-code or deployed-package changes were made.

Companion reports: [battle scale](rendered-battle-scale-and-collision-report-2026-10-23.md) and [camera controls](combat-camera-fine-control-report-2026-10-23.md).

## Baseline

The repository and installed mod both identify release 0.10.8; repository compatibility documentation identifies Terra Invicta 1.0.53b. The deployed DLL matches the repository package DLL byte for byte. Installed `Settings.xml` has `shipBalance/enabled=true`. This is on-disk evidence, not proof of patches loaded into a running game. The current campaign/skirmish cinematic-scale flag was not read.

| Input | SHA-256 |
|---|---|
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\AssetBundles\ships` (matches retained turret survey) | `F1804254B5C6F2C2FFBF78C333738F462BAA18023AC5AACF9A8EACAA4D9F09A4` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\DLC_Content\DarkSkies\AssetBundles\ships_prm` (matches retained turret survey) | `58F8B233CD7814F3CD536B46725369F69D6EC58AA260E580DE9D1E1268CF5D63` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\AssetBundles\shipweapons` (matches retained turret survey) | `9DFCF8E93536DC400A00428AF2D6C32226BAC44F1ECD88B874D8F946E9EA912E` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\Managed\Assembly-CSharp.dll` | `4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\Assembly\TIEconomyMod.dll` | `483727F3FB55435FCBCAED420128176AADC41946C7AA5E311455AD614D377A87` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\Mods\Enabled\Economic Equalization Overhaul\Assembly\TIEconomyMod.dll` | `483727F3FB55435FCBCAED420128176AADC41946C7AA5E311455AD614D377A87` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIGlobalConfig.json` | `415113FADEECB4D67FA79F34275E026396C3FE914B722D936D5ADCF737B07F4E` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIGlobalConfig.json` | `0BD72C41DFE79350D24117785568F2D27D102FC63422E8D0D09A176ACB2541E4` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\Patches\ProjectileCollisionPatches.cs` | `7C86F4979F4E185A31C690F0F061642B469214D4EC1F1753762D420DAD8097C4` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\Core\ProjectileCollisionMath.cs` | `EA3474443CC9AC7A4C647DF9DC17339C97285986BAF7627F8FBCE145C68182EC` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\Core\ProjectileGeometryRegistry.cs` | `958069D576B72351B34F8362DC4D916766FA1B2236A535497D7EB33F83C8D9C1` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\Patches\WeaponCadencePatches.cs` | `4B6B4AC04A5089EBBBF6AA73585B11172F41D95AC7032AC8E80554046E396697` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIGunTemplate.json` | `1D7093358EC282391003CB11E6566CC09258881F4EA2C10179FDE9AFD6723662` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIGunTemplate.json` | `62E0B9BDB6EEE4257DE0EF69335CC153104335789978DAF01F9CAF8FC1DCDF73` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIMagneticGunTemplate.json` | `DB4FCE6D3B9AE8AB31B10D2D0CDEFF1A18079C964FA545E7D3C0F127167BA5E3` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TIMagneticGunTemplate.json` | `09870E5530E7B51FD8D9D08DA985B17C7E20B98E547651DE7B4269A2C0C49176` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TILaserWeaponTemplate.json` | `EB2953A9B5C747E9392C891A8DDFF837BA65A129F138C334843D2EA295A76C32` |
| `C:\Users\imper\Documents\GitHub\TI-Economic-Equalization-Overhaul\TIEconomyMod\ModFiles\TILaserWeaponTemplate.json` | `FFDD44E9B41A65608A0B60E5F60070B11AAE81A4EF3C8CDB7472F6CABCFC7ED6` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIParticleWeaponTemplate.json` | `8E65189EE90D07F628D8CB5393374AD4DC40E348E67E4A1E7B30B32FEC926F16` |
| `D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\StreamingAssets\Templates\TIPlasmaWeaponTemplate.json` | `53D756601396A4F282940B5632CBA7D24C1ACEE4BCE10F94F93AA343417159B8` |

## S1 Combat scale and world units

Native type: `PavonisInteractive.TerraInvicta.SpaceCombatManager`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombatManager.decompiled.cs`.

Lines 262–264:

```csharp
	public float modelScalingFactor => 0.01f * GetScalingAdjustmentFactor();

	public float projectileScalingFactor => 0.025f * GetScalingAdjustmentFactor();
```

Lines 294–355:

```csharp
	public static float GetScalingAdjustmentFactor()
	{
		return _cachedScalingAdjustmentFactor;
	}

	public static void SetScalingAdjustmentFactor()
	{
		if (GameStateManager.GlobalValues() == null || GameStateManager.GlobalValues().scenarioCustomizations.cinematicCombatRealismScale)
		{
			_cachedScalingAdjustmentFactor = 1f;
		}
		else
		{
			_cachedScalingAdjustmentFactor = 0.1f;
		}
	}

	public static float GetFormationScalingFactor()
	{
		return GetScalingAdjustmentFactor();
	}

	public void SetWaypointDragging(bool value)
	{
		_waypointInputDragging = value;
	}

	public static Vector3 km_to_scale_vec3(Vector3 distance_km)
	{
		return distance_km * 0.05f;
	}

	public static float km_to_scale(float distance_km)
	{
		return distance_km * 0.05f;
	}

	public static float kps_to_scale(float velocity_kps)
	{
		return velocity_kps * 0.05f;
	}

	public static float scale_to_kps(float velocity)
	{
		return velocity / 0.05f;
	}

	public static float scale_to_km(float distance)
	{
		return distance / 0.05f;
	}

	public static Vector3 scale_to_km_vec3(Vector3 value)
	{
		return value / 0.05f;
	}

	public static Vector3 vector_km_to_scale(Vector3 vector_km)
	{
		return vector_km * 0.05f;
	}

```

## S2 Ship rendering and colliders

Native type: `PavonisInteractive.TerraInvicta.SpaceCombat.CombatShipController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombat.CombatShipController.decompiled.cs`.

Lines 310–326:

```csharp
		}
		shipVisualizerTransform.GetComponentInChildren<ShipModelController>(includeInactive: true).transform.rotation = Quaternion.identity;
		localPositionToRestore = shipVisualizerTransform.position;
		localScaleToRestore = shipVisualizerTransform.localScale;
		StrategyShipControllerTransform = shipVisualizerTransform.parent.transform;
		shipVisualizerTransform.SetParent(base.combatantTransform, worldPositionStays: true);
		shipVisualizerTransform.position = base.combatantTransform.position;
		shipVisualizerTransform.localScale = Vector3.one * GameControl.spaceCombat.modelScalingFactor;
		visualizationController = shipVisualizerTransform.GetComponent<ShipVisController>();
		visualizationController.enabled = true;
		ModelController = visualizationController.ModelController;
		ModelController.SetVectorThrusters(state.drive, state.template.designingFaction);
		ModelController.DisableRadiatorEmissives();
		ModelController.ResetManeuverCommandUI();
		hitColliders = ModelController.GetComponentsInChildren<Collider>().ToList();
		rootCollider = ModelController.GetComponent<Collider>();
		rootCollider.enabled = base.combatMgr.initialized && !base.combatMgr.IsInFormationSelectionMode;
```

Related native `CombatantShipController.GetCrossSectionalArea_m2` returns `ShipState.GetCrossSectionalArea_m2(angle)`: damage area is statistical geometry, distinct from the scaled prefab colliders. Native `CombatShipController` trigger callbacks at lines 1684–1709 register/remove nearby ship/hab navigation agents. Root/trigger behavior therefore also needs review before shrinking every collider.

## S2a Predicted threat sphere

Native type: `PavonisInteractive.TerraInvicta.TISpaceCombatProjectileState`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.TISpaceCombatProjectileState.decompiled.cs`.

Lines 171–194:

```csharp
	public bool WillHitSphere(Vector3 targetPosition, Vector3 targetVelocity, IDamageableType targetType, CombatantController targetController)
	{
		Vector3 vector = velocityVector_kps * 0.05f;
		float num = targetPosition.x - position.x;
		float num2 = targetVelocity.x - vector.x;
		float num3 = targetPosition.y - position.y;
		float num4 = targetVelocity.y - vector.y;
		float num5 = targetPosition.z - position.z;
		float num6 = targetVelocity.z - vector.z;
		float num7 = -1f * ((num * num2 + num3 * num4 + num5 * num6) / (num2 * num2 + num4 * num4 + num6 * num6));
		if (num7 > 0f)
		{
			Vector3 a = position + num7 * vector;
			Vector3 b = targetPosition + num7 * targetVelocity;
			float num8 = Vector3.Distance(a, b);
			return targetType switch
			{
				IDamageableType.Ship => num8 <= targetController.ref_shipController.ShipState.hull.length_m * GameControl.spaceCombat.modelScalingFactor, 
				IDamageableType.StationModule => num8 <= targetController.ref_habModuleController.combatHitColliders[0].radius * GameControl.spaceCombat.modelScalingFactor, 
				_ => num8 <= 0.02f * GameControl.spaceCombat.modelScalingFactor, 
			};
		}
		return false;
	}
```

## S3 Projectile visuals and hit detection

Native type: `PavonisInteractive.TerraInvicta.SpaceCombat.BallisticProjectileController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombat.BallisticProjectileController.decompiled.cs`.

Lines 35–59:

```csharp
	public override void Fire(Vector3 originPosition, Vector3 targetPosition, IDamageable target = null)
	{
		base.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
		base.projectileTransform = base.transform;
		base.projectileTransform.localScale = Vector3.one * GameControl.spaceCombat.projectileScalingFactor;
		base.velocityVector = SpaceCombatManager.vector_km_to_scale(base.projectileState.velocityVector_kps);
		base.originPosition = originPosition;
		base.projectileTransform.position = originPosition;
		base.projectileTransform.LookAt(targetPosition);
		projectileParticleSystem.Clear();
		destructionParticleSystem.Clear();
		projectileParticleSystem.Play();
		base.hasHit = false;
		base.beenDestroyed = false;
		base.clearedLauncher = false;
		velocityVector_Magnitude = base.velocityVector.magnitude;
		maximumDistance = Mathf.Max(base.weaponController.weaponTemplate.targetingRange_km * 1.25f, base.weaponController.weaponTemplate.targetingRange_km * 1.25f * (velocityVector_Magnitude / 0.05f) / gunTemplate.muzzleVelocity_kps);
		if (base.weaponTemplate.weaponClass == WeaponClass.Magnetic)
		{
			base.projectileTransform.localScale *= Mathf.Log10(base.warheadMass_kg);
			GetComponent<MagBulletParticleController>().UpdateMass(base.projectileState.effectiveMass_kg, base.warheadMass_kg);
		}
		_container.AddProjectile(this, base.projectileTransform, ProjectileJobData.MovementType.Ballistic, base.projectileState, 0f, float.MaxValue, float.MaxValue, base.velocityVector, originPosition, target, 0f, 0f, 0f, 0f);
	}
```

Lines 63–84:

```csharp
		if (base.hasHit || gameTime.Paused || base.beenDestroyed)
		{
			return;
		}
		if (Vector3.Distance(originPosition, base.projectileTransform.position) / 0.05f > maximumDistance)
		{
			Destruct();
		}
		if (!base.clearedLauncher && (float)gameTime.currentTime.DifferenceInSeconds(base.projectileState.launchTime) > 5f)
		{
			base.clearedLauncher = true;
		}
		float maxDistance = velocityVector_Magnitude * 1.2f * Time.deltaTime * gameTime.currentSpeed;
		raycastHitsArray = Physics.RaycastAll(base.projectileTransform.position, base.projectileTransform.forward, maxDistance, _collisionMask);
		if (raycastHitsArray.Length == 0)
		{
			return;
		}
		IDamageable hitDamageable;
		RaycastHit raycastHit = FilterRaycastsForHits(raycastHitsArray.ToList(), out hitDamageable);
		if (hitDamageable != null)
		{
```

Native `SpaceCombatManager.SetProjectile`, lines 451–482, initializes pooled projectiles and multiplies root scale by `projectileScalingFactor` when the root has a MeshRenderer. `BallisticProjectileController.Fire` then assigns its root scale explicitly. Missile handling differs: `MissileController.Fire`, lines 172–177, assigns the missile renderer object scale; lines 95–97 and 152–153 separately scale a distance-dependent tracking display. A multiplier alone does not establish each missile collider’s final dimensions.

## S3a Projectile hitboxes and friendly filtering

Native type: `PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombat.ProjectileController.decompiled.cs`.

Lines 138–167:

```csharp

	public void Awake()
	{
		_collisionMask = 1 << LayerMask.NameToLayer("HurtBox");
		gameTime = World.Active.GetExistingManager<GameTimeManager>();
		projectileParticleSystem = base.gameObject.GetComponentInChildren<ParticleSystem>();
		if ((bool)projectileParticleSystem)
		{
			projectileParticleTransform = projectileParticleSystem.transform;
			projectileParticleDefaultScale = projectileParticleTransform.localScale;
		}
		if (hitColliders == null)
		{
			hitColliders = new List<Collider>();
		}
		else
		{
			hitColliders.Clear();
		}
		if (projectileCollider != null)
		{
			hitColliders.Add(projectileCollider);
			return;
		}
		GameObject gameObject = new GameObject("Hitbox");
		projectileCollider = gameObject.AddComponent<BoxCollider>();
		(projectileCollider as BoxCollider).size = new Vector3(1f, 1f, 1f * GameControl.spaceCombat.projectileScalingFactor);
		hitColliders.Add(projectileCollider);
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
	}
```

Lines 416–447:

```csharp
	protected RaycastHit FilterRaycastsForHits(List<RaycastHit> raycastHitsList, out IDamageable hitDamageable)
	{
		float num = float.PositiveInfinity;
		RaycastHit result = default(RaycastHit);
		hitDamageable = null;
		foreach (RaycastHit raycastHits in raycastHitsList)
		{
			GameObject gameObject = raycastHits.collider.gameObject;
			CombatantController componentInParent = gameObject.GetComponentInParent<CombatantController>();
			if (raycastHits.distance > num)
			{
				continue;
			}
			if (componentInParent != null)
			{
				if (!(componentInParent.faction == projectileState.shootingFaction) && (!(componentInParent.ref_shipController != null) || !componentInParent.ref_shipController.ModelController.mainHullDestroyed) && (!(componentInParent.ref_habModuleController != null) || !componentInParent.ref_habModuleController.destructionTriggered))
				{
					num = raycastHits.distance;
					result = raycastHits;
					hitDamageable = componentInParent;
				}
				continue;
			}
			ProjectileController componentInParent2 = gameObject.GetComponentInParent<ProjectileController>();
			if (!(componentInParent2 != null) || (!(projectileState.shootingFaction == componentInParent2.projectileState.shootingFaction) && !componentInParent2.beenDestroyed))
			{
				num = raycastHits.distance;
				result = raycastHits;
				hitDamageable = componentInParent2;
			}
		}
		return result;
```

## S4 EEO projectile geometry

Source: [`TIEconomyMod/Patches/ProjectileCollisionPatches.cs`](../../TIEconomyMod/Patches/ProjectileCollisionPatches.cs).

Lines 26–61:

```csharp
        private static bool unsupportedColliderReported;

        public static void Apply(
            ProjectileController projectile, float diameter_mm)
        {
            Collider collider = projectile.projectileCollider;
            if (collider == null || diameter_mm <= 0f)
            {
                return;
            }

            // Ship combat models use metres times modelScalingFactor. Use the
            // same cinematic scale for projectile caliber: much smaller than
            // BulletGun's generic box, while remaining stable in Unity physics.
            float worldDiameter =
                ProjectileCollisionMath.WorldDiameter_gameUnits(
                    diameter_mm,
                    GameControl.spaceCombat.modelScalingFactor);
            Vector3 scale = collider.transform.lossyScale;
            float scaleX = SafeScale(scale.x);
            float scaleY = SafeScale(scale.y);
            float scaleZ = SafeScale(scale.z);

            BoxCollider box = collider as BoxCollider;
            if (box != null)
            {
                box.size = new Vector3(
                    worldDiameter / scaleX,
                    worldDiameter / scaleY,
                    worldDiameter / scaleZ);
                return;
            }

            SphereCollider sphere = collider as SphereCollider;
            if (sphere != null)
            {
```

Lines 114–135:

```csharp

            float diameter_mm;
            if (ProjectileGeometryRegistry.TryGetDiameter_mm(
                __instance.projectileState.originWeapon, out diameter_mm))
            {
                ProjectileColliderSizer.Apply(__instance, diameter_mm);
            }
        }
    }

    [HarmonyPatch(
        typeof(TISpaceCombatProjectileState),
        "CrossSectionalArea_m2",
        new Type[] { typeof(TIShipWeaponTemplate), typeof(float) })]
    public static class ProjectileCrossSectionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TIShipWeaponTemplate weaponTemplate)
        {
            TIProjectileWeaponTemplate projectileTemplate =
                weaponTemplate as TIProjectileWeaponTemplate;
```

The remaining sizer branches handle sphere/capsule colliders and replacement boxes. `ProjectileCollisionMath.WorldDiameter_gameUnits` computes `diameter_mm / 1000 * modelScalingFactor`. `ProjectileGeometryRegistry` covers NavalGun and Magnetic; magnetic fallback assumes a tungsten-density cylinder with length/diameter 10, using complete round mass. Cross-sectional damage area is separately patched to `pi*(diameter_mm/2000)^2`. Movement sweep is patched from native 1.2 to 1.0. These are feature-gated by the ship-balance setting. See the hashed source files above.

## S5 Hull gun aiming and firing

Native type: `PavonisInteractive.TerraInvicta.Ship.Weapon`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.Ship.Weapon.decompiled.cs`.

Lines 275–285:

```csharp
	public bool InArc(Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetAcceleration)
	{
		if (weaponTemplate.noseWeapon)
		{
			if (weaponTemplate.isLaserWeapon || weaponTemplate.isParticleWeapon)
			{
				return Vector3.Angle(targetPosition - position, combatantTransform.forward) <= weaponTemplate.pivotRange_deg;
			}
			if (targetAcceleration == Vector3.zero)
			{
				if (weaponTemplate.isMissileWeapon)
```

Lines 312–323:

```csharp
			}
			bool impossible4;
			Vector3 vector4 = TISpaceCombatProjectileState.SecondOrderInterceptPosition(position, combatant.velocityVector, SpaceCombatManager.km_to_scale(weaponTemplate.ref_gunWeapon.muzzleVelocity_kps), targetPosition, targetVelocity, targetAcceleration, weaponTemplate.cooldown_s, out impossible4);
			if (!impossible4)
			{
				return Vector3.Angle(vector4 - position, combatantTransform.forward) <= weaponTemplate.pivotRange_deg;
			}
			return false;
		}
		return true;
	}

```

Lines 394–409:

```csharp
	public ShipWeaponVisController SelectWeaponVisualization(Vector3 targetedPosition)
	{
		if (weaponTemplate.noseWeapon)
		{
			return weaponVisualization;
		}
		if (weaponTemplate.hullWeapon)
		{
			if (Vector3.Dot(targetedPosition - combatant.position, combatantTransform.up) >= 0f)
			{
				return weaponVisualization;
			}
			return altWeaponVisualization;
		}
		return null;
	}
```

Lines 475–491:

```csharp
	public bool TryFireCommon(DateTime currentTime)
	{
		if (OnCooldown(currentTime) || target == null || !combatant.WeaponCarrierState.WeaponCanFire(weaponData))
		{
			return false;
		}
		if (shotsFiredThisSalvo == weaponTemplate.salvo_shots)
		{
			shotsFiredThisSalvo = 0;
		}
		if (!OnTarget)
		{
			return false;
		}
		return true;
	}

```

## S5a Turret training and alignment

Native type: `PavonisInteractive.TerraInvicta.ShipWeaponVisController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.ShipWeaponVisController.decompiled.cs`.

Lines 346–354:

```csharp
	public bool OnTarget()
	{
		if (hasTarget && !weaponTemplate.staticLauncher)
		{
			Vector3 vector = GetTargetPosition();
			return Vector3.Angle(weaponObjectTransform.forward, (vector - weaponObjectTransform.position).normalized) < 1f;
		}
		return true;
	}
```

Lines 366–392:

```csharp
	public void RotateToTarget(bool forceToForward = false)
	{
		TIShipWeaponTemplate tIShipWeaponTemplate = weaponTemplate;
		if (tIShipWeaponTemplate == null || tIShipWeaponTemplate.staticLauncher)
		{
			return;
		}
		CombatWeaponCarrierState combatWeaponCarrierState = weaponCarrierState;
		if (combatWeaponCarrierState == null || !combatWeaponCarrierState.WeaponIsOperable(weaponModuleData))
		{
			return;
		}
		if (updateTime == null)
		{
			updateTime = new TIDateTime(gameTime.currentTime);
		}
		float num = (float)gameTime.currentTime.DifferenceInSeconds(updateTime);
		updateTime.CopyDateTime(gameTime.currentTime);
		if (forceToForward)
		{
			return;
		}
		Vector3 vector = GetTargetPosition();
		if (!(Vector3.Angle(baseObjectTransform.up, (vector - base.transform.position).normalized) <= weaponTemplate.pivotRange_deg))
		{
			return;
		}
```

## S5b Beam damage and target colliders

Native type: `PavonisInteractive.TerraInvicta.Ship.BeamWeapon`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.Ship.BeamWeapon.decompiled.cs`.

Lines 12–27:

```csharp
		{
			base.attacker = attacker;
			Ray ray = new Ray(start, end - start);
			float num = Vector3.Distance(end, start);
			foreach (Collider hitCollider in target.hitColliders)
			{
				if (hitCollider.Raycast(ray, out var hitInfo, num))
				{
					base.hitPosition = hitInfo.point;
					break;
				}
			}
			float range_km = SpaceCombatManager.scale_to_km(num);
			DamageBreakdown damageBreakdown = weaponTemplate.DamageAtRange_points(range_km, target.GetCrossSectionalArea_m2(), attacker);
			base.damage = new Damage(weaponTemplate, range_km, weaponTemplate.GetDamageType(), damageBreakdown.directDamage_Points, damageBreakdown.chippingDamage_Points, 0, attacker.GetFaction());
		}
```

Lines 62–79:

```csharp
	{
		beamWeapon = weaponData.moduleTemplate as TIBeamWeaponTemplate;
	}

	public override bool TryFire(DateTime currentTime)
	{
		if (!TryFireCommon(currentTime))
		{
			return false;
		}
		bool flag = base.target.damageableType == IDamageableType.BallisticProjectile || base.target.damageableType == IDamageableType.Missile;
		EnterCooldown(TISpaceShipState.LaserDownfiring(base.weaponTemplate, flag ? base.target.ref_projectile : null));
		base.combatant.WeaponCarrierState.FireWeapon(base.weaponData, flag ? base.target.ref_projectile : null);
		ShipWeaponVisController shipWeaponVisController = SelectWeaponVisualization(targetedPosition);
		shipWeaponVisController.Fire(flag);
		base.target.ApplyDamage(new Beam(base.target, shipWeaponVisController.transform.position, targetedPosition, beamWeapon, base.combatant.WeaponCarrierState));
		return true;
	}
```

Training continues through line 424: yaw/elevation use `turretTrainingRate_degsec` and elapsed simulation time. `SetPrefabs` sets the rate to `270 / internalSize` at line 205. The EEO cadence scheduler calls `AcquireTarget`, `RotateToTarget(false)`, then `TryFire`; see [WeaponCadencePatches.cs](../../TIEconomyMod/Patches/WeaponCadencePatches.cs), lines 171–193. `ProjectileWeapon.TryFire` launches from the selected fire point; `BeamWeapon.TryFire` directly applies target damage after common checks, with target-collider raycasts used to locate a hit point.

Fresh installed template inventory (hull mounts only):

| Family | Hull records | Distinct pivotRange_deg |
|---|---:|---:|
| `TIGunTemplate` | 4 | 180 |
| `TIMagneticGunTemplate` | 27 | 180 |
| `TILaserWeaponTemplate` | 38 | 180 |
| `TIParticleWeaponTemplate` | 13 | 180 |
| `TIPlasmaWeaponTemplate` | 8 | 180 |

The repository overrides for these families were searched; none supplies `pivotRange_deg`. Campaign-default code only alters the campaign reset toggles; no camera or global model-scale getter patch was found in the mod source.

## C1 Camera configuration and initialization

Fresh installed `TIGlobalConfig.json` values:

```json
{
  "combatCamera_maxZoom": 5000.0,
  "combatCamera_minZoom": 20.0,
  "combatCamera_maxPan": 8000.0,
  "combatCamera_minPan": -8000.0,
  "combatCamera_minCameraMovementSpeed": 5.0,
  "combatCamera_maxCameraMovementSpeed": 90.0,
  "combatCamera_minScrollSpeedOffset": -200.0,
  "combatCamera_maxScrollSpeedOffset": -600.0,
  "combatCamera_mouseRotateSpeedOffset": 0.9,
  "combatCamera_keyRotateSpeedOffset": 50.0,
  "combatCamera_keyZoomSpeed": 0.01
}
```

The EEO global override contains no `combatCamera_*` fields. The default input bindings are A/D/W/S and T/B for zoom; the controller reads configurable `TIInputManager.camera*` bindings, which can be mapped to arrow keys.

## C1a Camera initialization

Native type: `PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController.decompiled.cs`.

Lines 190–195:

```csharp
	private void InitializeCamera()
	{
		SpaceCombatManager.SetScalingAdjustmentFactor();
		_scale = 0.05f * SpaceCombatManager.GetScalingAdjustmentFactor();
		_spaceCombat = GameControl.spaceCombat;
		_activePlayerFleetControllerIdx = ((!_spaceCombat.fleetControllers[0].IsActivePlayerFleet) ? 1 : 0);
```

Lines 221–232:

```csharp
		_spaceCombatCamera.transform.LookAt(_cameraFocalPoint);
		_cameraEulerAngles = _spaceCombatCamera.transform.eulerAngles;
		float t = (float)(1.0 - ((double)ScaledMaxZoom - _polarOffset.radius) / (double)(ScaledMaxZoom - ScaledMinZoom));
		_cameraMovementSpeed = Mathf.Lerp(minCameraMovementSpeed, maxCameraMovementSpeed, t) * Scale;
		SetCameraState(Movement.FreeLook);
	}

	private void SetCameraState(Movement state)
	{
		if (_cameraMovement != state)
		{
			_followCameraOffset = Vector3.zero;
```

## C2 Keyboard and mouse movement

Native type: `PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController.decompiled.cs`.

Lines 326–349:

```csharp
		}
		else
		{
			xAxis = 0f;
			if (Input.GetKey(TIInputManager.cameraRight))
			{
				xAxis += 1f;
			}
			if (Input.GetKey(TIInputManager.cameraLeft))
			{
				xAxis -= 1f;
			}
			zAxis = 0f;
			if (Input.GetKey(TIInputManager.cameraUp))
			{
				zAxis += 1f;
			}
			if (Input.GetKey(TIInputManager.cameraDown))
			{
				zAxis -= 1f;
			}
		}
		if (_dragging & right)
		{
```

Lines 378–391:

```csharp
		}
		if (_gridCollider == null && !_gridColliderFound)
		{
			_gridCollider = GameControl.spaceCombat.combatGrid.GetComponent<Collider>();
			_gridColliderFound = true;
		}
		if (!TIStandaloneInputModule.current.IsPointerOverUIGameObject() && !_dragging && !TIInputManager.IsShiftKeyDown && (rightDown | middleDown))
		{
			Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
			_dragging = (Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, _layersMask) && hitInfo.collider == _gridCollider) || hitInfo.collider == null;
		}
		if (_dragging && ((rightUp | middleUp) || TIInputManager.IsShiftKeyDown))
		{
			_dragging = false;
```

Lines 658–688:

```csharp
	private void HandleMovement(float xAxis, float yAxis, float zAxis, bool middle, bool right)
	{
		if (!_spaceCombat.combatEnded && TIInputManager.acceptingInput)
		{
			if (_dragging && right)
			{
				_spaceCombatCamera.transform.position += _cameraMovementSpeed * 1.5f * zAxis * _spaceCombatCamera.transform.forward;
				_spaceCombatCamera.transform.position += _cameraMovementSpeed * 1.5f * xAxis * _spaceCombatCamera.transform.right;
			}
			else if (_dragging && !TIInputManager.IsControlKeyDown && middle)
			{
				Quaternion rotation = _spaceCombatCamera.transform.rotation;
				_spaceCombatCamera.transform.Rotate(0f - rotation.eulerAngles.x, 0f, 0f);
				_spaceCombatCamera.transform.position += _cameraMovementSpeed * 1.5f * xAxis * _spaceCombatCamera.transform.right;
				_spaceCombatCamera.transform.position += _cameraMovementSpeed * 1.5f * yAxis * _spaceCombatCamera.transform.up;
				_spaceCombatCamera.transform.rotation = rotation;
			}
			else
			{
				Quaternion rotation2 = _spaceCombatCamera.transform.rotation;
				_spaceCombatCamera.transform.Rotate(0f - rotation2.eulerAngles.x, 0f, 0f);
				_spaceCombatCamera.transform.position += _cameraMovementSpeed * 1.5f * zAxis * _spaceCombatCamera.transform.forward;
				_spaceCombatCamera.transform.position += _cameraMovementSpeed * 1.5f * xAxis * _spaceCombatCamera.transform.right;
				_spaceCombatCamera.transform.rotation = rotation2;
			}
			Ray ray = _spaceCombatCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
			GameControl.spaceCombat.combatGrid.CursorPositionRelativeToPlaneAt(Vector3.zero, Vector3.up, Input.mousePosition);
			GameControl.spaceCombat.combatGrid.GetDistanceToPointOfIntersection(ray, out var distance);
			Vector3 cameraFocalPoint = _spaceCombatCamera.transform.forward * distance;
			cameraFocalPoint.x = Mathf.Clamp(cameraFocalPoint.x, minPan, maxPan);
			cameraFocalPoint.z = Mathf.Clamp(cameraFocalPoint.z, minPan, maxPan);
```

## C3 Zoom and follow behavior

Native type: `PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController`. Temporary file: `.tmp/battle-scale-camera/PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController.decompiled.cs`.

Lines 246–269:

```csharp
	public void LookAtCombatant(CombatantController combatant)
	{
		_previousTarget = _focusedTarget;
		_focusedTarget = combatant;
		_cameraFocalPoint = combatant.position;
		_followCameraOffset = Vector3.zero;
		_spaceCombatCamera.transform.LookAt(_cameraFocalPoint);
		_targetPolarOffset = Mathd.Clamp(ScaledMinZoom + (ScaledMaxZoom - ScaledMinZoom) * _spaceCombat.modelScalingFactor, ScaledMinZoom, ScaledMaxZoom);
		_zoomTimeRemaining = 0.15f;
		SetCameraState(Movement.Follow);
	}

	public void LookAtObject(GameObject target)
	{
		_previousTarget = _focusedTarget;
		_cameraFocalPoint = target.transform.position;
		_followCameraOffset = Vector3.zero;
		_spaceCombatCamera.transform.LookAt(_cameraFocalPoint);
		_targetPolarOffset = Mathd.Clamp(ScaledMinZoom + (ScaledMaxZoom - ScaledMinZoom) * _spaceCombat.modelScalingFactor, ScaledMinZoom, ScaledMaxZoom);
		_zoomTimeRemaining = 0.15f;
		ClearFocusedTarget();
		_cameraEulerAngles = _spaceCombatCamera.transform.eulerAngles;
	}

```

Lines 710–764:

```csharp
	private void HandleZoom(float rAxis)
	{
		if (!TIInputManager.acceptingInput || TIInputManager.blockCombatZoom)
		{
			return;
		}
		if (!Mathf.Approximately(rAxis, 0f) && _screenRect.Contains(Input.mousePosition))
		{
			rAxis *= _scrollSpeedOffset;
			if ((_zoomTimeRemaining > 0f && rAxis < 0f && _isTargetPolarOffsetIncreasing) || (rAxis > 0f && !_isTargetPolarOffsetIncreasing))
			{
				_targetPolarOffset = Mathd.Clamp(_polarOffset.radius + (double)rAxis, ScaledMinZoom, ScaledMaxZoom);
				_isTargetPolarOffsetIncreasing = !_isTargetPolarOffsetIncreasing;
			}
			else
			{
				_targetPolarOffset = Mathd.Clamp(_targetPolarOffset + (double)rAxis, ScaledMinZoom, ScaledMaxZoom);
			}
			_zoomTimeRemaining = 0.15f;
			if (_cameraMovement == Movement.FreeLook)
			{
				float num = _freelookZoomMultiplier;
				if (TIInputManager.IsControlKeyDown)
				{
					num *= 0.25f;
				}
				_targetCameraPos = _spaceCombatCamera.transform.position + _spaceCombatCamera.transform.forward * ((0f - rAxis) * maxCameraMovementSpeed * num);
				_freelookZoomTimer = 0.1f;
			}
		}
		if (_zoomTimeRemaining > 0f)
		{
			float num2 = Mathf.Min(Time.unscaledDeltaTime, _zoomTimeRemaining);
			float num3 = num2 / _zoomTimeRemaining;
			_zoomTimeRemaining -= num2;
			if (_zoomTimeRemaining < 0.001f)
			{
				_polarOffset.radius = _targetPolarOffset;
				_zoomTimeRemaining = 0f;
			}
			else
			{
				double num4 = (_targetPolarOffset - _polarOffset.radius) * (double)num3;
				_polarOffset.radius += num4;
			}
			float t = (float)(1.0 - ((double)ScaledMaxZoom - _polarOffset.radius) / (double)(ScaledMaxZoom - ScaledMinZoom));
			_scrollSpeedOffset = Mathf.Lerp(minScrollSpeedOffset, maxScrollSpeedOffset, t) * Scale;
			_cameraMovementSpeed = Mathf.Lerp(minCameraMovementSpeed, maxCameraMovementSpeed, t) * Scale;
		}
		if (_freelookZoomTimer > 0f)
		{
			_freelookZoomTimer -= Time.deltaTime;
			_spaceCombatCamera.transform.position = Vector3.Lerp(_spaceCombatCamera.transform.position, _targetCameraPos, Time.deltaTime * 10f);
		}
	}
```

The follow-state branch in native `Update`, lines 149–180, shares `HandleZoom`, uses `_cameraMovementSpeed` for middle-button offsets, and switches to FreeLook when a movement key is pressed. `InitializeCamera` assigns `_cameraMovementSpeed` but never `_scrollSpeedOffset`; its default zero is first refreshed inside the zoom timer path. This predicts an ineffective first zoom input in a fresh controller; it was not tested in game.

## Reproduction and arithmetic checks

Run from the repository root; output remains temporary. For each native type named above:

```powershell
$dll = 'D:\Games\SteamLibrary\steamapps\common\Terra Invicta\TerraInvicta_Data\Managed\Assembly-CSharp.dll'
.tmp\ilspycmd\ilspycmd.exe --disable-updatecheck -t 'PavonisInteractive.TerraInvicta.SpaceCombat.SpaceCombatCameraController' -o .tmp\battle-scale-camera $dll
Get-FileHash -Algorithm SHA256 $dll
```

Arithmetic used by the reports (scene unit abbreviated u):

```text
A = 1 cinematic on, 0.1 cinematic off
1 km = 0.05 u; 1 m = 0.00005 u
ship model multiplier / true metres = (0.01*A) / 0.00005 = 200*A
generic projectile multiplier / true metres = (0.025*A) / 0.00005 = 500*A
S = 0.05*A; t = clamp01((r/S - 20) / 4980)
pan per frame = 1.5*S*(5 + 85*t) u for one held movement axis
scroll coefficient = S*(-200 - 400*t)
free-look target delta magnitude = abs(q)*S*(4500 + 9000*t) u
Ctrl free-look target delta = ordinary delta / 4
follow radius target delta magnitude = abs(q)*S*(200 + 400*t) u before clamping
```

The camera tables assume q=0.1 only as an illustrative wheel event. Hardware/Unity input-axis configuration can yield other values. Free-look target displacement is not the completed interpolated displacement during its 0.1-second timer. Pan rates assume uninterrupted input and the stated FPS; they are calculations, not runtime measurements.

Verification performed: DLL/template hashes; equality of packaged/deployed mod DLLs; retained ship/weapon bundle hashes; source searches for scale/camera/arc overrides; current hull pivot inventory; unit conversions; numeric camera examples; report-link and Markdown-fence checks. No build, deployment, game launch or manual battle test.
