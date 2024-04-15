const hre = require('hardhat');

async function main() {
  const wonkaLibrary = 
      await hre.ethers.deployContract('WonkaLibrary');

  await wonkaLibrary.waitForDeployment();

  console.log(`Wonka Library deployed to ${wonkaLibrary.target}`);

  const wonkaEngineMetadata = 
      await hre.ethers.deployContract('WonkaEngineMetadata');

  await wonkaEngineMetadata.waitForDeployment();

  console.log(`Wonka Engine Metadata deployed to ${wonkaEngineMetadata.target}`);
  
  const WonkaEngineRuleSets =
      await hre.ethers.getContractFactory("WonkaEngineRuleSets", {
        libraries: {
            WonkaLibrary: wonkaLibrary.target,
        },
      });

  const wonkaEngineRuleSets =
      await WonkaEngineRuleSets.deploy(wonkaEngineMetadata.target);

  console.log(`Wonka Engine RuleSets deployed to ${wonkaEngineRuleSets.target}`);

  const WonkaEngineOpt = await hre.ethers.getContractFactory("WonkaEngineOpt");

  const wonkaEngineOpt = 
     await WonkaEngineOpt.deploy(wonkaEngineMetadata.target, wonkaEngineRuleSets.target);

  console.log(`Wonka Engine deployed to ${wonkaEngineOpt.target}`);

  const WonkaRegistry
      = await hre.ethers.getContractFactory("WonkaRegistry");

  const wonkaRegistry = await WonkaRegistry.deploy();

  console.log(`Wonka Registry deployed to ${wonkaRegistry.target}`);

  const WonkaTransactionState
      = await hre.ethers.getContractFactory("WonkaTransactionState");

  const wonkaTrxState = await WonkaTransactionState.deploy();

  console.log(`Wonka Trx State deployed to ${wonkaTrxState.target}`);

}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});

