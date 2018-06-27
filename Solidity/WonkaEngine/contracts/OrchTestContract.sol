pragma solidity ^0.4.24;

contract OrchTestContract {
     
    address lastAddressProvided;

    function getAttrValueBytes32(bytes32 key) public pure returns(bytes32) { 

        if (key == "Title")
            return "The First Book";
        else if (key == "Price")
            return "0999";
        else if (key == "PageAmount")
            return "289";
        else if (key == "BankAccountID")
            return "1234567890";
        else if (key == "BankAccountName")
            return "JohnSmithFirstCheckingAccount";
        else if (key == "AccountStatus")
            return "OOS";
        else if (key == "AccountCurrValue")
            return "9999.99";
        else if (key == "AccountType")
            return "Checking";
        else if (key == "AccountCurrency")
            return "USD";
        else if (key == "Language")
            return "ENG";
        else
            return "";
    }

    function getAttrValueBytes(address ruler, bytes32 key) public returns(bytes32) { 

        lastAddressProvided = ruler;

        if (key == "Title")
            return "The First Book";
        else if (key == "Price")
            return "0999";
        else if (key == "PageAmount")
            return "289";
        else if (key == "BankAccountID")
            return "1234567890";
        else if (key == "BankAccountName")
            return "JohnSmithFirstCheckingAccount";
        else if (key == "AccountStatus")
            return "OOS";
        else if (key == "AccountCurrValue")
            return "9999.99";
        else if (key == "AccountType")
            return "Checking";
        else if (key == "AccountCurrency")
            return "USD";
        else if (key == "Language")
            return "ENG";
        else
            return "";
    }
}
