using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Nethereum.Contracts;
using Nethereum.Web3;

namespace WonkaEth.Extensions
{
    /// <summary>
    /// 
    /// This extensions class provides the functionality to perform actions on behalf of the Autogen
    /// classes, like deploy Wonka contracts to the chain.
    /// 
    /// </summary>
    public static class WonkaAutogenExtensions
    {
        /// <summary>
        /// 
        /// This method will use Nethereum to deploy a Wonka contract to the chain.
        /// 
        /// <param name="poDeployMsg">The deployment message with the bytecodes that can create the target contract</param>
        /// <param name="psWonkaAbi">The ABI of the target contract</param>
        /// <param name="psSenderAddress">The ABI of the target contract</param>
        /// <param name="psWeb3HttpUrl">The client node to which we will deploy the contract</param>
        /// <returns>The address of the new instance of the target contract</returns>
        /// </summary>
        public static string DeployContract(this ContractDeploymentMessage poDeployMsg, Web3 poWeb3, string psWonkaAbi, string psSenderAddress, string psWeb3HttpUrl = "")
        {
            var transactionHash =
                poWeb3.Eth.DeployContract.SendRequestAsync(psWonkaAbi, poDeployMsg.ByteCode, psSenderAddress).Result;

            // NOTE: Should we start/stop mining by default?
            // var mineResult = poWeb3.Miner.Start.SendRequestAsync(6).Result;

            var receipt = poWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash).Result;

            while (receipt == null)
            {
                Thread.Sleep(5000);
                receipt = poWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash).Result;
            }

            // NOTE: Should we start/stop mining by default?
            // mineResult = poWeb3.Miner.Stop.SendRequestAsync().Result;

            var contractAddress = receipt.ContractAddress;

            return contractAddress;
        }

        /// <summary>
        /// 
        /// This method will use Nethereum to deploy a Wonka contract to the chain.
        /// 
        /// <param name="poDeployMsg">The deployment message with the bytecodes that can create the target contract</param>
        /// <param name="psWonkaAbi">The ABI of the target contract</param>
        /// <param name="psSenderAddress">The ABI of the target contract</param>
        /// <param name="pnGas">The amount of gas supplied for the deployment of the contract</param>
        /// <param name="psWeb3HttpUrl">The client node to which we will deploy the contract</param>
        /// <returns>The address of the new instance of the target contract</returns>
        /// </summary>
        public static string DeployContract(this ContractDeploymentMessage poDeployMsg, Web3 poWeb3, string psWonkaAbi, string psSenderAddress, Nethereum.Hex.HexTypes.HexBigInteger pnGas, string psWeb3HttpUrl = "")
        {
            var transactionHash =
                poWeb3.Eth.DeployContract.SendRequestAsync(psWonkaAbi, poDeployMsg.ByteCode, psSenderAddress, pnGas).Result;

            // NOTE: Should we start/stop mining by default?
            // var mineResult = poWeb3.Miner.Start.SendRequestAsync(6).Result;

            var receipt = poWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash).Result;

            while (receipt == null)
            {
                Thread.Sleep(5000);
                receipt = poWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash).Result;
            }

            // NOTE: Should we start/stop mining by default?
            // mineResult = poWeb3.Miner.Stop.SendRequestAsync().Result;

            var contractAddress = receipt.ContractAddress;

            return contractAddress;
        }
    }
}
