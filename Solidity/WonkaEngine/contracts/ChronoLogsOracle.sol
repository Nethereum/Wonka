// SPDX-License-Identifier: MIT
pragma solidity ^0.6.8;

contract ChronoLogsOracle {

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

        bool isValue;
    }

    uint idCounter = 1;

    address oracleOwner;

    mapping(bytes32 => ChronoLogEvent) private chronoEventMap;
    ChronoLogEvent[] public chronoEvents;

    mapping(uint => uint) private dayIndex;

    mapping(bytes32 => bytes32[]) private typeIndex;

    modifier onlyOracleOwner()
    {
        require(msg.sender == oracleOwner, "The caller of this method does not have permission for this action.");

        // Do not forget the "_;"! It will
        // be replaced by the actual function
        // body when the modifier is used.
        _;
    }


    /// @dev Constructor for the ChronoLogsOracle contract
    /// @author Aaron Kendall
    constructor() public {
        oracleOwner = msg.sender;
    }

    function addChronoLogEvent(bytes32 uniqueName, bytes32 eType, string memory desc, bytes32 data, bytes32 hash, uint time) public onlyOracleOwner {

        require(chronoEventMap[uniqueName].isValue == false, "Event with unique ID already exists.");

        chronoEvents.push(ChronoLogEvent({
            eventId: idCounter,
            eventName: uniqueName,
            eventType: eType,
            eventDescription: desc,
            eventEpochTime: (time > 0) ? time: block.timestamp,
            publicData: data,
            privateHash: hash,
            isValue: true
        }));

        chronoEventMap[chronoEvents[chronoEvents.length-1].eventName] = chronoEvents[chronoEvents.length-1];

        uint dayIdxVal = time / 24;
        if (dayIndex[dayIdxVal] <= 0)
            dayIndex[dayIdxVal] = idCounter;

        idCounter += 1;

        (typeIndex[eType]).push(uniqueName);
    }

    function getChronoLogBasic(bytes32 uniqueName) public view returns (uint, bytes32, bytes32) {
        return (chronoEventMap[uniqueName].eventEpochTime, chronoEventMap[uniqueName].publicData, chronoEventMap[uniqueName].privateHash);
    }

    function getChronoLog(bytes32 uniqueName) public view returns (bytes32, string memory, uint, bytes32, bytes32) {
        return (chronoEventMap[uniqueName].eventType, chronoEventMap[uniqueName].eventDescription, chronoEventMap[uniqueName].eventEpochTime, chronoEventMap[uniqueName].publicData, chronoEventMap[uniqueName].privateHash);
    }

    function getChronoLogsByType(bytes32 eType) public view returns (bytes32[] memory) {
        return typeIndex[eType];
    }

    function getChronoLogsByType(bytes32 eType, uint startTime, uint endTime) public view returns (bytes32[] memory) {

        bytes32[] memory logNames;

        uint startDayIdx = dayIndex[startTime / 24];

        if (startDayIdx > 0) {

            uint nextDayIdx = dayIndex[startDayIdx+1];
            uint endIdx = 0;

            if (nextDayIdx > 0) {
                endIdx = nextDayIdx;
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