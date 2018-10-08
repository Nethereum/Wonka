# Wonka

A business rules engine (for both the .NET platform and the <a target="_blank" href="https://en.wikipedia.org/wiki/Ethereum">Ethereum</a> platform) that is inherently metadata-driven.  Once the rules are written into a markup language and are parsed/deserialized by the .NET form of the engine, these rules can then be serialized onto the blockchain using Nethereum, stored within a smart contract (i.e., the Ethereum version of the engine) built using the Solidity language.  Basically, after providing a number of rules and populating a record, a user can use Nethereum to submit the populated record for validation by the rules engine, whether it exists in .NET or the blockchain.

## Features

* XML markup language for defining a RuleTree, a logical and hierarchical set of rules.  The functionality for these rules can be predefined or user-defined.  There are [multiple](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestData/SimpleAccountCheck.xml) [examples](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestData/VATCalculationExample.xml) of a RuleTree's markup within the project.
* .NET framework that will parse XML markup and assembly a RuleTree data structure.
* .NET rules engine that can apply a RuleTree to a provided record for various purposes (validation, value assignment, etc.).
* Ethereum (i.e., Solidity contract) rules engine that can apply a RuleTree to a provided record for various purposes.
* .NET layer that can serialize a RuleTree data structure to the Ethereum rules engine.
* Orchestration 'get' functionality in the Ethereum engine, where the engine can be directed to assemble a virtual record by pulling values from other contracts within the blockchain.
* Orchestration 'set' functionality in the Ethereum engine, where the engine can be directed to set values on other contracts within the blockchain.
* Custom Operator functionality in the Ethereum engine, where the engine can execute an user-defined rule by calling a function on another contract within the blockchain.
* Registry and Grove functionality, helping users to discover/reuse existing RuleTree instances and group them into collections.

# Quick Setup

1. Run your Ethereum node of choice, with the appropriate gas limit set (i.e., 8388609).
2. Deploy the Solidity contracts to the Ethereum node by using the test script './Solidity/WonkaEngine/test/testdeploy.js'.
3. Populate the required values (sender,password,contract addresses) in the right locations (Program.cs, *.init.xml, etc.).
4. Run any test harness mentioned within the Code Samples section.

# Main Libraries

Project Source | Nuget_Package |  Description |
------------- |--------------------------|-----------|
[WonkaBre](https://github.com/Nethereum/Wonka/tree/master/WonkaSystem/WonkaBre)    | | This library contains the .NET implementation of the Wonka rules engine.  By using these classes, you can parse the XML markup, populate a RuleTree, and then invoke the RuleTree against a provided data record. |
[WonkaEth](https://github.com/Nethereum/Wonka/tree/master/WonkaSystem/WonkaEth) |  | This library contains the functions that know how to serialize a RuleTree (and other related info, like metadata defined using WonkaRef) to an instance of the Ethereum blockchain.  It also contains functions that know how to invoke an instance of the RuleTree on the blockchain and collect its results. |
[WonkaIpfs](https://github.com/Nethereum/Wonka/tree/master/WonkaSystem/WonkaIpfs)    | | This library contains some basic functionality regarding access to IPFS, for pulling back files that could be used for configuration (XML rules markup, Ethereum contract ABI, etc.).|
[WonkaPrd](https://github.com/Nethereum/Wonka/tree/master/WonkaSystem/WonkaPrd)| | This library contains the data structures that are used to define and hold a record defined by metadata.  This library is mainly used when invoking the .NET implementation of the rules engine.|
[WonkaRef](https://github.com/Nethereum/Wonka/tree/master/WonkaSystem/WonkaRef)| | This library contains the data structures used to define our metadata and the data domain using MDD (i.e., metadata-driven design).  This library is heavily used by the others in order to address data points. |

# Code Samples

|  Source Code |  Description |
| ------------- |------------|
[Simple Test](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaSimpleTest.cs)| This is a simple test of the .NET implementation of the rules engine. |
[Engine on Blockchain](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaNoviceNethereumTest.cs)| This is mainly a test of the Ethereum (i.e., Solidity) implementation of the rules engine, to validate a data record stored within the engine contract. |
[CQS #1](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaCQSTest.cs)| This test is a demonstration of how to package the previous example into a CQS design. |
[Orchestration](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaSimpleOrchestrationTest.cs)| This is mainly a test of the Ethereum (i.e., Solidity) implementation of the rules engine.  It showcases an example where the engine can get and set values on other predeployed contracts within the blockchain. |
[Custom Operators](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaSimpleCustomOpsTest.cs)| This is mainly a test of the Ethereum (i.e., Solidity) implementation of the rules engine.  It showcases an example where the engine can invoke functionality for a business rule that exists on other predeployed contracts within the blockchain. |
[CQS #2](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaCQSTest.cs)| This test is a demonstration of how to package all existing Wonka functionality into a CQS design.  Also, it provides examples for the Registry functionality (i.e., a way to discover and reuse existing RuleTrees) and the Grove functionality (i.e., a way to put RuleTrees into collections).|

# Important Notes
When running the Ethereum node that will be your deployment target for the Solidity contract(s), please make sure to run it with the maximum gas set to 8388608.  For example, if you are using 'ganache-cli', you would run the following command:

$ ganache-cli --gasLimit 8388609

# Notices

You can further understand the basis for this project by reading about the 
ideas and the general design presented in my <a target="_blank" href="https://www.infoq.com/articles/mdd-creating-user-friendly-dsl">InfoQ article</a>, which talks about metadata-driven design (i.e., MDD) and business rules.
