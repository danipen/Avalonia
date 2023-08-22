using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Data.Core;
using Xunit;

#nullable enable

namespace Avalonia.Base.UnitTests.Data.Core
{
    public class UntypedBindingExpressionTests_Negation
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Negate_Boolean_Value(bool value)
        {
            var data = new Test { Foo = value };
            var target = UntypedBindingExpression.Create(data, o => !o.Foo);
            var result = await target.Take(1);

            Assert.Equal(!value, (bool)result);

            GC.KeepAlive(data);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Negate_Boolean_Value_In_Path(bool value)
        {
            var data = new Test { Next = new Test { Foo = value } };
            var target = UntypedBindingExpression.Create(data, o => !o.Next!.Foo);
            var result = await target.Take(1);

            Assert.Equal(!value, (bool)result);

            GC.KeepAlive(data);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Double_Negate_Boolean_Value(bool value)
        {
            var data = new Test { Foo = value };
            var target = UntypedBindingExpression.Create(data, o => !!o.Foo);
            var result = await target.Take(1);

            Assert.Equal(value, (bool)result);

            GC.KeepAlive(data);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Double_Negate_Boolean_Value_In_Path(bool value)
        {
            var data = new Test { Next = new Test { Foo = value } };
            var target = UntypedBindingExpression.Create(data, o => !!o.Next!.Foo);
            var result = await target.Take(1);

            Assert.Equal(value, (bool)result);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Can_Set_Negated_Value()
        {
            var data = new Test { Foo = true };
            var target = UntypedBindingExpression.Create(data, o => !o.Foo);
            target.Subscribe(_ => { });

            Assert.True(target.SetValue(true));

            Assert.False(data.Foo);
        }

        [Fact]
        public void Can_Set_Negated_Value_In_Path()
        {
            var data = new Test { Next = new Test { Foo = true } };
            var target = UntypedBindingExpression.Create(data, o => !o.Next!.Foo);
            target.Subscribe(_ => { });

            Assert.True(target.SetValue(true));

            Assert.False(data.Next.Foo);
        }

        [Fact]
        public void Can_Set_Double_Negated_Value()
        {
            var data = new Test { Foo = true };
            var target = UntypedBindingExpression.Create(data, o => !!o.Foo);
            target.Subscribe(_ => { });

            Assert.True(target.SetValue(false));

            Assert.False(data.Foo);
        }

        [Fact]
        public void Can_Set_Double_Negated_Value_In_Path()
        {
            var data = new Test { Next = new Test { Foo = true } };
            var target = UntypedBindingExpression.Create(data, o => !!o.Next!.Foo);
            target.Subscribe(_ => { });

            Assert.True(target.SetValue(false));

            Assert.False(data.Next.Foo);
        }

        private class Test
        {
            public bool Foo { get; set; }
            public Test? Next { get; set; }
        }
    }
}
