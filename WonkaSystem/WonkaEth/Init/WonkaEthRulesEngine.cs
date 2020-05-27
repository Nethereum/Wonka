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
                                                                 bool pbAddToRegistry = true)
            : base(psRules,
				   poSourceMap,
				   poEthEngineInit.InitializeTokenOpMap(),
				   piMetadataSource,
				   pbAddToRegistry)
        {
			// NOTE: Should anything be done here?
        }
    }
}
