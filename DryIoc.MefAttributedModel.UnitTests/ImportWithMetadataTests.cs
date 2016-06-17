﻿using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportWithMetadataTests
    {
        [Test]
        public void It_should_throw_if_no_service_with_specified_metadata_found()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FooConsumerNotFound), typeof(FooHey), typeof(FooBlah));

            var ex = Assert.Throws<AttributedModelException>(() => 
                container.Resolve<FooConsumerNotFound>());
            Assert.AreEqual(ex.Error, Error.NotFindDependencyWithMetadata);
        }

        [Test, Ignore("#195 Composable Metadata as a IDictionary of string - object")]
        public void ImportMany_with_metadata_should_work()
        {
            var container = new Container(Rules.Default.WithMefAttributedModel());

            container.RegisterExports(typeof(RequiresManyOfMeta), typeof(SomeDep), typeof(BlahDep), typeof(HuhDep));

            var r = container.Resolve<RequiresManyOfMeta>();
            Assert.AreEqual(2, r.Deps.Count());
        }
    }
}
