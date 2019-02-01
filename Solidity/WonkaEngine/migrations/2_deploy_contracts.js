var WeEngine = artifacts.require("./WonkaEngine.sol");
var TestContract = artifacts.require("./OrchTestContract.sol");
var WrContract = artifacts.require("./WonkaRegistry.sol");
var WtsContract = artifacts.require("./WonkaTransactionState.sol");

module.exports = function(deployer) {
  deployer.deploy(TestContract);
  deployer.deploy(WeEngine);
  deployer.deploy(WrContract);
  deployer.deploy(WtsContract);
};