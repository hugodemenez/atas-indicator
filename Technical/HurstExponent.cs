﻿namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using ATAS.Indicators.Technical.Properties;

	using OFT.Attributes;

	[DisplayName("Hurst Exponent")]
	[FeatureId("NotReady")]
	public class HurstExponent : Indicator
	{
		#region Nested types

		public enum Period
		{
			[Display(Name = "32")]
			First = 32,

			[Display(Name = "64")]
			Second = 64,

			[Display(Name = "128")]
			Third = 128
		}

		#endregion

		#region Fields

		private readonly ValueDataSeries _renderSeries = new(Resources.Visualization);
		private Period _period;
		private int _vPeriods;

		#endregion

		#region Properties

		[Display(ResourceType = typeof(Resources), Name = "Period", GroupName = "Settings", Order = 100)]
		public Period Length
		{
			get => _period;
			set
			{
				_period = value;
				RecalculateValues();
			}
		}

		#endregion

		#region ctor

		public HurstExponent()
		{
			Panel = IndicatorDataProvider.NewPanel;
			_period = Period.First;

			_renderSeries.ShowZeroValue = false;
			DataSeries[0] = _renderSeries;
		}

		#endregion

		#region Protected methods

		protected override void OnCalculate(int bar, decimal value)
		{
			if (bar == 0)
			{
				_renderSeries.Clear();

				switch ((int)_period)
				{
					case 32:
						_vPeriods = 3;
						break;
					case 64:
						_vPeriods = 4;
						break;
					case 128:
						_vPeriods = 5;
						break;
				}
			}

			if (bar < (int)_period)
			{
				_renderSeries.SetPointOfEndLine(bar);
				return;
			}

			var shortLnSum = 0m;
			var shortAvgLnSum = 0m;
			var avgLnSum = 0m;
			var shortLnSquareSum = 0m;

			for (var pow = 3; Math.Pow(2, pow) <= (int)_period; pow++)
			{
				var shortPeriod = (int)Math.Pow(2, pow);
				var rescaledSum = 0m;

				for (var i = 0; i < (int)_period / shortPeriod; i++)
				{
					var mean = SourceDataSeries.CalcSum(shortPeriod, bar - i * shortPeriod) / shortPeriod;

					var adjSum = 0m;
					var maxSum = 0m;
					var minSum = 0m;
					var squareSum = 0m;

					for (var j = bar - i * shortPeriod; j >= bar - (i + 1) * shortPeriod; j--)
					{
						var diff = (decimal)SourceDataSeries[j] - mean;
						adjSum += diff;

						if (adjSum > maxSum || maxSum == 0)
							maxSum = adjSum;

						if (adjSum < minSum || minSum == 0)
							minSum = adjSum;

						squareSum += diff * diff;
					}

					var range = maxSum - minSum;

					var stdDev = (decimal)Math.Sqrt((double)(squareSum / shortPeriod));
					rescaledSum += range / stdDev;
				}

				var rescaledAvg = rescaledSum / shortPeriod;

				var shortLog = (decimal)Math.Log(shortPeriod);
				var rescaledLog = (decimal)Math.Log((double)rescaledAvg);

				shortLnSum += shortLog;
				avgLnSum += rescaledLog;
				shortAvgLnSum += shortLog * rescaledLog;
				shortLnSquareSum += shortLog * shortLog;
			}

			var exponent = (_vPeriods * shortAvgLnSum - shortLnSum * avgLnSum) / (_vPeriods * shortLnSquareSum - shortLnSum * shortLnSum);
			_renderSeries[bar] = Math.Abs(exponent);
		}

		#endregion
	}
}