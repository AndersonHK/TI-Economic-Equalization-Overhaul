"use strict";

// This is a dependency-free calibration harness, not gameplay code. Its inputs
// come from TI 1.0.47's 2022 templates and its vanilla equations come from the
// installed Assembly-CSharp.dll. See docs/economy-growth-simulator.md.

const countries = [
  { code: "USA", name: "United States", pcgdp: 66506, population: 335.39, cores: 4, resources: 5, density: 34.1, education: 9, government: 7.85, cohesion: 3, unrest: 3, inequality: 4.17, climate: 1.221 },
  { code: "CHN", name: "China", pcgdp: 18318, population: 1449.74, cores: 4, resources: 6, density: 150.8, education: 6.57, government: 2.21, cohesion: 8, unrest: 2, inequality: 3.82, climate: 1.131 },
  { code: "IND", name: "India", pcgdp: 7327, population: 1410.73, cores: 2, resources: 2, density: 444.1, education: 5.55, government: 6.91, cohesion: 3, unrest: 4, inequality: 3.57, climate: 1.347 },
  { code: "RUS", name: "Russia", pcgdp: 25924, population: 148.19, cores: 1, resources: 5, density: 8.7, education: 8.23, government: 3.24, cohesion: 8, unrest: 4, inequality: 3.53, climate: 0.392 },
  { code: "DEU", name: "Germany", pcgdp: 55296, population: 83.87, cores: 1, resources: 0, density: 234.7, education: 9.43, government: 8.67, cohesion: 4, unrest: 1, inequality: 3.17, climate: 1 },
  { code: "FRA", name: "France", pcgdp: 47310, population: 68.68, cores: 1, resources: 0, density: 104.6, education: 8.17, government: 7.99, cohesion: 4, unrest: 2, inequality: 3.24, climate: 1.043 },
  { code: "JPN", name: "Japan", pcgdp: 42560, population: 126.34, cores: 1, resources: 0, density: 340, education: 8.51, government: 8.15, cohesion: 7, unrest: 0, inequality: 3.29, climate: 1 },
  { code: "SAU", name: "Saudi Arabia", pcgdp: 48951, population: 36, cores: 0, resources: 3, density: 16.7, education: 7.89, government: 2.08, cohesion: 9, unrest: 5, inequality: 4.59, climate: 1 },
  { code: "NGA", name: "Nigeria", pcgdp: 5111, population: 218.43, cores: 0, resources: 1, density: 239.1, education: 4.99, government: 4.11, cohesion: 3, unrest: 6, inequality: 3.51, climate: 1.613 },
  { code: "BRA", name: "Brazil", pcgdp: 15012, population: 215.78, cores: 1, resources: 3, density: 24, education: 6.94, government: 6.86, cohesion: 3, unrest: 4, inequality: 4.89, climate: 1.028 },
  { code: "MEX", name: "Mexico", pcgdp: 19276, population: 131.97, cores: 1, resources: 2, density: 67.3, education: 7.03, government: 5.57, cohesion: 3, unrest: 6, inequality: 4.54, climate: 1.716 },
  { code: "EGY", name: "Egypt", pcgdp: 12880, population: 106.75, cores: 0, resources: 0, density: 97.7, education: 6.18, government: 2.93, cohesion: 7, unrest: 3, inequality: 3.15, climate: 1 },
  { code: "IDN", name: "Indonesia", pcgdp: 12558, population: 279.96, cores: 1, resources: 2, density: 189, education: 6.5, government: 6.71, cohesion: 4, unrest: 3, inequality: 3.7, climate: 1.832 },
  { code: "TUR", name: "Turkey", pcgdp: 32981, population: 85.7, cores: 1, resources: 1, density: 109.8, education: 7.31, government: 4.35, cohesion: 7, unrest: 5, inequality: 4.19, climate: 1 },
  { code: "CAN", name: "Canada", pcgdp: 50553, population: 38.94, cores: 1, resources: 2, density: 3.9, education: 8.94, government: 8.87, cohesion: 5, unrest: 0, inequality: 3.33, climate: 0 },
  { code: "AUS", name: "Australia", pcgdp: 54007, population: 26.16, cores: 2, resources: 2, density: 3.3, education: 9.24, government: 8.9, cohesion: 6, unrest: 1, inequality: 3.43, climate: 1.118 },
  { code: "GBR", name: "United Kingdom", pcgdp: 48138, population: 68.59, cores: 1, resources: 0, density: 262.7, education: 9.28, government: 8.1, cohesion: 3, unrest: 1, inequality: 3.51, climate: 1.003 }
];

const proposedParameters = {
  baseGain: 1,
  laborKnee: 37500,
  resourceKnee: 55000,
  startingLaborReturnFloor: 0.35,
  startingResourceReturnFloor: 0.45,
  resourceReference: 1000,
  resourceAbundanceExponent: 0.30,
  resourceMaximum: 1,
  resourceLift: 1,
  landMaximum: 0.25,
  landLift: 0.25,
  coreMaximum: 1.2,
  coreHalfSaturation: 2,
  fullTreeProductivity: 3.40,
  constraintReliefLinearShare: 0.10,
  climateGdpDamageMultiplier: 0.90
};

