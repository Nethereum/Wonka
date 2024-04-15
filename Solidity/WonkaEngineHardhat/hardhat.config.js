require('@nomicfoundation/hardhat-toolbox');

// NEVER record important private keys in your code - this is for demo purposes
const SEPOLIA_TESTNET_PRIVATE_KEY = '';
const ARBITRUM_MAINNET_TEMPORARY_PRIVATE_KEY = '';

/** @type import('hardhat/config').HardhatUserConfig */
module.exports = {
  solidity: {
    version: "0.8.9",
    settings: {
      optimizer: {
        enabled: true,
        runs: 1000,
      },
    },
  },
  networks: {
    hardhat: {
      chainId: 1337,
    },
    arbitrumSepolia: {
      url: 'https://sepolia-rollup.arbitrum.io/rpc',
      chainId: 421614,
      accounts: [SEPOLIA_TESTNET_PRIVATE_KEY]
    },
    arbitrumOne: {
      url: 'https://arb1.arbitrum.io/rpc',
      accounts: [ARBITRUM_MAINNET_TEMPORARY_PRIVATE_KEY]
    },
  },
};