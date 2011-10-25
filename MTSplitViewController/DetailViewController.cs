//  MTSplitViewController
//  https://github.com/Krumelur/MTSplitViewController
// 
//  Ported to Monotouch by René Ruppert, October 2011
//
//  Based on code by Matt Gemmell on 26/07/2010.
//  Copyright 2010 Instinctive Code.
//  https://github.com/mattgemmell/MGSplitViewController

using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.UIKit;
using MTSplitViewLib;

namespace MTSplitViewControllerDemo
{
	public class DetailViewController : UIViewController
	{
		public DetailViewController (MTSplitViewController oSplitViewController) : base()
		{
			this.oSplitViewController = oSplitViewController;
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			this.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			this.View.BackgroundColor = UIColor.White;
			this.oSplitViewController.WillHideViewController += HandleWillHideViewController;
			this.oSplitViewController.WillShowViewController += HandleWillShowViewController;
			this.oSplitViewController.WillPresentViewController += HandleWillPresentViewController;
			this.oSplitViewController.WillChangeSplitOrientationToVertical += HandleWillChangeSplitOrientationToVertical;
			this.oSplitViewController.WillMoveSplitToPosition += HandleWillMoveSplitToPosition;
			this.oSplitViewController.ConstrainSplitPosition += HandleConstrainSplitPosition;
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.oToolbar = new UIToolbar (new RectangleF (0, 0, this.View.Bounds.Width, 44));
			this.oToolbar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.View.AddSubview (this.oToolbar);
			
			this.oDetailDescriptionLabel = new UILabel (new RectangleF (0, 0, this.View.Bounds.Width, 30));
			this.oDetailDescriptionLabel.TextAlignment = UITextAlignment.Center;
			this.oDetailDescriptionLabel.Center = new PointF (this.View.Bounds.Width / 2, this.View.Bounds.Height / 2);
			Console.WriteLine(this.View.Bounds + "; " + this.oDetailDescriptionLabel.Center);
			this.oDetailDescriptionLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin;
			this.View.AddSubview (this.oDetailDescriptionLabel);
			
			this.oToggleItem = new UIBarButtonItem ("Show", UIBarButtonItemStyle.Bordered, delegate(object sender, EventArgs e) {
				this.oSplitViewController.ToggleMasterView ();
				this.ConfigureView ();
			});
			
			this.oVerticalItem = new UIBarButtonItem ("Horizontal", UIBarButtonItemStyle.Bordered, delegate(object sender, EventArgs e) {
				this.oSplitViewController.ToggleSplitOrientation ();
				this.ConfigureView ();
			});
			
			this.oDividerStyleItem = new UIBarButtonItem ("Dragging", UIBarButtonItemStyle.Bordered, delegate(object sender, EventArgs e) {
				MTSplitDividerView.DIVIDER_STYLE eNewStyle = this.oSplitViewController.DividerStyle == MTSplitDividerView.DIVIDER_STYLE.Thin ? MTSplitDividerView.DIVIDER_STYLE.PaneSplitter : MTSplitDividerView.DIVIDER_STYLE.Thin;
				this.oSplitViewController.SetDividerStyleAnimated (eNewStyle);
				this.ConfigureView ();
			});
			
			this.oMasterBeforeDetailItem = new UIBarButtonItem ("Detail First", UIBarButtonItemStyle.Bordered, delegate(object sender, EventArgs e) {
				this.oSplitViewController.ToggleMasterBeforeDetail ();
				this.ConfigureView ();
			});
			
			this.oToolbar.SetItems (new UIBarButtonItem[] {this.oToggleItem, this.oVerticalItem, this.oDividerStyleItem, this.oMasterBeforeDetailItem }, false);
		}

		private void HandleWillHideViewController (MTSplitViewController oSplitController, UIViewController oMasterControler, UIBarButtonItem oBarBtnItm, UIPopoverController oPopover)
		{
			Console.WriteLine("WillHideViewController()");
			if (oBarBtnItm!= null)
			{
				oBarBtnItm.Title = "Popover";
				List<UIBarButtonItem> aItems = new List<UIBarButtonItem>(this.oToolbar.Items);
				aItems.Insert(0, oBarBtnItm);
				this.oToolbar.SetItems(aItems.ToArray(), true);
			}
    		this.oPopoverController = oPopover;
		}

		private float HandleConstrainSplitPosition (MTSplitViewController oSplitController, float fProposedPosition, SizeF oViewSize)
		{
			Console.WriteLine("ConstrainSplitPosition(): " + fProposedPosition);
			return fProposedPosition;
		}

		private void HandleWillMoveSplitToPosition (MTSplitViewController oSplitControler, float fSplitPos)
		{
			Console.WriteLine("WillMoveSplitToPosition(): " + fSplitPos);
		}

		private void HandleWillChangeSplitOrientationToVertical (MTSplitViewController oSplitController, bool bIsVertical)
		{
			Console.WriteLine("WillChangeSplitOrientationToVertical(): " + bIsVertical);
		}

		private void HandleWillPresentViewController (MTSplitViewController oSplitController, UIPopoverController oPopoverController, UIViewController oMasterController)
		{
			Console.WriteLine("WillPresentViewController()");	
		}

		private void HandleWillShowViewController (MTSplitViewController oSplitController, UIViewController oMasterController, UIBarButtonItem oBarBtnItm)
		{
			Console.WriteLine("WillShowViewController()");
			if (oBarBtnItm != null)
			{
				List<UIBarButtonItem> aItems = new List<UIBarButtonItem>(this.oToolbar.Items);
				aItems.Remove(oBarBtnItm);
				this.oToolbar.SetItems(aItems.ToArray(), true);
			}
    		this.oPopoverController = null;
		}
		
		public string DetailItem
		{
			get
			{
				return this.sDetailitem;
			}
			set
			{
				this.sDetailitem = value;
				this.ConfigureView();
				if(this.oPopoverController != null)
				{
					this.oPopoverController.Dismiss(true);
				}
			}
		}
		private string sDetailitem;
		
		private MTSplitViewController oSplitViewController;
		private UIBarButtonItem oToggleItem;
		private UIBarButtonItem oVerticalItem;
		private UIBarButtonItem oDividerStyleItem;
		private UIBarButtonItem oMasterBeforeDetailItem;
		private UIPopoverController oPopoverController;
		private UILabel oDetailDescriptionLabel;
		private UIToolbar oToolbar;
		
		public void ConfigureView()
		{
			// Update the user interface for the detail item.
    		this.oDetailDescriptionLabel.Text = this.sDetailitem;
			this.oToggleItem.Title = this.oSplitViewController.IsShowingMaster ? "Hide Master" : "Show Master";
			this.oVerticalItem.Title = this.oSplitViewController.IsVertical ? "Horizontal Split" : "Vertical Split";
			this.oDividerStyleItem.Title = this.oSplitViewController.DividerStyle.ToString();
			this.oMasterBeforeDetailItem.Title = this.oSplitViewController.MasterBeforeDetail ? "Detail First" : "Master First";
		}
	}
}

