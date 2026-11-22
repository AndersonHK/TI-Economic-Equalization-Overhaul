# Runs inside validate-earth-orbits-and-luna.ps1 with its native Harmony hooks.
function Set-PlaneFixtureField($Instance, [Type]$Owner, [string]$Name, $Value) {
    $Owner.GetField($Name, [Reflection.BindingFlags]'Instance,Public,NonPublic').SetValue($Instance, $Value)
}

function Set-PlaneFixtureId($Instance, [int]$Id) {
    $Instance.ID = [Activator]::CreateInstance($Instance.ID.GetType(), [object[]]@($Id))
}

$bodyTemplateType = $gameAssembly.GetType('TISpaceBodyTemplate', $true)
$bodyTemplate = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($bodyTemplateType)
Set-PlaneFixtureField $bodyTemplate $bodyTemplateType 'mass_kg' 5.972e24
Set-PlaneFixtureField $bodyTemplate $bodyTemplateType 'equatorialRadius_km' 6378.137
Set-PlaneFixtureField $body $gameStateType 'template' $bodyTemplate
Set-PlaneFixtureField $body $bodyType '_meanRadius_m' 6356751.107
$body.exists = $true
Set-PlaneFixtureId $body 100
Set-PlaneFixtureField $body $bodyType.BaseType '<hillRadius_m>k__BackingField' 1471527180.0
Set-PlaneFixtureField $body $bodyType.BaseType '<sphereOfInfluence_m>k__BackingField' 925000000.0

$clockType = $gameAssembly.GetType($nativeNamespace + 'TITimeState', $true)
$clock = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($clockType)
$dateType = $gameAssembly.GetType('TIDateTime', $true)
function New-PlaneFixtureDate([datetime]$Date) {
    [Activator]::CreateInstance($dateType, [object[]]@($Date))
}
Set-PlaneFixtureField $clock $clockType 'currentDateTime' (New-PlaneFixtureDate '2044-04-04T16:56:32')
$gameAssembly.GetType($nativeNamespace + 'GameStateManager', $true).GetField(
    'time', [Reflection.BindingFlags]'Static,NonPublic').SetValue($null, $clock)

$faction = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($factionType)
Set-PlaneFixtureId $faction 101
$factionTemplateType = $gameAssembly.GetType('TIFactionTemplate', $true)
Set-PlaneFixtureField $faction $gameStateType 'template' (
    [Runtime.Serialization.FormatterServices]::GetUninitializedObject($factionTemplateType))
$controlType = $gameAssembly.GetType('GameControl', $true)
$control = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($controlType)
Set-PlaneFixtureField $control $controlType '<activePlayer>k__BackingField' $faction
$controlType.GetProperty('control').SetValue($null, $control)

$spaceType = $assetType.BaseType
$spaceGameType = $gameAssembly.GetType($nativeNamespace + 'TISpaceGameState', $true)
Set-PlaneFixtureId $fleet 102
Set-PlaneFixtureField $fleet $assetType '<faction>k__BackingField' $faction
Set-PlaneFixtureField $fleet $assetType '<orbitState>k__BackingField' $leo
Set-PlaneFixtureField $fleet $spaceGameType '<barycenter>k__BackingField' $body
Set-PlaneFixtureField $fleet $assetType '_meanAnomalyAtEpoch_Rad' 6.47139646607665
Set-PlaneFixtureField $fleet $assetType '_epoch_JYears' 2463412.57708359
Set-PlaneFixtureField $fleet $spaceType '<epoch_DateTime>k__BackingField' (
    New-PlaneFixtureDate '2032-06-29T01:51:00.022')
