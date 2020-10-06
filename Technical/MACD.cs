namespace ATAS.Indicators.Technical
{
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using System.Windows.Media;

	using ATAS.Indicators.Properties;

	using Utils.Common.Attributes;
	using Utils.Common.Localization;

	[DisplayName("MACD")]
	[LocalizedDescription(typeof(Resources), "MACD")]
	[HelpLink("https://support.orderflowtrading.ru/knowledge-bases/2/articles/8125-macd")]
	public class MACD : Indicator
	{
		#region Fields

		private readonly EMA _long = new EMA();
		private readonly EMA _short = new EMA();
		private readonly SMA _sma = new SMA();

		#endregion

		#region Properties

		[Parameter]
		[Display(ResourceType = typeof(Resources),
			Name = "LongPeriod",
			GroupName = "Common",
			Order = 20)]
		public int LongPeriod
		{
			get => _long.Period;
			set
			{
				if (value <= 0)
					return;

				_long.Period = value;
				RecalculateValues();
			}
		}

		[Parameter]
		[Display(ResourceType = typeof(Resources),
			Name = "ShortPeriod",
			GroupName = "Common",
			Order = 20)]
		public int ShortPeriod
		{
			get => _short.Period;
			set
			{
				if (value <= 0)
					return;

				_short.Period = value;
				RecalculateValues();
			}
		}

		[Parameter]
		[Display(ResourceType = typeof(Resources),
			Name = "SignalPeriod",
			GroupName = "Common",
			Order = 20)]
		public int SignalPeriod
		{
			get => _sma.Period;
			set
			{
				if (value <= 0)
					return;

				_sma.Period = value;
				RecalculateValues();
			}
		}

		#endregion

		#region ctor

		public MACD()
		{
			Panel = IndicatorDataProvider.NewPanel;

			((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Histogram;
			((ValueDataSeries)DataSeries[0]).Color = Colors.CadetBlue;

			DataSeries.Add(new ValueDataSeries("Signal")
			{
				VisualType = VisualMode.Line,
				LineDashStyle = LineDashStyle.Dash
			});

			LongPeriod = 26;
			ShortPeriod = 12;
			SignalPeriod = 9;
		}

		#endregion

		#region Protected methods

		protected override void OnCalculate(int bar, decimal value)
		{
			var macd = _short.Calculate(bar, value) - _long.Calculate(bar, value);
			var signal = _sma.Calculate(bar, macd);

			this[bar] = macd;
			DataSeries[1][bar] = signal;
		}

		#endregion
	}
}