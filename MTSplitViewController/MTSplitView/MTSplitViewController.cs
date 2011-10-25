//  MTSplitViewController
//  https://github.com/Krumelur/MTSplitViewController
// 
//  Ported to Monotouch by René Ruppert, October 2011
//
//  Based on code by Matt Gemmell on 26/07/2010.
//  Copyright 2010 Instinctive Code.
//  https://github.com/mattgemmell/MGSplitViewController

using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Drawing;

namespace MTSplitView
{
	public class MTSplitViewController : UIViewController
	{
		// default width of master view in UISplitViewController.
		public const float MG_DEFAULT_SPLIT_POSITION = 320.0f;
		// default width of split-gutter in UISplitViewController.
		public const float MG_DEFAULT_SPLIT_WIDTH = 1.0f;
		// default corner-radius of overlapping split-inner corners on the master and detail views.
		public const float MG_DEFAULT_CORNER_RADIUS = 5.0f;
		// default color of intruding inner corners (and divider background).
		public UIColor MG_DEFAULT_CORNER_COLOR = UIColor.Black;
		// corner-radius of split-inner corners for MGSplitViewDividerStylePaneSplitter style.
		public const float MG_PANESPLITTER_CORNER_RADIUS = 0.0f;		
		// width of split-gutter for MGSplitViewDividerStylePaneSplitter style.
		public const float MG_PANESPLITTER_SPLIT_WIDTH = 25.0f;	
		// minimum width a view is allowed to become as a result of changing the splitPosition.
		public const float MG_MIN_VIEW_WIDTH = 200.0f;
		
		public MTSplitViewController () : base()
		{
			this.Setup ();
		}
		
		public MTSplitViewController (NSCoder coder) : base(coder)
		{
			this.Setup ();
		}
		
		public MTSplitViewController (NSObjectFlag t) : base(t)
		{
			this.Setup ();
		}
		
		public MTSplitViewController (IntPtr handle) : base(handle)
		{
			this.Setup ();
		}
		
		public MTSplitViewController (string nibName, NSBundle bundle) : base(nibName, bundle)
		{
			this.Setup ();
		}
		
		/// <summary>
		/// Will be triggered if the orientation of the panels changes.
		/// </summary>
		public event Action<MTSplitViewController, bool> WillChangeSplitOrientationToVertical;
		/// <summary>
		/// Will be triggered if the user wants to change the split position.
		/// </summary>
		public event Func<MTSplitViewController, float, SizeF, float> ConstrainSplitPosition;
		/// <summary>
		/// Will be triggered if the split position is moved.
		/// </summary>
		public event Action<MTSplitViewController, float> WillMoveSplitToPosition;
		/// <summary>
		/// Will be triggered if the master view controller will be hidden.
		/// </summary>
		public event Action<MTSplitViewController, UIViewController, UIBarButtonItem, UIPopoverController> WillHideViewController;
		/// <summary>
		/// Will be called if the master view controller will be shown.
		/// </summary>
		public event Action<MTSplitViewController, UIViewController, UIBarButtonItem> WillShowViewController;
		/// <summary>
		/// Will be called if the master view controller will be shown in a popove.
		/// </summary>
		public event Action<MTSplitViewController, UIPopoverController, UIViewController> WillPresentViewController;
		
		// Indicates if the popover has to be reconfigured.
		private bool bReconfigurePopup;
		
