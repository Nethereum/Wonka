const { expect } = require("chai");

describe("WonkaEngineTest", function() {
  it("Testing deployed WonkaEngine", async function() {

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

    expect(await wonkaEngine.getNumberOfAttributes()).to.equal(3);
  });
});

/*
var WlContract = artifacts.require("./WonkaLibrary.sol");
var WeEngine = artifacts.require("./WonkaEngine.sol");
var TestContract = artifacts.require("./OrchTestContract.sol");
var WrContract = artifacts.require("./WonkaRegistry.sol");
var WtsContract = artifacts.require("./WonkaTransactionState.sol");
var ChlContract = artifacts.require("./ChronoLog.sol");

module.exports = function(deployer) {
  deployer.deploy(WlContract);
  deployer.deploy(TestContract);
  deployer.link(WlContract, WeEngine);
  deployer.deploy(WeEngine);
  deployer.deploy(WrContract);
  deployer.deploy(WtsContract);
  deployer.deploy(ChlContract);
};
*/

/*
describe("Greeter", function() {
  it("Should return the new greeting once it's changed", async function() {
    const Greeter = await ethers.getContractFactory("Greeter");
    const greeter = await Greeter.deploy("Hello, world!");
    
    await greeter.deployed();
    expect(await greeter.greet()).to.equal("Hello, world!");

    await greeter.setGreeting("Hola, mundo!");
    expect(await greeter.greet()).to.equal("Hola, mundo!");
  });
});
*/
