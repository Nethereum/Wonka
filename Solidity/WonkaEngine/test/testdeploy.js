var WonkaEngine = artifacts.require("./WonkaEngine.sol");
var OrchTestContract = artifacts.require("./OrchTestContract.sol");
var WonkaRegistry = artifacts.require("./WonkaRegistry.sol");
var WonkaTransactionState = artifacts.require("./WonkaTransactionState.sol");

// NOTE: in mist web3 is already available, so check first if it's available before instantiating
// var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:8545"));
// var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:7545"));

var version = web3.version.api;
console.log("Web3 version is now (" + version + ")");

contract('WonkaEngine', function(accounts) {
contract('OrchTestContract', function(accounts) {
contract('WonkaRegistry', function(accounts) {
contract('WonkaTransactionState', function(accounts) {
  
  it("should be 3 attributes stored in the engine", function() {
    return WonkaEngine.deployed().then(function(wInstance) {
      return OrchTestContract.deployed().then(function(tInstance) {
        return WonkaRegistry.deployed().then(function(rInstance) {
          return WonkaTransactionState.deployed().then(function(xInstance) {

            xInstance.setOwner(accounts[0], 100);

            xInstance.setExecutor(accounts[0]);

            xInstance.addConfirmation(accounts[0]);

            xInstance.setMinScoreRequirement(1);

            console.log("Address of WonkaEngine is (" + wInstance.address + ")");

            console.log("Address of OrchTestContract is (" + tInstance.address + ")");

            console.log("Address of WonkaRegistry is (" + rInstance.address + ")");

            console.log("Address of WonkaTransactionState is (" + xInstance.address + ")");

            return wInstance.getNumberOfAttributes.call();

          }).then(function(balance) {
            assert.equal(balance.valueOf(), 3, "More or less than 3 attributes populated");
          });
        });
      });
    });
  });

});
});
});
});