		/// <summary>
		/// Gets or sets the hidden popover controller used to display the master view controller.
		/// </summary>
		public UIPopoverController HiddenPopoverController
		{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the bar button item used to present the master view controller from.
		/// </summary>
		/// <value>
		/// The bar button item.
		/// </value>
		public UIBarButtonItem BarButtonItem
		{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether the master controller is shown in portrait mode.
		/// master in portrait.
		/// </summary>
		/// <value>
		/// <c>true</c> if shows master in portrait; otherwise, <c>false</c>.
		/// </value>
		public bool ShowsMasterInPortrait
		{
			get
			{
				return this.bShowsMasterInPortrait;
			}
			set
			{
				if (value != this.ShowsMasterInPortrait)
				{
					this.bShowsMasterInPortrait = value;
		
					if (!this.IsLandscape)
					{
						// i.e. if this will cause a visual change.
						if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
						{
							this.HiddenPopoverController.Dismiss (false);
						}
			
						// Rearrange views.
						this.bReconfigurePopup = true;
						this.LayoutSubviews ();
					}
				}

			}
		}
		private bool bShowsMasterInPortrait;
		
		/// <summary>
		/// Gets or sets a value indicating whether the master controller is visible in landscape mode.
		/// master in landscape.
		/// </summary>
		/// <value>
		/// <c>true</c> if shows master in landscape; otherwise, <c>false</c>.
		/// </value>
		public bool ShowsMasterInLandscape
		{
			get
			{
				return this.bShowsMasterInLandscape;
			}
			set
			{
				this.bShowsMasterInLandscape = value;		
				if (this.IsLandscape)
				{
					// i.e. if this will cause a visual change.
					if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
					{
						this.HiddenPopoverController.Dismiss (false);
					}
			
					// Rearrange views.
					this.bReconfigurePopup = true;
					this.LayoutSubviews ();
				}
			}
		}
		private bool bShowsMasterInLandscape;
		
		/// <summary>
		/// If FALSE, split is horizontal, i.e. master above detail. If TRUE, split is vertical, i.e. master left of detail.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is vertical; otherwise, <c>false</c>.
		/// </value>
		public bool IsVertical
		{
			get
			{
				return this.bIsVertical;
			}
			
			set
			{
				if (value != this.IsVertical)
				{
					if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
					{
						this.HiddenPopoverController.Dismiss (false);
					}
		
					this.bIsVertical = value;
		
					// Inform delegate.
					if (this.WillChangeSplitOrientationToVertical != null)
					{
						this.WillChangeSplitOrientationToVertical (this, this.bIsVertical);
					}
				}
		
				this.LayoutSubviews ();
			}
		}
		private bool bIsVertical;
		
		/// <summary>
		/// If FALSE, master view is below/right of detail. Otherwise master view is above/left of detail.
		/// before detail.
		/// </summary>
		/// <value>
		/// <c>true</c> if master before detail; otherwise, <c>false</c>.
		/// </value>
		public bool MasterBeforeDetail
		{
			get
			{
				return this.bMasterBeforeDetail;
			}
			set
			{
				if (value != this.MasterBeforeDetail)
				{
					if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
					{
						this.HiddenPopoverController.Dismiss (false);
					}
		
					this.bMasterBeforeDetail = value;
		
					if (this.IsShowingMaster)
					{
						this.LayoutSubviews ();
					}
				}
			}
		}
		private bool bMasterBeforeDetail;
		
		
		/// <summary>
		/// Starting position of split in units, relative to top/left (depending on IsVertical setting) if MasterBeforeDetail is TRUE,
		/// else relative to bottom/right.
		/// </summary>
		/// <value>
		/// The split position.
		/// </value>
		public float SplitPosition
		{
			get
			{
				return this.fSplitPosition;
			}
			set
			{
				// Check to see if delegate wishes to constrain the position.
				bool bConstrained = false;
				SizeF oFullSize = this.SplitViewSizeForOrientation (this.InterfaceOrientation);
				float fNewPos = value;
				if (this.ConstrainSplitPosition != null)
				{
					fNewPos = this.ConstrainSplitPosition (this, value, oFullSize);
					bConstrained = true; // implicitly trust delegate's response.
				}
				else
				{
					// Apply default constraints if delegate doesn't wish to participate.
					float fMinPos = MG_MIN_VIEW_WIDTH;
					float fMaxPos = ((this.IsVertical) ? oFullSize.Width : oFullSize.Height) - (MG_MIN_VIEW_WIDTH + this.SplitWidth);
					bConstrained = (fNewPos != this.SplitPosition && fNewPos >= fMinPos && fNewPos <= fMaxPos);
				}
				if (bConstrained)
				{
					if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
					{
						this.HiddenPopoverController.Dismiss (false);
					}
					
					this.fSplitPosition = fNewPos;
					
					// Inform delegate.
					if (this.WillMoveSplitToPosition != null)
					{
						this.WillMoveSplitToPosition (this, this.SplitPosition);
					}
					
					if (this.IsShowingMaster)
					{
						this.LayoutSubviews ();
					}
				}
			}
		}
		
		/// <summary>
		/// Sets the split position animated.
		/// Note: split position is the width (in a left/right split, or height in a top/bottom split) of the master view.
		/// It is relative to the appropriate side of the splitView, which can be any of the four sides depending on the values
		/// in IsMasterBeforeDetail and IsVertical:
		/// IsVertical = YES, isMasterBeforeDetail = YES: splitPosition is relative to the LEFT edge. (Default)
		/// IsVertical = YES, isMasterBeforeDetail = FALSE: split position is relative to the RIGHT edge.
 		/// IsVertical = NO, isMasterBeforeDetail = TRUE: split position is relative to the TOP edge.
 		/// IsVertical = NO, isMasterBeforeDetail = FALSE: split position is relative to the BOTTOM edge.
		/// This implementation was chosen so you don't need to recalculate equivalent splitPositions if the user toggles masterBeforeDetail themselves.
		/// </summary>
		/// <param name='fNewPos'>the new position in units</param>
		public void SetSplitPositionAnimated (float fNewPos)
		{
			bool bAnimate = this.IsShowingMaster;
			if (bAnimate)
			{
				UIView.BeginAnimations ("SplitPosition");
			}
			this.SplitPosition = fNewPos;
			if (bAnimate)
			{
				UIView.CommitAnimations ();
			}
		}

		private float fSplitPosition;
		
		/// <summary>
		/// Gets or sets the width of the split.
		/// </summary>
		/// <value>
		/// The width of the split.
		/// </value>
		public float SplitWidth
		{
			get
			{
				return this.fSplitWidth;
			}
			set
			{
				if (value != this.SplitWidth && value >= 0)
				{
					this.fSplitWidth = value;
					if (this.IsShowingMaster)
					{
						this.LayoutSubviews ();
					}
				}
			}
		}

		private float fSplitWidth;
		
		/// <summary>
		/// Whether to let the user drag the divider to alter the split position.
		/// dragging divider.
		/// </summary>
		/// <value>
		/// <c>true</c> if allows dragging divider; otherwise, <c>false</c>.
		/// </value>
		public bool AllowsDraggingDivider
		{
			get
			{
				return this.DividerView != null && this.DividerView.AllowsDragging;
			}
			set
			{
				if (this.DividerView != null)
				{
					this.DividerView.AllowsDragging = value;
				}
			}
		}
		
		/// <summary>
		/// Array of UIViewControllers; master is at index 0, detail is at index 1.
		/// </summary>
		/// <value>
		/// The view controllers.
		/// </value>
		/// <exception cref='ArgumentException'>
		/// Is thrown when an argument passed to a method is invalid.
		/// </exception>
		public UIViewController[] ViewControllers
		{
			get
			{
				return this.aViewControllers;
			}
			set
			{
				if (this.aViewControllers != null)
				{
					foreach (UIViewController oController in this.aViewControllers)
					{	
						if (oController == null)
						{
							continue;
						}
						oController.View.RemoveFromSuperview ();
					}
				}
			
				this.aViewControllers = new UIViewController[2];
				if (value != null && value.Length >= 2)
				{
					this.MasterViewController = value [0];
					this.DetailViewController = value [1];
				}
				else
				{
					throw new ArgumentException ("Error: require 2 view-controllers.");
				}
		
				this.LayoutSubviews ();
			}
		}

		private UIViewController[] aViewControllers;
		
		/// <summary>
		/// Gets or sets the master view controller.
		/// </summary>
		/// <value>
		/// The master view controller.
		/// </value>
		public UIViewController MasterViewController
		{
			get
			{
				if (this.ViewControllers != null && this.ViewControllers.Length > 0)
				{
					return this.ViewControllers [0];
				}
				return null;
			}
			set
			{
				if (this.ViewControllers == null)
				{
					this.ViewControllers = new UIViewController[2];
					this.ViewControllers [1] = null;
				}
	
				if (this.ViewControllers [0] == value)
				{
					return;
				}
				
				this.ViewControllers [0] = value;
				this.LayoutSubviews ();
			}
		}
		
		/// <summary>
		/// Gets or sets the detail view controller.
		/// </summary>
		/// <value>
		/// The detail view controller.
		/// </value>
		public UIViewController DetailViewController
		{
			get
			{
				if (this.ViewControllers != null && this.ViewControllers.Length >= 2)
				{
					return this.ViewControllers [1];
				}
				return null;
			}
			set
			{
				if (this.ViewControllers == null)
				{
					this.ViewControllers = new UIViewController[2];
					this.ViewControllers [0] = null;
				}
				
				if (this.ViewControllers [1] == value)
				{
					return;
				}
				
				this.ViewControllers [1] = value;
				this.LayoutSubviews ();
			}
		}
		
		/// <summary>
		/// Gets or sets the divider view.
		/// </summary>
		/// <value>
		/// The divider view.
		/// </value>
		public MTSplitDividerView DividerView
		{
			get
			{
				return this.oDividerView;
			}
			set
			{
				if (value == this.oDividerView)
				{
					return;
				}
				if(this.oDividerView != null)
				{
					this.oDividerView.Dispose ();
				}
				this.oDividerView = value;
				this.oDividerView.SplitViewController = this;
				this.oDividerView.BackgroundColor = MG_DEFAULT_CORNER_COLOR;
				if (this.IsShowingMaster)
				{
					this.LayoutSubviews ();
				}
			}
		}

		private MTSplitDividerView oDividerView;
		
		/// <summary>
		/// Gets or sets the divider style.
		/// </summary>
		/// <value>
		/// The divider style.
		/// </value>
		public MTSplitDividerView.DIVIDER_STYLE DividerStyle
		{
			get
			{
				return this.eDividerStyle;
			}
			set
			{
				if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
				{
					this.HiddenPopoverController.Dismiss (false);
				}
	
				// We don't check to see if newStyle equals _dividerStyle, because it's a meta-setting.
				// Aspects could have been changed since it was set.
				this.eDividerStyle = value;
	
				// Reconfigure general appearance and behaviour.
				float fCornerRadius = 0f;
				if (this.eDividerStyle == MTSplitDividerView.DIVIDER_STYLE.Thin)
				{
					fCornerRadius = MG_DEFAULT_CORNER_RADIUS;
					this.SplitWidth = MG_DEFAULT_SPLIT_WIDTH;
					this.AllowsDraggingDivider = false;
				}
				else if (this.eDividerStyle == MTSplitDividerView.DIVIDER_STYLE.PaneSplitter)
				{
					fCornerRadius = MG_PANESPLITTER_CORNER_RADIUS;
					this.SplitWidth = MG_PANESPLITTER_SPLIT_WIDTH;
					this.AllowsDraggingDivider = true;
				}
	
				// Update divider and corners.
				this.DividerView.SetNeedsDisplay ();
				if (this.CornerViews != null)
				{
					foreach (MTSplitCornersView corner in this.CornerViews)
					{
						corner.CornerRadius = fCornerRadius;
					}
				}
				// Layout all views.
				this.LayoutSubviews ();
			}
		}

		private MTSplitDividerView.DIVIDER_STYLE eDividerStyle;
		
		/// <summary>
		/// Sets the divider style animated.
		/// </summary>
		public void SetDividerStyleAnimated (MTSplitDividerView.DIVIDER_STYLE eStyle)
		{
			bool bAnimate = this.IsShowingMaster;
			if (bAnimate)
			{
				UIView.BeginAnimations ("DividerStyle");
			}
			this.DividerStyle = eStyle;
			if (bAnimate)
			{
				UIView.CommitAnimations ();
			}
		}
		
		
		/// <summary>
		/// Returns TRUE if this view controller is in either of the two Landscape orientations, else FALSE.
		/// </summary>
		public bool IsLandscape
		{
			get
			{
				return this.InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || this.InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the master controller is currently shown.
		/// </summary>
		public bool IsShowingMaster
		{
			get
			{
				return this.ShouldShowMaster
					&& this.MasterViewController != null
					&& this.MasterViewController.View != null
					&& this.MasterViewController.View.Superview == this.View;
			}
		}
		
		/// <summary>
		/// Finds out if the master controller should be shown.
		/// </summary>
		/// <value>
		/// <c>true</c> if should show master; otherwise, <c>false</c>.
		/// </value>
		private bool ShouldShowMaster
		{
			get
			{
				return this.ShouldShowMasterForInterfaceOrientation (this.InterfaceOrientation);
			}
		}

		
		/// <summary>
		/// Gets or sets the corner views.
		/// Returns an array of two MGSplitCornersView objects, used to draw the inner corners.
 		/// The first view is the "leading" corners (top edge of screen for left/right split, left edge of screen for top/bottom split).
 		/// The second view is the "trailing" corners (bottom edge of screen for left/right split, right edge of screen for top/bottom split).
 		/// Do NOT modify them, except to:
		/// 1. Change their background color
		/// 2. Change their corner radius
		/// </summary>
		/// <value>
		/// The corner views.
		/// </value>
		public MTSplitCornersView[] CornerViews
		{
			get
			{
				return this.aCornerViews;
			}
			set
			{
				this.aCornerViews = value;
			}
		}

		private MTSplitCornersView[] aCornerViews;
		
		/// <summary>
		/// Gets a human readable name of the interface orientation.
		/// </summary>
		/// <returns>
		/// The of interface orientation.
		/// </returns>
		/// <param name='theOrientation'>
		/// The orientation.
		/// </param>
		public string NameOfInterfaceOrientation (UIInterfaceOrientation theOrientation)
		{
			string sOrientationName = null;
			switch (theOrientation)
			{
			case UIInterfaceOrientation.Portrait:
				sOrientationName = "Portrait"; // Home button at bottom
				break;
			case UIInterfaceOrientation.PortraitUpsideDown:
				sOrientationName = "Portrait (Upside Down)"; // Home button at top
				break;
			case UIInterfaceOrientation.LandscapeLeft:
				sOrientationName = "Landscape (Left)"; // Home button on left
				break;
			case UIInterfaceOrientation.LandscapeRight:
				sOrientationName = "Landscape (Right)"; // Home button on right
				break;
			default:
				break;
			}
	
			return sOrientationName;
		}
		
		/// <summary>
		/// Returns TRUE if master view should be shown directly embedded in the splitview, instead of hidden in a popover.
		/// </summary>
		private bool ShouldShowMasterForInterfaceOrientation (UIInterfaceOrientation eOrientation)
		{
			if (eOrientation == UIInterfaceOrientation.LandscapeLeft || eOrientation == UIInterfaceOrientation.LandscapeRight)
			{
				return this.ShowsMasterInLandscape;
			}
			else
			{
				return this.ShowsMasterInPortrait;
			}
		}
		
		/// <summary>
		/// Setup this instance's basic properties.
		/// </summary>
		private void Setup ()
		{
			// Configure default behaviour.
			this.SplitWidth = MG_DEFAULT_SPLIT_WIDTH;
			this.ShowsMasterInPortrait = false;
			this.ShowsMasterInLandscape = true;
			this.bReconfigurePopup = false;
			this.IsVertical = true;
			this.MasterBeforeDetail = true;
			this.SplitPosition = MG_DEFAULT_SPLIT_POSITION;
			RectangleF oDivRect = this.View.Bounds;
			if (this.IsVertical)
			{
				oDivRect.Y = this.SplitPosition;
				oDivRect.Height = this.SplitWidth;
			}
			else
			{
				oDivRect.X = this.SplitPosition;
				oDivRect.Width = this.SplitWidth;
			}
			this.DividerView = new MTSplitDividerView (oDivRect);
			this.DividerView.SplitViewController = this;
			this.DividerView.BackgroundColor = MG_DEFAULT_CORNER_COLOR;
			this.DividerStyle = MTSplitDividerView.DIVIDER_STYLE.Thin;
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
		
		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (this.MasterViewController != null)
			{
				this.MasterViewController.WillRotate (toInterfaceOrientation, duration);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.WillRotate (toInterfaceOrientation, duration);
			}
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			if (this.MasterViewController != null)
			{
				this.MasterViewController.DidRotate (fromInterfaceOrientation);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.DidRotate (fromInterfaceOrientation);
			}
		}
		
		public override void WillAnimateRotation (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (this.MasterViewController != null)
			{
				this.MasterViewController.WillAnimateRotation (toInterfaceOrientation, duration);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.WillAnimateRotation (toInterfaceOrientation, duration);
			}
	
			// Hide popover.
			if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
			{
				this.HiddenPopoverController.Dismiss (false);
			}
	
			// Re-tile views.
			this.bReconfigurePopup = true;
			this.LayoutSubviewsForInterfaceOrientation (toInterfaceOrientation, true);
		}
		
		public override void WillAnimateFirstHalfOfRotation (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (this.MasterViewController != null)
			{
				this.MasterViewController.WillAnimateFirstHalfOfRotation (toInterfaceOrientation, duration);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.WillAnimateFirstHalfOfRotation (toInterfaceOrientation, duration);
			}
		}
		
		public override void DidAnimateFirstHalfOfRotation (UIInterfaceOrientation toInterfaceOrientation)
		{
			if (this.MasterViewController != null)
			{
				this.MasterViewController.DidAnimateFirstHalfOfRotation (toInterfaceOrientation);
			}
		
			if (this.DetailViewController != null)
			{
				this.DetailViewController.DidAnimateFirstHalfOfRotation (toInterfaceOrientation);
			}
		}
		
		public override void WillAnimateSecondHalfOfRotation (UIInterfaceOrientation fromInterfaceOrientation, double duration)
		{
			if (this.MasterViewController != null)
			{
				this.MasterViewController.WillAnimateSecondHalfOfRotation (fromInterfaceOrientation, duration);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.WillAnimateSecondHalfOfRotation (fromInterfaceOrientation, duration);
			}
		}
		
		private void LayoutSubviewsWithAnimation (bool animate)
		{
			this.LayoutSubviewsForInterfaceOrientation (this.InterfaceOrientation, animate);
		}

		private void LayoutSubviews ()
		{
			this.LayoutSubviewsForInterfaceOrientation (this.InterfaceOrientation, true);
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			if (this.IsShowingMaster)
			{
				this.MasterViewController.ViewWillAppear (animated);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.ViewWillAppear (animated);
			}	
			this.bReconfigurePopup = true;
			this.LayoutSubviews ();
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			if (this.IsShowingMaster)
			{
				this.MasterViewController.ViewDidAppear (animated);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.ViewDidAppear (animated);
			}	
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			
			if (this.IsShowingMaster)
			{
				this.MasterViewController.ViewWillDisappear (animated);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.ViewWillDisappear (animated);
			}	
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			
			if (this.IsShowingMaster)
			{
				this.MasterViewController.ViewDidDisappear (animated);
			}
			if (this.DetailViewController != null)
			{
				this.DetailViewController.ViewDidDisappear (animated);
			}	
		}
		
		/// <summary>
		/// Gets the split view size for a sepcific orientation.
		/// </summary>
		/// <returns>
		/// The view size for orientation.
		/// </returns>
		/// <param name='theOrientation'>
		/// The orientation.
		/// </param>
		private SizeF SplitViewSizeForOrientation (UIInterfaceOrientation theOrientation)
		{
			UIScreen oScreen = UIScreen.MainScreen;
			RectangleF oFullScreenRect = oScreen.Bounds; // always implicitly in Portrait orientation.
			RectangleF oAppFrame = oScreen.ApplicationFrame;
	
			// Find status bar height by checking which dimension of the applicationFrame is narrower than screen bounds.
			// Little bit ugly looking, but it'll still work even if they change the status bar height in future.
			float fStatusBarHeight = Math.Max ((oFullScreenRect.Width - oAppFrame.Width), (oFullScreenRect.Height - oAppFrame.Height));
	
			// Initially assume portrait orientation.
			float fWidth = oFullScreenRect.Width;
			float fHeight = oFullScreenRect.Height;
	
			// Correct for orientation.
			if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight)
			{
				fWidth = fHeight;
				fHeight = oFullScreenRect.Width;
			}
	
			// Account for status bar, which always subtracts from the height (since it's always at the top of the screen).
			fHeight -= fStatusBarHeight;
	
			return new SizeF (fWidth, fHeight);
		}
		
		/// <summary>
		/// Layouts the subviews.
		/// </summary>
		private void LayoutSubviewsForInterfaceOrientation (UIInterfaceOrientation eOrientation, bool animate)
		{
			if (this.bReconfigurePopup)
			{
				this.ReconfigureForMasterInPopover (!this.ShouldShowMasterForInterfaceOrientation (eOrientation));
			}
	
			// Layout the master, detail and divider views appropriately, adding/removing subviews as needed.
			// First obtain relevant geometry.
			SizeF oFullSize = this.SplitViewSizeForOrientation (eOrientation);
			float width = oFullSize.Width;
			float height = oFullSize.Height;
	
#if DEBUG
	Console.WriteLine("Target orientation is " + this.NameOfInterfaceOrientation(eOrientation) + " dimensions will be " + width + " x " + height );
#endif
			
			// Layout the master, divider and detail views.
			RectangleF eNewFrame = new RectangleF (0, 0, width, height);
			UIViewController oController;
			UIView oView = null;
			bool bShouldShowMaster = this.ShouldShowMasterForInterfaceOrientation (eOrientation);
			bool bMasterFirst = this.MasterBeforeDetail;
			if (this.IsVertical)
			{
				// Master on left, detail on right (or vice versa).
				RectangleF oMasterRect;
				RectangleF oDividerRect;
				RectangleF oDetailRect;
				if (bMasterFirst)
				{
					if (!bShouldShowMaster)
					{
						// Move off-screen.
						eNewFrame.X -= (this.SplitPosition + this.SplitWidth);
					}
					eNewFrame.Width = this.SplitPosition;
					oMasterRect = eNewFrame;
			
					eNewFrame.X += eNewFrame.Width;
					eNewFrame.Width = this.SplitWidth;
					oDividerRect = eNewFrame;
			
					eNewFrame.X += eNewFrame.Width;
					eNewFrame.Width = width - eNewFrame.X;
					oDetailRect = eNewFrame;
				}
				else
				{
					if (!bShouldShowMaster)
					{
						// Move off-screen.
						eNewFrame.Width += (this.SplitPosition + this.SplitWidth);
					}
			
					eNewFrame.Width -= (this.SplitPosition + this.SplitWidth);
					oDetailRect = eNewFrame;
			
					eNewFrame.X += eNewFrame.Width;
					eNewFrame.Width = this.SplitWidth;
					oDividerRect = eNewFrame;
			
					eNewFrame.X += eNewFrame.Width;
					eNewFrame.Width = this.SplitPosition;
					oMasterRect = eNewFrame;
				}
		
				// Position master.
				oController = this.MasterViewController;
				if (oController != null)
				{
					oView = oController.View;
					if (oView != null)
					{
						oView.Frame = oMasterRect;
						if (oView.Superview == null)
						{
							oController.ViewWillAppear (false);
							this.View.AddSubview (oView);
							oController.ViewDidAppear (false);
						}
					}
				}
		
				// Position divider.
				if (oView != null)
				{
					oView = this.DividerView;
					oView.Frame = oDividerRect;
					if (oView.Superview == null)
					{
						this.View.AddSubview (oView);
					}
				}
		
				// Position detail.
				oController = this.DetailViewController;
				if (oController != null)
				{
					oView = oController.View;
					if (oView != null)
					{
						oView.Frame = oDetailRect;
						if (oView.Superview == null)
						{
							this.View.InsertSubviewAbove (oView, this.MasterViewController.View);
						}
						else
						{
							this.View.BringSubviewToFront (oView);
						}
					}
				}
			}
			else
			{
				// Master above, detail below (or vice versa).
				RectangleF oMasterRect, oDividerRect, oDetailRect;
				if (bMasterFirst)
				{
					if (!bShouldShowMaster)
					{
						// Move off-screen.
						eNewFrame.Y -= (this.SplitPosition + this.SplitWidth);
					}
			
					eNewFrame.Height = this.SplitPosition;
					oMasterRect = eNewFrame;
			
					eNewFrame.Y += eNewFrame.Height;
					eNewFrame.Height = this.SplitWidth;
					oDividerRect = eNewFrame;
			
					eNewFrame.Y += eNewFrame.Height;
					eNewFrame.Height = height - eNewFrame.Y;
					oDetailRect = eNewFrame;
				}
				else
				{
					if (!bShouldShowMaster)
					{
						// Move off-screen.
						eNewFrame.Height += (this.SplitPosition + this.SplitWidth);
					}
			
					eNewFrame.Height -= (this.SplitPosition + this.SplitWidth);
					oDetailRect = eNewFrame;
			
					eNewFrame.Y += eNewFrame.Height;
					eNewFrame.Height = this.SplitWidth;
					oDividerRect = eNewFrame;
			
					eNewFrame.Y += eNewFrame.Height;
					eNewFrame.Height = this.SplitPosition;
					oMasterRect = eNewFrame;
				}
		
				// Position master.
				oController = this.MasterViewController;
				if (oController != null)
				{
					oView = oController.View;
					if (oView != null)
					{
						oView.Frame = oMasterRect;
						if (oView.Superview == null)
						{
							oController.ViewWillAppear (false);
							this.View.AddSubview (oView);
							oController.ViewDidAppear (false);
						}
					}
				}
		
				// Position divider.
				if (oView != null)
				{
					oView = this.DividerView;
					oView.Frame = oDividerRect;
					if (oView.Superview == null)
					{
						this.View.AddSubview (oView);
					}
				}
		
				// Position detail.
				oController = this.DetailViewController;
				if (oController != null)
				{
					oView = oController.View;
					if (oView != null)
					{
						oView.Frame = oDetailRect;
						if (oView.Superview == null)
						{
							this.View.InsertSubviewAbove (oView, this.MasterViewController.View);
						}
						else
						{
							this.View.BringSubviewToFront (oView);
						}
					}
				}
			}
	
			// Create corner views if necessary.
			MTSplitCornersView oLeadingCorners; // top/left of screen in vertical/horizontal split.
			MTSplitCornersView oTrailingCorners; // bottom/right of screen in vertical/horizontal split.
			if (this.CornerViews == null)
			{
				RectangleF oCornerRect = new RectangleF (0, 0, 10, 10); // arbitrary, will be resized below.
				oLeadingCorners = new MTSplitCornersView (oCornerRect);
				oLeadingCorners.SplitViewController = this;
				oLeadingCorners.CornerBackgroundColor = MG_DEFAULT_CORNER_COLOR;
				oLeadingCorners.CornerRadius = MG_DEFAULT_CORNER_RADIUS;
				oTrailingCorners = new MTSplitCornersView (oCornerRect);
				oTrailingCorners.SplitViewController = this;
				oTrailingCorners.CornerBackgroundColor = MG_DEFAULT_CORNER_COLOR;
				oTrailingCorners.CornerRadius = MG_DEFAULT_CORNER_RADIUS;
				this.CornerViews = new MTSplitCornersView[]{ oLeadingCorners, oTrailingCorners };
			}
			else
			{
				oLeadingCorners = this.CornerViews [0];
				oTrailingCorners = this.CornerViews [1];
			}
	
			// Configure and layout the corner-views.
			oLeadingCorners.CornersPosition = (this.IsVertical) ? MTSplitCornersView.CORNERS_POSITION.LeadingVertical : MTSplitCornersView.CORNERS_POSITION.LeadingHorizontal;
			oTrailingCorners.CornersPosition = (this.IsVertical) ? MTSplitCornersView.CORNERS_POSITION.TrailingVertical : MTSplitCornersView.CORNERS_POSITION.TrailingHorizontal;
			oLeadingCorners.AutoresizingMask = (this.IsVertical) ? UIViewAutoresizing.FlexibleBottomMargin : UIViewAutoresizing.FlexibleRightMargin;
			oTrailingCorners.AutoresizingMask = (this.IsVertical) ? UIViewAutoresizing.FlexibleTopMargin : UIViewAutoresizing.FlexibleLeftMargin;
	
			float fX;
			float fY;
			float fCornersWidth;
			float fCornersHeight;
	
			RectangleF oLeadingRect;
			RectangleF oTrailingRect;
			
			float fRadius = oLeadingCorners.CornerRadius;
			if (this.IsVertical)
			{
				// left/right split
				fCornersWidth = (fRadius * 2.0f) + this.SplitWidth;
				fCornersHeight = fRadius;
				fX = ((bShouldShowMaster) ? ((bMasterFirst) ? this.SplitPosition : width - (this.SplitPosition + this.SplitWidth)) : (0 - this.SplitWidth)) - fRadius;
				fY = 0;
				oLeadingRect = new RectangleF (fX, fY, fCornersWidth, fCornersHeight); // top corners
				oTrailingRect = new RectangleF (fX, (height - fCornersHeight), fCornersWidth, fCornersHeight); // bottom corners
		
			}
			else
			{
				// top/bottom split
				fX = 0;
				fY = ((bShouldShowMaster) ? ((bMasterFirst) ? this.SplitPosition : height - (this.SplitPosition + this.SplitWidth)) : (0 - this.SplitWidth)) - fRadius;
				fCornersWidth = fRadius;
				fCornersHeight = (fRadius * 2.0f) + this.SplitWidth;
				oLeadingRect = new RectangleF (fX, fY, fCornersWidth, fCornersHeight); // left corners
				oTrailingRect = new RectangleF ((width - fCornersWidth), fY, fCornersWidth, fCornersHeight); // right corners
			}
	
			oLeadingCorners.Frame = oLeadingRect;
			oTrailingCorners.Frame = oTrailingRect;
	
			// Ensure corners are visible and frontmost.
			if (this.DetailViewController != null)
			{
				if (oLeadingCorners.Superview == null)
				{
					this.View.InsertSubviewAbove (oLeadingCorners, this.DetailViewController.View);
					this.View.InsertSubviewAbove (oTrailingCorners, this.DetailViewController.View);
				}
				else
				{
					this.View.BringSubviewToFront (oLeadingCorners);
					this.View.BringSubviewToFront (oTrailingCorners);
				}
			}
		}
		
		/// <summary>
		/// Reconfigures the controller to show master controller in a popover or as a real pane.
		/// </summary>
		/// <param name='bInPopover'>
		/// B in popover.
		/// </param>
		private void ReconfigureForMasterInPopover (bool bInPopover)
		{
			this.bReconfigurePopup = false;
		
			if ((bInPopover && this.HiddenPopoverController != null)
			|| (!bInPopover && this.HiddenPopoverController == null) || this.MasterViewController == null)
			{
				// Nothing to do.
				return;
			}
		
			if (bInPopover && this.HiddenPopoverController == null && this.BarButtonItem == null)
			{
				// Create and configure popover for our masterViewController.
				if(this.HiddenPopoverController != null)
				{
					this.HiddenPopoverController.Dispose ();
				}
				this.HiddenPopoverController = null;
					
				this.MasterViewController.ViewWillDisappear (false);
				this.HiddenPopoverController = new  UIPopoverController (this.MasterViewController);
				this.MasterViewController.ViewDidDisappear (false);
			
				// Create and configure _barButtonItem.
				this.BarButtonItem = new UIBarButtonItem ("Master", UIBarButtonItemStyle.Bordered, this.ShowMasterPopover);
			
				// Inform delegate of this state of affairs.
				if (this.WillHideViewController != null)
				{
					this.WillHideViewController (this, this.MasterViewController, this.BarButtonItem, this.HiddenPopoverController);
				}
			}
			else if (!bInPopover && this.HiddenPopoverController != null && this.BarButtonItem != null)
			{
				// I know this looks strange, but it fixes a bizarre issue with UIPopoverController leaving masterViewController's views in disarray.
				this.HiddenPopoverController.PresentFromRect (RectangleF.Empty, this.View, UIPopoverArrowDirection.Any, false);
		
				// Remove master from popover and destroy popover, if it exists.
				this.HiddenPopoverController.Dismiss (false);
				this.HiddenPopoverController.Dispose ();
				this.HiddenPopoverController = null;
		
				// Inform delegate that the _barButtonItem will become invalid.
				if (this.WillShowViewController != null)
				{
					this.WillShowViewController (this, this.MasterViewController, this.BarButtonItem);
				}
			
				// Destroy _barButtonItem.
				if(this.BarButtonItem != null)
				{
					this.BarButtonItem.Dispose ();
				}
				this.BarButtonItem = null;
			
				// Move master view.
				UIView masterView = this.MasterViewController.View;
				if (masterView != null && masterView.Superview != this.View)
				{
					masterView.RemoveFromSuperview ();
				}
			}
		}
		
		private void PopoverControllerDidDismissPopover (UIPopoverController popoverController)
		{
			this.ReconfigureForMasterInPopover (false);
		}

		/// <summary>
		/// Toggles the split orientation.
		/// </summary>
		public void ToggleSplitOrientation ()
		{
			bool bShowingMaster = this.IsShowingMaster;
			if (bShowingMaster)
			{
				if (this.CornerViews != null)
				{
					foreach (UIView oCorner in this.CornerViews)
					{
						oCorner.Hidden = true;
					}
					this.DividerView.Hidden = true;
				}
				UIView.Animate(0.1f, delegate
				{
					this.IsVertical = !this.IsVertical;
				},
				delegate
				{
					foreach (UIView oCorner in this.CornerViews)
					{
						oCorner.Hidden = false;
					}
					this.DividerView.Hidden = false;
				});
			}
		}
		
		/// <summary>
		/// Toggles the master before detail.
		/// </summary>
		public void ToggleMasterBeforeDetail ()
		{
			bool bShowingMaster = this.IsShowingMaster;
			if (bShowingMaster)
			{
				if (this.CornerViews != null)
				{
					foreach (UIView oCorner in this.CornerViews)
					{
						oCorner.Hidden = true;
					}
					this.DividerView.Hidden = true;
				}
				UIView.Animate(0.1f, delegate
				{
					this.MasterBeforeDetail = !this.MasterBeforeDetail;
				},
				delegate
				{
					foreach (UIView oCorner in this.CornerViews)
					{
						oCorner.Hidden = false;
					}
					this.DividerView.Hidden = false;
				});
			}
		}
		
		/// <summary>
		/// Toggles the visibility of the master view.
		/// </summary>
		public void ToggleMasterView ()
		{
			if (this.HiddenPopoverController != null && this.HiddenPopoverController.PopoverVisible)
			{
				this.HiddenPopoverController.Dismiss (false);
			}
	
			if (!this.IsShowingMaster)
			{
				// We're about to show the master view. Ensure it's in place off-screen to be animated in.
				this.bReconfigurePopup = true;
				this.ReconfigureForMasterInPopover (false);
				this.LayoutSubviews ();
			}
	
			// This action functions on the current primary orientation; it is independent of the other primary orientation.
			UIView.BeginAnimations ("toggleMaster");
			if (this.IsLandscape)
			{
				this.ShowsMasterInLandscape = !this.ShowsMasterInLandscape;
			}
			else
			{
				this.ShowsMasterInPortrait = !this.ShowsMasterInPortrait;
			}
			UIView.CommitAnimations ();
		}

		/// <summary>
		/// Called from the bar button item to show the master controller in a popover.
		/// </summary>
		public void ShowMasterPopover (object sender, EventArgs args)
		{
			if (this.HiddenPopoverController != null && !this.HiddenPopoverController.PopoverVisible)
			{
				// Inform delegate.
				if (this.WillPresentViewController != null)
				{
					this.WillPresentViewController (this, this.HiddenPopoverController, this.MasterViewController);
				}
				// Show popover.
				this.HiddenPopoverController.PresentFromBarButtonItem (this.BarButtonItem, UIPopoverArrowDirection.Any, true);
			}
		}
	}
}