// These two values intentionally belong to different models. The starting
// multiplier is shared, while the deployed benchmark must retain its actual
// full-tree value instead of inheriting the retuned proposal.
const startingProductivity = 1.0201;
const deployedBenchmarkFullTreeProductivity = 3.7964;

function clamp(value, minimum, maximum) {
  return Math.max(minimum, Math.min(maximum, value));
}

function technologyAt(fraction, finalProgress,
  fullTreeProductivity = proposedParameters.fullTreeProductivity) {
  const progress = finalProgress * fraction;
  return {
    productivity: startingProductivity *
      Math.pow(fullTreeProductivity / startingProductivity, progress),
    labor: progress,
    resource: progress
  };
}

function proposedAbundance(country, gdpBillions, pcgdp,
  parameters = proposedParameters) {
  const stability = clamp(1 - country.unrest / 10, 0, 1);
  const resourceRatio = country.resources * parameters.resourceReference /
    Math.max(gdpBillions, 1);
  const poweredResource = Math.pow(resourceRatio,
    parameters.resourceAbundanceExponent);
  const resource = parameters.resourceMaximum * poweredResource /
    (1 + poweredResource) * stability;
  const landRatio = 50 / Math.max(country.density, 0.1);
  const land = parameters.landMaximum * landRatio / (1 + landRatio) *
    stability * (0.25 + 0.75 / (1 + pcgdp / 30000));
  return { resource, land, support: 1 + resource + land };
}

function proposedGain(country, gdpBillions, technology,
  parameters = proposedParameters) {
  const pcgdp = gdpBillions * 1000 / country.population;
  const abundance = proposedAbundance(country, gdpBillions, pcgdp, parameters);
  const core = 1 + parameters.coreMaximum * country.cores /
    (parameters.coreHalfSaturation + country.cores);
  const labor = core * (1 + 0.15 * country.education) *
    (1 + 0.05 * country.government) *
    (1.2 - 0.04 * Math.abs(country.cohesion - 5));
  const referenceLabor = (1 + parameters.coreMaximum / 3) *
    (1 + 0.15 * 7) * (1 + 0.05 * 6) * 1.2;
  const laborPressure = (pcgdp / parameters.laborKnee) /
    Math.max(labor / referenceLabor, 0.05);
  const resourcePressure = (pcgdp / parameters.resourceKnee) /
    Math.max(abundance.support, 0.05);
  const relief = progress => progress *
    (parameters.constraintReliefLinearShare +
      (1 - parameters.constraintReliefLinearShare) * progress);
  const technologyAdjustedLaborFloor =
    parameters.startingLaborReturnFloor +
    (1 - parameters.startingLaborReturnFloor) * relief(technology.labor);
  const technologyAdjustedResourceFloor =
    parameters.startingResourceReturnFloor +
    (1 - parameters.startingResourceReturnFloor) *
      relief(technology.resource);
  const laborConstraint = technologyAdjustedLaborFloor +
    (1 - technologyAdjustedLaborFloor) /
    (1 + Math.pow(laborPressure, 1.4));
  const resourceConstraint = technologyAdjustedResourceFloor +
    (1 - technologyAdjustedResourceFloor) /
    (1 + Math.pow(resourcePressure, 1.2));
  return parameters.baseGain * technology.productivity *
    laborConstraint * resourceConstraint *
    (1 + parameters.resourceLift * abundance.resource +
      parameters.landLift * abundance.land);
}

function currentModGain(country, gdpBillions, technology) {
  const pcgdp = gdpBillions * 1000 / country.population;
  const stability = clamp(1 - country.unrest / 10, 0, 1);
  const core = 1 + 0.6 * country.cores / (2 + country.cores);
  const resourceRatio = country.resources * 100 /
    Math.max(gdpBillions, 1);
  const resource = resourceRatio / (1 + resourceRatio) * stability;
  const landRatio = 50 / Math.max(country.density, 0.1);
  const land = 0.25 * landRatio / (1 + landRatio) * stability *
    (0.25 + 0.75 / (1 + pcgdp / 30000));
  return 0.33 * 0.40 * core *
    (1 + 0.15 * country.education) *
    (1 + 0.05 * country.government) *
    (1.2 - 0.04 * Math.abs(country.cohesion - 5)) *
    6 * Math.pow(0.96, pcgdp / 1000) *
    technology.productivity * (1 + resource + land);
}

function vanillaGain(country) {
  const perCapita = (3 + 1.5 * country.resources +
    1.5 * country.cores + 0.5 * country.government +
    country.education) * Math.pow(country.population / 50, -0.35);
  return perCapita * country.population / 1000;
}

function modInvestmentPoints(country, gdpBillions) {
  const pcgdp = gdpBillions * 1000 / country.population;
  const lowIncome = 0.7 + 0.3 * clamp(pcgdp / 15000, 0, 1);
  const unrest = 1 - Math.max(country.unrest - 2, 0) / 10;
  return gdpBillions / 100 * lowIncome * 1.05 * unrest;
}

