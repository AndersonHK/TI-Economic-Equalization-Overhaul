[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$TargetManagedDir,
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$newtonsoftPath = Join-Path $TargetManagedDir 'Newtonsoft.Json.dll'
$gamePath = Join-Path $TargetManagedDir 'Assembly-CSharp.dll'
$unityPath = Join-Path $TargetManagedDir 'UnityEngine.CoreModule.dll'
foreach ($path in @($newtonsoftPath, $unityPath, $gamePath)) {
    [void][Reflection.Assembly]::LoadFrom($path)
}

# Exercise the installed merger, not a PowerShell approximation of the JSON.
# The default mode merges arrays by index, so an empty array does not clear one.
Add-Type -ReferencedAssemblies @(
    $newtonsoftPath, $gamePath, $unityPath,
    (Join-Path $TargetManagedDir 'netstandard.dll'),
    'System.Core', 'Microsoft.CSharp'
) -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PavonisInteractive.TerraInvicta.Modding;

public static class TechnologyTemplateMergeValidation
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static List<JObject> Read(string path)
    {
        return JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(path));
    }

    private static JObject Find(List<JObject> rows, string name)
    {
        return rows.Single(row => (string)row["dataName"] == name);
    }

    private static List<JObject> MergeAndVerify(
        JsonController controller, string baselinePath, string overridePath)
    {
        List<JObject> baseline = Read(baselinePath);
        JsonMod mod = controller.LoadJson(overridePath);
        string fileName = Path.GetFileName(overridePath);
        Require(mod != null, "Native loader failed for " + fileName);
        Require(mod.TemplatesToReplace == null || !mod.TemplatesToReplace.Contains(fileName),
            "Do not replace the entire template file: " + fileName);
        MergeArrayHandling mode = MergeArrayHandling.Merge;
        if (mod.TemplatesToConcatArrays != null && mod.TemplatesToConcatArrays.Contains(fileName))
            mode = MergeArrayHandling.Concat;
        if (mod.TemplatesToReplaceArrays != null && mod.TemplatesToReplaceArrays.Contains(fileName))
            mode = MergeArrayHandling.Replace;
        if (fileName == "TITechTemplate.json")
            Require(mode == MergeArrayHandling.Replace,
                "TITechTemplate.json must use TemplatesToReplaceArrays to clear inherited prerequisites and effects.");

        List<JObject> merged = controller.CombineJson(Read(baselinePath), mod.FileContents, false, mode);
        Require(merged.Count == baseline.Count, "Unexpected template additions/removals in " + fileName);
        foreach (JObject original in baseline)
        {
            string name = (string)original["dataName"];
            JObject actual = Find(merged, name);
            JObject change = mod.FileContents.SingleOrDefault(row => (string)row["dataName"] == name);
            foreach (JProperty field in original.Properties())
                if (change == null || change.Property(field.Name) == null)
                    Require(JToken.DeepEquals(actual[field.Name], field.Value),
                        "Unspecified baseline field changed: " + name + "." + field.Name);
            if (change == null) continue;
            foreach (JProperty field in change.Properties())
                Require(JToken.DeepEquals(actual[field.Name], field.Value),
                    "Native merged field differs from authored override: " + name + "." + field.Name);
        }
        return merged;
    }

    public static string Run(string templates, string modFiles)
    {
        JsonController controller = new JsonController();
        string techPath = Path.Combine(templates, "TITechTemplate.json");
        string overridePath = Path.Combine(modFiles, "TITechTemplate.json");
        List<JObject> overrides = Read(overridePath);

        // Reproduce the original loader bug before asserting the configured fix.
        List<JObject> oldMerge = controller.CombineJson(Read(techPath), overrides, false, MergeArrayHandling.Merge);
        JObject oldNeural = Find(oldMerge, "AdvancedNeuralNetworks");
        Require(((JArray)oldNeural["prereqs"]).Values<string>().SequenceEqual(new[] { "PhotonicComputing" }),
            "Installed default merge no longer reproduces the inherited prerequisite; reassess this regression.");
        Require((int)oldNeural["researchCost"] == 2500,
            "The scalar price should already merge correctly independently of array replacement.");

        List<JObject> technologies = MergeAndVerify(controller, techPath, overridePath);
        JObject neural = Find(technologies, "AdvancedNeuralNetworks");
        Require((int)neural["researchCost"] == 2500 && ((JArray)neural["prereqs"]).Count == 0,
            "Resolved Advanced Neural Networks must cost 2,500 and have no prerequisites.");
        Require((int)Find(technologies, "MissiontoMars")["researchCost"] == (int)neural["researchCost"],
            "Resolved Neural Networks must match Mission to Mars.");
        foreach (string name in new[] { "DeuteriumTritiumFusion", "NuclearFusioninSpace" })
            Require((int)Find(technologies, name)["researchCost"] == 40000 &&
                (int)Find(technologies, name)["researchCost"] ==
                    (int)Find(technologies, "HighTemperatureSuperconductors")["researchCost"],
                "Resolved fusion entry price mismatch: " + name);

        List<JObject> starts = MergeAndVerify(controller,
            Path.Combine(templates, "TIStartTimeTemplate.json"),
            Path.Combine(modFiles, "TIStartTimeTemplate.json"));
        Require(((JArray)Find(starts, "2026Start")["globalTechsCompleted"]).Values<string>()
            .Count(name => name == "AdvancedNeuralNetworks") == 1,
            "The resolved 2026 start must complete Neural Networks exactly once.");
        Require(!((JArray)Find(starts, "ModernDayStart")["globalTechsCompleted"]).Values<string>()
            .Contains("AdvancedNeuralNetworks"),
            "The resolved 2022 start must not complete Neural Networks.");
        return "PASS: native JSON merge validates " + technologies.Count +
            " technologies, authored arrays and preserved baseline fields, 2,500-cost prerequisite-free Neural Networks, and the 2026-only completion grant.";
    }
}
'@

$templates = Join-Path (Split-Path -Parent $TargetManagedDir) 'StreamingAssets\Templates'
$modFiles = Join-Path $RepositoryRoot 'TIEconomyMod\ModFiles'
Write-Host ([TechnologyTemplateMergeValidation]::Run($templates, $modFiles))
