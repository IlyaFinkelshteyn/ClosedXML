using System;
using System.Collections.Generic;
using System.Linq;

namespace ClosedXML.Excel
{
    internal class XLConditionalFormat: IXLConditionalFormat, IXLStylized
    {
        private sealed class FullEqualityComparer : IEqualityComparer<IXLConditionalFormat>
        {
            private readonly bool _compareRange;
            private readonly DictionaryComparer<int, XLColor> _colorsComparer = new DictionaryComparer<int, XLColor>();
            private readonly EnumerableComparer<string> _listComparer = new EnumerableComparer<string>();
            private readonly DictionaryComparer<int, XLCFContentType> _contentsTypeComparer = new DictionaryComparer<int, XLCFContentType>();
            private readonly DictionaryComparer<int, XLCFIconSetOperator> _iconSetTypeComparer = new DictionaryComparer<int, XLCFIconSetOperator>();

            public FullEqualityComparer(bool compareRange)
            {
                _compareRange = compareRange;
            }

            public bool Equals(IXLConditionalFormat x, IXLConditionalFormat y)
            {
                var xx = (XLConditionalFormat) x;
                var yy = (XLConditionalFormat) y;
                if (ReferenceEquals(xx, yy)) return true;
                if (ReferenceEquals(xx, null)) return false;
                if (ReferenceEquals(yy, null)) return false;
                if (xx.GetType() != yy.GetType()) return false;

                var xxValues = xx.Values.Values.Where(v => !v.IsFormula).Select(v=>v.Value);
                var yyValues = yy.Values.Values.Where(v => !v.IsFormula).Select(v => v.Value);
                var xxFormulas = xx.Values.Values.Where(v => v.IsFormula).Select(f => ((XLCell)x.Range.FirstCell()).GetFormulaR1C1(f.Value));
                var yyFormulas = yy.Values.Values.Where(v => v.IsFormula).Select(f => ((XLCell)y.Range.FirstCell()).GetFormulaR1C1(f.Value));

                return Equals(xx._style, yy._style) 
                    && xx._styleCacheId == yy._styleCacheId 
                    && xx.CopyDefaultModify == yy.CopyDefaultModify 
                    && xx.UpdatingStyle == yy.UpdatingStyle
                    && xx.ConditionalFormatType == yy.ConditionalFormatType 
                    && xx.TimePeriod == yy.TimePeriod 
                    && xx.IconSetStyle == yy.IconSetStyle 
                    && xx.Operator == yy.Operator 
                    && xx.Bottom == yy.Bottom 
                    && xx.Percent == yy.Percent 
                    && xx.ReverseIconOrder == yy.ReverseIconOrder 
                    && xx.ShowIconOnly == yy.ShowIconOnly 
                    && xx.ShowBarOnly == yy.ShowBarOnly
                    && _listComparer.Equals(xxValues, yyValues)
                    && _listComparer.Equals(xxFormulas, yyFormulas)
                    && _colorsComparer.Equals(xx.Colors, yy.Colors)
                    && _contentsTypeComparer.Equals(xx.ContentTypes, yy.ContentTypes)
                    && _iconSetTypeComparer.Equals(xx.IconSetOperators, yy.IconSetOperators)
                    && (!_compareRange || Equals(xx.Range.RangeAddress, yy.Range.RangeAddress)) ;
            }

            public int GetHashCode(IXLConditionalFormat obj)
            {
                throw new NotImplementedException();
            }
        }

        private static readonly IEqualityComparer<IXLConditionalFormat> FullComparerInstance = new FullEqualityComparer(true);
        public static IEqualityComparer<IXLConditionalFormat> FullComparer
        {
            get { return FullComparerInstance; }
        }

        private static readonly IEqualityComparer<IXLConditionalFormat> NoRangeComparerInstance = new FullEqualityComparer(false);
        public static IEqualityComparer<IXLConditionalFormat> NoRangeComparer
        {
            get { return NoRangeComparerInstance; }
        }

