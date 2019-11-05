//-----------------------------------------
//BizDataStorageDefinition.cs
//-----------------------------------------
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace Wonka.Eth.Autogen.BizDataStorage
{

    /*
	public partial class BizDataStorageDeployment : BizDataStorageDeploymentBase
	{
		public BizDataStorageDeployment() : base(BYTECODE) { }
		public BizDataStorageDeployment(string byteCode) : base(byteCode) { }
	}

	public class BizDataStorageDeploymentBase : ContractDeploymentMessage
	{
		public static string BYTECODE = "0x0";
		public BizDataStorageDeploymentBase() : base(BYTECODE) { }
		public BizDataStorageDeploymentBase(string byteCode) : base(byteCode) { }

	}
	*/

	public partial class AddAttributeFunction : AddAttributeFunctionBase { }

	[Function("addAttribute", "bool")]
	public class AddAttributeFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "pAttrName", 1)]
		public virtual byte[] PAttrName { get; set; }
		[Parameter("uint256", "pMaxLen", 2)]
		public virtual BigInteger PMaxLen { get; set; }
		[Parameter("uint256", "pMaxNumVal", 3)]
		public virtual BigInteger PMaxNumVal { get; set; }
		[Parameter("bytes32", "pDefVal", 4)]
		public virtual byte[] PDefVal { get; set; }
		[Parameter("bool", "pIsStr", 5)]
		public virtual bool PIsStr { get; set; }
		[Parameter("bool", "pIsNum", 6)]
		public virtual bool PIsNum { get; set; }
	}

	public partial class AddEntityTypeFunction : AddEntityTypeFunctionBase { }

	[Function("addEntityType", "bool")]
	public class AddEntityTypeFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("bytes32[]", "attrNameList", 2)]
		public virtual List<string> AttrNameList { get; set; }
	}

	public partial class AddEntityChildTypeFunction : AddEntityChildTypeFunctionBase { }

	[Function("addEntityChildType", "bool")]
	public class AddEntityChildTypeFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "parentEntityName", 1)]
		public virtual byte[] ParentEntityName { get; set; }
		[Parameter("bytes32", "childEntityName", 2)]
		public virtual byte[] ChildEntityName { get; set; }
	}

	public partial class GetEntityChildrenFunction : GetEntityChildrenFunctionBase { }

	[Function("getEntityChildren", "uint256[]")]
	public class GetEntityChildrenFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("uint256", "entityId", 2)]
		public virtual BigInteger EntityId { get; set; }
		[Parameter("bytes32", "childEntityName", 3)]
		public virtual byte[] ChildEntityName { get; set; }
	}

	public partial class GetEntityFunction : GetEntityFunctionBase { }

	[Function("getEntity", typeof(GetEntityOutputDTO))]
	public class GetEntityFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("uint256", "entityId", 2)]
		public virtual BigInteger EntityId { get; set; }
	}

	public partial class QueryEntitiesFunction : QueryEntitiesFunctionBase { }

	[Function("queryEntities", "uint256[]")]
	public class QueryEntitiesFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("bytes32[]", "matchFieldNames", 2)]
		public virtual List<string> MatchFieldNames { get; set; }
		[Parameter("bytes32[]", "matchFieldValues", 3)]
		public virtual List<string> MatchFieldValues { get; set; }
	}

	public partial class QueryChildEntitiesFunction : QueryChildEntitiesFunctionBase { }

	[Function("queryChildEntities", "uint256[]")]
	public class QueryChildEntitiesFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("uint256", "entityId", 2)]
		public virtual BigInteger EntityId { get; set; }
		[Parameter("bytes32", "childEntityName", 3)]
		public virtual byte[] ChildEntityName { get; set; }
		[Parameter("bytes32[]", "fieldNames", 4)]
		public virtual List<string> FieldNames { get; set; }
		[Parameter("bytes32[]", "fieldValues", 5)]
		public virtual List<string> FieldValues { get; set; }
	}

	public partial class SetEntityFunction : SetEntityFunctionBase { }

	[Function("setEntity", "bool")]
	public class SetEntityFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("uint256", "entityId", 2)]
		public virtual BigInteger EntityId { get; set; }
		[Parameter("bytes32[]", "fieldNames", 3)]
		public virtual List<string> FieldNames { get; set; }
		[Parameter("bytes32[]", "fieldValues", 4)]
		public virtual List<string> FieldValues { get; set; }
	}

	public partial class SetEntityChildFunction : SetEntityChildFunctionBase { }

	[Function("setEntityChild", "bool")]
	public class SetEntityChildFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("uint256", "entityId", 2)]
		public virtual BigInteger EntityId { get; set; }
		[Parameter("bytes32", "childEntityName", 3)]
		public virtual byte[] ChildEntityName { get; set; }
		[Parameter("uint256", "childEntityId", 4)]
		public virtual BigInteger ChildEntityId { get; set; }
	}

	public partial class SetEntityChildrenFunction : SetEntityChildrenFunctionBase { }

	[Function("setEntityChildren", "bool")]
	public class SetEntityChildrenFunctionBase : FunctionMessage
	{
		[Parameter("bytes32", "entityName", 1)]
		public virtual byte[] EntityName { get; set; }
		[Parameter("uint256", "entityId", 2)]
		public virtual BigInteger EntityId { get; set; }
		[Parameter("bytes32", "childEntityName", 3)]
		public virtual byte[] ChildEntityName { get; set; }
		[Parameter("uint256[]", "childEntityIds", 4)]
		public virtual List<string> ChildEntityIds { get; set; }
	}

	public partial class GetEntityOutputDTO : GetEntityOutputDTOBase { }

	[FunctionOutput]
	public class GetEntityOutputDTOBase : IFunctionOutputDTO
	{
		[Parameter("bytes32", "entityTypeName", 1)]
		public virtual byte[] EntityTypeName { get; set; }
		[Parameter("uint256", "eID", 2)]
		public virtual BigInteger EID { get; set; }
		[Parameter("uint256[]", "attrIds", 3)]
		public virtual List<string> AttrIds { get; set; }
		[Parameter("bytes32[]", "attrValues", 4)]
		public virtual List<string> AttrValues { get; set; }
	}

}
