MTSplitViewController
=====================

MTSplitViewController is a replacement for UISplitViewController, with various useful enhancements.
It is a 1:1 port of Matt Gemmell's brilliant MGSplitViewController (https://github.com/mattgemmell/MGSplitViewController)

I needed a substitute for the limited UISplitViewController and read a lot about MGSplitViewController on the web.
Unfortunately, the code was ObjectiveC only. There are bindings available to use the ObjC version in Monotouch but I don't like
dealing with Xcode and I don't like dealing external bindings, so I decided to port the whole thing over to Monotouch.

This code has been tested with Monodevelop 2.8 and Monotouch 4.2 on the iOS simulator and on an iPad 2.

You can use the code in private or commercial applications without limitations from my side, however I would be happy
to receive information or a screenshot of the application you built.

Donations
---------

If you find this code useful please donate to Matt Gemmell, as he is the original inventor and writer of the ObjectiveC version.
A Paypal donation (or something from his Amazon.co.uk Wishlist) would be very much appreciated.
Appropriate links can be found here: <http://mattgemmell.com/source>


Features
--------

Please note that, since split-views are commonly used for "Master-Detail" interfaces,
I call the first sub-view the "master" and the second sub-view the "detail".

- Unlike MGSplitViewController, the Monotouch implementation does not mimic the delegate interface but instead
  offers C# events to attach to UI changes.
- MTSplitViewController is not limited to iPad use only but will (with some code changes involved, like removing the iPad only UIPopoverController) work on iPhone.
- MTSplitViewController mimics the appearance and (complete) behaviour of UISplitViewController.
  It accepts two UIViewControllers (or subclasses thereof).
- Allows toggling the visibility of the master view in either interface-orientation;
  i.e. you can have master-detail or detail-only in either landscape and/or portrait orientations (independently, and/or interactively).
- Allows choosing whether the split orientation is vertical (i.e. left/right, like UISplitViewController),
  or horizontal (master above, and detail below). You can toggle between modes interactively, with animation.
- Allows choosing whether the master view is before (above, or to left of) the detail view,
  or after it (below, or to the right).
- Allows you to choose (and change) the position of the split, i.e. the relative sizes of the master and detail views.
- Allows you to enable dragging of the split/divider between the master and detail views,
  with optional constraining via a delegate method.
- Allows you to choose the width of the split between the master and detail views.
- Preset divider styles: one for non-draggable UISplitViewController-like dividers, and one for draggable, thicker style with a grip-strip.
- Allows you to substitute your own divider-view (an MTSplitDividerView subclass), used to draw the split between the master and detail views.


How to use
----------

This Monodevelop 2.8 project contains a XIB free example of how to use the controller.


Interface Builder support
-------------------------

I have not used MTSplitViewController with Interface Builder.

License and Warranty
--------------------

The license for the code is included with the project; it's basically a BSD license with attribution.

You're welcome to use it in commercial, closed-source, open source, free or any other kind of software, as long as you credit me appropriately.

The MTSplitViewController code comes with no warranty of any kind.
I hope it'll be useful to you (it certainly is to me), but I make no guarantees regarding its functionality or otherwise.


Support / Contact / Bugs / Features
-----------------------------------

Please email me about issues or bugs or just to say thanks if you find the code useful.
If you create an app which uses the code, I'd also love to hear about it. 

Likewise, if you want to submit a feature request or bug report, feel free to get in touch. Better yet,
fork the code and implement the feature/fix yourself, then submit a pull request.

Enjoy the code!


Cheers,  
René Ruppert


Contact René Ruppert:
Email: rene.ruppert@gmail.com

Contact Matt Gemmell:
Writing: http://mattgemmell.com/  
Contact: http://mattgemmell.com/about  
Twitter: http://twitter.com/mattgemmell  
Hire Me: http://instinctivecode.com/