        public XLConditionalFormat(XLRange range, Boolean copyDefaultModify = false)
        {
            Range = range;
            Style = new XLStyle(this, range.Worksheet.Style);
            Values = new XLDictionary<XLFormula>();
            Colors = new XLDictionary<XLColor>();
            ContentTypes = new XLDictionary<XLCFContentType>();
            IconSetOperators = new XLDictionary<XLCFIconSetOperator>();
            CopyDefaultModify = copyDefaultModify;
        }
        public XLConditionalFormat(XLConditionalFormat other)
        {
            Range = other.Range;
            Style = new XLStyle(this, other.Style);
            Values = new XLDictionary<XLFormula>(other.Values);
            Colors = new XLDictionary<XLColor>(other.Colors);
            ContentTypes = new XLDictionary<XLCFContentType>(other.ContentTypes);
            IconSetOperators = new XLDictionary<XLCFIconSetOperator>(other.IconSetOperators);


            ConditionalFormatType = other.ConditionalFormatType;
            TimePeriod = other.TimePeriod;
            IconSetStyle = other.IconSetStyle;
            Operator = other.Operator;
            Bottom = other.Bottom;
            Percent = other.Percent;
            ReverseIconOrder = other.ReverseIconOrder;
            ShowIconOnly = other.ShowIconOnly;
            ShowBarOnly = other.ShowBarOnly;
        }

        public Boolean CopyDefaultModify { get; set; }
        private IXLStyle _style;
        private Int32 _styleCacheId;
        public IXLStyle Style{ get { return GetStyle(); } set { SetStyle(value); } }
        private IXLStyle GetStyle()
        {
            //return _style;
            if (_style != null)
                return _style;

            return _style = new XLStyle(this, Range.Worksheet.Workbook.GetStyleById(_styleCacheId), CopyDefaultModify);
        }
        private void SetStyle(IXLStyle styleToUse)
        {
            //_style = new XLStyle(this, styleToUse);
            _styleCacheId = Range.Worksheet.Workbook.GetStyleId(styleToUse);
            _style = null;
            StyleChanged = false;
        }

        public IEnumerable<IXLStyle> Styles
        {
            get
            {
                UpdatingStyle = true;
                yield return Style;
                UpdatingStyle = false;
            }
        }

        public bool UpdatingStyle { get; set; }

        public IXLStyle InnerStyle { get; set; }

        public IXLRanges RangesUsed
        {
            get { return new XLRanges(); }
        }

        public bool StyleChanged { get; set; }
        public XLDictionary<XLFormula> Values { get; private set; }
        public XLDictionary<XLColor> Colors { get; private set; }
        public XLDictionary<XLCFContentType> ContentTypes { get; private set; }
        public XLDictionary<XLCFIconSetOperator> IconSetOperators { get; private set; }

        public IXLRange Range { get; set; }
        public XLConditionalFormatType ConditionalFormatType { get; set; }
        public XLTimePeriod TimePeriod { get; set; }
        public XLIconSetStyle IconSetStyle { get; set; }
        public XLCFOperator Operator { get;  set; }
        public Boolean Bottom { get;  set; }
        public Boolean Percent { get;  set; }
        public Boolean ReverseIconOrder { get;  set; }
        public Boolean ShowIconOnly { get;  set; }
        public Boolean ShowBarOnly { get;  set; }

        public void CopyFrom(IXLConditionalFormat other)
        {
            Style = other.Style;
            ConditionalFormatType = other.ConditionalFormatType;
            TimePeriod = other.TimePeriod;
            IconSetStyle = other.IconSetStyle;
            Operator = other.Operator;
            Bottom = other.Bottom;
            Percent = other.Percent;
            ReverseIconOrder = other.ReverseIconOrder;
            ShowIconOnly = other.ShowIconOnly;
            ShowBarOnly = other.ShowBarOnly;

            Values.Clear();
            other.Values.ForEach(kp => Values.Add(kp.Key, new XLFormula(kp.Value)));
            //CopyDictionary(Values, other.Values);
            CopyDictionary(Colors, other.Colors);
            CopyDictionary(ContentTypes, other.ContentTypes);
            CopyDictionary(IconSetOperators, other.IconSetOperators);
        }

