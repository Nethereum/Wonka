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
 
  /*
  beforeEach(function () {
    console.log("top beforeEach");
  });
  afterEach(function () {
    console.log("top afterEach");
  });
  */

  it("should be 3 attributes stored in the engine", function() {

    return WonkaEngine.deployed().then(function(instance) {

      return instance.getNumberOfAttributes.call();
    }).then(function(balance) {
      assert.equal(balance.valueOf(), 3, "More or less than 3 attributes populated");
    });
  });
  it("name of first Attribute should be 'Title'", function() {
    return WonkaEngine.deployed().then(function(instance) {
      return instance.getAttributeName.call(0);
    }).then(function(attrName) {
      console.log("Name of first attribute is (" + web3.toAscii(attrName.valueOf()) + ")");
    });
  });
  it("add a new Attribute called 'Language'", function() {
    return WonkaEngine.deployed().then(function(instance) {

      instance.addAttribute(web3.fromAscii('Language'),         64, 0, new String('ENG').valueOf(),   true, false);
      console.log("Added another Attribute!");

      instance.addAttribute(web3.fromAscii('BankAccountID'),   256, 0, new String('Blank').valueOf(), true, false);
      instance.addAttribute(web3.fromAscii('BankAccountName'), 1024, 0, new String('Blank').valueOf(), true, false);
      instance.addAttribute(web3.fromAscii('AccountStatus'),     3,  0, new String('ACT').valueOf(), true, false);
      instance.addAttribute(web3.fromAscii('AccountCurrValue'), 64, 100000, new String('').valueOf(), false, true);
      instance.addAttribute(web3.fromAscii('AccountType'),    1024, 0, new String('Checking').valueOf(), true, false);
      instance.addAttribute(web3.fromAscii('AccountCurrency'),   3, 0, new String('USD').valueOf(), true, false);

      console.log("Added more Attributes!");
    });
  });
  it("name of fourth Attribute should be 'Language'", function() {
    return WonkaEngine.deployed().then(function(instance) {
      return instance.getAttributeName.call(3);
    }).then(function(attrName) {
      console.log("Name of last attribute is (" + web3.toAscii(attrName.valueOf()) + ")");
    });
  });
  it("check for the ruletree", function() {
    return WonkaEngine.deployed().then(function(instance) {
      return instance.hasRuleTree.call(accounts[0]);
    }).then(function(treeExists) {
      console.log("Current ruletree for owner(" + accounts[0] + ") exists?  [" + treeExists + "]");      
    });
  });
  it("adding the data structures for rules", function() {
    return WonkaEngine.deployed().then(function(instance) {

      //var events = engine.allEvents();
      
      //var done = true;
      //instance.addRuleTree(accounts[0], web3.fromAscii('JohnSmithRuleTree'), new String('John Smith Rule Tree').valueOf(), true, true, false).then(new Promise(
      //  function(resolve, reject){
      //    events.watch(function(error, log){ resolve(log, done); });
      // }).then(function(log, done){
      //  assert.equal(log.event, "Error", "Event must be an Error");
      // }).then(done).catch(done));
      //

      var event1 = instance.CallAddRuleTree(function(error, result) {
        if (!error)
          console.log("CALLBACK -> Added the ruletree assigned to ruler: (" + result.args.ruler + ")");
      });

      instance.addRuleTree(accounts[0], web3.fromAscii('JohnSmithRuleTree'), new String('John Smith Rule Tree').valueOf(), true, true, false);

      console.log("Added the root ruletree!");      

      instance.addRule(accounts[0], web3.fromAscii('JohnSmithRuleTree'), web3.fromAscii('AccntNameEqualRule'), web3.fromAscii('BankAccountName'), EQUAL_TO_RULE, new String('JohnSmithFirstCheckingAccount').valueOf(), false, true);

      console.log("Added the rule to the root ruleset!");

      instance.addRuleSet(accounts[0], web3.fromAscii('CheckAccntSts'), new String('Will determine the account status').valueOf(), web3.fromAscii('JohnSmithRuleTree'), false, false, false);

      console.log("Added the first child ruleset to the root ruleset!");

      instance.addRule(accounts[0], web3.fromAscii('CheckAccntSts'), web3.fromAscii('CheckForTooLittleRule'), web3.fromAscii('AccountCurrValue'), LESS_THAN_RULE, new String('1000').valueOf(), false, true);
      instance.addRule(accounts[0], web3.fromAscii('CheckAccntSts'), web3.fromAscii('CheckForTooMuchRule'), web3.fromAscii('AccountCurrValue'), GREATER_THAN_RULE, new String('1000000').valueOf(), false, true);
      instance.addRule(accounts[0], web3.fromAscii('CheckAccntSts'), web3.fromAscii('AccountTypeRule'), web3.fromAscii('AccountType'), IN_DOMAIN_RULE, new String('Checking,Savings,TaxHaven').valueOf(), false, true);

      console.log("Added the rules to the first child ruleset!");

      instance.addRuleSet(accounts[0], web3.fromAscii('CheckAccntStsLeaf'), new String('Will determine the account status - leaf').valueOf(), web3.fromAscii('CheckAccntSts'), true, true, false);

      console.log("Added the leaf ruleset to the first child ruleset!");

      instance.addRule(accounts[0], web3.fromAscii('CheckAccntStsLeaf'), web3.fromAscii('ValidateStatusRule'), web3.fromAscii('AccountStatus'), EQUAL_TO_RULE, new String('OOS').valueOf(), false, true);
      instance.addRule(accounts[0], web3.fromAscii('CheckAccntStsLeaf'), web3.fromAscii('PopulatedValueRule'), web3.fromAscii('Language'), POPULATED_RULE, new String('').valueOf(), false, true);

      console.log("Added the rules to the leaf ruleset for the first child RS!");

      //Does this actually work?
      //event1.stopWatching();
    });
  });
  it("check for the ruletree (after creation)", function() {
    return WonkaEngine.deployed().then(function(instance) {
      return instance.hasRuleTree.call(accounts[0]);
    }).then(function(treeExists) {
      console.log("Current ruletree for owner(" + accounts[0] + ") exists?  [" + treeExists + "]");      
    });
  });  
  it("add Values into current record", function() {
    return WonkaEngine.deployed().then(function(instance) {

      instance.setValueOnRecord(accounts[0], web3.fromAscii('Title'), new String('The First Book').valueOf());
      instance.setValueOnRecord(accounts[0], web3.fromAscii('Price'), new String('0999').valueOf()); // in cents
      instance.setValueOnRecord(accounts[0], web3.fromAscii('PageAmount'), new String('289').valueOf());
      console.log("Added the values to the current record!");

      instance.setValueOnRecord(accounts[0], web3.fromAscii('BankAccountID'), new String('1234567890').valueOf());
      instance.setValueOnRecord(accounts[0], web3.fromAscii('BankAccountName'), new String('JohnSmithFirstCheckingAccount').valueOf());
      instance.setValueOnRecord(accounts[0], web3.fromAscii('AccountStatus'), new String('OOS').valueOf());
      // instance.setValueOnRecord(accounts[0], web3.fromAscii('AccountStatus'), new String('ACT').valueOf());
      instance.setValueOnRecord(accounts[0], web3.fromAscii('AccountCurrValue'), new String('999').valueOf());
      instance.setValueOnRecord(accounts[0], web3.fromAscii('AccountCurrency'), new String('USD').valueOf());
      instance.setValueOnRecord(accounts[0], web3.fromAscii('AccountType'), new String('Checking').valueOf());
      console.log("Added more values onto current record!");

      instance.setValueOnRecord(accounts[0], web3.fromAscii('Language'), new String('ENG').valueOf());
    });
  });
  it("run the business rules on the currently populated record", function() {
    return WonkaEngine.deployed().then(function(instance) {

      var eventRT = instance.CallRuleTree(function(error, result) {
        if (!error)
          console.log("CALLBACK -> Entering the ruletree assigned to ruler: (" + result.args.ruler + ")");
      });

      var eventRS = instance.CallRuleSet(function(error, result) {
        if (!error)
          console.log("CALLBACK -> Entering RS of ruler: (" + result.args.ruler + ") with ID(" + result.args.tmpRuleSetId + ")");
      });

      var eventRL = instance.CallRule(function(error, result) {
        if (!error)
          console.log("CALLBACK -> Entering rule of ruler: (" + result.args.ruler + 
                      ") with RSID(" + result.args.ruleSetId + ") -> RuleId(" + result.args.ruleId + 
                      ") and Type(" + result.args.ruleType + ")");
      });

      //
      //var event = instance.CallRuleTree({ruler:accounts[0]});
      //event.watch(function(error, result){
      //    if (!error) {
      //        // alert("wait for a while, check for block Synchronization or block creation");
      //        console.log(result);
      //    }
      //});
      //

      return instance.execute.call(accounts[0]);
      // instance.executeWithReport(accounts[0]);

    }).then(function(recordValid) {

      console.log("Current record for owner(" + accounts[0] + ") is valid?  [" + recordValid + "]");      
    });
  });
  it("name of fourth Attribute should be 'Language'", function() {
    return WonkaEngine.deployed().then(function(instance) {
      return instance.getAttributeName.call(3);
    }).then(function(attrName) {
      console.log("Name of last attribute is (" + web3.toAscii(attrName.valueOf()) + ")");

      /*
      ** 
      // Sleep for 5 seconds, in order to get all event output
      var delay = 5; // 5 second delay
      var now = new Date();
      var desiredTime = new Date().setSeconds(now.getSeconds() + delay);
      
      while (now < desiredTime) {
          now = new Date(); // update the current time
      }
      */
      
      // If I don't call this method, this script never dies and the Ethereum node keeps printing 'eth_getFilterChanges()'
      process.exit();
    });
  });

});
