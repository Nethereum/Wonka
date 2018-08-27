# Wonka

A business rules engine (for both the .NET platform and the <a target="_blank" href="https://en.wikipedia.org/wiki/Ethereum">Ethereum</a> platform) that is inherently metadata-driven.  Once the rules are written into a markup language and are parsed/deserialized by the .NET form of the engine, these rules can then be serialized onto the blockchain, into the form of a smart contract using the Solidity language.  Basically, after providing a number of rules and populating a record, a user can submit the populated record for validation by the rules engine, whether it exists in .NET or the blockchain.

## Features

* XML markup language for defining a RuleTree and the (predefined or user-defined) rules within it.
* .NET framework that will parse XML markup and assembly a RuleTree data structure.
* .NET rules engine that can apply a RuleTree to a provided record for various purposes (validation, value assignment, etc.).
* Ethereum (i.e., Solidity contract) rules engine that can apply a RuleTree to a provided record for various purposes.
* .NET layer that can serialize a RuleTree data structure to the Ethereum rules engine.
* Orchestration 'get' functionality in the Ethereum engine, where the engine can be directed to assemble a virtual record by pulling values from other contracts within the blockchain.
* Orchestration 'set' functionality in the Ethereum engine, where the engine can be directed to set values on other contracts within the blockchain.
* Orchestration 'custom' functionality in the Ethereum engine, where the engine can execute an user-defined rule by calling a function on another contract within the blockchain.

# Notes
When running the Ethereum node that will be your deployment target for the Solidity contract(s), please make sure to run it with the maximum gas set to 8388608.  For example, if you are using 'ganache-cli', you would run the following command:

$ ganache-cli --gasLimit 8388609

# Notices

You can further understand the basis for this project by reading about the 
ideas and the general design presented in my <a target="_blank" href="https://www.infoq.com/articles/mdd-creating-user-friendly-dsl">InfoQ article</a>, which talks about metadata-driven design (i.e., MDD) and business rules.