        private void CopyDictionary<T>(XLDictionary<T> target, XLDictionary<T> source)
        {
            target.Clear();
            source.ForEach(kp => target.Add(kp.Key, kp.Value));
        }

        public IXLStyle WhenIsBlank()
        {
            ConditionalFormatType = XLConditionalFormatType.IsBlank;
            return Style;
        }
        public IXLStyle WhenNotBlank()
        {
            ConditionalFormatType = XLConditionalFormatType.NotBlank;
            return Style;
        }
        public IXLStyle WhenIsError()
        {
            ConditionalFormatType = XLConditionalFormatType.IsError;
            return Style;
        }
        public IXLStyle WhenNotError()
        {
            ConditionalFormatType = XLConditionalFormatType.NotError;
            return Style;
        }
        public IXLStyle WhenDateIs(XLTimePeriod timePeriod)
        {
            TimePeriod = timePeriod;
            ConditionalFormatType = XLConditionalFormatType.TimePeriod;
            return Style;
        }
        public IXLStyle WhenContains(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            ConditionalFormatType = XLConditionalFormatType.ContainsText;
            Operator = XLCFOperator.Contains;
            return Style;
        }
        public IXLStyle WhenNotContains(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            ConditionalFormatType = XLConditionalFormatType.NotContainsText;
            Operator = XLCFOperator.NotContains;
            return Style;
        }
        public IXLStyle WhenStartsWith(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            ConditionalFormatType = XLConditionalFormatType.StartsWith;
            Operator = XLCFOperator.StartsWith;
            return Style;
        }
        public IXLStyle WhenEndsWith(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            ConditionalFormatType = XLConditionalFormatType.EndsWith;
            Operator = XLCFOperator.EndsWith;
            return Style;
        }

