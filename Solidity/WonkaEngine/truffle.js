module.exports = {
  // See <http://truffleframework.com/docs/advanced/configuration>
  // to customize your Truffle configuration!
  networks: {
    development: {
    host: "localhost",
    port: 8545,
    network_id: "*" // Match any network id
    , gas: 8388608
    // , gas: 20000000
   }
  } 
};