$ship = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($shipType)
Set-PlaneFixtureId $ship 103
$ships = [Activator]::CreateInstance([System.Collections.Generic.List`1].MakeGenericType($shipType))
$ships.Add($ship)
Set-PlaneFixtureField $fleet $fleetType '<ships>k__BackingField' $ships

$station.exists = $true
Set-PlaneFixtureId $station 104
Set-PlaneFixtureField $station $spaceGameType '<barycenter>k__BackingField' $body
Set-PlaneFixtureField $station $assetType '_meanAnomalyAtEpoch_Rad' 0.71047815992634
Set-PlaneFixtureField $station $assetType '_epoch_JYears' 2461072.49998843
Set-PlaneFixtureField $station $spaceType '<epoch_DateTime>k__BackingField' (
    New-PlaneFixtureDate '2026-01-31T23:59:59')

$request = $plannerType.GetMethod('RequestTrajectories')
$callbackType = $request.GetParameters()[2].ParameterType
$fixtureCandidates = [System.Collections.ArrayList]::new()
$fixtureArray = [Linq.Expressions.Expression]::Parameter($callbackType.GenericTypeArguments[0], 'candidates')
$add = [Linq.Expressions.Expression]::Call(
    [Linq.Expressions.Expression]::Constant($fixtureCandidates),
    $fixtureCandidates.GetType().GetMethod('Add'), $fixtureArray)
$capture = [Linq.Expressions.Expression]::Block([Linq.Expressions.Expression[]]@(
    $add, [Linq.Expressions.Expression]::Empty()))
$callback = [Linq.Expressions.Expression]::Lambda(
    $callbackType, $capture, [Linq.Expressions.ParameterExpression[]]@($fixtureArray)).Compile()

function Invoke-PlaneFixture($Target, [single]$Acceleration, [single]$Budget) {
    $fixtureCandidates.Clear()
    Set-PlaneFixtureField $ship $shipType '<cruiseAcceleration_mps2>k__BackingField' $Acceleration
    Set-PlaneFixtureField $ship $shipType '<currentDeltaV_kps>k__BackingField' $Budget
    $request.Invoke($null, [object[]]@($fleet, $Target, $callback, 0.0, $false, $false, [single]1, $true))
}

function Assert-PlaneFixturePaths([double]$Acceleration, $Target) {
    $paths = @($fixtureCandidates[0] | Sort-Object duration_s)
    if ($paths.Count -lt 2 -or $paths[0].DV_mps - $paths[-1].DV_mps -lt 100) {
        throw 'The transfer picker must receive a meaningful time versus delta-v tradeoff.'
    }
    for ($i = 0; $i -lt $paths.Count; $i++) {
        $trajectory = $paths[$i]
        $segment = $trajectory.Segments[0]
        $spent = $trajectory.DVConsumedOnTrajectory_mps($trajectory.arrivalTime)
        if ($segment.GetType().FullName -ne 'TIEconomyMod.Core.FiniteThrustPlaneChangeSegment' -or
            !$trajectory.isPlausible() -or
            $segment.peakThrustBound_mps2 -gt $Acceleration -or
            [math]::Abs($trajectory.DV_mps - $spent) -gt 0.001 -or
            ($i -gt 0 -and ($trajectory.DV_mps -ge $paths[$i - 1].DV_mps -or
                $trajectory.duration_s -le $paths[$i - 1].duration_s))) {
            throw 'A slider option violated the acceleration, fuel, timing or dominance contract.'
        }
        $arrival = $trajectory.arrivalTime
        $endpoint = $trajectory.ToGlobalCartesianStateAtTime($arrival)
        $preview = $trajectory.PositionAtTime($arrival, $false, [ref]$false)
        $trailPoint = $segment.GetType().GetMethod('PreviewPoint',
            [Reflection.BindingFlags]'Instance,NonPublic').Invoke($segment, [object[]]@(1.0))
        if (!(($preview - $endpoint.position).magnitude -le 0.001) -or
            !(($trailPoint - $preview).magnitude -le 0.001) -or
            !(($trajectory.destinationPosition - $preview).magnitude -le 0.001)) {
            throw 'The arrival preview, stored destination and flown path must agree.'
        }
        $subdivisions = 32
        for ($sample = 1; $sample -lt $subdivisions; $sample++) {
            $time = [Activator]::CreateInstance($dateType, [object[]]@(
                $trajectory.launchTime, ($trajectory.flightDuration_s * $sample / $subdivisions)))
            $before = [Activator]::CreateInstance($dateType, [object[]]@($time, -1.0))
            $after = [Activator]::CreateInstance($dateType, [object[]]@($time, 1.0))
            $local = $trajectory.ToGlobalCartesianStateAtTime($time).ToLocal($body, $time)
            $v0 = $trajectory.ToGlobalCartesianStateAtTime($before).ToLocal($body, $before).velocity
            $v1 = $trajectory.ToGlobalCartesianStateAtTime($after).ToLocal($body, $after).velocity
            $r = $local.position.magnitude
            $gravityScale = $body.mu / ($r * $r * $r)
            $thrustX = ($v1.x - $v0.x) * 0.5 + $local.position.x * $gravityScale
            $thrustY = ($v1.y - $v0.y) * 0.5 + $local.position.y * $gravityScale
            $thrustZ = ($v1.z - $v0.z) * 0.5 + $local.position.z * $gravityScale
            $thrust = [math]::Sqrt($thrustX * $thrustX + $thrustY * $thrustY + $thrustZ * $thrustZ)
            if (!($r -gt $body.maxRadiusDimension_m) -or !($thrust -le ($Acceleration + 0.001))) {
                throw 'The flown position and velocity must imply a body-clear, thrust-limited path.'
            }
        }
        if ($Target -eq $station) {
            $rendezvous = $station.ToGlobalCartesianStateAtTime($arrival)
            if (($endpoint.position - $rendezvous.position).magnitude -gt 10 -or
                ($endpoint.velocity - $rendezvous.velocity).magnitude -gt 0.02) {
                throw 'The fixed-phase path must rendezvous within the game clock precision.'
            }
        }
    }
    return $paths[0]
}

$target40 = $inclined[1]
$cruise = [single]0.2660899
$result = Invoke-PlaneFixture $target40 $cruise 32.7005
if ($result.Result.ToString() -ne 'Success') { throw "The reported plane-change case failed: $($result.Result)." }
$fast = Assert-PlaneFixturePaths $cruise $target40
$cheapest = $fixtureCandidates[0] | Sort-Object DV_mps | Select-Object -First 1
Write-Host ("Earth Defense options span {0:N2} km/s in {1:N2} hours to {2:N2} km/s in {3:N2} hours." -f
    $fast.DV_kps, $fast.duration_h, $cheapest.DV_kps, $cheapest.duration_h)

$serializationApi = $gameAssembly.GetType('StringSerializationAPI', $true)
$trajectoryBaseType = $gameAssembly.GetType($nativeNamespace + 'Trajectory', $true)
$encoded = $serializationApi.GetMethod('SerializeCompressed').Invoke(
    $null, [object[]]@($trajectoryBaseType, $fast.PSObject.BaseObject))
$restored = $serializationApi.GetMethod('Deserialize').Invoke($null, [object[]]@($trajectoryBaseType, $encoded))
$middle = [Activator]::CreateInstance($dateType, [object[]]@($fast.launchTime, ($fast.flightDuration_s / 2)))
$copy = $restored.Segments[0].ReducedCopy($middle, $restored.arrivalTime)
if (($restored.ToGlobalCartesianStateAtTime($middle).position -
        $fast.ToGlobalCartesianStateAtTime($middle).position).magnitude -gt 0.001 -or
    [math]::Abs($restored.DV_mps - $fast.DV_mps) -gt 0.001 -or
    [math]::Abs($copy.DV_mps + $restored.DVConsumedOnTrajectory_mps($middle) - $restored.DV_mps) -gt 0.001) {
    throw 'Flight state, propellant and the reduced segment must survive native serialization.'
}

$result = Invoke-PlaneFixture $station $cruise 32.7005
if ($result.Result.ToString() -ne 'Success') { throw "The phase-matched station case failed: $($result.Result)." }
$null = Assert-PlaneFixturePaths $cruise $station

$ion = [single]0.00980665
$result = Invoke-PlaneFixture $target40 $ion 32.7005
if ($result.Result.ToString() -ne 'Success') { throw "The 1 milligee case failed: $($result.Result)." }
$slow = Assert-PlaneFixturePaths $ion $target40
if ($slow.flightDuration_s -le $fast.flightDuration_s) { throw 'Less thrust must require a slower flight.' }

$result = Invoke-PlaneFixture $target40 $cruise 1.0
if ($result.Result.ToString() -ne 'Fail_InsufficientDV' -or @($fixtureCandidates[0]).Count -ne 0 -or $result.Value -lt 5000) {
    throw 'The planner must reject a phase-change-sized delta-v budget for the 40 degree route.'
}
Write-Host 'PASS: finite-thrust slider frontier at 27.1 and 1 milligee, station phase, fuel, and native serialization.'
