module.exports = {
  // See <http://truffleframework.com/docs/advanced/configuration>
  // to customize your Truffle configuration!
  compilers: {
    solc: {
      version: "0.6.0",
    }
  },
  networks: {
    development: {
      // host: "localhost",
      host: "MyUbuntuVM",
      port: 8545,
      network_id: "*" // Match any network id
      // , gas: 8388608
      , gas: 10000000
    },
    nethereum: {
      host: "testchain.nethereum.com",
      port: 8545,
      network_id: "*" // match any network
      , gas: 8388608
      //, websockets: true
    },
    ropsten: {
      provider: function() {
        return new HDWalletProvider(MNEMONIC, "https://ropsten.infura.io/YOUR_API_KEY")
      },
      network_id: 3,
      gas: 4000000      //make sure this gas allocation isn't over 4M, which is the max
    }
  }
  , solc: {
    optimizer: {
      enabled: true,
      runs: 180
    }
  }	
};
