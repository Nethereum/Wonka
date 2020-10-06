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