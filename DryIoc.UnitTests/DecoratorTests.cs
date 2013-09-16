﻿using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DecoratorTests
    {
        [Test]
        public void Should_resolve_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: FactorySetup.AsDecorator());

            var decorator = container.Resolve<IOperation>();

            Assert.That(decorator, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_resolve_decorator_of_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: FactorySetup.AsDecorator());
            container.Register<IOperation, RetryOperationDecorator>(setup: FactorySetup.AsDecorator());

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_resolve_decorator_for_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(named: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: FactorySetup.AsDecorator());

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.That(decorator.Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_NOT_cache_decorator_so_it_could_decorated_another_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(named: "Some");
            container.Register<IOperation, AnotherOperation>(named: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: FactorySetup.AsDecorator());

            var some = (RetryOperationDecorator)container.Resolve<IOperation>("Some");
            var another = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.That(some.Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(another.Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_resolve_generic_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: FactorySetup.AsDecorator());

            var decorator = container.Resolve<IOperation<string>>();

            Assert.That(decorator, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<string>>());
        }

        [Test]
        public void Should_resolve_closed_servise_with_open_generic_decorator()
        {
            var container = new Container();
            container.Register<IOperation<int>, SomeOperation<int>>();
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: FactorySetup.AsDecorator());

            var operation = container.Resolve<IOperation<int>>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: FactorySetup.AsDecorator());
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: FactorySetup.AsDecorator());

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_closed_decorator_of_generic_service()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: FactorySetup.AsDecorator());
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: FactorySetup.AsDecorator());

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Resolve_could_NOT_select_closed_over_generic_decorator_cause_their_are_not_related()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: FactorySetup.AsDecorator());
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: FactorySetup.AsDecorator());

            var decorator = (MeasureExecutionTimeOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: FactorySetup.AsDecorator());

            var ops = container.Resolve<IOperation[]>();

            Assert.That(ops[0], Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[0]).Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(ops[1], Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[1]).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_resolve_wrappers_of_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: FactorySetup.AsDecorator());

            var ops = container.Resolve<Lazy<IOperation>[]>();

            Assert.That(ops[0].Value, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[0].Value).Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(ops[1].Value, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[1].Value).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_NOT_support_decorator_implementation_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(setup: FactorySetup.AsDecorator());

            Assert.Throws<ContainerException>(() => 
                container.Resolve<IOperation>());
        }

        [Test]
        public void Should_support_decorating_of_wrapped_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, LazyDecorator>(setup: FactorySetup.AsDecorator());
            //container.Register<IOperation, LazyDecorator>(setup: new DecoratorSetup());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        //[Test]
        //public void Should_support_decorating_of_wrapped_named_service()
        //{
        //    var container = new Container();
        //    container.Register<IOperation, SomeOperation>(named: "some");
        //    container.Register<IOperation, LazyDecorator>(setup: FactorySetup.AsDecorator());

        //    var operation = container.Resolve<IOperation>("some");

        //    Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        //}

        //[Test]
        //public void Should_propogate_metadata_to_Meta_wrapper()
        //{
        //    var container = new Container();
        //    container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: FactorySetup.WithMetadata("blah"));
        //    container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: FactorySetup.AsDecorator());
        //    container.RegisterPublicTypes(typeof(OperationUser<>));

        //    var user = container.Resolve<OperationUser<object>>();

        //    Assert.That(user.GetOperation.Metadata, Is.EqualTo("blah"));
        //    Assert.That(user.GetOperation.Value(), Is.InstanceOf<MeasureExecutionTimeOperationDecorator<object>>());
        //}

        //[Test]
        //public void Resolving_Func_with_parameters_of_decorated_service_is_NOT_supported()
        //{
        //    var container = new Container();
        //    container.Register<IOperation, ParemeterizedOperation>();
        //    container.Register<IOperation, RetryOperationDecorator>(setup: FactorySetup.AsDecorator());

        //    Assert.Throws<ContainerException>(() =>
        //        container.Resolve<Func<object, IOperation>>());
        //}

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterInstance<Func<IOperation, IOperation>>(
                op => new MeasureExecutionTimeOperationDecorator(op), FactorySetup.AsDecorator());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service_with_additional_dependencies_resolved_from_Container()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IMeasurer, Measurer>();
            container.RegisterDelegate<Func<IOperation, IOperation>>(
                r => (service => MeasureExecutionTimeOperationDecorator.MeasureWith(service, r.Resolve<IMeasurer>())),
                setup: FactorySetup.AsDecorator());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }
    }

    #region CUT

    public class OperationUser<T>
    {
        public Meta<Func<IOperation<T>>, string> GetOperation { get; set; }

        public OperationUser(Meta<Func<IOperation<T>>, string> getOperation)
        {
            GetOperation = getOperation;
        }
    }

    public interface IOperation
    {
    }

    public class SomeOperation : IOperation
    {
    }

    public class AnotherOperation : IOperation
    {
    }

    public class ParemeterizedOperation : IOperation
    {
        public object Param { get; set; }

        public ParemeterizedOperation(object param)
        {
            Param = param;
        }
    }

    public class MeasureExecutionTimeOperationDecorator : IOperation
    {
        public IOperation DecoratedOperation;

        public MeasureExecutionTimeOperationDecorator(IOperation operation)
        {
            DecoratedOperation = operation;
        }

        public static IOperation MeasureWith(IOperation operation, IMeasurer measurer)
        {
            return new MeasureExecutionTimeOperationDecorator(operation) {Measurer = measurer};
        }

        public IMeasurer Measurer { get; set; }
    }

    public interface IMeasurer
    {
    }

    internal class Measurer : IMeasurer
    {
    }

    public class RetryOperationDecorator : IOperation
    {
        public IOperation Decorated;

        public RetryOperationDecorator(IOperation operation)
        {
            Decorated = operation;
        }
    }

    public interface IOperation<T>
    {
    }

    public class SomeOperation<T> : IOperation<T>
    {
    }

    public class RetryOperationDecorator<T> : IOperation<T>
    {
        public IOperation<T> Decorated;

        public RetryOperationDecorator(IOperation<T> operation)
        {
            Decorated = operation;
        }
    }

    public class MeasureExecutionTimeOperationDecorator<T> : IOperation<T>
    {
        public IOperation<T> Decorated;

        public MeasureExecutionTimeOperationDecorator(IOperation<T> operation)
        {
            Decorated = operation;
        }
    }

    public class LazyDecorator : IOperation
    {
        public Lazy<IOperation> Decorated;

        public LazyDecorator(Lazy<IOperation> decorated)
        {
            Decorated = decorated;
        }
    }

    #endregion
}