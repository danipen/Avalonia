using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Core.ExpressionNodes.Reflection;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Threading;
using Avalonia.UnitTests;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Avalonia.Base.UnitTests.Data.Core
{
    public class UntypedBindingExpressionTests_Property
    {
        [Fact]
        public async Task Should_Get_Simple_Property_Value()
        {
            var data = new { Foo = "foo" };
            var target = UntypedBindingExpression.Create(data, o => o.Foo);
            var result = await target.Take(1);

            Assert.Equal("foo", result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Get_Simple_Property_Value_Null()
        {
            var data = new { Foo = (string)null };
            var target = UntypedBindingExpression.Create(data, o => o.Foo);
            var result = await target.Take(1);

            Assert.Null(result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Get_Simple_Property_From_Base_Class()
        {
            var data = new Class3 { Foo = "foo" };
            var target = UntypedBindingExpression.Create(data, o => o.Foo);
            var result = await target.Take(1);

            Assert.Equal("foo", result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Return_BindingNotification_Error_For_Root_Null()
        {
            var target = UntypedBindingExpression.Create(default(Class3), o => o.Foo);
            var result = await target.Take(1);

            Assert.Equal(
                new BindingNotification(
                        new BindingChainException("Value is null.", "Foo", string.Empty),
                        BindingErrorType.Error,
                        AvaloniaProperty.UnsetValue),
                result);
        }

        [Fact]
        public async Task Should_Return_BindingNotification_Error_For_Root_UnsetValue()
        {
            var target = UntypedBindingExpression.Create(AvaloniaProperty.UnsetValue, o => (o as Class3).Foo);
            var result = await target.Take(1);

            Assert.Equal(
                new BindingNotification(
                        new BindingChainException("Value is null.", "Foo", string.Empty),
                        BindingErrorType.Error,
                        AvaloniaProperty.UnsetValue),
                result);
        }

        [Fact]
        public async Task Should_Get_Simple_Property_Chain()
        {
            var data = new { Foo = new { Bar = new { Baz = "baz" } } };
            var target = UntypedBindingExpression.Create(data, o => o.Foo.Bar.Baz);
            var result = await target.Take(1);

            Assert.Equal("baz", result);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Return_BindingNotification_Error_For_Chain_With_Null_Value()
        {
            var data = new { Foo = default(Class1) };
            var target = UntypedBindingExpression.Create(data, o => o.Foo.Foo.Length);
            var result = new List<object>();

            target.Subscribe(x => result.Add(x));

            Assert.Equal(
                new[]
                {
                            new BindingNotification(
                                new BindingChainException("Value is null.", "Foo.Foo.Length", "Foo"),
                                BindingErrorType.Error,
                                AvaloniaProperty.UnsetValue),
                },
                result);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Track_Simple_Property_Value()
        {
            var data = new Class1 { Foo = "foo" };
            var target = UntypedBindingExpression.Create(data, o => o.Foo);
            var result = new List<object>();

            var sub = target.Subscribe(x => result.Add(x));
            data.Foo = "bar";

            Assert.Equal(new[] { "foo", "bar" }, result);

            sub.Dispose();

            // Forces WeakEvent compact
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, data.PropertyChangedSubscriptionCount);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Trigger_PropertyChanged_On_Null_Or_Empty_String()
        {
            var data = new Class1 { Bar = "foo" };
            var target = UntypedBindingExpression.Create(data, o => o.Bar);
            var result = new List<object>();

            var sub = target.Subscribe(x => result.Add(x));

            Assert.Equal(new[] { "foo" }, result);

            data.Bar = "bar";

            Assert.Equal(new[] { "foo" }, result);

            data.RaisePropertyChanged(string.Empty);

            Assert.Equal(new[] { "foo", "bar" }, result);

            data.SetBarWithoutRaising("baz");

            Assert.Equal(new[] { "foo", "bar" }, result);

            data.RaisePropertyChanged(null);

            Assert.Equal(new[] { "foo", "bar", "baz" }, result);

            sub.Dispose();

            // Forces WeakEvent compact
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, data.PropertyChangedSubscriptionCount);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Track_End_Of_Property_Chain_Changing()
        {
            var data = new Class1 { Next = new Class2 { Bar = "bar" } };
            var target = UntypedBindingExpression.Create(data, o => (o.Next as Class2).Bar);
            var result = new List<object>();

            var sub = target.Subscribe(x => result.Add(x));
            ((Class2)data.Next).Bar = "baz";
            ((Class2)data.Next).Bar = null;

            Assert.Equal(new[] { "bar", "baz", null }, result);

            sub.Dispose();
            // Forces WeakEvent compact
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, data.PropertyChangedSubscriptionCount);
            Assert.Equal(0, data.Next.PropertyChangedSubscriptionCount);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Track_Property_Chain_Changing()
        {
            var data = new Class1 { Next = new Class2 { Bar = "bar" } };
            var target = UntypedBindingExpression.Create(data, o => (o.Next as Class2).Bar);
            var result = new List<object>();

            var sub = target.Subscribe(x => result.Add(x));
            var old = data.Next;
            data.Next = new Class2 { Bar = "baz" };
            data.Next = new Class2 { Bar = null };

            Assert.Equal(new[] { "bar", "baz", null }, result);

            sub.Dispose();

            // Forces WeakEvent compact
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, data.PropertyChangedSubscriptionCount);
            Assert.Equal(0, data.Next.PropertyChangedSubscriptionCount);
            Assert.Equal(0, old.PropertyChangedSubscriptionCount);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Track_Property_Chain_Breaking_With_Null_Then_Mending()
        {
            var data = new Class1
            {
                Next = new Class2
                {
                    Next = new Class2
                    {
                        Bar = "bar"
                    }
                }
            };

            var target = UntypedBindingExpression.Create(data, o => ((o.Next as Class2).Next as Class2).Bar);
            var result = new List<object>();

            var sub = target.Subscribe(x => result.Add(x));
            var old = data.Next;
            data.Next = new Class2 { Bar = "baz" };
            data.Next = old;

            Assert.Equal(
                new object[]
                {
                            "bar",
                            new BindingNotification(
                                new BindingChainException("Value is null.", "Next.Next.Bar", "Next.Next"),
                                BindingErrorType.Error,
                                AvaloniaProperty.UnsetValue),
                            "bar"
                },
                result);

            sub.Dispose();

            // Forces WeakEvent compact
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, data.PropertyChangedSubscriptionCount);
            Assert.Equal(0, data.Next.PropertyChangedSubscriptionCount);
            Assert.Equal(0, old.PropertyChangedSubscriptionCount);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Track_Property_Chain_Breaking_With_Missing_Member_Then_Mending()
        {
            var data = new Class1 { Next = new Class2 { Bar = "bar" } };
            var target = UntypedBindingExpression.Create(data, o => (o.Next as Class2).Bar);
            var result = new List<object>();

            var sub = target.Subscribe(x => result.Add(x));
            var old = data.Next;
            var breaking = new WithoutBar();
            data.Next = breaking;
            data.Next = new Class2 { Bar = "baz" };

            Assert.Equal(
                new object[]
                {
                            "bar",
                            new BindingNotification(
                                new BindingChainException(
                                    $"Could not find a matching property accessor for '{nameof(Class2.Bar)}' on '{typeof(WithoutBar)}'",
                                    "Next.Bar",
                                    "Next.Bar"),
                                BindingErrorType.Error),
                            "baz",
                },
                result);

            sub.Dispose();

            // Forces WeakEvent compact
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0, data.PropertyChangedSubscriptionCount);
            Assert.Equal(0, data.Next.PropertyChangedSubscriptionCount);
            Assert.Equal(0, breaking.PropertyChangedSubscriptionCount);
            Assert.Equal(0, old.PropertyChangedSubscriptionCount);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Not_Keep_Source_Alive()
        {
            Func<Tuple<UntypedBindingExpression, WeakReference>> run = () =>
            {
                var source = new Class1 { Foo = "foo" };
                var target = UntypedBindingExpression.Create(source, o => o.Foo);
                return Tuple.Create(target, new WeakReference(source));
            };

            var result = run();
            result.Item1.Subscribe(x => { });

            // Mono trickery
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.WaitForPendingFinalizers();
            GC.Collect(2);


            Assert.Null(result.Item2.Target);
        }

        [Fact]
        public void Should_Not_Throw_Exception_On_Duplicate_Properties()
        {
            // Repro of https://github.com/AvaloniaUI/Avalonia/issues/4733.
            var source = new MyViewModel();
            var target = UntypedBindingExpression.Create(source, x => x.Name);
            var result = new List<object>();

            target.Subscribe(x => result.Add(x));

            Assert.Equal(new[] { "NewName" }, result);
        }

        public class MyViewModelBase { public object Name => "Name"; }

        public class MyViewModel : MyViewModelBase { public new string Name => "NewName"; }

        private interface INext
        {
            int PropertyChangedSubscriptionCount { get; }
        }

        private class Class1 : NotifyingBase
        {
            private string _foo;
            private INext _next;

            public string Foo
            {
                get { return _foo; }
                set
                {
                    _foo = value;
                    RaisePropertyChanged(nameof(Foo));
                }
            }

            private string _bar;
            public string Bar
            {
                get { return _bar; }
                set { _bar = value; }
            }

            public INext Next
            {
                get { return _next; }
                set
                {
                    _next = value;
                    RaisePropertyChanged(nameof(Next));
                }
            }

            public void SetBarWithoutRaising(string value) => _bar = value;
        }

        private class Class2 : NotifyingBase, INext
        {
            private string _bar;
            private INext _next;

            public string Bar
            {
                get { return _bar; }
                set
                {
                    _bar = value;
                    RaisePropertyChanged(nameof(Bar));
                }
            }

            public INext Next
            {
                get { return _next; }
                set
                {
                    _next = value;
                    RaisePropertyChanged(nameof(Next));
                }
            }
        }

        private class Class3 : Class1
        {
        }

        private class WithoutBar : NotifyingBase, INext
        {
        }

        private static Recorded<Notification<T>> OnNext<T>(long time, T value)
        {
            return new Recorded<Notification<T>>(time, Notification.CreateOnNext<T>(value));
        }
    }
}
