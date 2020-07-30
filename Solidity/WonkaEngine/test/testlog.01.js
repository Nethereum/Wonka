var ChronoLog = artifacts.require("./ChronoLog.sol");

var version = web3.version.api;
console.log("Web3 version is now (" + version + ")");

contract('ChronoLog', function(accounts) {
 
  it("adding first chrono log", function() {
    return ChronoLog.deployed().then(function(instance) {

      instance.addChronoLogEvent(web3.utils.fromAscii('Event00001'), web3.utils.fromAscii('WonkaRuleTree'), new String('Engine Run #001').valueOf(), web3.utils.fromAscii('333'), web3.utils.fromAscii('98765AFC'), new String('http://www.google.com/something').valueOf());
      console.log("Added chrono log!");
    });
  });
  it("get first chrono log", function() {

    return ChronoLog.deployed().then(function(instance) {

      return instance.getChronoLogEvent.call(web3.utils.fromAscii('Event00001'));
    }).then(function(results) {

      // bytes32, string memory, uint, bytes32, bytes32, string memory
      // assert.equal(balance.valueOf(), 3, "More or less than 3 attributes populated");
      // chronoEventMap[uniqueName].eventType, chronoEventMap[uniqueName].eventDescription, chronoEventMap[uniqueName].eventEpochTime, chronoEventMap[uniqueName].publicData, chronoEventMap[uniqueName].privateHash, chronoEventMap[uniqueName].eventInfoUrl);
      
      var eventType   = results[0];
      var eventDesc   = results[1];
      var eventTime   = results[2];
      var publicData  = results[3];
      var privateHash = results[4];
      var eventUrl    = results[5];

      console.log("First Log Type is (" + web3.utils.toAscii(eventType.valueOf()) + ")");
      console.log("First Log Desc is (" + new String(eventDesc).valueOf() + ")");
      console.log("First Log Time is (" + new String(eventTime).valueOf() + ")");
      console.log("First Log Public Data is (" + web3.utils.toAscii(publicData.valueOf()) + ")");
      console.log("First Log Private Hash is (" + web3.utils.toAscii(privateHash.valueOf()) + ")");
      console.log("First Log Event URL is (" + new String(eventUrl).valueOf() + ")");
    });
  });
  it("adding second chrono log", function() {
    return ChronoLog.deployed().then(function(instance) {

      instance.addChronoLogEvent(web3.utils.fromAscii('Event00101'), web3.utils.fromAscii('WonkaRuleTree'), new String('Engine Run #101').valueOf(), web3.utils.fromAscii('444'), web3.utils.fromAscii('53689BDC'), new String('http://www.google.com/something2').valueOf());
      console.log("Added second chrono log!");
    });
  }); 
  it("get logs", function() {

    return ChronoLog.deployed().then(function(instance) {

      return instance.getChronoLogEventsByType.call(web3.utils.fromAscii('WonkaRuleTree'));
    }).then(function(eventLogs) {
      
      var logUniqueName1 = eventLogs[0];
      var logUniqueName2 = eventLogs[1];

      console.log("First Log Name is (" + web3.utils.toAscii(logUniqueName1) + ")");
      console.log("Second Log Name is (" + web3.utils.toAscii(logUniqueName2) + ")");
    });
  });
  it("get logs by time range", function() {

    return ChronoLog.deployed().then(function(instance) {

      return instance.getChronoLogEventsByTypeAndTime.call(web3.utils.fromAscii('WonkaRuleTree'), 1596055524, 1696055524);
    }).then(function(eventLogsByTime) {
      
      var logUniqueName01 = eventLogsByTime[0];
      var logUniqueName02 = eventLogsByTime[1];

      console.log("First Log Name is (" + web3.utils.toAscii(logUniqueName01) + ")");
      console.log("Second Log Name is (" + web3.utils.toAscii(logUniqueName02) + ")");
    });
  });

}); // end of the scope for ChronoLog
