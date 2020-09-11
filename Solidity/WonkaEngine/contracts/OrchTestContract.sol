// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

import "./AggregatorV3Interface.sol";

contract OrchTestContract {
     
    address lastAddressProvided;

    mapping(bytes32 => bytes32) private testRecord;

    bytes32 valueHolder;

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

        testRecord["NewSalesTransSeq"] = "123456789";
        testRecord["NewSaleEAN"] = "9781234567890";
        testRecord["NewSaleVATRateDenom"] = "0";
        testRecord["NewSaleItemType"] = "Widget";
        testRecord["CountryOfSale"] = "UK";
        testRecord["NewSalePrice"] = "100";
        testRecord["PrevSellTaxAmount"] = "5";
        testRecord["NewSellTaxAmount"] = "0";
        testRecord["NewVATAmountForHMRC"] = "0";
        testRecord["StartSaleDate"] = "1560633165";
    }

    function getAttrValueBytes32(bytes32 key) public view returns(bytes32) { 

        return testRecord[key];
    }

    function getAttrValueBytes(address ruler, bytes32 key) public returns(bytes32) { 

        lastAddressProvided = ruler;

        return testRecord[key];
    }

    function lookupVATDenominator(bytes32 saleItemType, bytes32 countryOfSale, bytes32 dummyVal1, bytes32 dummyVal2) public returns(bytes32)
    {
        valueHolder = dummyVal1;
        valueHolder = dummyVal2;

        if (saleItemType == "Widget" && countryOfSale == "UK")
            return "5";
        else if (saleItemType == "Widget")
            return "10";
        else
            return "20";
    }

    function getLatestPriceForCrypto(bytes32 contractAddrFirstHalf, bytes32 contractAddrSecondHalf, bytes32, bytes32) public view returns (int)
    {
        string memory firstHalf = bytes32ToString(contractAddrFirstHalf);
        string memory secondHalf = bytes32ToString(contractAddrSecondHalf);

        string memory aggregatorContractAddressString = strConcat(firstHalf, secondHalf);

        address contractAddr = parseAddr(aggregatorContractAddressString);

        AggregatorV3Interface priceFeed = AggregatorV3Interface(contractAddr);

        (, int price, , , ) = priceFeed.latestRoundData();

        return price;
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
    function bytes32ToString(bytes32 x) public pure returns (string memory) {

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
        for (uint k = 0; k < charCount; k++) {
            bytesStringTrimmed[k] = bytesString[k];
        }

        return string(bytesStringTrimmed);
    }     

    function parseAddr(string memory _a) internal pure returns (address _parsedAddress) {
        bytes memory tmp = bytes(_a);
        uint160 iaddr = 0;
        uint160 b1;
        uint160 b2;
        for (uint i = 2; i < 2 + 2 * 20; i += 2) {
            iaddr *= 256;
            b1 = uint160(uint8(tmp[i]));
            b2 = uint160(uint8(tmp[i + 1]));
            if ((b1 >= 97) && (b1 <= 102)) {
                b1 -= 87;
            } else if ((b1 >= 65) && (b1 <= 70)) {
                b1 -= 55;
            } else if ((b1 >= 48) && (b1 <= 57)) {
                b1 -= 48;
            }
            if ((b2 >= 97) && (b2 <= 102)) {
                b2 -= 87;
            } else if ((b2 >= 65) && (b2 <= 70)) {
                b2 -= 55;
            } else if ((b2 >= 48) && (b2 <= 57)) {
                b2 -= 48;
            }
            iaddr += (b1 * 16 + b2);
        }
        return address(iaddr);
    }

    /// @dev This method will convert a 'uint' type to a 'bytes32' type     
    function parseInt(string memory _a, uint _b) internal pure returns (uint) {

        bytes memory bresult = bytes(_a);
        
        uint bint = _b;
        uint mint = 0;
        bool decimals = false;

        for (uint i = 0; i < bresult.length; i++) {
            
            uint8 tmpNum = uint8(bresult[i]);
            
            if ((tmpNum >= 48) && (tmpNum <= 57)) {
                if (decimals) {
                    if (bint == 0) 
                        break;
                    else 
                        bint--;
                }
                mint *= 10;
                mint += tmpNum - 48;
                
            } else if (tmpNum == 46) 
                decimals = true;
        }

        return mint;
    }

    /// @dev This method will concatenate the provided strings into one larger string
    /// @notice 
    function strConcat(string memory _a, string memory _b) private pure returns (string memory) {

        bytes memory _ba = bytes(_a);
        bytes memory _bb = bytes(_b);
        string memory abcde = new string(_ba.length + _bb.length);
        bytes memory babcde = bytes(abcde);
        
        uint k = 0;
        
        for (uint a = 0; a < _ba.length; a++) {
            babcde[k++] = _ba[a];
        }

        for (uint b = 0; b < _bb.length; b++) {
            babcde[k++] = _bb[b];
        }

        return string(babcde);
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