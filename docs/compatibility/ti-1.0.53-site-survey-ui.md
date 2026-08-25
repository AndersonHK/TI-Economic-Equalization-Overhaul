# TI 1.0.53 per-site survey UI compatibility

## Scope

EEO stores new probe progress on `TIHabSiteState` while retaining vanilla
whole-body intel as a legacy compatibility signal. A site is known when either
its own intel is complete or its parent body was surveyed by an older/vanilla
campaign path.

The 1.0.53 UI has several independent whole-body assumptions which must be
adapted together:

- The Intel space-body row launches a probe directly against the body instead
  of entering the operation targeting flow.
- The body row treats any probe in flight as a reason to offer only the legacy
  override-probe path, preventing parallel surveys of different sites.
- Surface marker icons and resource strings test parent-body intel even when
  they are rendering one site.
- The Intel hab-site tab and its list are populated only from fully prospected
  bodies.
- The vanilla probe-arrival notification enumerates every site on the body.

## Required behavior

- The Intel probe button opens `TIOperationTargeting_HabSite` and lists only
  sites which are neither surveyed nor already assigned a probe.
- Different sites on one body may have probes in flight concurrently.
- A completed site immediately gains its surveyed marker and exact resource
  output, appears in the Intel hab-site list, and can be used as a base target.
- One completion notification contains only the completed site's result.
- The parent body becomes fully prospected only after all its sites have
  completed individual surveys. Until then its aggregate resource values
  remain ranges because unsurveyed sites are still unknown.
- Existing body-wide survey intel remains authoritative for every site on that
  body. This preserves old saves such as a vanilla-surveyed Luna without
  manufacturing per-site intel entries.

## Save-state evidence

The reported save contains both compatibility modes:

- Luna has body intel `1.0` and no per-site intel entries, representing the
  legacy whole-body survey path.
- Mars has 24 sites at site intel `1.0`, one site at the probe-in-flight marker,
  and no completed body intel. Serialized site resource yields are present.

This rules out data loss and makes UI/notification integration the repair
target. No migration of either body's intel is required.

## Startup safety

Harmony patches are installed before Unity populates `AssetCacheManager`.
Patching a UI method whose IL directly dereferences that type can make Harmony
compile the method early and permanently fault the type initializer. Survey UI
patches therefore do not wrap the body-row `Refresh` or the native empty-site
icon selector. Parallel-launch availability comes from the shared prospecting
state predicate, and the surveyed marker is applied after the native marker
method has run, with the initialized sprite resolved lazily by reflection.

## Validation and deployment

The first implementation exposed this startup-order constraint in a real
campaign load: Harmony wrapping the body-row `Refresh` method forced
`AssetCacheManager..cctor()` before Unity populated its fields. EEO rolled back
its patches, and later canvas initialization terminated on the faulted type.

The corrected implementation removes both early asset-bearing patch targets.
The focused validator now rejects either target and rejects direct static
`AssetCacheManager` dereferences in the survey UI patch source. The final full
verification passed all 34 pooled validators and 1,172 formula assertions, and
deployment installed DLL SHA-256
`486928F27CA8220FFBED09349354B583C1AF837B936CC0A3DAFA1EC80054176D`.
