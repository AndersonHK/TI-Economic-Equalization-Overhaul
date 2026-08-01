"use strict";

// Reproducible 2022-2030 sanity projection for the land-military rework.
// Source nation/army/region values: installed Terra Invicta 1.0.49 templates.
// Military burdens: SIPRI, Trends in World Military Expenditure, 2022.

const years = 8;
const daysPerYear = 365;
const daysPerMonth = daysPerYear / 12;
const nationalEffortShareOfGdp = 0.50;
const militaryTechSplit = 0.80;
const buildArmySplit = 0.20;
const techCap = 5;
const navyUpkeep = 0.5;

const countries = [
  {
    id: "USA",
    name: "United States",
    gdp: 22_305_188_400_000,
    unrest: 3,
    technology: 4.5,
    armies: 6,
    maximumArmies: 14,
    awayArmies: 1,
    navies: 6,
    initialBuildFraction: 0.30,
    missingStrength: 0,
    militaryBurdenOfGdp: 0.035,
    growthRate: 0.026,
  },
  {
    id: "CHN",
    name: "China",
    gdp: 26_556_574_880_000,
    unrest: 2,
    technology: 3.9,
    armies: 4,
    maximumArmies: 26,
    awayArmies: 0,
    navies: 2,
    initialBuildFraction: 0.35,
    missingStrength: 0,
    militaryBurdenOfGdp: 0.016,
    growthRate: 0.057,
  },
  {
    id: "RUS",
    name: "Russia",
    gdp: 3_841_589_840_000,
    unrest: 4,
    technology: 3.7,
    armies: 3,
    maximumArmies: 6,
    awayArmies: 1,
    navies: 1,
    initialBuildFraction: 0.40,
    missingStrength: 1.20,
    militaryBurdenOfGdp: 0.041,
    growthRate: 0.042,
  },
];

function armyCost(technology) {
  return 2 * Math.pow(2, technology);
}

function catchUpMultiplier(technology, cap) {
  return 1 / (1 + Math.max(0, cap - technology));
}

function doctrineRate(technology, cap) {
  return 500 * Math.log(2) * Math.pow(2, technology - 1) *
    catchUpMultiplier(technology, cap);
}

function doctrineCost(fromTechnology, toTechnology, cap) {
  if (toTechnology <= fromTechnology) return 0;
  const segments = 128;
  const width = (toTechnology - fromTechnology) / segments;
  let sum = doctrineRate(fromTechnology, cap) + doctrineRate(toTechnology, cap);
  for (let index = 1; index < segments; index += 1) {
    const technology = fromTechnology + width * index;
    sum += doctrineRate(technology, cap) * (index % 2 === 0 ? 2 : 4);
  }
  return sum * width / 3;
}

function miltechCost(fromTechnology, toTechnology, armyCount, cap) {
  return doctrineCost(fromTechnology, toTechnology, cap) +
    armyCount * (armyCost(toTechnology) - armyCost(fromTechnology));
}

function technologyAfterInvestment(technology, investment, armyCount, cap) {
  if (investment <= 0 || technology >= cap) return technology;
  if (investment >= miltechCost(technology, cap, armyCount, cap)) return cap;
  let low = technology;
  let high = cap;
  for (let iteration = 0; iteration < 60; iteration += 1) {
    const middle = (low + high) / 2;
    if (miltechCost(technology, middle, armyCount, cap) < investment) low = middle;
    else high = middle;
  }
  return (low + high) / 2;
}

function assertNear(actual, expected, tolerance, label) {
  if (!Number.isFinite(actual) || Math.abs(actual - expected) > tolerance) {
    throw new Error(`${label}: expected ${expected}, got ${actual}`);
  }
}

assertNear(armyCost(3), 16, 1e-12, "tech-3 army cost");
assertNear(armyCost(5) - armyCost(4), 32, 1e-12, "tech-4-to-5 army upgrade");
assertNear(doctrineCost(4, 5, 5), 2883.5919, 0.001,
  "tech-4-to-5 doctrine cost");

function rawIpBeforePenalties(gdp) {
  return gdp / 100_000_000_000 * 1.05;
}

function availableIpBeforeUpkeep(country, gdp) {
  const unrestPenalty = Math.max(0, country.unrest - 2) / 10;
  return rawIpBeforePenalties(gdp) * (1 - unrestPenalty);
}

function monthlyUpkeep(country, technology, armyCount) {
  const homeArmies = armyCount - country.awayArmies;
  return homeArmies * technology / 10 +
    country.awayArmies * technology / 3 +
    country.navies * navyUpkeep;
}

function startingShares(country, mode) {
  const raw = rawIpBeforePenalties(country.gdp);
  const gross = availableIpBeforeUpkeep(country, country.gdp);
  const upkeep = monthlyUpkeep(country, country.technology, country.armies);
  const net = Math.max(0, gross - upkeep);
  const mappedMilitaryShare = country.militaryBurdenOfGdp /
    nationalEffortShareOfGdp;
  let discretionaryShare;
  if (mode === "budget-consistent") {
    // Preserve the observed military burden as a share of GDP even when unrest
    // reduces the nation's available IP; unrest therefore raises the required
    // priority share rather than shrinking the historical military budget.
    const targetMilitaryIp = raw * mappedMilitaryShare;
    discretionaryShare = Math.max(0, targetMilitaryIp - upkeep) / net;
  } else if (mode === "direct-priority") {
    discretionaryShare = mappedMilitaryShare;
  } else if (mode === "double-direct") {
    discretionaryShare = mappedMilitaryShare * 2;
  } else {
    throw new Error(`Unknown mode: ${mode}`);
  }
  return {
    total: discretionaryShare,
    military: discretionaryShare * militaryTechSplit,
    build: discretionaryShare * buildArmySplit,
    gross,
    raw,
    upkeep,
    net,
    mappedMilitaryShare,
  };
}

