The following tutorial will incrementally build an application clalled <tt>ImageDecorator</tt> in eight steps in QCV. Each step will introduce new concepts supported in QCV.

Having completed all eight steps the <tt>ImageDecorator</tt> application will be able to process images from a camera and draw a user defined border around these images. These images will be visualized to provide a user feedback. Optionally, the user has the ability to save images to disk.

You might want to read the [Design](Design.md) page to get an overview of QCV before continuing with the tutorial.



# Hello World #

Every programming language starts with a so called HelloWorld. We can do the same for QCV and write a filter that does nothing, except writing 'HelloWorld' to the console.

Open a file called <tt>image_decorator.cs</tt> in your favorite editor and paste the following code.

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

// QCV namespaces
using QCV.Base;
using QCV.Base.Extensions;

// Emgu namespaces
using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {
 
  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    // Create a filter list containing just us.
    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        this,
      };
    }

    // On each execution of this filter print a console message.
    public void Execute(Dictionary<string, object> b) {
      Console.WriteLine("Hello World");
      b.GetRuntime.RequestStop();
    }
  }
}
```

In the above example, <tt>ImageDecorator</tt> implements the <tt>QCV.Base.IFilter</tt> and <tt>QCV.Base.IFilterListProvider</tt> interface. In our HelloWorld scenario, the filter list contains a single filter, <tt>ImageDecorator</tt> itself. The filter implementation is pretty unspectacular as it just prints a console message.

Run the example by typing
```
qcv.exe -s image_decorator.cs Example.ImageDecorator
```
in your command line. This will tell <tt>qcv.exe</tt> to load and compile <tt>image_decorator.cs</tt> and use instances of <tt>Example.ImageDecorator</tt> as provider for filter lists.

Next press 'Run' to start processing the filter list. The runtime will invoke each filter in the list as retrieved by calling <tt>Example.ImageDecorator.CreateFilterList</tt>. In our case the filter list is composed of a single filter that will print a message and use the bundle parameter to request a stop of the runtime.

| **Tip** <tt>qcv.exe</tt> automatically compiles all sources passed by the <tt>-s</tt> command line switch and makes them available for usage. Whether compilation succeeds or failed can be seen in the 'Console'. <tt>qcv.exe</tt> detects file modifications of all sources. In case a change is detected all sources are recompiled and on success, existing objects are replaced with instances of their new types.|
|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

| **Tip** To execute the filter list immediately after starting <tt>qcv.exe</tt> supply the <tt>--run</tt> argument.|
|:------------------------------------------------------------------------------------------------------------------|

# Camera Input #

QCV provides a toolbox commonly used filters. Among those filters are so called sources that read input from devices. On such source is called <tt>QCV.Toolbox.Camera</tt> and supports reading images from cameras.

To read images from a camera, an instance of <tt>QCV.Toolbox.Camera</tt> needs to be initialized. All sources have a name attribute that allows you to customize the key that is used to place the image in the bundle.

Modify the previous example to become

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      Console.WriteLine(String.Format("Size: {0}", image.Size));
    }
  }
}
```

QCV ships with support for the following image input sources
  * <tt>QCV.Toolbox.Camera</tt> - Images from a Video-For-Windows camera device.
  * <tt>QCV.Toolbox.Video</tt> -  Images from a video stored at disk.
  * <tt>QCV.Toolbox.ImageList</tt> - Images from a collection of images stored at disk.

| **Tip** You can read and modify the properties of filters by selecting the filter from the dropdown box in <tt>qcv.exe</tt>.|
|:----------------------------------------------------------------------------------------------------------------------------|

| **Tip** To connect to multiple sources, simply put more such sources in your filter list and make sure that they produce images under a different key name. |
|:------------------------------------------------------------------------------------------------------------------------------------------------------------|

# Showing Images #

To visualize the images produced by the camera we will use the <tt>Show</tt> method of the <tt>QCV.Base.IDataInteractor</tt> interface. An instance of a class that implements this interface is stored in the bundle parameter that is passed to our filter.

Modify your code to become

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      // Get the interactor and show the image
      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
    }
  }
}
```

Showing requires a name to pass along, 'camera input' in our case. Since our filter is executed in a loop, we'd like to use the same window to visualize an updated version of the camera image to achieve the effect of a live video. <tt>qcv.exe</tt> is smart enough to reuse visualizations when duplicate names are passed. If you'd like to open a second visualization showing with equal content, modify the <tt>Execute</tt> to become

```
    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      // Get the interactor and show the image
      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
      idi.Show("another window", image);
    }
```

# Implementing the Algorithm #

The next step will add code to modify the image to draw a fixed border around it.

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      // Modify the image
      image.Draw(new Rectangle(Point.Empty, image.Size), new Bgr(Color.Red), 10);

      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
    }
  }
}
```

Try changing the color and thickness in your source while <tt>qcv.exe</tt> is running. It should detect changes to your file, recompile and on success exchange types at runtime.

# Algorithm Parameters #

