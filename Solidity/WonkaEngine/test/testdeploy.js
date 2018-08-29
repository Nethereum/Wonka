var WonkaEngine = artifacts.require("./WonkaEngine.sol");
var OrchTestContract = artifacts.require("./OrchTestContract.sol");
var WonkaRegistry = artifacts.require("./WonkaRegistry.sol");

// NOTE: in mist web3 is already available, so check first if it's available before instantiating
// var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:8545"));
// var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:7545"));

var version = web3.version.api;
console.log("Web3 version is now (" + version + ")");

var EQUAL_TO_RULE     = 0;
var LESS_THAN_RULE    = 1;
var GREATER_THAN_RULE = 2;
var POPULATED_RULE    = 3;
var IN_DOMAIN_RULE    = 4;
var ASSIGN_RULE       = 5;

contract('WonkaEngine', function(accounts) {
contract('OrchTestContract', function(accounts) {
contract('WonkaRegistry', function(accounts) {
  
  it("should be 3 attributes stored in the engine", function() {
    return WonkaEngine.deployed().then(function(wInstance) {
      return OrchTestContract.deployed().then(function(tInstance) {
        return WonkaRegistry.deployed().then(function(rInstance) {

          console.log("Address of WonkaEngine is (" + wInstance.address + ")");

          console.log("Address of OrchTestContract is (" + tInstance.address + ")");

          console.log("Address of WonkaRegistry is (" + rInstance.address + ")");

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