function simulate(country, mode, growGdp) {
  const shares = startingShares(country, mode);
  let gdp = country.gdp;
  let technology = country.technology;
  let armies = country.armies;
  let buildProgress = country.initialBuildFraction * armyCost(technology) -
    0.5 * armyCost(technology) * country.missingStrength;
  let technologyCapDate = null;
  let armyCapDate = null;
  let cumulativeGrossIp = 0;
  let cumulativeNetIp = 0;
  let cumulativeMilitaryIp = 0;
  let cumulativeBuildIp = 0;
  const annual = [];

  for (let day = 0; day < years * daysPerYear; day += 1) {
    if (growGdp && day > 0 && day % daysPerYear === 0) {
      gdp *= 1 + country.growthRate;
    }

    const grossMonthly = availableIpBeforeUpkeep(country, gdp);
    const upkeepMonthly = monthlyUpkeep(country, technology, armies);
    const netMonthly = Math.max(0, grossMonthly - upkeepMonthly);
    const dailyNet = netMonthly / daysPerMonth;
    cumulativeGrossIp += grossMonthly / daysPerMonth;
    cumulativeNetIp += dailyNet;

    let militaryShare = shares.military;
    let buildShare = shares.build;
    if (armies >= country.maximumArmies) {
      militaryShare += buildShare;
      buildShare = 0;
    }
    if (technology >= techCap - 1e-10) {
      buildShare += militaryShare;
      militaryShare = 0;
    }

    const militaryInvestment = dailyNet * militaryShare;
    const buildInvestment = dailyNet * buildShare;
    cumulativeMilitaryIp += militaryInvestment;
    cumulativeBuildIp += buildInvestment;
    technology = technologyAfterInvestment(
      technology, militaryInvestment, armies, techCap);
    buildProgress += buildInvestment;

    while (armies < country.maximumArmies &&
      buildProgress + 1e-10 >= armyCost(technology)) {
      buildProgress -= armyCost(technology);
      armies += 1;
      if (armies === country.maximumArmies && armyCapDate === null) {
        armyCapDate = 2022 + (day + 1) / daysPerYear;
      }
    }
    if (technology >= techCap - 1e-10 && technologyCapDate === null) {
      technologyCapDate = 2022 + (day + 1) / daysPerYear;
    }

    if ((day + 1) % daysPerYear === 0) {
      annual.push({
        year: 2022 + (day + 1) / daysPerYear,
        technology,
        armies,
        buildProgress,
      });
    }
  }

  return {
    country: country.name,
    mode,
    growGdp,
    shares,
    technology,
    armies,
    maximumArmies: country.maximumArmies,
    buildProgress,
    technologyCapDate,
    armyCapDate,
    cumulativeGrossIp,
    cumulativeNetIp,
    cumulativeMilitaryIp,
    cumulativeBuildIp,
    annual,
  };
}

function pct(value) {
  return `${(value * 100).toFixed(2)}%`;
}

const verificationOnly = process.argv.includes("--verify");
for (const mode of ["budget-consistent", "direct-priority", "double-direct"]) {
  if (!verificationOnly) console.log(`\n## ${mode}`);
  for (const country of countries) {
    const result = simulate(country, mode, false);
    const summary = {
      country: result.country,
      startingRawIpMonth: Number(result.shares.raw.toFixed(2)),
      startingGrossIpMonth: Number(result.shares.gross.toFixed(2)),
      startingUpkeepMonth: Number(result.shares.upkeep.toFixed(2)),
      startingNetIpMonth: Number(result.shares.net.toFixed(2)),
      startingMiltechCostToCap: Number(miltechCost(
        country.technology, techCap, country.armies, techCap).toFixed(1)),
      startingArmyCost: Number(armyCost(country.technology).toFixed(1)),
      militaryPriority: pct(result.shares.military),
      buildArmyPriority: pct(result.shares.build),
      tech2030: Number(result.technology.toFixed(4)),
      armies2030: result.armies,
      maximumArmies: result.maximumArmies,
      buildProgress2030: Number(result.buildProgress.toFixed(2)),
      cumulativeMilitaryIp: Number(result.cumulativeMilitaryIp.toFixed(1)),
      cumulativeBuildIp: Number(result.cumulativeBuildIp.toFixed(1)),
      technologyCapDate: result.technologyCapDate,
      armyCapDate: result.armyCapDate,
      annual: mode === "budget-consistent"
        ? result.annual.map((row) => ({
          year: row.year,
          technology: Number(row.technology.toFixed(4)),
          armies: row.armies,
        }))
        : undefined,
    };
    if (!verificationOnly) console.log(JSON.stringify(summary));
  }
}

if (!verificationOnly) {
  console.log("\n## budget-consistent with GDP-growth sensitivity");
}
for (const country of countries) {
  const result = simulate(country, "budget-consistent", true);
  const summary = {
    country: result.country,
    assumedAnnualGdpGrowth: pct(country.growthRate),
    tech2030: Number(result.technology.toFixed(4)),
    armies2030: result.armies,
    maximumArmies: result.maximumArmies,
    technologyCapDate: result.technologyCapDate,
    armyCapDate: result.armyCapDate,
  };
  if (!verificationOnly) console.log(JSON.stringify(summary));
}

if (verificationOnly) {
  console.log("PASS: military 2022-2030 projection and formula self-checks.");
}
