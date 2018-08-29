var WeEngine = artifacts.require("./WonkaEngine.sol");
var TestContract = artifacts.require("./OrchTestContract.sol");
var WrContract = artifacts.require("./WonkaRegistry.sol");

module.exports = function(deployer) {
  deployer.deploy(TestContract);
  deployer.deploy(WeEngine);
  deployer.deploy(WrContract);
};
