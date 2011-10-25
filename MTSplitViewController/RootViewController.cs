//  MTSplitViewController
//  https://github.com/Krumelur/MTSplitViewController
// 
//  Ported to Monotouch by René Ruppert, October 2011
//
//  Based on code by Matt Gemmell on 26/07/2010.
//  Copyright 2010 Instinctive Code.
//  https://github.com/mattgemmell/MGSplitViewController

using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MTSplitViewControllerDemo
{
	public class RootViewController : UITableViewController
	{
		public RootViewController () : base()
		{
		}
		
		public RootViewController (UITableViewStyle withStyle) : base(withStyle)
		{
		}
		
		public DetailViewController DetailViewController;
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.ClearsSelectionOnViewWillAppear = false;
			this.ContentSizeForViewInPopover = new SizeF(320f, 600f);
			this.TableView.Source = new Source(this);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
		
		public void SelectFirstRow()
		{
			if(this.TableView.NumberOfSections() > 0 && this.TableView.NumberOfRowsInSection(0) > 0)
			{
				NSIndexPath oIndexPath = NSIndexPath.FromRowSection(0, 0);
				this.TableView.SelectRow(oIndexPath, true, UITableViewScrollPosition.Top);
				((Source)(this.TableView.Source)).RowSelected(this.TableView, oIndexPath);
			}
		}
		
		public class Source : UITableViewSource
		{
			public Source(RootViewController oController)
			{
				this.oController = oController;
			}
			private RootViewController oController;
			
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				return 10;
			}
			
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				string sCellID = "CellID";
				UITableViewCell oCell =  tableView.DequeueReusableCell(sCellID);
				if(oCell == null)
				{
					oCell = new UITableViewCell(UITableViewCellStyle.Default, sCellID);
					oCell.Accessory = UITableViewCellAccessory.None;
				}
				oCell.TextLabel.Text = "Row " + indexPath.Row;
				return oCell;
			}
			
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				this.oController.DetailViewController.DetailItem = "Row " + indexPath.Row;
			}
		}
	}
}

