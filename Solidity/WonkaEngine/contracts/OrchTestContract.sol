pragma solidity ^0.4.24;

contract OrchTestContract {
     
    address lastAddressProvided;

    mapping(bytes32 => bytes32) private testRecord;

    /// @dev Constructor for the Orchestration contract
    /// @author Aaron Kendall
    constructor() public {

        //testRecord["Title"] = "The First Book";
        //testRecord["Price"] = "0999";
        //testRecord["PageAmount"] = "289";

        testRecord["BankAccountID"] = "1234567890";
        testRecord["BankAccountName"] = "JohnSmithFirstCheckingAccount";

        // testRecord["AccountStatus"] = "OOS";
        testRecord["AccountStatus"] = "ACT";

        testRecord["AccountCurrValue"] = "999";
        // testRecord["AccountCurrValue"] = "9";

        testRecord["AccountPrevValue"] = "1500";

        //testRecord["AccountType"] = "Checking";
        testRecord["AccountType"] = "WillCauseAnError";

        testRecord["AccountCurrency"] = "USD";
        testRecord["Language"] = "ENG";
    }    

    function getAttrValueBytes32(bytes32 key) public view returns(bytes32) { 

        return testRecord[key];
    }

    function getAttrValueBytes(address ruler, bytes32 key) public returns(bytes32) { 

        lastAddressProvided = ruler;

        return testRecord[key];
    }

    function setAttrValueBytes32(bytes32 key, bytes32 value) public returns(bytes32) { 

        testRecord[key] = value;

        return value;
    }

    function setAttrValueBytes(address ruler, bytes32 key, bytes32 value) public returns(bytes32) { 

        lastAddressProvided = ruler;

        testRecord[key] = value;

        return value;
    }    

}