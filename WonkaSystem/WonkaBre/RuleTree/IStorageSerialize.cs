using System;

namespace Wonka.BizRulesEngine.RuleTree
{
	public interface IStorageSerialize
	{
		bool IsSerializable();

		bool SerializeToStorage();
	}
}
