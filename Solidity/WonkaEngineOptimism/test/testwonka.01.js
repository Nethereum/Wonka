var WonkaEngineOpt        = artifacts.require("./WonkaEngineOpt.sol");
var OrchTestContract      = artifacts.require("./OrchTestContract.sol");
var WonkaRegistry         = artifacts.require("./WonkaRegistry.sol");
var WonkaTransactionState = artifacts.require("./WonkaTransactionState.sol");

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
var OP_ADD_RULE       = 6;
var OP_SUB_RULE       = 7;
var OP_MUL_RULE       = 8;
var OP_DIV_RULE       = 9;
var CUSTOM_OP_RULE    = 10;

function sleep(milliseconds) {
  const date = Date.now();
  let currentDate = null;
  do {
    currentDate = Date.now();
  } while (currentDate - date < milliseconds);
}

contract('WonkaEngineOpt', function(accounts) {
contract('OrchTestContract', function(accounts) {
contract('WonkaRegistry', function(accounts3) {
contract('WonkaTransactionState', function(accounts4) {
 
  it("should be 3 attributes stored in the engine", function() {

    return WonkaEngineOpt.deployed().then(function(instance) {

      return instance.getNumberOfAttributes.call();
    }).then(function(balance) {
      assert.equal(balance.valueOf(), 3, "More or less than 3 attributes populated");
    });
  });
  it("add a new Attribute called 'Language'", function() {
    return WonkaEngineOpt.deployed().then(function(instance) {

      instance.addAttribute(web3.utils.fromAscii('Language'),         64, 0, new String('ENG').valueOf(),   true, false);
      console.log("Added another Attribute!");

      instance.addAttribute(web3.utils.fromAscii('BankAccountID'),   256, 0, new String('Blank').valueOf(), true, false);
      sleep(1000);
      instance.addAttribute(web3.utils.fromAscii('BankAccountName'), 1024, 0, new String('Blank').valueOf(), true, false);
      instance.addAttribute(web3.utils.fromAscii('AccountStatus'),     3,  0, new String('ACT').valueOf(), true, false);
      sleep(1000);
      instance.addAttribute(web3.utils.fromAscii('AccountCurrValue'), 64, 100000, new String('').valueOf(), false, true);
      instance.addAttribute(web3.utils.fromAscii('AccountType'),    1024, 0, new String('Checking').valueOf(), true, false);
      sleep(1000);
      instance.addAttribute(web3.utils.fromAscii('AccountCurrency'),   3, 0, new String('USD').valueOf(), true, false);
      instance.addAttribute(web3.utils.fromAscii('AccountPrevValue'), 64, 100000, new String('').valueOf(), false, true);
      instance.addAttribute(web3.utils.fromAscii('StartSaleDate'),    64, 0, new String('').valueOf(), true, true);
      sleep(1000);

      console.log("Added more Attributes!");
    });
  });
  it("check for the ruletree", function() {

    return WonkaEngineOpt.deployed().then(function(instance) {
      return instance.hasRuleTree.call(accounts[0]);
    }).then(function(treeExists) {
      console.log("Current ruletree for owner(" + accounts[0] + ") exists?  [" + treeExists + "]");      
    });
  });
  it("adding the data structures for rules", function() {

    return WonkaEngineOpt.deployed().then(function(instance) {

      instance.addRuleTree(accounts[0], web3.utils.fromAscii('JohnSmithRuleTree'), new String('John Smith Rule Tree').valueOf(), true, true, false);

      sleep(3000);

      console.log("Added the root ruletree!");      

      instance.addRule(accounts[0], web3.utils.fromAscii('JohnSmithRuleTree'), web3.utils.fromAscii('AccntNameEqualRule'), web3.utils.fromAscii('BankAccountName'), EQUAL_TO_RULE, new String('JohnSmithFirstCheckingAccount').valueOf(), false, true);

      sleep(3000);

      console.log("Added the rule to the root ruleset!");

      instance.addRuleSet(accounts[0], web3.utils.fromAscii('CheckAccntSts'), new String('Will determine the account status').valueOf(), web3.utils.fromAscii('JohnSmithRuleTree'), false, false, false);

      sleep(5000);

      console.log("Added the first child ruleset to the root ruleset!");

      instance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntSts'), web3.utils.fromAscii('CheckForTooLittleRule'), web3.utils.fromAscii('AccountCurrValue'), LESS_THAN_RULE, new String('1000').valueOf(), false, true);
      sleep(1000);
      instance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntSts'), web3.utils.fromAscii('CheckForTooMuchRule'), web3.utils.fromAscii('AccountCurrValue'), GREATER_THAN_RULE, new String('2000').valueOf(), false, true);
      sleep(1000);
      instance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntSts'), web3.utils.fromAscii('AccountTypeRule'), web3.utils.fromAscii('AccountType'), IN_DOMAIN_RULE, new String('Checking,Savings,TaxHaven').valueOf(), false, true);

      sleep(3000);

      console.log("Added the rules to the first child ruleset!");

      instance.addRuleSet(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), new String('Will determine the account status - leaf').valueOf(), web3.utils.fromAscii('CheckAccntSts'), true, true, false);

      sleep(5000);

      console.log("Added the leaf ruleset to the first child ruleset!");

      instance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('ValidateStatusRule'), web3.utils.fromAscii('AccountStatus'), EQUAL_TO_RULE, new String('ACT').valueOf(), false, true);
      sleep(1000);
      instance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('PopulatedValueRule'), web3.utils.fromAscii('Language'), POPULATED_RULE, new String('').valueOf(), false, true);
      sleep(1000);
      instance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('SaleDateRule'), web3.utils.fromAscii('StartSaleDate'), LESS_THAN_RULE, new String('NOW').valueOf(), false, true);

      console.log("Added the rules to the leaf ruleset for the first child RS!");

    });
  });
  it("check for the ruletree (after creation)", function() {

    sleep(2000);

    return WonkaEngineOpt.deployed().then(function(instance) {

      return instance.hasRuleTree.call(accounts[0]);

    }).then(function(treeExists) {

      console.log("Current ruletree for owner(" + accounts[0] + ") exists?  [" + treeExists + "]");

    });
  });  
  it("getting RuleTree Props", function() {

    sleep(1000);
    
    return WonkaEngineOpt.deployed().then(function(instance) {

      console.log("Calling getRuleTreeProps() method");

      return instance.getRuleTreeProps.call(accounts[0]).then(function(results) {

        var id           = results[0].toString();
        var desc         = results[1].toString();
        var rootRSName   = web3.utils.toAscii(results[2].toString());
        var totalRuleCnt = results[3].toString();

        var nTotalRuleCount = parseInt(totalRuleCnt, 10);

        console.log("Root RS Name for tree of [" + accounts[0] + "] is (" + rootRSName + "), Total Rule Count for tree is (" + nTotalRuleCount + ")");

      });
    });
  });
  it("getting Root RuleSet Props", function() {

    sleep(1000);
    
    return WonkaEngineOpt.deployed().then(function(instance) {

      console.log("Calling getRuleSetProps() method for 'JohnSmithRuleTree'");

      return instance.getRuleSetProps.call(accounts[0], web3.utils.fromAscii('JohnSmithRuleTree')).then(function(results) {

        var evalRuleCount  = results[3].toString();
        var aggrRuleCount  = results[4].toString();
        var childRSCount   = results[5].toString();

        var nEvalRuleCount = parseInt(evalRuleCount, 10);
        var nAggrRuleCount = parseInt(aggrRuleCount, 10);
        var nChildRSCount  = parseInt(childRSCount, 10);

        console.log("Props are: EvalCount(" + nEvalRuleCount + "), AggrRuleCount(" + nAggrRuleCount + "), ChildRSCount(" + nChildRSCount + ")");

      });
    });
  });
  it("add Values into current record", function() {

    sleep(1000);

    return WonkaEngineOpt.deployed().then(function(instance) {

      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('Title'), new String('The First Book').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('Price'), new String('0999').valueOf()); // in cents
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('PageAmount'), new String('289').valueOf());
      console.log("Added the values to the current record!");

      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('BankAccountID'), new String('1234567890').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('BankAccountName'), new String('JohnSmithFirstCheckingAccount').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('AccountStatus'), new String('OOS').valueOf());
      //instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('AccountStatus'), new String('ACT').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('AccountCurrValue'), new String('999').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('AccountCurrency'), new String('USD').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('AccountType'), new String('Checking').valueOf());
      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('Language'), new String('ENG').valueOf());
      console.log("Added more values onto current record!");

      instance.setValueOnRecord(accounts[0], web3.utils.fromAscii('Language'), new String('ENG').valueOf());

      sleep(1000);
      
    });
  });
  it("run the business rules on the currently populated record", function() {

    sleep(2000);

    return WonkaEngineOpt.deployed().then(function(instance) {

      return instance.executeRuleTree.call(accounts[0]);

    }).then(function(recordValid) {

      console.log("Current record for owner(" + accounts[0] + ") is valid through default execution?  [" + recordValid + "]");      
    });
  });
  it("Get events for RuleTree run #1", function() {

    sleep(1000);

    return WonkaEngineOpt.deployed().then(function(instance) {

      /*
       * When we want to observed all events
       *
      const filter = web3.eth.filter({
        fromBlock: 0,
        toBlock: 'latest',
        address: instance.address
      });
      
      filter.watch((error, result) => {
        if (error)
          console.log('Error in event handler: ' + error);
        else
          console.log('Event: ' + JSON.stringify(eventResult.args));
      });
       */

    });
  });
  it("Running the rules engine with Orchestration mode enabled", function() {

    sleep(1000);

    return WonkaEngineOpt.deployed().then(function(wInstance) {      
      return OrchTestContract.deployed().then(function(testInstance) {

        wInstance.setOrchestrationMode(true, web3.utils.fromAscii('TEST'));

        console.log("Set Orchestration mode to on");

        wInstance.addSource(web3.utils.fromAscii('TEST'), web3.utils.fromAscii('ACT'), testInstance.address, web3.utils.fromAscii('getAttrValueBytes32'), web3.utils.fromAscii('setAttrValueBytes32'));

        sleep(1000);

        return wInstance.getDefaultSource.call();

      }).then(function(defSource) {

        console.log("Value of Default Source is (" + web3.utils.toAscii(defSource) + ")");

        return wInstance.getValueOnRecord.call(accounts[0], web3.utils.fromAscii('AccountStatus'));

      }).then(function(accountStatus) {

        console.log("Value of AccountStatus attribute is (" + new String(accountStatus).valueOf() + ")");

        return wInstance.getValueOnRecord.call(accounts[0], web3.utils.fromAscii('Language'));

      }).then(function(langCd) {

        console.log("Value of Language attribute is (" + new String(langCd).valueOf() + ")");

        return wInstance.getValueOnRecord.call(accounts[0], web3.utils.fromAscii('StartSaleDate'));

      }).then(function(startSaleDt) {

        console.log("Value of StartSaleDate attribute is (" + new String(startSaleDt).valueOf() + ")"); 

        return wInstance.executeRuleTree.call(accounts[0]);

      }).then(function(recordValid) {         

        sleep(1000);

        console.log("Current record for owner is valid through Orchestration execution?  [" + recordValid + "]");

        // Now let's add an assignment rule to the last ruleset, where we set the Language to '???'
        wInstance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('AssignLangRule'), web3.utils.fromAscii('Language'), ASSIGN_RULE, new String('???').valueOf(), false, true);

        sleep(3000);

        console.log("Added assignment rule to set a value on the Orchestration contract using Assembly.");
     
        // Since we've now added an assignment rule (which can now change the blockchain), we must execute the engine's validation within a transaction
        wInstance.executeRuleTree(accounts[0]);

        sleep(4000);

        // Now let's check the validation result, which should still be false
        return wInstance.getLastTransactionSuccess.call();

      }).then(function(recordValid) {
  
        console.log("Current record for owner is valid, with added Assignment rule?  [" + recordValid + "]");   

        // Now let's check the record on the Orchestration contract, to ensure that the Language has been set to '???'
        return wInstance.getValueOnRecord.call(accounts[0], web3.utils.fromAscii('Language'));

      }).then(function(currLang) {
  
        sleep(1000);

        console.log("Current value of Language is (" + new String(currLang).valueOf() + ")");      

        // Now let's add an OpAdd rule to the last ruleset, where we set the AccountCurrValue = AccountCurrValue + AccountPrevValue + 1 (i.e., 2500 = 999 + 1500 + 1)
        wInstance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('SumForCurrValue'), web3.utils.fromAscii('AccountCurrValue'), OP_ADD_RULE, new String('AccountCurrValue,AccountPrevValue,1').valueOf(), false, true);      

        sleep(2000);

        console.log("Added OP_ADD rule to set a value on the Orchestration contract using Assembly.");

        sleep(2000);

        console.log("Running RuleTree with new OP_ADD rule.");
     
        // Since we've now added an assignment rule (which can now change the blockchain), we must execute the engine's validation within a transaction
        wInstance.executeRuleTree(accounts[0]);

        sleep(5000);

        // Now let's check the validation result, which should still be false
        return wInstance.getLastTransactionSuccess.call();

      }).then(function(recordValid) {
  
        console.log("Current record for owner is valid, with added OP_ADD rule?  [" + recordValid + "]");   

        // Now let's check the record on the Orchestration contract, to ensure that the Language has been set to '???'
        return wInstance.getValueOnRecord.call(accounts[0], web3.utils.fromAscii('AccountCurrValue'));

      }).then(function(currAcctValue) {
  
        console.log("Current value of AccountCurrValue is (" + new String(currAcctValue).valueOf() + ")");      
   
      });
    });
  });
  it("Set Transaction State for RuleTree", function() {

    sleep(2000);

    return WonkaEngineOpt.deployed().then(function(instance) {

      console.log("STS - Got the handle for the RuleTree.");

      return WonkaTransactionState.deployed().then(function(tInstance) {

        console.log("Started the setting of transaction state for the RuleTree.");

        tInstance.setOwner(accounts[0], 100);

        sleep(500);

        tInstance.setExecutor(accounts[0]);

        sleep(500);

        tInstance.addConfirmation(accounts[0]);

        sleep(500);

        tInstance.setMinScoreRequirement(1);

        sleep(500);

        instance.setTransactionState(accounts[0], tInstance.address);

        console.log("Completed the setting of transaction state for the RuleTree.");

      });
    });
  });
  it("Running the rules engine with a Custom Operator rule", function() {

    sleep(1000);

    return WonkaEngineOpt.deployed().then(function(wInstance) {      

      return OrchTestContract.deployed().then(function(testInstance) {

        console.log("Define a new custom operator");

        wInstance.addCustomOp(web3.utils.fromAscii('MyCustomOp'), web3.utils.fromAscii('ACT'), testInstance.address, web3.utils.fromAscii('performMyCalc'));

        console.log("Add a new rule with the new custom operator focused on the AccountCurrValue");

        sleep(2000);

        // The value "MyCustomOp,AccountCurrValue,11,40,50" indicates that this Custom Operator will invoke the method defined by 'MyCustomOp' with the arguments AccountCurrValue,500,1000,100
        wInstance.addRule(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('InvokeCustomOp'), web3.utils.fromAscii('AccountCurrValue'), CUSTOM_OP_RULE, new String('MyCustomOp').valueOf(), false, true); 

        sleep(2000);

        console.log("Add args to the custom operator");
        wInstance.addRuleCustomOpArgs(accounts[0], web3.utils.fromAscii('CheckAccntStsLeaf'), web3.utils.fromAscii('AccountCurrValue'), web3.utils.fromAscii('500'), web3.utils.fromAscii('1000'), web3.utils.fromAscii('100'));

        sleep(2000);

        console.log("Running the engine now with the new Custom Operator rule");

        // First, invoke the OpAdd rule, where AccountCurrValue = AccountCurrValue + AccountPrevValue + 1 (i.e., 4001 = 2500 + 1500 + 1)
        // And then, we invoke the Custom Operator rule, where we set the AccountCurrValue = (((AccountCurrValue - 500) + 1000) / 100)
        // The final result should be: 45 = (((4001 - 500) + 1000) / 100)]
        wInstance.executeRuleTree(accounts[0]);

        sleep(3000);

        return wInstance.getLastTransactionSuccess.call();

      }).then(function(recordValid) {
  
        console.log("O -> Current record for owner(" + accounts[0] + ") is valid?  [" + recordValid + "]");

        return wInstance.getValueOnRecord.call(accounts[0], web3.utils.fromAscii('AccountCurrValue'));

      }).then(function(acctCurrValue) {

        console.log("Value of AccountCurrValue attribute is (" + new String(acctCurrValue).valueOf() + ")");        
        
        // If I don't call this method, this script never dies and the Ethereum node keeps printing 'eth_getFilterChanges()'
        process.exit();     
      });
    });
  });

})  // end of the scope for WonkaTransactionState
})  // end of the scope for WonkaRegistry
})  // end of the scope for OrchTestContract
}); // end of the scope for WonkaEngineOpt
