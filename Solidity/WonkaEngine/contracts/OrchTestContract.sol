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

    function performMyCalc(bytes32 arg1, bytes32 arg2, bytes32 arg3, bytes32 arg4) public pure returns(bytes32)    
    {
        uint finalAmt = 125;
        uint minusAmt = 0;
        uint addAmt = 0;
        uint divAmt = 0;

        string memory tmpValue;

        if (arg1 != "") {

            tmpValue = bytes32ToString(arg1);
            finalAmt = parseInt(tmpValue, 0);

            if (arg2 != "") {
                tmpValue = bytes32ToString(arg2);
                minusAmt = parseInt(tmpValue, 0);

                finalAmt -= minusAmt;

                if (arg3 != "") {
                    tmpValue = bytes32ToString(arg3);
                    addAmt = parseInt(tmpValue, 0);

                    finalAmt += addAmt;

                    if (arg4 != ""){
                        tmpValue = bytes32ToString(arg4);
                        divAmt = parseInt(tmpValue, 0);

                        finalAmt /= divAmt;
                    }
                }
            }
        }

        return uintToBytes(finalAmt);
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

    /**
     ** SUPPORT METHODS
     **/

    /// @dev This method will convert a bytes32 type to a String
    /// @notice 
    function bytes32ToString(bytes32 x) public pure returns (string) {

        bytes memory bytesString = new bytes(32);
        uint charCount = 0;
        for (uint j = 0; j < 32; j++) {
            byte char = byte(bytes32(uint(x) * 2 ** (8 * j)));
            if (char != 0) {
                bytesString[charCount] = char;
                charCount++;
            }
        }

        bytes memory bytesStringTrimmed = new bytes(charCount);
        for (j = 0; j < charCount; j++) {
            bytesStringTrimmed[j] = bytesString[j];
        }

        return string(bytesStringTrimmed);
    }     

    /// @dev This method will convert a 'uint' type to a 'bytes32' type     
    function parseInt(string _a, uint _b) internal pure returns (uint) {

        bytes memory bresult = bytes(_a);
        uint bint = _b;
        uint mint = 0;
        bool decimals = false;

        for (uint i = 0; i < bresult.length; i++) {
            if ((bresult[i] >= 48) && (bresult[i] <= 57)) {
                if (decimals) {
                    if (bint == 0) 
                        break;
                    else 
                        bint--;
                }
                mint *= 10;
                mint += uint(bresult[i]) - 48;
            } else if (bresult[i] == 46) 
                decimals = true;
        }

        return mint;
    }

    /// @notice Copied this code from MIT implentation
    /// @dev This method will convert a 'uint' type to a 'bytes32' type
    function uintToBytes(uint targetVal) private pure returns (bytes32 ret) {

        uint v = targetVal;

        if (v == 0) {
            ret = "0";
        }
        else {
            while (v > 0) {
                ret = bytes32(uint(ret) / (2 ** 8));
                ret |= bytes32(((v % 10) + 48) * 2 ** (8 * 31));
                v /= 10;
            }
        }

        return ret;
    }

}