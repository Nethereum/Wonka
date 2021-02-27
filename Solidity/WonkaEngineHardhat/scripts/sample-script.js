// We require the Hardhat Runtime Environment explicitly here. This is optional 
// but useful for running the script in a standalone fashion through `node <script>`.
//
// When running the script with `hardhat run <script>` you'll find the Hardhat
// Runtime Environment's members available in the global scope.
const hre = require("hardhat");

async function main() {
  // Hardhat always runs the compile task when running scripts with its command
  // line interface.
  //
  // If this script is run directly using `node` you may want to call compile 
  // manually to make sure everything is compiled
  // await hre.run('compile');

  const TestContract = 
      await ethers.getContractFactory("OrchTestContract");

  const test = await TestContract.deploy();
  await test.deployed();

  const LibraryContract = 
    await ethers.getContractFactory("WonkaLibrary");

  const library = await LibraryContract.deploy();
  await library.deployed();

  const WonkaEngineContract = 
      await ethers.getContractFactory("WonkaEngine",
        {
            libraries: {
              WonkaLibrary: library.address
            }
        }
      );

  const wonkaEngine = await WonkaEngineContract.deploy();

  await wonkaEngine.deployed();
  
  console.log("Default number of Attributes launched with engine:", wonkaEngine.getNumberOfAttributes());
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main()
  .then(() => process.exit(0))
  .catch(error => {
    console.error(error);
    process.exit(1);
  });
