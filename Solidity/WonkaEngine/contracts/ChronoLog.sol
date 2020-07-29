// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

contract ChronoLog {

    /// @title Defines an chronological event
    /// @author Aaron Kendall
    struct ChronoLogEvent {

        uint eventId;

        bytes32 eventName;

        bytes32 eventType;

        string eventDescription;

        uint eventEpochTime;

        bytes32 publicData;

        bytes32 privateHash;

        string eventInfoUrl;

        bool isValue;
    }

    uint idCounter = 1;

    address logOwner;

    mapping(bytes32 => ChronoLogEvent) private chronoEventMap;
    ChronoLogEvent[] public chronoEvents;

    mapping(uint => uint) private dayIndex;

    mapping(bytes32 => bytes32[]) private typeIndex;

    modifier onlyLogOwner()
    {
        require(msg.sender == logOwner, "The caller of this method does not have permission for this action.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }


    /// @dev Constructor for the ChronoLog contract
    /// @author Aaron Kendall
    constructor() public {
        logOwner = msg.sender;
    }

    function addChronoLogEvent(bytes32 uniqueName, bytes32 eType, string memory desc, bytes32 data, bytes32 hash, string memory url) public onlyLogOwner {

        require(chronoEventMap[uniqueName].isValue == false, "Event with unique ID already exists.");

        chronoEvents.push(ChronoLogEvent({
            eventId: idCounter,
            eventName: uniqueName,
            eventType: eType,
            eventDescription: desc,
            eventEpochTime: block.timestamp,
            publicData: data,
            privateHash: hash,
            eventInfoUrl: url,
            isValue: true
        }));

        chronoEventMap[chronoEvents[chronoEvents.length-1].eventName] = chronoEvents[chronoEvents.length-1];

        uint dayIdxVal = chronoEvents[chronoEvents.length-1].eventEpochTime / 86400;
        if (dayIndex[dayIdxVal] <= 0)
            dayIndex[dayIdxVal] = idCounter;

        idCounter += 1;

        (typeIndex[eType]).push(uniqueName);
    }

    function getChronoLogBasic(bytes32 uniqueName) public view returns (uint, bytes32, bytes32, string memory) {
        return (chronoEventMap[uniqueName].eventEpochTime, chronoEventMap[uniqueName].publicData, chronoEventMap[uniqueName].privateHash, chronoEventMap[uniqueName].eventInfoUrl);
    }

    function getChronoLogEvent(bytes32 uniqueName) public view returns (bytes32, string memory, uint, bytes32, bytes32, string memory) {
        return (chronoEventMap[uniqueName].eventType, chronoEventMap[uniqueName].eventDescription, chronoEventMap[uniqueName].eventEpochTime, chronoEventMap[uniqueName].publicData, chronoEventMap[uniqueName].privateHash, chronoEventMap[uniqueName].eventInfoUrl);
    }

    function getChronoLogEventsByType(bytes32 eType) public view returns (bytes32[] memory) {
        return typeIndex[eType];
    }

    function getChronoLogEventsByTypeAndTime(bytes32 eType, uint startTime, uint endTime) public view returns (bytes32[] memory) {

        bytes32[] memory logNames;

        uint startDayIdx = dayIndex[startTime / 86400];

        if (startDayIdx > 0) {

            // Adjust
            startDayIdx -= 1;

            uint nextDayIdx = dayIndex[startDayIdx+1];
            uint endIdx = 0;

            if (nextDayIdx > 0) {
                // Adjust
                endIdx = nextDayIdx - 1;
                logNames = new bytes32[](nextDayIdx - startDayIdx);
            }
            else {
                endIdx = chronoEvents.length;
                logNames = new bytes32[](chronoEvents.length - startDayIdx);
            }

            uint resultIdx = 0;
            for (uint idx = startDayIdx; idx < endIdx; ++idx) {
                if ((eType == chronoEvents[idx].eventType) && (chronoEvents[idx].eventEpochTime <= endTime)) {
                    logNames[resultIdx++] = chronoEvents[idx].eventName;
                }
                else {
                    break;
                }
            }
        }

        return logNames;
    }

}