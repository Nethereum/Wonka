using System;
using System.Collections.Generic;
using System.Text;

using Wonka.MetaData;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.RuleTree;

using Wonka.Eth.Extensions;

namespace Wonka.Eth.Init
{
	/**
	 ** NOTE: This class has not yet been tested
	 **/
	public class WonkaEthRulesEngine : WonkaBizRulesEngine
	{
        public WonkaEthRulesEngine(StringBuilder                      psRules,
                                   Dictionary<string, WonkaBizSource> poSourceMap,
				                         WonkaEthEngineInitialization poEthEngineInit,
					                             IMetadataRetrievable piMetadataSource,
												               string psERC20Address,
															   string psERC721Address,
                                                                 bool pbAddToRegistry = true)
            : base(psRules,
				   poSourceMap,
				   poEthEngineInit.InitializeTokenOpMap(psERC20Address, psERC721Address),
				   piMetadataSource,
				   pbAddToRegistry)
        {
			// NOTE: Should anything be done here?
        }
    }
}