        public IXLStyle WhenEquals(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            Operator = XLCFOperator.Equal;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenNotEquals(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            Operator = XLCFOperator.NotEqual;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenGreaterThan(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            Operator = XLCFOperator.GreaterThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenLessThan(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            Operator = XLCFOperator.LessThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenEqualOrGreaterThan(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            Operator = XLCFOperator.EqualOrGreaterThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenEqualOrLessThan(String value)
        {
            Values.Initialize(new XLFormula { Value = value });
            Operator = XLCFOperator.EqualOrLessThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenBetween(String minValue, String maxValue)
        {
            Values.Initialize(new XLFormula { Value = minValue });
            Values.Add(new XLFormula { Value = maxValue });
            Operator = XLCFOperator.Between;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenNotBetween(String minValue, String maxValue)
        {
            Values.Initialize(new XLFormula { Value = minValue });
            Values.Add(new XLFormula { Value = maxValue });
            Operator = XLCFOperator.NotBetween;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }

        public IXLStyle WhenEquals(Double value)
        {
            Values.Initialize(new XLFormula(value));
            Operator = XLCFOperator.Equal;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenNotEquals(Double value)
        {
            Values.Initialize(new XLFormula(value));
            Operator = XLCFOperator.NotEqual;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenGreaterThan(Double value)
        {
            Values.Initialize(new XLFormula(value));
            Operator = XLCFOperator.GreaterThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenLessThan(Double value)
        {
            Values.Initialize(new XLFormula(value));
            Operator = XLCFOperator.LessThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenEqualOrGreaterThan(Double value)
        {
            Values.Initialize(new XLFormula(value));
            Operator = XLCFOperator.EqualOrGreaterThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenEqualOrLessThan(Double value)
        {
            Values.Initialize(new XLFormula(value));
            Operator = XLCFOperator.EqualOrLessThan;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenBetween(Double minValue, Double maxValue)
    {
            Values.Initialize(new XLFormula (minValue));
            Values.Add(new XLFormula(maxValue));
            Operator = XLCFOperator.Between;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }
        public IXLStyle WhenNotBetween(Double minValue, Double maxValue)
        {
            Values.Initialize(new XLFormula(minValue));
            Values.Add(new XLFormula(maxValue));
            Operator = XLCFOperator.NotBetween;
            ConditionalFormatType = XLConditionalFormatType.CellIs;
            return Style;
        }

        public IXLStyle WhenIsDuplicate()
        {
            ConditionalFormatType = XLConditionalFormatType.IsDuplicate;
            return Style;
        }
        public IXLStyle WhenIsUnique()
        {
            ConditionalFormatType = XLConditionalFormatType.IsUnique;
            return Style;
        }
        public IXLStyle WhenIsTrue(String formula)
        {
            String f = formula.TrimStart()[0] == '=' ? formula : "=" + formula;
            Values.Initialize(new XLFormula {Value = f});
            ConditionalFormatType = XLConditionalFormatType.Expression;
            return Style;
        }
        public IXLStyle WhenIsTop(Int32 value, XLTopBottomType topBottomType = XLTopBottomType.Items)
        {
            Values.Initialize(new XLFormula(value));
            Percent = topBottomType == XLTopBottomType.Percent;
            ConditionalFormatType = XLConditionalFormatType.Top10;
            Bottom = false;
            return Style;
        }
        public IXLStyle WhenIsBottom(Int32 value, XLTopBottomType topBottomType = XLTopBottomType.Items)
        {
            Values.Initialize(new XLFormula(value));
            Percent = topBottomType == XLTopBottomType.Percent;
            ConditionalFormatType = XLConditionalFormatType.Top10;
            Bottom = true;
            return Style;
        }
        
        public IXLCFColorScaleMin ColorScale()
        {
            ConditionalFormatType = XLConditionalFormatType.ColorScale;
            return new XLCFColorScaleMin(this);
        }
        public IXLCFDataBarMin DataBar(XLColor color, Boolean showBarOnly = false)
        {
            Colors.Initialize(color);
            ShowBarOnly = showBarOnly;
            ConditionalFormatType = XLConditionalFormatType.DataBar;
            return new XLCFDataBarMin(this);
        }
        public IXLCFIconSet IconSet(XLIconSetStyle iconSetStyle, Boolean reverseIconOrder = false, Boolean showIconOnly = false)
        {
            IconSetOperators.Clear();
            Values.Clear();
            ContentTypes.Clear();
            ConditionalFormatType = XLConditionalFormatType.IconSet;
            IconSetStyle = iconSetStyle;
            ReverseIconOrder = reverseIconOrder;
            ShowIconOnly = showIconOnly;
            return new XLCFIconSet(this);
        }
    }

    public class DictionaryComparer<TKey, TValue> :
        IEqualityComparer<Dictionary<TKey, TValue>>
    {
        private readonly IEqualityComparer<TValue> _valueComparer;
        public DictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
        {
            this._valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }
        public bool Equals(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
        {
            if (x.Count != y.Count)
                return false;
            if (x.Keys.Except(y.Keys).Any())
                return false;
            if (y.Keys.Except(x.Keys).Any())
                return false;
            foreach (var pair in x)
                if (!_valueComparer.Equals(pair.Value, y[pair.Key]))
                    return false;
            return true;
        }

        public int GetHashCode(Dictionary<TKey, TValue> obj)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        private readonly IEqualityComparer<T> _valueComparer;
        public EnumerableComparer(IEqualityComparer<T> valueComparer = null)
        {
            this._valueComparer = valueComparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            return SetEquals(x, y, _valueComparer);
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            throw new NotImplementedException();
        }

        public static bool SetEquals(IEnumerable<T> first, IEnumerable<T> second,
            IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(second, comparer ?? EqualityComparer<T>.Default)
                .SetEquals(first);
        }
    }
}

