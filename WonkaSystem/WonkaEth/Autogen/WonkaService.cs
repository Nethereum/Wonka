using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;

namespace Wonka.Eth.Autogen
{
    public partial class WonkaEngineService
    {
        /**
         ** NOTE: Compilation issues
         **
        public static Task DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, WonkaEngineDeployment wonkaEngineDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler().SendRequestAndWaitForReceiptAsync(wonkaEngineDeployment, cancellationTokenSource);
        }

        public static Task DeployContractAsync(Nethereum.Web3.Web3 web3, WonkaEngineDeployment wonkaEngineDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler().SendRequestAsync(wonkaEngineDeployment);
        }

        public static async Task DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, WonkaEngineDeployment wonkaEngineDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, wonkaEngineDeployment, cancellationTokenSource);
            return new WonkaEngineService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public WonkaEngineService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task RulesMasterQueryAsync(RulesMasterFunction rulesMasterFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(rulesMasterFunction, blockParameter);
        }


        public Task RulesMasterQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(null, blockParameter);
        }

        public Task AttrCounterQueryAsync(AttrCounterFunction attrCounterFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(attrCounterFunction, blockParameter);
        }


        public Task AttrCounterQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(null, blockParameter);
        }

        public Task RuleCounterQueryAsync(RuleCounterFunction ruleCounterFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(ruleCounterFunction, blockParameter);
        }


        public Task RuleCounterQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(null, blockParameter);
        }

        public Task AttributesQueryAsync(AttributesFunction attributesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync(attributesFunction, blockParameter);
        }

        public Task AttributesQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var attributesFunction = new AttributesFunction();
            attributesFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryDeserializingToObjectAsync(attributesFunction, blockParameter);
        }

        public Task RulesetsQueryAsync(RulesetsFunction rulesetsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync(rulesetsFunction, blockParameter);
        }

        public Task RulesetsQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var rulesetsFunction = new RulesetsFunction();
            rulesetsFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryDeserializingToObjectAsync(rulesetsFunction, blockParameter);
        }

        public Task AddAttributeRequestAsync(AddAttributeFunction addAttributeFunction)
        {
            return ContractHandler.SendRequestAsync(addAttributeFunction);
        }

        public Task AddAttributeRequestAndWaitForReceiptAsync(AddAttributeFunction addAttributeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(addAttributeFunction, cancellationToken);
        }

        public Task AddAttributeRequestAsync(byte[] pAttrName, BigInteger pMaxLen, BigInteger pMaxNumVal, string pDefVal, bool pIsStr, bool pIsNum)
        {
            var addAttributeFunction = new AddAttributeFunction();
            addAttributeFunction.PAttrName = pAttrName;
            addAttributeFunction.PMaxLen = pMaxLen;
            addAttributeFunction.PMaxNumVal = pMaxNumVal;
            addAttributeFunction.PDefVal = pDefVal;
            addAttributeFunction.PIsStr = pIsStr;
            addAttributeFunction.PIsNum = pIsNum;

            return ContractHandler.SendRequestAsync(addAttributeFunction);
        }

        public Task AddAttributeRequestAndWaitForReceiptAsync(byte[] pAttrName, BigInteger pMaxLen, BigInteger pMaxNumVal, string pDefVal, bool pIsStr, bool pIsNum, CancellationTokenSource cancellationToken = null)
        {
            var addAttributeFunction = new AddAttributeFunction();
            addAttributeFunction.PAttrName = pAttrName;
            addAttributeFunction.PMaxLen = pMaxLen;
            addAttributeFunction.PMaxNumVal = pMaxNumVal;
            addAttributeFunction.PDefVal = pDefVal;
            addAttributeFunction.PIsStr = pIsStr;
            addAttributeFunction.PIsNum = pIsNum;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(addAttributeFunction, cancellationToken);
        }

        public Task AddRuleTreeRequestAsync(AddRuleTreeFunction addRuleTreeFunction)
        {
            return ContractHandler.SendRequestAsync(addRuleTreeFunction);
        }

        public Task AddRuleTreeRequestAndWaitForReceiptAsync(AddRuleTreeFunction addRuleTreeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(addRuleTreeFunction, cancellationToken);
        }

        public Task AddRuleTreeRequestAsync(string ruler, byte[] rsName, string desc, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately)
        {
            var addRuleTreeFunction = new AddRuleTreeFunction();
            addRuleTreeFunction.Ruler = ruler;
            addRuleTreeFunction.RsName = rsName;
            addRuleTreeFunction.Desc = desc;
            addRuleTreeFunction.SevereFailureFlag = severeFailureFlag;
            addRuleTreeFunction.UseAndOperator = useAndOperator;
            addRuleTreeFunction.FlagFailImmediately = flagFailImmediately;

            return ContractHandler.SendRequestAsync(addRuleTreeFunction);
        }

        public Task AddRuleTreeRequestAndWaitForReceiptAsync(string ruler, byte[] rsName, string desc, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately, CancellationTokenSource cancellationToken = null)
        {
            var addRuleTreeFunction = new AddRuleTreeFunction();
            addRuleTreeFunction.Ruler = ruler;
            addRuleTreeFunction.RsName = rsName;
            addRuleTreeFunction.Desc = desc;
            addRuleTreeFunction.SevereFailureFlag = severeFailureFlag;
            addRuleTreeFunction.UseAndOperator = useAndOperator;
            addRuleTreeFunction.FlagFailImmediately = flagFailImmediately;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(addRuleTreeFunction, cancellationToken);
        }

        public Task AddRuleSetRequestAsync(AddRuleSetFunction addRuleSetFunction)
        {
            return ContractHandler.SendRequestAsync(addRuleSetFunction);
        }

        public Task AddRuleSetRequestAndWaitForReceiptAsync(AddRuleSetFunction addRuleSetFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(addRuleSetFunction, cancellationToken);
        }

        public Task AddRuleSetRequestAsync(string ruler, byte[] ruleSetName, string desc, byte[] parentRSName, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately)
        {
            var addRuleSetFunction = new AddRuleSetFunction();
            addRuleSetFunction.Ruler = ruler;
            addRuleSetFunction.RuleSetName = ruleSetName;
            addRuleSetFunction.Desc = desc;
            addRuleSetFunction.ParentRSName = parentRSName;
            addRuleSetFunction.SevereFailureFlag = severeFailureFlag;
            addRuleSetFunction.UseAndOperator = useAndOperator;
            addRuleSetFunction.FlagFailImmediately = flagFailImmediately;

            return ContractHandler.SendRequestAsync(addRuleSetFunction);
        }

        public Task AddRuleSetRequestAndWaitForReceiptAsync(string ruler, byte[] ruleSetName, string desc, byte[] parentRSName, bool severeFailureFlag, bool useAndOperator, bool flagFailImmediately, CancellationTokenSource cancellationToken = null)
        {
            var addRuleSetFunction = new AddRuleSetFunction();
            addRuleSetFunction.Ruler = ruler;
            addRuleSetFunction.RuleSetName = ruleSetName;
            addRuleSetFunction.Desc = desc;
            addRuleSetFunction.ParentRSName = parentRSName;
            addRuleSetFunction.SevereFailureFlag = severeFailureFlag;
            addRuleSetFunction.UseAndOperator = useAndOperator;
            addRuleSetFunction.FlagFailImmediately = flagFailImmediately;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(addRuleSetFunction, cancellationToken);
        }

        public Task AddRuleRequestAsync(AddRuleFunction addRuleFunction)
        {
            return ContractHandler.SendRequestAsync(addRuleFunction);
        }

        public Task AddRuleRequestAndWaitForReceiptAsync(AddRuleFunction addRuleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(addRuleFunction, cancellationToken);
        }

        public Task AddRuleRequestAsync(string ruler, byte[] ruleSetId, byte[] ruleName, byte[] attrName, BigInteger rType, string rVal, bool notFlag, bool passiveFlag)
        {
            var addRuleFunction = new AddRuleFunction();
            addRuleFunction.Ruler = ruler;
            addRuleFunction.RuleSetId = ruleSetId;
            addRuleFunction.RuleName = ruleName;
            addRuleFunction.AttrName = attrName;
            addRuleFunction.RType = rType;
            addRuleFunction.RVal = rVal;
            addRuleFunction.NotFlag = notFlag;
            addRuleFunction.PassiveFlag = passiveFlag;

            return ContractHandler.SendRequestAsync(addRuleFunction);
        }

        public Task AddRuleRequestAndWaitForReceiptAsync(string ruler, byte[] ruleSetId, byte[] ruleName, byte[] attrName, BigInteger rType, string rVal, bool notFlag, bool passiveFlag, CancellationTokenSource cancellationToken = null)
        {
            var addRuleFunction = new AddRuleFunction();
            addRuleFunction.Ruler = ruler;
            addRuleFunction.RuleSetId = ruleSetId;
            addRuleFunction.RuleName = ruleName;
            addRuleFunction.AttrName = attrName;
            addRuleFunction.RType = rType;
            addRuleFunction.RVal = rVal;
            addRuleFunction.NotFlag = notFlag;
            addRuleFunction.PassiveFlag = passiveFlag;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(addRuleFunction, cancellationToken);
        }

        public Task ExecuteRequestAsync(ExecuteFunction executeFunction)
        {
            return ContractHandler.SendRequestAsync(executeFunction);
        }

        public Task ExecuteRequestAndWaitForReceiptAsync(ExecuteFunction executeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(executeFunction, cancellationToken);
        }

        public Task ExecuteRequestAsync(string ruler)
        {
            var executeFunction = new ExecuteFunction();
            executeFunction.Ruler = ruler;

            return ContractHandler.SendRequestAsync(executeFunction);
        }

        public Task ExecuteRequestAndWaitForReceiptAsync(string ruler, CancellationTokenSource cancellationToken = null)
        {
            var executeFunction = new ExecuteFunction();
            executeFunction.Ruler = ruler;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(executeFunction, cancellationToken);
        }

        public Task ExecuteWithReportRequestAsync(ExecuteWithReportFunction executeWithReportFunction)
        {
            return ContractHandler.SendRequestAsync(executeWithReportFunction);
        }

        public Task ExecuteWithReportRequestAndWaitForReceiptAsync(ExecuteWithReportFunction executeWithReportFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(executeWithReportFunction, cancellationToken);
        }

        public Task ExecuteWithReportRequestAsync(string ruler)
        {
            var executeWithReportFunction = new ExecuteWithReportFunction();
            executeWithReportFunction.Ruler = ruler;

            return ContractHandler.SendRequestAsync(executeWithReportFunction);
        }

        public Task ExecuteWithReportRequestAndWaitForReceiptAsync(string ruler, CancellationTokenSource cancellationToken = null)
        {
            var executeWithReportFunction = new ExecuteWithReportFunction();
            executeWithReportFunction.Ruler = ruler;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(executeWithReportFunction, cancellationToken);
        }

        public Task GetAttributeNameQueryAsync(GetAttributeNameFunction getAttributeNameFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(getAttributeNameFunction, blockParameter);
        }


        public Task GetAttributeNameQueryAsync(BigInteger idx, BlockParameter blockParameter = null)
        {
            var getAttributeNameFunction = new GetAttributeNameFunction();
            getAttributeNameFunction.Idx = idx;

            return ContractHandler.QueryAsync(getAttributeNameFunction, blockParameter);
        }

        public Task GetValueOnRecordQueryAsync(GetValueOnRecordFunction getValueOnRecordFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(getValueOnRecordFunction, blockParameter);
        }


        public Task GetValueOnRecordQueryAsync(string ruler, byte[] key, BlockParameter blockParameter = null)
        {
            var getValueOnRecordFunction = new GetValueOnRecordFunction();
            getValueOnRecordFunction.Ruler = ruler;
            getValueOnRecordFunction.Key = key;

            return ContractHandler.QueryAsync(getValueOnRecordFunction, blockParameter);
        }

        public Task GetNumberOfAttributesQueryAsync(GetNumberOfAttributesFunction getNumberOfAttributesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(getNumberOfAttributesFunction, blockParameter);
        }


        public Task GetNumberOfAttributesQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(null, blockParameter);
        }

        public Task GetOrchestrationModeQueryAsync(GetOrchestrationModeFunction getOrchestrationModeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(getOrchestrationModeFunction, blockParameter);
        }


        public Task GetOrchestrationModeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(null, blockParameter);
        }

        public Task GetIsSourceMappedQueryAsync(GetIsSourceMappedFunction getIsSourceMappedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(getIsSourceMappedFunction, blockParameter);
        }


        public Task GetIsSourceMappedQueryAsync(byte[] key, BlockParameter blockParameter = null)
        {
            var getIsSourceMappedFunction = new GetIsSourceMappedFunction();
            getIsSourceMappedFunction.Key = key;

            return ContractHandler.QueryAsync(getIsSourceMappedFunction, blockParameter);
        }

        public Task HasRuleTreeQueryAsync(HasRuleTreeFunction hasRuleTreeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(hasRuleTreeFunction, blockParameter);
        }


        public Task HasRuleTreeQueryAsync(string ruler, BlockParameter blockParameter = null)
        {
            var hasRuleTreeFunction = new HasRuleTreeFunction();
            hasRuleTreeFunction.Ruler = ruler;

            return ContractHandler.QueryAsync(hasRuleTreeFunction, blockParameter);
        }

        public Task SetValueOnRecordRequestAsync(SetValueOnRecordFunction setValueOnRecordFunction)
        {
            return ContractHandler.SendRequestAsync(setValueOnRecordFunction);
        }

        public Task SetValueOnRecordRequestAndWaitForReceiptAsync(SetValueOnRecordFunction setValueOnRecordFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setValueOnRecordFunction, cancellationToken);
        }

        public Task SetValueOnRecordRequestAsync(string ruler, byte[] key, string value)
        {
            var setValueOnRecordFunction = new SetValueOnRecordFunction();
            setValueOnRecordFunction.Ruler = ruler;
            setValueOnRecordFunction.Key = key;
            setValueOnRecordFunction.Value = value;

            return ContractHandler.SendRequestAsync(setValueOnRecordFunction);
        }

        public Task SetValueOnRecordRequestAndWaitForReceiptAsync(string ruler, byte[] key, string value, CancellationTokenSource cancellationToken = null)
        {
            var setValueOnRecordFunction = new SetValueOnRecordFunction();
            setValueOnRecordFunction.Ruler = ruler;
            setValueOnRecordFunction.Key = key;
            setValueOnRecordFunction.Value = value;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setValueOnRecordFunction, cancellationToken);
        }

        public Task Bytes32ToStringQueryAsync(Bytes32ToStringFunction bytes32ToStringFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync(bytes32ToStringFunction, blockParameter);
        }


        public Task Bytes32ToStringQueryAsync(byte[] x, BlockParameter blockParameter = null)
        {
            var bytes32ToStringFunction = new Bytes32ToStringFunction();
            bytes32ToStringFunction.X = x;

            return ContractHandler.QueryAsync(bytes32ToStringFunction, blockParameter);
        }
        */
    }
}