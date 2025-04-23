using System;
using System.Globalization;
using Linguini.Shared.Util;

#nullable enable
namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a number in double precision
    /// </summary>
    public record FluentNumber : IFluentType
    {
        public readonly double Value;
        private readonly FluentNumberOptions _options;

        private FluentNumber(double value, FluentNumberOptions options)
        {
            Value = value;
            _options = options;
        }

        public string AsString()
        {
            var stringVal = Value.ToString(CultureInfo.InvariantCulture);
            if (_options.MinimumFractionDigits != null)
            {
                var minfd = _options.MinimumFractionDigits.Value;
                var pos = stringVal.IndexOf('.');
                if (pos != -1)
                {
                    var fracNum = stringVal.Length - pos - 1;
                    var missing = fracNum > minfd
                        ? 0
                        : minfd - fracNum;
                    var pattern = new String('0', missing);
                    stringVal = $"{stringVal}{pattern}";
                }
                else
                {
                    stringVal = $"{stringVal}.{new String('0', minfd)}";
                }
            }

            return stringVal;
        }

        public bool IsError()
        {
            return false;
        }

        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
        }

        public static FluentNumber FromString(ReadOnlySpan<char> input)
        {
            var parsed = Double.Parse(input.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            var options = new FluentNumberOptions();
            if (input.IndexOf('.') != -1)
            {
                options.MinimumFractionDigits = input.Length - input.IndexOf('.') - 1;
            }
            return new FluentNumber(parsed, options);
        }

        public static FluentNumber FromString(string input)
        {
            return FromString(input.AsSpan());
        }

        public static IFluentType TryNumber(ReadOnlySpan<char> valueSpan)
        {
            try
            {
                return FromString(valueSpan);
            }
            catch (Exception)
            {
                return new FluentString(valueSpan);
            }
        }

        public static implicit operator double(FluentNumber fs) => fs.Value;
        public static implicit operator FluentNumber(double db) => new(db, new FluentNumberOptions());
        public static implicit operator FluentNumber(float fl) => new(fl, new FluentNumberOptions());

        public IFluentType Copy()
        {
            return new FluentNumber(Value, _options);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public record FluentNumberOptions
    {
        public FluentNumberStyle Style;
        public string? Currency;
        public FluentNumberCurrencyDisplayStyle CurrencyDisplayStyle;
        public bool UseGrouping;
        public int? MinimumIntegerDigits;
        public int? MinimumFractionDigits;
        public int? MaximumFractionDigits;
        public int? MinimumSignificantDigits;
        public int? MaximumSignificantDigits;

        public FluentNumberOptions()
        {
            Style = FluentNumberStyle.Decimal;
            Currency = null;
            CurrencyDisplayStyle = FluentNumberCurrencyDisplayStyle.Symbol;
            UseGrouping = true;
            MinimumIntegerDigits = null;
            MinimumFractionDigits = null;
            MaximumFractionDigits = null;
            MinimumSignificantDigits = null;
            MaximumSignificantDigits = null;
        }
    }

    public enum FluentNumberStyle
    {
        Decimal,
        Currency,
        Percent,
    }

    public enum FluentNumberCurrencyDisplayStyle
    {
        Symbol,
        Code,
        Name,
    }
}
