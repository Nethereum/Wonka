using System;

using Wonka.MetaData;

namespace Wonka.Product
{
    public static class WonkaPrdExtensions
    {
		public static string GetAttributeValue(this WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr)
		{
			string sAttrValue = "";

			if (poTargetProduct.HasProductGroup(poTargetAttr.GroupId))
			{
				if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0].ContainsKey(poTargetAttr.AttrId))
					sAttrValue = poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId];
			}
				
			return sAttrValue;
		}

		public static bool SetAttribute(this WonkaProduct poTargetProduct, WonkaRefAttr poTargetAttr, string psTargetValue)
        {
			bool bSuccess = true;

			if (poTargetProduct.GetProductGroup(poTargetAttr.GroupId).GetRowCount() <= 0)
				poTargetProduct.GetProductGroup(poTargetAttr.GroupId).AppendRow();

			poTargetProduct.GetProductGroup(poTargetAttr.GroupId)[0][poTargetAttr.AttrId] = psTargetValue;

			return bSuccess;
		}
    }
}
