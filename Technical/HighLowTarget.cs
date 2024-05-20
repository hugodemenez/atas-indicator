namespace ATAS.Indicators.Technical;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical.Properties;

using OFT.Attributes;

[DisplayName("HighLowTarget")]
[HelpLink("https://support.atas.net/knowledge-bases/2/articles/387-daily-highlow")]

[FeatureId("-1PIHx1Uab-XdQTCQfJBZeTasHyqfqn-3Uwf")]
public class HighLowTarget : Indicator
{
    #region Fields

    private readonly ValueDataSeries _highSeries = new("High")
    {
        Color = Color.FromArgb(255, 135, 135, 135),
        VisualType = VisualMode.Square
    };

    private readonly ValueDataSeries _lowSeries = new("Low")
    {
        Color = Color.FromArgb(255, 135, 135, 135),
        VisualType = VisualMode.Square
    };

    private readonly ValueDataSeries _highTargetSeries = new("High Target")
    {
        Color = DefaultColors.Yellow.Convert(),
        VisualType = VisualMode.Square
    };
    private readonly ValueDataSeries _lowTargetSeries = new("Low Target")
    {
        Color = DefaultColors.Blue.Convert(),
        VisualType = VisualMode.Square
    };

    private int _days = 20;
    private int _target = 130;

    private decimal _tickSize;

    private decimal _high;
    private bool _highSpecified;
    private DateTime _lastSessionTime;
    private decimal _low;
    private bool _lowSpecified;
    private decimal _lowTraget;
    private decimal _highTarget;

    private int _targetBar;

    #endregion

    #region Properties

    [Display(GroupName = "Calculation", Name = "Target", Order = int.MaxValue, Description = "TargetDescription")]
    [Range(0, 200)]
    public int Target
    {
        get => _target;
        set
        {
            _target = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Resources), GroupName = "Calculation", Name = "DaysLookBack", Order = int.MaxValue, Description = "DaysLookBackDescription")]
    [Range(0, 1000)]
    public int Days
    {
        get => _days;
        set
        {
            _days = value;
            RecalculateValues();
        }
    }

    #endregion

    #region ctor

    public HighLowTarget()
        : base(true)
    {
        DenyToChangePanel = true;

        DataSeries[0] = _highSeries;
        DataSeries.Add(_lowSeries);
        DataSeries.Add(_highTargetSeries);
        DataSeries.Add(_lowTargetSeries);
    }

    #endregion

    #region Public methods

    public override string ToString()
    {
        return "High Low Target";
    }

    #endregion

    #region Protected methods

    protected override void OnCalculate(int bar, decimal value)
    {
        _tickSize = InstrumentInfo.TickSize;

        if (bar == 0)
        {
            if (_days == 0)
                _targetBar = 0;
            else
            {
                var days = 0;

                for (var i = CurrentBar - 1; i >= 0; i--)
                {
                    _targetBar = i;

                    if (!IsNewSession(i))
                        continue;

                    days++;

                    if (days == _days)
                        break;
                }
            }

            _high = _low = _lowTraget = _highTarget = 0;
            _highSpecified = _lowSpecified = false;
            DataSeries.ForEach(x => x.Clear());
        }

        if (bar < _targetBar)
            return;

        var candle = GetCandle(bar);

        if (IsNewSession(bar))
        {
            if (_lastSessionTime != candle.Time)
            {
                _lastSessionTime = candle.Time;
                _high = _low = _highTarget = _lowTraget = 0;
                _highSpecified = _lowSpecified = false;
            }
        }

        if (candle.High > _high || !_highSpecified)
        {
            _high = candle.High;
            _highTarget = candle.High - _target*_tickSize ;
        }

        if (candle.Low < _low || !_lowSpecified)
        {
            _low = candle.Low;
            _lowTraget = candle.Low + _target*_tickSize;
        }


        _highSpecified = _lowSpecified = true;
        _highSeries[bar] = _high;
        _lowSeries[bar] = _low;
        _highTargetSeries[bar] = _highTarget;
        _lowTargetSeries[bar] = _lowTraget;
    }

    #endregion
}