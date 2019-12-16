pragma solidity ^0.5.0;

import "./CloneFactory.sol";

/// @title The factory class that helps to create proxies for an instance of the rules engine, according to the spec EIP-1167
/// @author Aaron Kendall
/// @notice UNDER CONSTRUCTION
contract WonkaEngineFactory is CloneFactory {

    // NOTE: Only an example value attached - to be replaced
    address Template = 0xE0f5206BBD039e7b0592d8918820024e2a7437b9;

    function createEngineProxy() external returns (address newEngineProxy) {
        newEngineProxy = createClone(Template);
    }
}
