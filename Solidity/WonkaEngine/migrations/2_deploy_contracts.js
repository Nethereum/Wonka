var WeEngine = artifacts.require("./WonkaEngine.sol");
var TestContract = artifacts.require("./OrchTestContract.sol");

module.exports = function(deployer) {
  deployer.deploy(TestContract);
  deployer.deploy(WeEngine);
};