function vanillaInvestmentPoints(country, gdpBillions) {
  const unrest = 1 - Math.max(country.unrest - 2, 0) / 10;
  return Math.pow(gdpBillions, 0.35) * unrest;
}

function annualClimateDamage(temperature, inequality, exposure) {
  if (temperature <= 0.25 || exposure <= 0) return 0;
  const excess = temperature - 0.25;
  const percent = (0.14577 * excess * excess + 0.31839 * excess) *
    Math.pow(1.14, inequality);
  return clamp(percent / 100 * exposure, 0, 0.99);
}

function simulate(country, model, options = {}) {
  const allocation = options.allocation ?? 0.50;
  const endYear = options.endYear ?? 2050;
  const endTemperature = options.endTemperature ?? 2.7;
  const finalTechnology = options.finalTechnology ?? 0.50;
  const months = (endYear - 2022) * 12;
  let gdpBillions = country.pcgdp * country.population / 1000;
  let inequality = country.inequality;
  let previousYearGdp = gdpBillions;
  const points = [];

  for (let month = 0; month <= months; month += 1) {
    const fraction = months === 0 ? 1 : month / months;
    // The deployed benchmark keeps its existing full-tree 3.7964x trajectory.
    // Only the proposed model uses the newly calibrated 3.40x productivity cap.
    const fullTreeProductivity = model === "current"
      ? deployedBenchmarkFullTreeProductivity
      : proposedParameters.fullTreeProductivity;
    const technology = technologyAt(fraction, finalTechnology,
      fullTreeProductivity);
    const temperature = 1.2601 +
      (endTemperature - 1.2601) * fraction;
    const gain = model === "proposed"
      ? proposedGain(country, gdpBillions, technology)
      : model === "current"
        ? currentModGain(country, gdpBillions, technology)
        : vanillaGain(country);
    const investmentPoints = model === "vanilla"
      ? vanillaInvestmentPoints(country, gdpBillions)
      : modInvestmentPoints(country, gdpBillions);
    const climateAnnual = annualClimateDamage(temperature,
      inequality, country.climate) *
      (model === "proposed"
        ? proposedParameters.climateGdpDamageMultiplier
        : 1);
    const climateMonthly = 1 - Math.pow(1 - climateAnnual, 1 / 12);
    const priorityMonthly = investmentPoints * allocation * gain /
      gdpBillions;

    if (month % 12 === 0) {
      const grossAnnual = (Math.pow(1 + priorityMonthly, 12) - 1) * 100;
      const netAnnual = month === 0
        ? (Math.pow((1 + priorityMonthly) * (1 - climateMonthly), 12) - 1) * 100
        : (gdpBillions / previousYearGdp - 1) * 100;
      points.push({
        year: 2022 + month / 12,
        pcgdp: gdpBillions * 1000 / country.population,
        grossAnnual,
        climateAnnual: climateAnnual * 100,
        netAnnual,
        inequality,
        gain
      });
      previousYearGdp = gdpBillions;
    }

    if (month < months) {
      gdpBillions = (gdpBillions +
        investmentPoints * allocation * gain) * (1 - climateMonthly);
      inequality += climateMonthly / 5 * (model === "vanilla" ? 1 : 2);
    }
  }
  return points;
}

function parseArguments(argv) {
  const options = {};
  for (let index = 0; index < argv.length; index += 2) {
    const value = Number(argv[index + 1]);
    if (argv[index] === "--allocation") options.allocation = value;
    if (argv[index] === "--end-year") options.endYear = value;
    if (argv[index] === "--end-temperature") options.endTemperature = value;
    if (argv[index] === "--technology") options.finalTechnology = value;
  }
  return options;
}

function printComparison(options) {
  console.log("| Country | 0.7.0 initial net | Pre-0.7.0 initial net | Vanilla initial net | 0.7.0 end GDP/c | Pre-0.7.0 end GDP/c | Vanilla end GDP/c |");
  console.log("|---|---:|---:|---:|---:|---:|---:|");
  for (const country of countries) {
    const proposed = simulate(country, "proposed", options);
    const current = simulate(country, "current", options);
    const vanilla = simulate(country, "vanilla", options);
    const proposedEnd = proposed[proposed.length - 1].pcgdp.toFixed(0);
    const currentEnd = current[current.length - 1].pcgdp.toFixed(0);
    const vanillaEnd = vanilla[vanilla.length - 1].pcgdp.toFixed(0);
    console.log(`| ${country.name} | ${proposed[0].netAnnual.toFixed(1)}% | ${current[0].netAnnual.toFixed(1)}% | ${vanilla[0].netAnnual.toFixed(1)}% | $${proposedEnd} | $${currentEnd} | $${vanillaEnd} |`);
  }
}

module.exports = {
  annualClimateDamage,
  countries,
  currentModGain,
  modInvestmentPoints,
  proposedAbundance,
  proposedGain,
  proposedParameters,
  simulate,
  technologyAt,
  vanillaGain,
  vanillaInvestmentPoints
};

if (require.main === module) {
  printComparison(parseArguments(process.argv.slice(2)));
}
