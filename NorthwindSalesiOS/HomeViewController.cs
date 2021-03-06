using System;
using MonoTouch.UIKit;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Collections.Generic;
using Simple.OData.Client;
using Infragistics;
using BigTed;

namespace NorthwindSalesiOS
{
	public class HomeViewController : UIViewController
	{
		readonly ODataClient client;
		IGChartView chart;

		public HomeViewController ()
		{
			client = new ODataClient ("http://services.odata.org/v3/Northwind/Northwind.svc/");
		}

		public override void ViewDidLoad ()
		{
			Title = "Northwind Sales Dashboard";
			EdgesForExtendedLayout = UIRectEdge.None;
			var fetchDataButton = new UIBarButtonItem ("Fetch Data", UIBarButtonItemStyle.Plain, OnFetchDataTap);
			NavigationItem.SetRightBarButtonItem (fetchDataButton, false);	
		}

	

		async void OnFetchDataTap (object sender, EventArgs e)
		{
			InitializeChart ();

			BTProgressHUD.Show ("Fetching data");

			var data = await GetDataAsync ();

			// set data source
			var categorySeriesSource = new IGCategorySeriesDataSourceHelper ();
			categorySeriesSource.Values = data.ProductSales.ToArray ();
			categorySeriesSource.Labels = data.ProductName.ToArray ();


			// Create axis types and add it to the chart
			var xAxisBar = new IGNumericXAxis ("xAxis");
			var yAxisBar = new IGCategoryYAxis ("yAxis");
			yAxisBar.LabelAlignment = IGHorizontalAlign.IGHorizontalAlignRight;

			chart.AddAxis (xAxisBar);
			chart.AddAxis (yAxisBar);

			// decide on what series need to be displayed on the chart
			var barSeries = new IGBarSeries ("series");
			barSeries.XAxis = xAxisBar;
			barSeries.YAxis = yAxisBar;

			// set the appropriate data sources
			barSeries.DataSource = categorySeriesSource;
			chart.AddSeries (barSeries);

			/* //TODO: Comment the above bar series code and UnComment the below code for column series

			// Create axis types and add it to the chart
			var xAxisBar = new IGCategoryXAxis ("xAxis");
			var yAxisBar = new IGNumericYAxis ("yAxis");
			yAxisBar.LabelAlignment = IGHorizontalAlign.IGHorizontalAlignRight;

			chart.AddAxis (xAxisBar);
			chart.AddAxis (yAxisBar);

			// decide on what series need to be displayed on the chart
			var columnSeries = new IGColumnSeries ("series");
			columnSeries.XAxis = xAxisBar;
			columnSeries.YAxis = yAxisBar;

			// set the appropriate data sources
			columnSeries.DataSource = categorySeriesSource;
			chart.AddSeries (columnSeries);

			*/
			BTProgressHUD.Dismiss ();


		}

		async Task<SalesByCategory> GetDataAsync ()
		{
			var salesData = await client.For("Sales_by_Categories")
				.Top (15).OrderBy ("ProductSales").FindEntriesAsync ();

			var salesByCategory = new SalesByCategory ();
			foreach (var sale in salesData) {
				salesByCategory.ProductName.Add (NSObject.FromObject (sale ["ProductName"].ToString ()));
				salesByCategory.ProductSales.Add (NSObject.FromObject (sale ["ProductSales"].ToString ()));
			}

			return salesByCategory;
		}

		void InitializeChart ()
		{
			// if chart was added previously, remove it from the view before constructing it with new values
			if (chart != null)
				chart.RemoveFromSuperview ();
			chart = new IGChartView (View.Bounds);
			chart.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			chart.Theme = IGChartGradientThemes.IGThemeDark ();
			View.Add (chart);
		}
	}

	/// <summary>
	/// Data Model
	/// </summary>
	public class SalesByCategory
	{
		public SalesByCategory ()
		{
			ProductName = new List<NSObject> ();
			ProductSales = new List<NSObject> ();

		}

		public List<NSObject> ProductName {
			get;
			set;
		}

		public List<NSObject> ProductSales {
			get;
			set;
		}
	}
}