The previous version of our algorithm specifies the thickness and the color of the border.   Actually these constants should become variables that the user can modify. Such user definable parameters are implemented by properties. <tt>qcv.exe</tt> will allow the user to read and modify the state of such properties.

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    private int _thickness = 10;
    [Description("Specifies the thickness of the border drawn.")]
    public int Thickness {
      get { return _thickness; }
      set { _thickness = value; }
    }

    private Color _color = Color.Red;
    [Description("Specifies the fill color of the border.")]
    public Color Color {
      get { return _color; }
      set { _color = value; }
    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      image.Draw(new Rectangle(Point.Empty, image.Size), new Bgr(_color), _thickness);

      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
    }
  }
}
```

When running, select the <tt>ImageDecorator</tt> filter in the drop-down box of <tt>qcv.exe</tt> and try to modify the values.

Note that changing properties is asynchronous event. QCV currently reflects any change to a property directly to the instance of the object. Therefore you might need to synchronize access to the property yourself. In future versions of QCV property changes will be cached and executed when the filter asks for it (see [Tutorial#Event\_Notifications](Tutorial#Event_Notifications.md) for details.

| **Tip** <tt>qcv.exe</tt> uses <tt>System.Windows.Forms.PropertyGrid</tt> to show properties of the selected filter. As such, it supports all attributes that are known and processed by the <tt>PropertyGrid</tt> class. Consult the <tt>PropertyGrid</tt> documentation for details.|
|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

# Event Notifications #

Besides properties your filter can expose events that user can click. For example, <tt>ImageDecorator</tt> will expose an event that allows the user to save a screenshot of the current camera image combined with the border drawn.

Such events are implemented by defining a method called 'OnDoSomething' that takes a bundle as parameter and returns void. Such events are asynchronous by nature. QCV will not directly invoke your filter when an event occurs, i.e user clicks, but cache them.

When your filter is ready, it will execute the pending events. See below

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    private int _thickness = 10;
    [Description("Specifies the thickness of the border drawn.")]
    public int Thickness {
      get { return _thickness; }
      set { _thickness = value; }
    }

    private Color _color = Color.Red;
    [Description("Specifies the fill color of the border.")]
    public Color Color {
      get { return _color; }
      set { _color = value; }
    }

    // Save image event
    public void OnSaveImage(Dictionary<string, object> bundle) {
      bundle.GetImage("source").Save("source.png");
    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      image.Draw(new Rectangle(Point.Empty, image.Size), new Bgr(_color), _thickness);

      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
 
      // Ready to execute pending events
      idi.ExecutePendingEvents(this, bundle);
    }
  }
}
```

# Showing Arbitrary Values #

Besides image, QCV supports showing of arbitrary values using the <tt>Show</tt> method of <tt>QCV.Base.IDataInteractor</tt>. The way the variables are rendered depends on the available rendering capatibilities of the class that implements the data iteractor interface. Besides images, at least everything that converts to a string, via the <tt>Object.ToString</tt> method works.

What holds true for showing images is also true for showing arbitrary values. If a name occurs more than one time, the new value will override the visualization of the old value.

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    private int _thickness = 10;
    [Description("Specifies the thickness of the border drawn.")]
    public int Thickness {
      get { return _thickness; }
      set { _thickness = value; }
    }

    private Color _color = Color.Red;
    [Description("Specifies the fill color of the border.")]
    public Color Color {
      get { return _color; }
      set { _color = value; }
    }

    public void OnSaveImage(Dictionary<string, object> bundle) {
      // Filename based on current date and time
      string filename = String.Format("source_{0:yyyy-MM-dd_hh-mm-ss}.png", DateTime.Now);
      bundle.GetImage("source").Save(filename);

      IDataInteractor idi = bundle.GetInteractor();
      // Show the filename
      idi.Show("Last Image Saved", filename);
    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      image.Draw(new Rectangle(Point.Empty, image.Size), new Bgr(_color), _thickness);

      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
      idi.ExecutePendingEvents(this, bundle);
    }
  }
}
```


# Queries #

Finally, we'd like to extend <tt>ImageDecorator</tt> to allow the user to specify a location to save the screenshot to. We can achieve this by so called queries. Queries will  hold the execution of the workflow until the user positively responds (clicks Ok) or cancels.

You can attach any object to a query. The properties of this object are additional parameters the user has to specify. In our case this corresponds to specifying the directory and the filename.

```
// qcv.exe -s image_decorator.cs Example.ImageDecorator --run

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

using QCV.Base;
using QCV.Base.Extensions;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Example {

  [Addin]
  public class ImageDecorator : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        new QCV.Toolbox.Camera(0, 640, 480, "source"),
        this,
      };
    }

    private int _thickness = 10;
    [Description("Specifies the thickness of the border drawn.")]
    public int Thickness {
      get { return _thickness; }
      set { _thickness = value; }
    }

    private Color _color = Color.Red;
    [Description("Specifies the fill color of the border.")]
    public Color Color {
      get { return _color; }
      set { _color = value; }
    }

    public void OnSaveImage(Dictionary<string, object> bundle) {
      IDataInteractor idi = bundle.GetInteractor();
      
      // Will be attached to the query
      FilePathInfo fni = new FilePathInfo();
      // Post the query
      if (idi.Query("Choose the filename", fni)) {
        // User responded positively
        string path = System.IO.Path.Combine(fni.Directory, fni.Filename);
        bundle.GetImage("source").Save(path);
        idi.Show("Last Image Saved", path);
      }

    }

    public void Execute(Dictionary<string, object> bundle) {
      Image<Bgr, byte> image = bundle.GetImage("source");
      image.Draw(new Rectangle(Point.Empty, image.Size), new Bgr(_color), _thickness);

      IDataInteractor idi = bundle.GetInteractor();
      idi.Show("camera input", image);
      idi.ExecutePendingEvents(this, bundle);
    }

    // Attached to query
    class FilePathInfo {
      private string _filename = "source.png";
      private string _directory = Environment.CurrentDirectory;

      [Description("Specify the directory to save to")]
      [EditorAttribute(
        typeof(System.Windows.Forms.Design.FolderNameEditor),
        typeof(System.Drawing.Design.UITypeEditor))]
      public string Directory {
        get { return _directory; }
        set { _directory = value; }
      }

      [Description("Specifiy the filename")]
      public string Filename {
        get { return _filename; }
        set { _filename = value; }
      }
    }
  }
}
```

You have successfully completed the tutorial!