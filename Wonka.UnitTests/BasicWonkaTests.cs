using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Moq;
using Xunit;

using Wonka.BizRulesEngine;
using Wonka.Eth.Contracts;
using Wonka.MetaData;

namespace Wonka.UnitTests
{
    public class BasicWonkaTests
    {
        public const string CONST_INFURA_IPFS_GATEWAY_URL = "https://ipfs.infura.io/ipfs/";

        private readonly WonkaRefEnvironment _refEnvHandle = null;

        private readonly WonkaBizRulesEngine _rulesEngine = null;

        private readonly Mock<IOrchestrate> _client;

        public BasicWonkaTests()
        {
            var metadataSource =
                new Wonka.BizRulesEngine.Samples.WonkaBreMetadataTestSource();

            _refEnvHandle = WonkaRefEnvironment.CreateInstance(false, metadataSource);

            using (var client = new System.Net.Http.HttpClient())
            {
                var sIpfsUrl       = String.Format("{0}/{1}", CONST_INFURA_IPFS_GATEWAY_URL, "QmXcsGDQthxbGW8C3Sx9r4tV9PGSj4MxJmtXF7dnXN5XUT");
                var sRulesContents = client.GetStringAsync(sIpfsUrl).Result;

                _rulesEngine = new WonkaBizRulesEngine(new StringBuilder(sRulesContents), metadataSource);
            }

            _client = new Mock<IOrchestrate>();
        }

        [Fact]
        public void Metadata_NotEmptyCollection()
        {
            var collection = _refEnvHandle.AttrCache;

            // Assert
            Assert.NotEmpty(collection);
        }

        [Fact]
        public void RulesEngine_NotEmptyCollection()
        {
            var collection = _rulesEngine.RuleTreeRoot.ChildRuleSets;

            // Assert
            Assert.NotEmpty(collection);
        }

        [Fact]
        public void Metadata_SingleItem()
        {
            var collection = _refEnvHandle.AttrCache;

            // Assert
            Assert.True(collection.Exists(x => x.AttrName == "AuditReviewFlag"));
        }

        [Fact]
        public void RulesEngine_SingleItem()
        {
            var collection = _rulesEngine.RuleTreeRoot.ChildRuleSets;

            // Assert
            Assert.Single(collection);

            // Assert
            Assert.True(collection.Exists(x => x.Description.Contains("the incoming record")));
        }

        [Fact]
        public void Orchestrate_WithBadData()
        {
            bool bContractInitialized = false;

            try
            {
                var contract =
                    _client.Object.GetContract(new BizRulesEngine.RuleTree.WonkaBizSource("", "", "", "", "", "", "", null));

                bContractInitialized = true;
            }
            catch (Exception ex)
			{
                bContractInitialized = false;
			}

            Assert.False(bContractInitialized);
        }
    }
}