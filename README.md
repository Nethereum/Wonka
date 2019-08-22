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
* Standard Operator functionality in the Ethereum engine, where the engine can invoke normal operations (like 'addition' and 'greater than') and blockchain-specific operations (like 'now' and 'block.number' via Solidity).
* Custom Operator functionality in the Ethereum engine, where the engine can execute an user-defined rule by calling a function on another contract within the blockchain.
* Registry and Grove functionality, helping users to discover/reuse existing RuleTree instances and group them into collections.
* Export functionality, so that a RuleTree existing on the blockchain side can be extracted and then serialized into a legible form (like XML).

# Quick Run

Though not yet quite available, the code from the [Test Online](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaNoviceOnlineChainTest.cs) example will soon be incorporated into the [Nethereum Playground](http://playground.nethereum.com/) set of samples.  There, you will be able to deploy an instance of Wonka to an hosted Ethereum client, after which you can then create and invoke a RuleTree as separate transactions.

# Quick Setup (i.e., for local use)

1. Run your Ethereum node of choice, with the appropriate gas limit set (i.e., 8388609).
2. Deploy the Solidity contracts to the Ethereum node by using either the JavaScript file [testdeploy.js](https://github.com/Nethereum/Wonka/blob/master/Solidity/WonkaEngine/test/testdeploy.js) or by using [C# extension methods](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaEth/Extensions/WonkaAutogenExtensions.cs).
3. Populate the required values (sender,password,contract addresses,etc.) in the right locations (Program.cs, *.init.xml, etc.).
4. Run any test harness mentioned within the Code Samples section.

# Advanced Setup (i.e., for remote use)

1. Follow steps 1 and 2 specified in Quick Setup and ensure that the machine hosting the Ethereum client is accessible from the outside.
2. Using the [Wonka Rest Service](https://github.com/jaerith/wonkarestservice) as a middle tier, populate the required values (sender, password,contract addresses,Web3 URL, etc.) for pointing the service at the Ethereum client.  Then, deploy the ASP.NET service to an IIS server.
3. As a demonstration of a GUI client and perhaps as a template for something more advanced, run the [Wonka Blazor Web App](https://github.com/jaerith/WonkaBlazorWebApp) on a Windows desktop machine, after pointing the Blazor app at the Wonka REST service by making the necessary alterations to config files.  (Important note: Your selected browser must be WASM-compatible for the Blazor app to run correctly.)

* Hopefully, in the future, the REST service and the Blazor Web App will be merged into one project.

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
[Online Test (Recommended)](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaNoviceOnlineChainTest.cs)|This test provides the fastest and easiest example of using the engine, since it requires no setup.  Instead of relying on local files and a local Ethereum client, this example uses IPFS and an Ethereum client hosted online (via HTTP).  It demonstrates all of the functionality available within Wonka, including the deployment (and invocation) of an instance of the Wonka engine onto the blockchain.|
[Simple Test](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaSimpleTest.cs)| This is a simple test of the .NET implementation of the rules engine. |
[Engine on Blockchain](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaNoviceNethereumTest.cs)| This is mainly a test of the Ethereum (i.e., Solidity) implementation of the rules engine, to validate a data record stored within the engine contract. |
[CQS #1](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaCQSTest.cs)| This test is a demonstration of how to package the previous example into a CQS design. |
[Orchestration](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaSimpleOrchestrationTest.cs)| This is mainly a test of the Ethereum (i.e., Solidity) implementation of the rules engine.  It showcases an example where the engine can get and set values on other predeployed contracts within the blockchain. |
[Custom Operators](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaSimpleCustomOpsTest.cs)| This is mainly a test of the Ethereum (i.e., Solidity) implementation of the rules engine.  It showcases an example where the engine can invoke functionality for a business rule that exists on other predeployed contracts within the blockchain. |
[CQS #2](https://github.com/Nethereum/Wonka/blob/master/WonkaSystem/WonkaSystem/TestHarness/WonkaCQSTest.cs)| This test is a demonstration of how to package all existing Wonka functionality into a CQS design.  Also, it provides examples for the Registry functionality (i.e., a way to discover and reuse existing RuleTrees) and the Grove functionality (i.e., a way to put RuleTrees into collections).  Lastly, it demonstrates how to export a registered RuleTree on the blockchain, back into its original XML markup.|

## Video Guides
Video #1 - Here is a brief description of a rules engine, as well as a quick explanation of the Wonka engine's design:

[![rules engine overview](http://img.youtube.com/vi/6DPiasEe2P4/0.jpg)](https://www.youtube.com/watch?v=6DPiasEe2P4&vq=hd1080 "rules engine overview")

Video #2 - Below is a short introduction to the Wonka engine, showcasing an everyday example of how to use it:

[![first example](http://img.youtube.com/vi/L7kStyGM7F4/0.jpg)](https://www.youtube.com/watch?v=L7kStyGM7F4&vq=hd1080 "first example")

Video #3 - Shown beneath is a quick demonstration of how the Wonka engine uses Nethereum to serialize a RuleTree and then interact with it:

[![first drilldown](http://img.youtube.com/vi/9XZyNtPrKOc/0.jpg)](https://www.youtube.com/watch?v=9XZyNtPrKOc&vq=hd1080 "first drilldown")

# Important Notes
When running the Ethereum node that will be your deployment target for the Solidity contract(s), please make sure to run it with the maximum gas set to 8388608.  For example, if you are using 'ganache-cli', you would run the following command:

$ ganache-cli --gasLimit 8388609

# Notices

You can further understand the basis for this project by reading about the 
ideas and the general design presented in my <a target="_blank" href="https://www.infoq.com/articles/mdd-creating-user-friendly-dsl">InfoQ article</a>, which talks about metadata-driven design (i.e., MDD) and business rules.
