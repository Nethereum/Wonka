var WonkaEngine = artifacts.require("./WonkaEngine.sol");

// create an instance of web3 using the HTTP provider.
// NOTE: in mist web3 is already available, so check first if it's available before instantiating
// var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:8545"));
// var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:7545"));

var version = web3.version.api;
console.log("Web3 version is now (" + version + ")"); // "0.2.0"

var EQUAL_TO_RULE     = 0;
var LESS_THAN_RULE    = 1;
var GREATER_THAN_RULE = 2;
var POPULATED_RULE    = 3;
var IN_DOMAIN_RULE    = 4;
var ASSIGN_RULE       = 5;

contract('WonkaEngine', function(accounts) {
  
  it("should be 3 attributes stored in the engine", function() {

    return WonkaEngine.deployed().then(function(instance) {

      return instance.getNumberOfAttributes.call();
    }).then(function(balance) {
      assert.equal(balance.valueOf(), 3, "More or less than 3 attributes populated");
    });
  });

});
