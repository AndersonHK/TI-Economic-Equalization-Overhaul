# Native method excerpts

Decompiled read-only from the installed Assembly-CSharp.dll during this audit using the repository-local ILSpy command. Assembly SHA-256: `4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350`. These excerpts support the static measurement model; they are not a runtime capture.

## public static void SetWeapon(

```csharp
public static void SetWeapon(string resource, ShipVisController parentController, ShipWeaponVisController targetController, ModuleDataEntry moduleDataEntry, bool forVisualizationOnly)
	{
		targetController.Initialize(parentController, moduleDataEntry, forVisualizationOnly);
		GameObject gameObject = GameControl.assetLoader.LoadAsset<GameObject>(resource);
		Transform child = gameObject.transform.GetChild(0);
		targetController.baseObject.GetComponent<MeshFilter>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		targetController.baseObject.GetComponent<MeshRenderer>().sharedMaterial = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
		targetController.weaponObject.GetComponent<MeshFilter>().sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;
		targetController.weaponObject.GetComponent<MeshRenderer>().sharedMaterial = child.GetComponent<MeshRenderer>().sharedMaterial;
		targetController.weaponObject.transform.localPosition = child.transform.localPosition;
		if (targetController.weaponModuleData.moduleTemplate.ref_weapon.staticLauncher)
		{
			targetController.weaponObject.transform.localRotation = child.transform.localRotation;
		}
		if (child.transform.childCount > 0)
		{
			targetController.firePoint.transform.localPosition = child.transform.GetChild(0).localPosition;
		}
		else
		{
			Log.Error("Missing FirePoint for resource " + resource);
		}
		TISpaceShipState shipState = targetController.shipVisController.shipState;
		if ((object)shipState != null && !shipState.hull.simpleHull)
		{
			targetController.baseObject.transform.localScale = gameObject.transform.localScale;
			targetController.weaponObject.transform.localScale = child.transform.localScale;
		}
	}
```

## private void BuildWeapons(

```csharp
private void BuildWeapons(ShipVisController parentController, TISpaceShipTemplate ship, TISpaceShipState shipState = null)
	{
		foreach (ShipWeaponVisController allWeaponController in allWeaponControllers)
		{
			allWeaponController.baseObject.SetActive(value: false);
		}
		foreach (ModuleDataEntry noseWeapon in ship.noseWeapons)
		{
			ShipWeaponVisController shipWeaponVisController = noseWeaponControllers[SlotToWeaponMountIndex(noseWeapon.slotIndex, noseWeapon.moduleTemplate.ref_weapon.mount)];
			shipWeaponVisController.weaponTemplate = noseWeapon.moduleTemplate.ref_weapon;
			SetWeapon(noseWeapon.moduleTemplate.modelResource, parentController, shipWeaponVisController, noseWeapon, shipState == null);
			shipWeaponVisController.baseObject.SetActive(value: true);
		}
		foreach (ModuleDataEntry hullWeapon in ship.hullWeapons)
		{
			if (dorsalHullWeaponControllers.Count > 0)
			{
				ShipWeaponVisController shipWeaponVisController2 = dorsalHullWeaponControllers[SlotToWeaponMountIndex(hullWeapon.slotIndex, hullWeapon.moduleTemplate.ref_weapon.mount)];
				shipWeaponVisController2.weaponTemplate = hullWeapon.moduleTemplate.ref_weapon;
				SetWeapon(hullWeapon.moduleTemplate.modelResource, parentController, dorsalHullWeaponControllers[SlotToWeaponMountIndex(hullWeapon.slotIndex, hullWeapon.moduleTemplate.ref_weapon.mount)], hullWeapon, shipState == null);
				shipWeaponVisController2.baseObject.SetActive(value: true);
			}
			if (ventralHullWeaponControllers.Count > 0)
			{
				ShipWeaponVisController shipWeaponVisController3 = ventralHullWeaponControllers[SlotToWeaponMountIndex(hullWeapon.slotIndex, hullWeapon.moduleTemplate.ref_weapon.mount)];
				shipWeaponVisController3.weaponTemplate = hullWeapon.moduleTemplate.ref_weapon;
				SetWeapon(hullWeapon.moduleTemplate.modelResource, parentController, ventralHullWeaponControllers[SlotToWeaponMountIndex(hullWeapon.slotIndex, hullWeapon.moduleTemplate.ref_weapon.mount)], hullWeapon, shipState == null);
				shipWeaponVisController3.baseObject.SetActive(value: true);
			}
		}
	}
```

## private void InitializeCommon(

```csharp
private void InitializeCommon(bool createPrefabs = true)
	{
		gameTime = World.Active.GetExistingManager<GameTimeManager>();
		if (!string.IsNullOrEmpty(weaponTemplate.fireSoundFXResource))
		{
			eventPath = weaponTemplate.fireSoundFXResource;
		}
		turretTrainingRate_degsec = 270f / (float)weaponTemplate.internalSize;
		if (createPrefabs)
		{
			SetPrefabs();
		}
		RotateToFoward();
		_initialBaseObjectRotation = new GameObject("Base Orientation").transform;
		_initialBaseObjectRotation.SetParent(baseObjectTransform.parent, worldPositionStays: false);
		_initialBaseObjectRotation.position = baseObjectTransform.position;
		_initialBaseObjectRotation.localRotation = baseObjectTransform.localRotation;
		baseObject.transform.SetParent(_initialBaseObjectRotation);
		baseObject.transform.localRotation = Quaternion.identity;
		_originalBaseMaterial = baseObject.GetComponent<MeshRenderer>().material;
		_originalWeaponMaterial = weaponObject.GetComponent<MeshRenderer>().material;
		_destroyedMaterial = GameControl.assetLoader.LoadAsset<Material>("spacecombat/MAT_TEX_Weapon_Destroyed");
	}
```

## public float averageCooldown_s

```csharp
public float averageCooldown_s
	{
		get
		{
			if (salvo_shots > 1)
			{
				return (cooldown_s + intraSalvoCooldown_s * (float)(salvo_shots - 1)) / (float)salvo_shots;
			}
			return cooldown_s;
		}
	}
```
