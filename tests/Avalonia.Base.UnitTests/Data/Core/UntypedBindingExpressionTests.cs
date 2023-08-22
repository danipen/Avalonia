using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.UnitTests;
using Moq;
using Xunit;

namespace Avalonia.Base.UnitTests.Data.Core
{
    public class UntypedBindingExpressionTests : IClassFixture<InvariantCultureFixture>
    {
        [Fact]
        public async Task Should_Get_Source_Value()
        {
            var data = "foo";
            var target = UntypedBindingExpression.Create(data, o => o);
            var result = await target.Take(1);

            Assert.Equal("foo", result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Convert_String_To_Double()
        {
            var data = new Class1 { StringValue = $"{5.6}" };
            var target = UntypedBindingExpression.Create(data, o => o.StringValue, targetType: typeof(double));
            var result = await target.Take(1);

            Assert.Equal(5.6, result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Getting_Invalid_Double_String_Should_Return_BindingError()
        {
            var data = new Class1 { StringValue = "foo" };
            var target = UntypedBindingExpression.Create(data, o => o.StringValue, targetType: typeof(double));
            var result = await target.Take(1);

            Assert.IsType<BindingNotification>(result);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Convert_Set_String_To_Double()
        {
            var data = new Class1 { StringValue = $"{5.6}" };
            var target = UntypedBindingExpression.Create(data, o => o.StringValue, targetType: typeof(double));

            using (target.Subscribe(x => { }))
            {
                target.SetValue($"{6.7}");
            }

            Assert.Equal($"{6.7}", data.StringValue);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Convert_Double_To_String()
        {
            var data = new Class1 { DoubleValue = 5.6 };
            var target = UntypedBindingExpression.Create(data, o => o.DoubleValue, targetType: typeof(string));
            var result = await target.Take(1);

            Assert.Equal($"{5.6}", result);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Should_Convert_Set_Double_To_String()
        {
            var data = new Class1 { DoubleValue = 5.6 };
            var target = UntypedBindingExpression.Create(data, o => o.DoubleValue);

            using (target.Subscribe(x => { }))
            {
                target.SetValue($"{6.7}");
            }

            Assert.Equal(6.7, data.DoubleValue);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Return_BindingNotification_With_FallbackValue_For_NonConvertibe_Target_Value()
        {
            var data = new Class1 { StringValue = "foo" };
            var target = UntypedBindingExpression.Create(
                data, 
                o => o.StringValue,
                fallbackValue: 42,
                targetType: typeof(int));
            var result = await target.Take(1);

            Assert.Equal(
                new BindingNotification(
                    new InvalidCastException("Cannot convert 'foo' (System.String) to 'System.Int32'."),
                    BindingErrorType.Error,
                    42),
                result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Return_BindingNotification_With_FallbackValue_For_NonConvertibe_Target_Value_With_Data_Validation()
        {
            var data = new Class1 { StringValue = "foo" };
            var target = UntypedBindingExpression.Create(
                data, 
                o => o.StringValue,
                enableDataValidation: true,
                fallbackValue: 42,
                targetType: typeof(int));
            var result = await target.Take(1);

            Assert.Equal(
                new BindingNotification(
                    new InvalidCastException("Cannot convert 'foo' (System.String) to 'System.Int32'."),
                    BindingErrorType.Error,
                    42),
                result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Return_BindingNotification_For_Invalid_FallbackValue()
        {
            var data = new Class1 { StringValue = "foo" };
            var target = UntypedBindingExpression.Create(
                data, 
                o => o.StringValue,
                fallbackValue: "bar",
                targetType: typeof(int));
            var result = await target.Take(1);

            Assert.Equal(
                new BindingNotification(
                    new AggregateException(
                        new InvalidCastException("Cannot convert 'foo' (System.String) to 'System.Int32'."),
                        new InvalidCastException("Cannot convert fallback value 'bar' (System.String) to 'System.Int32'.")),
                    BindingErrorType.Error),
                result);

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Should_Return_BindingNotification_For_Invalid_FallbackValue_With_Data_Validation()
        {
            var data = new Class1 { StringValue = "foo" };
            var target = UntypedBindingExpression.Create(
                data, 
                o => o.StringValue,
                enableDataValidation: true,
                fallbackValue: "bar",
                targetType: typeof(int));
            var result = await target.Take(1);

            Assert.Equal(
                new BindingNotification(
                    new AggregateException(
                        new InvalidCastException("Cannot convert 'foo' (System.String) to 'System.Int32'."),
                        new InvalidCastException("Cannot convert fallback value 'bar' (System.String) to 'System.Int32'.")),
                    BindingErrorType.Error),
                result);

            GC.KeepAlive(data);
        }

        [Fact]
        public void Setting_Invalid_Double_String_Should_Not_Change_Target()
        {
            var data = new Class1 { DoubleValue = 5.6 };
            var target = UntypedBindingExpression.Create(data, o => o.DoubleValue, targetType: typeof(string));

            target.SetValue("foo");

            Assert.Equal(5.6, data.DoubleValue);

            GC.KeepAlive(data);
        }

        ////[Fact]
        ////public void Setting_Invalid_Double_String_Should_Use_FallbackValue()
        ////{
        ////    var data = new Class1 { DoubleValue = 5.6 };
        ////    var target = new BindingExpression(
        ////        UntypedBindingExpression.Create(data, o => o.DoubleValue),
        ////        typeof(string),
        ////        "9.8",
        ////        AvaloniaProperty.UnsetValue,
        ////        DefaultValueConverter.Instance);

        ////    target.OnNext("foo");

        ////    Assert.Equal(9.8, data.DoubleValue);

        ////    GC.KeepAlive(data);
        ////}

        ////[Fact]
        ////public void Should_Coerce_Setting_UnsetValue_Double_To_Default_Value()
        ////{
        ////    var data = new Class1 { DoubleValue = 5.6 };
        ////    var target = new BindingExpression(UntypedBindingExpression.Create(data, o => o.DoubleValue), typeof(string));

        ////    target.OnNext(AvaloniaProperty.UnsetValue);

        ////    Assert.Equal(0, data.DoubleValue);

        ////    GC.KeepAlive(data);
        ////}

        [Fact]
        public void Should_Pass_ConverterParameter_To_Convert()
        {
            var data = new Class1 { DoubleValue = 5.6 };
            var converter = new Mock<IValueConverter>();

            var target = UntypedBindingExpression.Create(
                data, 
                o => o.DoubleValue,                
                converter: converter.Object,
                converterParameter: "foo",
                targetType: typeof(string));

            target.Subscribe(_ => { });

            converter.Verify(x => x.Convert(5.6, typeof(string), "foo", CultureInfo.CurrentCulture));

            GC.KeepAlive(data);
        }

        ////[Fact]
        ////public void Should_Pass_ConverterParameter_To_ConvertBack()
        ////{
        ////    var data = new Class1 { DoubleValue = 5.6 };
        ////    var converter = new Mock<IValueConverter>();
        ////    var target = UntypedBindingExpression.Create(
        ////        data, 
        ////        o => o.DoubleValue,
        ////        converter: converter.Object,
        ////        converterParameter: "foo",
        ////        targetType: typeof(string));

        ////    target.SetValue("bar");

        ////    converter.Verify(x => x.ConvertBack("bar", typeof(double), "foo", CultureInfo.CurrentCulture));

        ////    GC.KeepAlive(data);
        ////}

        ////[Fact]
        ////public void Should_Handle_DataValidation()
        ////{
        ////    var data = new Class1 { DoubleValue = 5.6 };
        ////    var converter = new Mock<IValueConverter>();
        ////    var target = UntypedBindingExpression.Create(
        ////        data, 
        ////        o => o.DoubleValue, 
        ////        enableDataValidation: true,
        ////        targetType: typeof(string));
        ////    var result = new List<object>();

        ////    target.Subscribe(x => result.Add(x));
        ////    target.SetValue(1.2);
        ////    target.SetValue($"{3.4}");
        ////    target.SetValue("bar");

        ////    Assert.Equal(
        ////        new[]
        ////        {
        ////            new BindingNotification($"{5.6}"),
        ////            new BindingNotification($"{1.2}"),
        ////            new BindingNotification($"{3.4}"),
        ////            new BindingNotification(
        ////                new InvalidCastException("'bar' is not a valid number."),
        ////                BindingErrorType.Error)
        ////        },
        ////        result);

        ////    GC.KeepAlive(data);
        ////}

        [Fact]
        public void Null_Value_Should_Use_TargetNullValue()
        {
            var data = new Class1 { StringValue = "foo" };

            var target = UntypedBindingExpression.Create(
                data, 
                o => o.StringValue,
                targetNullValue: "bar",
                targetType: typeof(string));

            object result = null;
            target.Subscribe(x => result = x);

            Assert.Equal("foo", result);

            data.StringValue = null;
            Assert.Equal("bar", result);

            GC.KeepAlive(data);
        }

        private class Class1 : NotifyingBase
        {
            private string _stringValue;
            private double _doubleValue;

            public string StringValue
            {
                get { return _stringValue; }
                set { _stringValue = value; RaisePropertyChanged(); }
            }

            public double DoubleValue
            {
                get { return _doubleValue; }
                set { _doubleValue = value; RaisePropertyChanged(); }
            }
        }
    }
}
