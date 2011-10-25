//  MTSplitViewController
//  https://github.com/Krumelur/MTSplitViewController
// 
//  Ported to Monotouch by René Ruppert, October 2011
//
//  Based on code by Matt Gemmell on 26/07/2010.
//  Copyright 2010 Instinctive Code.
//  https://github.com/mattgemmell/MGSplitViewController

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MTSplitView;

namespace MTSplitViewControllerDemo
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		MTSplitViewController oSplitViewController;
		RootViewController oMasterController;
		DetailViewController oDetailController;
		UINavigationController oNavController;
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			this.oSplitViewController = new MTSplitViewController ();
			
			this.oMasterController = new RootViewController();
			this.oDetailController = new DetailViewController(this.oSplitViewController);
			this.oMasterController.DetailViewController = this.oDetailController;
			this.oNavController = new UINavigationController(this.oMasterController);
			
			
			this.oSplitViewController.ViewControllers = new UIViewController[] {this.oNavController, this.oDetailController };
			this.oSplitViewController.ShowsMasterInLandscape = true;
			this.oSplitViewController.ShowsMasterInPortrait = true;
			window.AddSubview(this.oSplitViewController.View);
	
			window.MakeKeyAndVisible ();
			
			if (false)
			{
				// whether to allow dragging the divider to move the split.
				this.oSplitViewController.SplitWidth = 15.0f; // make it wide enough to actually drag!
				this.oSplitViewController.AllowsDraggingDivider = true;
			}
	
			 this.oMasterController.SelectFirstRow();
				this.oDetailController.ConfigureView();
	
	
			return true;
		}
	}
}

