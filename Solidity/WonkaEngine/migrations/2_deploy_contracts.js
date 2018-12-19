var WeEngine = artifacts.require("./WonkaEngine.sol");
var TestContract = artifacts.require("./OrchTestContract.sol");
var WrContract = artifacts.require("./WonkaRegistry.sol");
var WsContract = artifacts.require("./WonkaSerializer.sol");

module.exports = function(deployer) {
  deployer.deploy(TestContract);
  deployer.deploy(WrContract);
  deployer.deploy(WsContract);
  deployer.link(WsContract, WeEngine);
  deployer.deploy(WeEngine);
};
