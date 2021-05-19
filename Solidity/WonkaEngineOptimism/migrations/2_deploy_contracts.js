var WlContract = artifacts.require("./WonkaLibrary.sol");
var WeEngine = artifacts.require("./WonkaEngineOpt.sol");
var WeMdContract = artifacts.require("./WonkaEngineMetadata.sol");
var WeRsEngine = artifacts.require("./WonkaEngineRuleSets.sol");
var TestContract = artifacts.require("./OrchTestContract.sol");
var WrContract = artifacts.require("./WonkaRegistry.sol");
var WtsContract = artifacts.require("./WonkaTransactionState.sol");

module.exports = function(deployer) {
  deployer.deploy(WlContract);
  deployer.link(WlContract, WeEngine);
  deployer.link(WlContract, WeMdContract);
  deployer.link(WlContract, WeRsEngine);

  deployer.deploy(WeMdContract)
  // Wait until the contract is deployed
  .then(() => WeMdContract.deployed())
  // Deploy the RS contract, while passing the address of the
  // MD contract
  .then(() => deployer.deploy(WeRsEngine, WeMdContract.address))
  .then(() => WeRsEngine.deployed())
  .then(() => deployer.deploy(WeEngine, WeMdContract.address, WeRsEngine.address))
  .then(() => WeEngine.deployed())
  .then(() => deployer.deploy(TestContract));

  /*
  deployer.deploy(WeMdContract);
  deployer.link(WeMdContract, WeRsEngine);
  deployer.link(WeMdContract, WeEngine);
  deployer.deploy(WeRsEngine);
  deployer.link(WeRsEngine, WeEngine);
  deployer.deploy(WeEngine);
  */

  deployer.deploy(WrContract);
  deployer.deploy(WtsContract);
};