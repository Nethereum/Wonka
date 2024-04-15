const {
    time,
    loadFixture,
  } = require("@nomicfoundation/hardhat-toolbox/network-helpers");
  const { anyValue } = require("@nomicfoundation/hardhat-chai-matchers/withArgs");
  const { expect } = require("chai");
  
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

  describe("Wonka", function () {
    // We define a fixture to reuse the same setup in every test.
    // We use loadFixture to run this setup once, snapshot that state,
    // and reset Hardhat Network to that snapshot in every test.
    async function deployWonka() {

        // Contracts are deployed using the first signer/account by default
        const [owner, otherAccount] = await ethers.getSigners();

        console.log(`Deployment owner will be ${owner.address}`);

        const wonkaLibrary = 
            await ethers.deployContract('WonkaLibrary');
  
        await wonkaLibrary.waitForDeployment();
    
        console.log(`Wonka Library deployed to ${wonkaLibrary.target}`);
    
        const wonkaEngineMetadata = 
            await ethers.deployContract('WonkaEngineMetadata');
    
        await wonkaEngineMetadata.waitForDeployment();
    
        console.log(`Wonka Engine Metadata deployed to ${wonkaEngineMetadata.target}`);
        
        const WonkaEngineRuleSets =
            await ethers.getContractFactory("WonkaEngineRuleSets", {
            libraries: {
                WonkaLibrary: wonkaLibrary.target,
            },
            });
    
        const wonkaEngineRuleSets =
            await WonkaEngineRuleSets.deploy(wonkaEngineMetadata.target);
    
        console.log(`Wonka Engine RuleSets deployed to ${wonkaEngineRuleSets.target}`);
    
        const WonkaEngineOpt = await ethers.getContractFactory("WonkaEngineOpt");
    
        const wonkaEngineOpt = 
            await WonkaEngineOpt.deploy(wonkaEngineMetadata.target, wonkaEngineRuleSets.target);
    
        console.log(`Wonka Engine deployed to ${wonkaEngineOpt.target}`);
    
        const WonkaRegistry = await ethers.getContractFactory("WonkaRegistry");
    
        const wonkaRegistry = await WonkaRegistry.deploy();
    
        console.log(`Wonka Registry deployed to ${wonkaRegistry.target}`);
    
        const WonkaTransactionState = await ethers.getContractFactory("WonkaTransactionState");
    
        const wonkaTrxState = await WonkaTransactionState.deploy();
    
        console.log(`Wonka Trx State deployed to ${wonkaTrxState.target}`);

        const ownerAddress = owner.address;
        const userAddress  = otherAccount.address;

        return { wonkaEngineOpt, wonkaRegistry, wonkaTrxState, ownerAddress, userAddress };
    }
  
    describe("Deployment", function () {

      it("Main Test", async function () {
        const { wonkaEngineOpt, wonkaRegistry, wonkaTrxState, ownerAddress, userAddress } = await loadFixture(deployWonka);

        const owner = ownerAddress;
  
        expect(await wonkaEngineOpt.getNumberOfAttributes()).to.equal(3);

        /** 
         ** ADD ATTRIBUTES 
         **/
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("Language"),          64, 0, new String('ENG').valueOf(),   true, false);
        console.log("Added another Attribute!");
  
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("BankAccountID"),    256, 0, new String('Blank').valueOf(), true, false);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("BankAccountName"), 1024,  0, new String('Blank').valueOf(), true, false);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("AccountStatus"),      3,  0, new String('ACT').valueOf(), true, false);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("AccountCurrValue"),  64, 100000, new String('').valueOf(), false, true);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("AccountType"),     1024,      0, new String('Checking').valueOf(), true, false);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("AccountCurrency"),    3,      0, new String('USD').valueOf(), true, false);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("AccountPrevValue"),  64, 100000, new String('').valueOf(), false, true);
        await wonkaEngineOpt.addAttribute(ethers.encodeBytes32String("StartSaleDate"),     64,      0, new String('').valueOf(), true, true);
  
        console.log("Added more Attributes!");

         /** 
          ** CREATE RULETREE
          **/
        const treeExistsBefore = await wonkaEngineOpt.hasRuleTree(owner);
        console.log("BEFORE -> Current ruletree for owner(" + owner + ") exists?  [" + treeExistsBefore + "]");

        await wonkaEngineOpt.addRuleTree(owner, 
                                         ethers.encodeBytes32String('JohnSmithRuleTree'), 
                                         new String('John Smith Rule Tree').valueOf(), 
                                         true, true, false);
        console.log("Added the root ruletree!");      
  
        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('JohnSmithRuleTree'), 
                                     ethers.encodeBytes32String('AccntNameEqualRule'), 
                                     ethers.encodeBytes32String('BankAccountName'), 
                                     EQUAL_TO_RULE, 
                                     new String('JohnSmithFirstCheckingAccount').valueOf(), 
                                     false, true); 
        console.log("Added the rule to the root ruleset!");
  
        await wonkaEngineOpt.addRuleSet(owner, 
                                        ethers.encodeBytes32String('CheckAccntSts'), 
                                        new String('Will determine the account status').valueOf(), 
                                        ethers.encodeBytes32String('JohnSmithRuleTree'), 
                                        false, false, false);  
        console.log("Added the first child ruleset to the root ruleset!");
  
        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntSts'), 
                                     ethers.encodeBytes32String('CheckForTooLittleRule'), 
                                     ethers.encodeBytes32String('AccountCurrValue'), 
                                     LESS_THAN_RULE, 
                                     new String('1000').valueOf(), 
                                    false, true);

        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntSts'), 
                                     ethers.encodeBytes32String('CheckForTooMuchRule'), 
                                     ethers.encodeBytes32String('AccountCurrValue'), 
                                     GREATER_THAN_RULE, 
                                     new String('2000').valueOf(), 
                                     false, true);

        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntSts'), 
                                     ethers.encodeBytes32String('AccountTypeRule'), 
                                     ethers.encodeBytes32String('AccountType'), 
                                     IN_DOMAIN_RULE, 
                                     new String('Checking,Savings,TaxHaven').valueOf(), 
                                     false, true);
    
        console.log("Added the rules to the first child ruleset!");
  
        await wonkaEngineOpt.addRuleSet(owner, 
                                        ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                        new String('Will determine the account status - leaf').valueOf(), 
                                        ethers.encodeBytes32String('CheckAccntSts'), 
                                        true, true, false);
    
        console.log("Added the leaf ruleset to the first child ruleset!");
  
        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                     ethers.encodeBytes32String('ValidateStatusRule'), 
                                     ethers.encodeBytes32String('AccountStatus'), 
                                     EQUAL_TO_RULE, 
                                     new String('ACT').valueOf(), 
                                     false, true);

        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                     ethers.encodeBytes32String('PopulatedValueRule'), 
                                     ethers.encodeBytes32String('Language'), 
                                     POPULATED_RULE, 
                                     new String('').valueOf(), 
                                     false, true);

        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                     ethers.encodeBytes32String('SaleDateRule'), 
                                     ethers.encodeBytes32String('StartSaleDate'), 
                                     LESS_THAN_RULE, 
                                     new String('NOW').valueOf(), 
                                     false, true);
  
        console.log("Added the rules to the leaf ruleset for the first child RS!");

        const treeExistsAfter = await wonkaEngineOpt.hasRuleTree(owner);
        console.log("AFTER -> Current ruletree for owner(" + owner + ") exists?  [" + treeExistsAfter + "]");

         /** 
          ** GET RULE TREE PROPS
          **/
        const [ idOut, descOut, rootRSNameOut, totalRuleCntOut ] = await wonkaEngineOpt.getRuleTreeProps(owner);
        console.log("Id is [" + idOut + "], Desc[" + descOut + "]");
        var id              = idOut;
        var desc            = descOut;
        var rootRSName      = ethers.decodeBytes32String(rootRSNameOut);
        var totalRuleCnt    = totalRuleCntOut; 
        var nTotalRuleCount = parseInt(totalRuleCnt, 10);
        console.log("Root RS Name for tree of [" + owner + "] is (" + rootRSName + "), Total Rule Count for tree is (" + nTotalRuleCount + ")");

        /**
         ** SET DATA VALUES (TO BE EVALUATED BY RULETREE) 
         **/
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('Title'), new String('The First Book').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('Price'), new String('0999').valueOf()); // in cents
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('PageAmount'), new String('289').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('BankAccountID'), new String('1234567890').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('BankAccountName'), new String('JohnSmithFirstCheckingAccount').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('AccountStatus'), new String('OOS').valueOf());
         //await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('AccountStatus'), new String('ACT').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('AccountCurrValue'), new String('999').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('AccountCurrency'), new String('USD').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('AccountType'), new String('Checking').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('Language'), new String('ENG').valueOf());
         await wonkaEngineOpt.setValueOnRecord(owner, ethers.encodeBytes32String('Language'), new String('ENG').valueOf());
         console.log("Set values on current record!");

         /**
          ** RUN RULETREE AGAINST CURRENT RECORD - TEST #01
          **/
        await wonkaEngineOpt.executeRuleTree(owner);
        const firstTestResult = await wonkaEngineOpt.getLastTransactionSuccess();
        console.log("TEST #01 (Basic) - Current record for owner(" + owner + ") is valid through default execution?  " + firstTestResult + "");

        /**
         ** TURN ORCHESTRATION MODE ON 
         **/
        await wonkaEngineOpt.setOrchestrationMode(true, ethers.encodeBytes32String('TEST'));
        console.log("Set Orchestration mode to on");

        const OrchTestContract = await ethers.getContractFactory("OrchTestContract");    
        const orchTestContract = await OrchTestContract.deploy();
        console.log(`Orch Test Contract deployed to ${orchTestContract.target}`);
 
        /**
         ** SET DEFAULT DATA SOURCE (FOR ORCHESTRATION) AND TEST ORCHESTRATION VIA DATA RETRIEVAL
         **/
        await wonkaEngineOpt.addSource(ethers.encodeBytes32String('TEST'), 
                                       ethers.encodeBytes32String('ACT'), 
                                       orchTestContract.target, 
                                       ethers.encodeBytes32String('getAttrValueBytes32'), 
                                       ethers.encodeBytes32String('setAttrValueBytes32'));

        const defaultSource = await wonkaEngineOpt.getDefaultSource();
        console.log("Value of Default Source is (" + ethers.decodeBytes32String(defaultSource) + ")");
        
        await wonkaEngineOpt.getValueOnRecord(owner, ethers.encodeBytes32String('AccountStatus'));
        const AccountStatusValue01 = await wonkaEngineOpt.getStoredRecordValue();

        await wonkaEngineOpt.getValueOnRecord(owner, ethers.encodeBytes32String('Language'));
        const LanguageValue01 = await wonkaEngineOpt.getStoredRecordValue();

        console.log("Value of AccountStatus attribute is (" + new String(AccountStatusValue01).valueOf() + 
                    "), Language attribute is (" + new String(LanguageValue01).valueOf() + ")");

        /**
         ** RUN RULETREE AGAINST CURRENT RECORD - TEST #02
         **/
        await wonkaEngineOpt.executeRuleTree(owner);
        const secondTestResult = await wonkaEngineOpt.getLastTransactionSuccess();
        console.log("Test #02 (Orchestration) - Current record for owner(" + owner + ") is valid through execution?  " + secondTestResult + "");
  
        /**
         ** ADD ASSIGNMENT RULE(S) TO RULETREE WITH ORCHESTRATION ENABLED
         ** THEN RUN RULETREE AGAINST CURRENT RECORD - TEST #03
         **/
        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                     ethers.encodeBytes32String('AssignLangRule'), 
                                     ethers.encodeBytes32String('Language'), 
                                     ASSIGN_RULE, 
                                     new String('???').valueOf(), 
                                     false, true);

        console.log("Added assignment rule to set a value on the Orchestration contract using Assembly.");

        await wonkaEngineOpt.executeRuleTree(owner);
        const thirdTestResult = await wonkaEngineOpt.getLastTransactionSuccess();
        console.log("Test #03 (Assignment Rules) - Current record for owner(" + owner + ") is valid through execution?  " + thirdTestResult + "");

        // Now let's check the record on the Orchestration contract, to ensure that the Language has been set to '???'
        await wonkaEngineOpt.getValueOnRecord(owner, ethers.encodeBytes32String('Language'));
        const LanguageValue02 = await wonkaEngineOpt.getStoredRecordValue();
        console.log("Value of Language attribute is (" + new String(LanguageValue02).valueOf() + ")");

        expect(LanguageValue02).to.equal('???');

        /**
         ** NOW ADD OPADD RULE TO THE LAST RULESET:
         **     where we set the AccountCurrValue = AccountCurrValue + AccountPrevValue + 1 (i.e., 2500 = 999 + 1500 + 1)
         **
         ** THEN RUN RULETREE AGAINST CURRENT RECORD - TEST #04
         **/
        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                     ethers.encodeBytes32String('SumForCurrValue'), 
                                     ethers.encodeBytes32String('AccountCurrValue'), 
                                     OP_ADD_RULE, 
                                     new String('AccountCurrValue,AccountPrevValue,1').valueOf(), 
                                     false, true);
        
        console.log("Added OP_ADD rule to set a value on the Orchestration contract using Assembly.");

        await wonkaEngineOpt.executeRuleTree(owner);
        const fourthTestResult = await wonkaEngineOpt.getLastTransactionSuccess();
        console.log("Test #04 (OpAdd Rules) - Current record for owner(" + owner + ") is valid through execution?  " + fourthTestResult + "");

        // Now let's check the record on the Orchestration contract, to ensure that the AccountCurrValue has been set to '2500'
        await wonkaEngineOpt.getValueOnRecord(owner, ethers.encodeBytes32String('AccountCurrValue'));
        const AccountCurrValue01 = await wonkaEngineOpt.getStoredRecordValue();
        console.log("Value of AccountCurrValue attribute is (" + new String(AccountCurrValue01).valueOf() + ")");

        expect(AccountCurrValue01).to.equal('2500');

        /**
         ** NOW ADD CUSTOM OP TO THE LAST RULESET:
         ** 
         **     where the custom operator is a method on a third-party contract
         **
         **     First, invoke the OpAdd rule, where AccountCurrValue = AccountCurrValue + AccountPrevValue + 1 (i.e., 4001 = 2500 + 1500 + 1)
         **     And then, we invoke the Custom Operator rule, where we set the AccountCurrValue = (((AccountCurrValue - 500) + 1000) / 100)
         **     The final result should be: 45 = (((4001 - 500) + 1000) / 100)]
         **
         ** THEN RUN RULETREE AGAINST CURRENT RECORD - TEST #05
         **/
        await wonkaEngineOpt.addCustomOp(ethers.encodeBytes32String('MyCustomOp'), 
                                         ethers.encodeBytes32String('ACT'), 
                                         orchTestContract.target, 
                                         ethers.encodeBytes32String('performMyCalc'));

        console.log("Add a new rule with the new custom operator focused on the AccountCurrValue");

        await wonkaEngineOpt.addRule(owner, 
                                     ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                     ethers.encodeBytes32String('InvokeCustomOp'), 
                                     ethers.encodeBytes32String('AccountCurrValue'), 
                                     CUSTOM_OP_RULE, 
                                     new String('MyCustomOp').valueOf(), 
                                     false, true);

        console.log("Add args to the custom operator");

        await wonkaEngineOpt.addRuleCustomOpArgs(owner, 
                                                 ethers.encodeBytes32String('CheckAccntStsLeaf'), 
                                                 ethers.encodeBytes32String('AccountCurrValue'), 
                                                 ethers.encodeBytes32String('500'), 
                                                 ethers.encodeBytes32String('1000'), 
                                                 ethers.encodeBytes32String('100'));

        await wonkaEngineOpt.executeRuleTree(owner);
        const fifthTestResult = await wonkaEngineOpt.getLastTransactionSuccess();
        console.log("Test #05 (Custom Op Rules) - Current record for owner(" + owner + ") is valid through execution?  " + fifthTestResult + "");
                                         
         // Now let's check the record on the Orchestration contract, to ensure that the AccountCurrValue has been set to '45'
         await wonkaEngineOpt.getValueOnRecord(owner, ethers.encodeBytes32String('AccountCurrValue'));
         const AccountCurrValue02 = await wonkaEngineOpt.getStoredRecordValue();
         console.log("Value of AccountCurrValue attribute is (" + new String(AccountCurrValue02).valueOf() + ")");
 
         expect(AccountCurrValue02).to.equal('45');

      });
  
  });
});
