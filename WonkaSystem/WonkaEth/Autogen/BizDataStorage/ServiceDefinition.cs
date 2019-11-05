//-----------------------------------------
//BizDataStorageService.cs
//-----------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Wonka.Eth.Autogen.BizDataStorage
{
	public partial class BizDataStorageService
	{
		/*
		public static Task DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, BizDataStorageDeployment bizDataStorageDeployment, CancellationTokenSource cancellationTokenSource = null)
		{
			return web3.Eth.GetContractDeploymentHandler().SendRequestAndWaitForReceiptAsync(bizDataStorageDeployment, cancellationTokenSource);
		}

		public static Task DeployContractAsync(Nethereum.Web3.Web3 web3, BizDataStorageDeployment bizDataStorageDeployment)
		{
			return web3.Eth.GetContractDeploymentHandler().SendRequestAsync(bizDataStorageDeployment);
		}

		public static async Task DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, BizDataStorageDeployment bizDataStorageDeployment, CancellationTokenSource cancellationTokenSource = null)
		{
			var receipt = await DeployContractAndWaitForReceiptAsync(web3, bizDataStorageDeployment, cancellationTokenSource);
			return new BizDataStorageService(web3, receipt.ContractAddress);
		}
		*/

		protected Nethereum.Web3.Web3 Web3 { get; }

		public ContractHandler ContractHandler { get; }

		public BizDataStorageService(Nethereum.Web3.Web3 web3, string contractAddress)
		{
			Web3 = web3;
			ContractHandler = web3.Eth.GetContractHandler(contractAddress);
		}

		public Task AddAttributeRequestAsync(AddAttributeFunction addAttributeFunction)
		{
			return ContractHandler.SendRequestAsync(addAttributeFunction);
		}

		public Task AddAttributeRequestAndWaitForReceiptAsync(AddAttributeFunction addAttributeFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(addAttributeFunction, cancellationToken);
		}

		public Task AddAttributeRequestAsync(byte[] pAttrName, BigInteger pMaxLen, BigInteger pMaxNumVal, byte[] pDefVal, bool pIsStr, bool pIsNum)
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

		public Task AddAttributeRequestAndWaitForReceiptAsync(byte[] pAttrName, BigInteger pMaxLen, BigInteger pMaxNumVal, byte[] pDefVal, bool pIsStr, bool pIsNum, CancellationTokenSource cancellationToken = null)
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

		public Task AddEntityTypeRequestAsync(AddEntityTypeFunction addEntityTypeFunction)
		{
			return ContractHandler.SendRequestAsync(addEntityTypeFunction);
		}

		public Task AddEntityTypeRequestAndWaitForReceiptAsync(AddEntityTypeFunction addEntityTypeFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(addEntityTypeFunction, cancellationToken);
		}

		public Task AddEntityTypeRequestAsync(byte[] entityName, List<string> attrNameList)
		{
			var addEntityTypeFunction = new AddEntityTypeFunction();
			addEntityTypeFunction.EntityName = entityName;
			addEntityTypeFunction.AttrNameList = attrNameList;

			return ContractHandler.SendRequestAsync(addEntityTypeFunction);
		}

		public Task AddEntityTypeRequestAndWaitForReceiptAsync(byte[] entityName, List<string> attrNameList, CancellationTokenSource cancellationToken = null)
		{
			var addEntityTypeFunction = new AddEntityTypeFunction();
			addEntityTypeFunction.EntityName = entityName;
			addEntityTypeFunction.AttrNameList = attrNameList;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(addEntityTypeFunction, cancellationToken);
		}

		public Task AddEntityChildTypeRequestAsync(AddEntityChildTypeFunction addEntityChildTypeFunction)
		{
			return ContractHandler.SendRequestAsync(addEntityChildTypeFunction);
		}

		public Task AddEntityChildTypeRequestAndWaitForReceiptAsync(AddEntityChildTypeFunction addEntityChildTypeFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(addEntityChildTypeFunction, cancellationToken);
		}

		public Task AddEntityChildTypeRequestAsync(byte[] parentEntityName, byte[] childEntityName)
		{
			var addEntityChildTypeFunction = new AddEntityChildTypeFunction();
			addEntityChildTypeFunction.ParentEntityName = parentEntityName;
			addEntityChildTypeFunction.ChildEntityName = childEntityName;

			return ContractHandler.SendRequestAsync(addEntityChildTypeFunction);
		}

		public Task AddEntityChildTypeRequestAndWaitForReceiptAsync(byte[] parentEntityName, byte[] childEntityName, CancellationTokenSource cancellationToken = null)
		{
			var addEntityChildTypeFunction = new AddEntityChildTypeFunction();
			addEntityChildTypeFunction.ParentEntityName = parentEntityName;
			addEntityChildTypeFunction.ChildEntityName = childEntityName;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(addEntityChildTypeFunction, cancellationToken);
		}

		public Task GetEntityChildrenRequestAsync(GetEntityChildrenFunction getEntityChildrenFunction)
		{
			return ContractHandler.SendRequestAsync(getEntityChildrenFunction);
		}

		public Task GetEntityChildrenRequestAndWaitForReceiptAsync(GetEntityChildrenFunction getEntityChildrenFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(getEntityChildrenFunction, cancellationToken);
		}

		public Task GetEntityChildrenRequestAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName)
		{
			var getEntityChildrenFunction = new GetEntityChildrenFunction();
			getEntityChildrenFunction.EntityName = entityName;
			getEntityChildrenFunction.EntityId = entityId;
			getEntityChildrenFunction.ChildEntityName = childEntityName;

			return ContractHandler.SendRequestAsync(getEntityChildrenFunction);
		}

		public Task GetEntityChildrenRequestAndWaitForReceiptAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, CancellationTokenSource cancellationToken = null)
		{
			var getEntityChildrenFunction = new GetEntityChildrenFunction();
			getEntityChildrenFunction.EntityName = entityName;
			getEntityChildrenFunction.EntityId = entityId;
			getEntityChildrenFunction.ChildEntityName = childEntityName;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(getEntityChildrenFunction, cancellationToken);
		}

		public Task GetEntityRequestAsync(GetEntityFunction getEntityFunction)
		{
			return ContractHandler.SendRequestAsync(getEntityFunction);
		}

		public Task GetEntityRequestAndWaitForReceiptAsync(GetEntityFunction getEntityFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(getEntityFunction, cancellationToken);
		}

		public Task GetEntityRequestAsync(byte[] entityName, BigInteger entityId)
		{
			var getEntityFunction = new GetEntityFunction();
			getEntityFunction.EntityName = entityName;
			getEntityFunction.EntityId = entityId;

			return ContractHandler.SendRequestAsync(getEntityFunction);
		}

		public Task GetEntityRequestAndWaitForReceiptAsync(byte[] entityName, BigInteger entityId, CancellationTokenSource cancellationToken = null)
		{
			var getEntityFunction = new GetEntityFunction();
			getEntityFunction.EntityName = entityName;
			getEntityFunction.EntityId = entityId;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(getEntityFunction, cancellationToken);
		}

		public Task QueryEntitiesRequestAsync(QueryEntitiesFunction queryEntitiesFunction)
		{
			return ContractHandler.SendRequestAsync(queryEntitiesFunction);
		}

		public Task QueryEntitiesRequestAndWaitForReceiptAsync(QueryEntitiesFunction queryEntitiesFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(queryEntitiesFunction, cancellationToken);
		}

		public Task QueryEntitiesRequestAsync(byte[] entityName, List<string> matchFieldNames, List<string> matchFieldValues)
		{
			var queryEntitiesFunction = new QueryEntitiesFunction();
			queryEntitiesFunction.EntityName = entityName;
			queryEntitiesFunction.MatchFieldNames = matchFieldNames;
			queryEntitiesFunction.MatchFieldValues = matchFieldValues;

			return ContractHandler.SendRequestAsync(queryEntitiesFunction);
		}

		public Task QueryEntitiesRequestAndWaitForReceiptAsync(byte[] entityName, List<string> matchFieldNames, List<string> matchFieldValues, CancellationTokenSource cancellationToken = null)
		{
			var queryEntitiesFunction = new QueryEntitiesFunction();
			queryEntitiesFunction.EntityName = entityName;
			queryEntitiesFunction.MatchFieldNames = matchFieldNames;
			queryEntitiesFunction.MatchFieldValues = matchFieldValues;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(queryEntitiesFunction, cancellationToken);
		}

		public Task QueryChildEntitiesRequestAsync(QueryChildEntitiesFunction queryChildEntitiesFunction)
		{
			return ContractHandler.SendRequestAsync(queryChildEntitiesFunction);
		}

		public Task QueryChildEntitiesRequestAndWaitForReceiptAsync(QueryChildEntitiesFunction queryChildEntitiesFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(queryChildEntitiesFunction, cancellationToken);
		}

		public Task QueryChildEntitiesRequestAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, List<string> fieldNames, List<string> fieldValues)
		{
			var queryChildEntitiesFunction = new QueryChildEntitiesFunction();
			queryChildEntitiesFunction.EntityName = entityName;
			queryChildEntitiesFunction.EntityId = entityId;
			queryChildEntitiesFunction.ChildEntityName = childEntityName;
			queryChildEntitiesFunction.FieldNames = fieldNames;
			queryChildEntitiesFunction.FieldValues = fieldValues;

			return ContractHandler.SendRequestAsync(queryChildEntitiesFunction);
		}

		public Task QueryChildEntitiesRequestAndWaitForReceiptAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, List<string> fieldNames, List<string> fieldValues, CancellationTokenSource cancellationToken = null)
		{
			var queryChildEntitiesFunction = new QueryChildEntitiesFunction();
			queryChildEntitiesFunction.EntityName = entityName;
			queryChildEntitiesFunction.EntityId = entityId;
			queryChildEntitiesFunction.ChildEntityName = childEntityName;
			queryChildEntitiesFunction.FieldNames = fieldNames;
			queryChildEntitiesFunction.FieldValues = fieldValues;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(queryChildEntitiesFunction, cancellationToken);
		}

		public Task SetEntityRequestAsync(SetEntityFunction setEntityFunction)
		{
			return ContractHandler.SendRequestAsync(setEntityFunction);
		}

		public Task SetEntityRequestAndWaitForReceiptAsync(SetEntityFunction setEntityFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(setEntityFunction, cancellationToken);
		}

		public Task SetEntityRequestAsync(byte[] entityName, BigInteger entityId, List<string> fieldNames, List<string> fieldValues)
		{
			var setEntityFunction = new SetEntityFunction();
			setEntityFunction.EntityName = entityName;
			setEntityFunction.EntityId = entityId;
			setEntityFunction.FieldNames = fieldNames;
			setEntityFunction.FieldValues = fieldValues;

			return ContractHandler.SendRequestAsync(setEntityFunction);
		}

		public Task SetEntityRequestAndWaitForReceiptAsync(byte[] entityName, BigInteger entityId, List<string> fieldNames, List<string> fieldValues, CancellationTokenSource cancellationToken = null)
		{
			var setEntityFunction = new SetEntityFunction();
			setEntityFunction.EntityName = entityName;
			setEntityFunction.EntityId = entityId;
			setEntityFunction.FieldNames = fieldNames;
			setEntityFunction.FieldValues = fieldValues;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(setEntityFunction, cancellationToken);
		}

		public Task SetEntityChildRequestAsync(SetEntityChildFunction setEntityChildFunction)
		{
			return ContractHandler.SendRequestAsync(setEntityChildFunction);
		}

		public Task SetEntityChildRequestAndWaitForReceiptAsync(SetEntityChildFunction setEntityChildFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(setEntityChildFunction, cancellationToken);
		}

		public Task SetEntityChildRequestAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, BigInteger childEntityId)
		{
			var setEntityChildFunction = new SetEntityChildFunction();
			setEntityChildFunction.EntityName = entityName;
			setEntityChildFunction.EntityId = entityId;
			setEntityChildFunction.ChildEntityName = childEntityName;
			setEntityChildFunction.ChildEntityId = childEntityId;

			return ContractHandler.SendRequestAsync(setEntityChildFunction);
		}

		public Task SetEntityChildRequestAndWaitForReceiptAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, BigInteger childEntityId, CancellationTokenSource cancellationToken = null)
		{
			var setEntityChildFunction = new SetEntityChildFunction();
			setEntityChildFunction.EntityName = entityName;
			setEntityChildFunction.EntityId = entityId;
			setEntityChildFunction.ChildEntityName = childEntityName;
			setEntityChildFunction.ChildEntityId = childEntityId;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(setEntityChildFunction, cancellationToken);
		}

		public Task SetEntityChildrenRequestAsync(SetEntityChildrenFunction setEntityChildrenFunction)
		{
			return ContractHandler.SendRequestAsync(setEntityChildrenFunction);
		}

		public Task SetEntityChildrenRequestAndWaitForReceiptAsync(SetEntityChildrenFunction setEntityChildrenFunction, CancellationTokenSource cancellationToken = null)
		{
			return ContractHandler.SendRequestAndWaitForReceiptAsync(setEntityChildrenFunction, cancellationToken);
		}

		public Task SetEntityChildrenRequestAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, List<string> childEntityIds)
		{
			var setEntityChildrenFunction = new SetEntityChildrenFunction();
			setEntityChildrenFunction.EntityName = entityName;
			setEntityChildrenFunction.EntityId = entityId;
			setEntityChildrenFunction.ChildEntityName = childEntityName;
			setEntityChildrenFunction.ChildEntityIds = childEntityIds;

			return ContractHandler.SendRequestAsync(setEntityChildrenFunction);
		}

		public Task SetEntityChildrenRequestAndWaitForReceiptAsync(byte[] entityName, BigInteger entityId, byte[] childEntityName, List<string> childEntityIds, CancellationTokenSource cancellationToken = null)
		{
			var setEntityChildrenFunction = new SetEntityChildrenFunction();
			setEntityChildrenFunction.EntityName = entityName;
			setEntityChildrenFunction.EntityId = entityId;
			setEntityChildrenFunction.ChildEntityName = childEntityName;
			setEntityChildrenFunction.ChildEntityIds = childEntityIds;

			return ContractHandler.SendRequestAndWaitForReceiptAsync(setEntityChildrenFunction, cancellationToken);
		}
	}
}