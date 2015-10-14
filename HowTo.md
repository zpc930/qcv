

# Stop the Runtime from Within a Filter #

Stopping the runtime can be accomplished through the <tt>QCV.Base.IRuntime</tt> interface. An instance of the described interface can be found in the bundle provided.

```
// qcv.exe -s stop_runtime.cs Example.Basic.StopRuntime --run

using System;
using System.Collections.Generic;

using QCV.Base;
using QCV.Base.Extensions;

namespace Example.Basic {

  [Addin]
  public class StopRuntime : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        this
      };
    }

    public void Execute(Dictionary<string, object> bundle) {
      // Request a stop of the runtime.
      // The runtime will process the request as soon as possible,
      // but not before the filter has completed its work.
      bool request_ok = bundle.GetRuntime().RequestStop();
      
      if (!request_ok) {
        // In case the request wasn't posted with success and stopping
        // is a must one can force the runtime to stop by triggering
        // an exception.
        throw new ApplicationException("Failed to request a stop of runtime.");
      }
    }

  }
}
```




# Stay Responsive to Cancellation Requests #

When you have a long running operation carried out in your filter, make sure that you periodically test for a pending cancellation request. In case a cancellation event is pending, gracefully abort the filters work. Doing so ensures a responsive behaviour of the runtime.

Here is an example that imitates a long running operation by an endless loop.

```
// qcv.exe -s stay_responsive.cs Example.Design.StayResponsive --run

using System;
using System.Collections.Generic;

using QCV.Base;
using QCV.Base.Extensions;

namespace Example.Design {

  [Addin]
  public class StayResponsive : IFilter, IFilterListProvider {

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        this
      };
    }

    public void Execute(Dictionary<string, object> bundle) {

      // If you have a long running operation, make sure 
      // to stay responsive to cancellation events.

      while (true) {
        // Test if a stop request is pending
        if (bundle.GetRuntime().StopRequested) {
          return;
        }

        System.Threading.Thread.Sleep(50);
      }
    }

  }
}
```

# Writing Log Entries #

QCV uses the [log4net](http://logging.apache.org/log4net/index.html) project for logging purposes. From log4net hompage:
> log4net is a tool to help the programmer output log statements to a variety of output targets. log4net is a port of the excellent log4j framework to the .NET runtime.

```
// qcv.exe -s logging.cs Example.Basic.Logging --run

using System;
using System.Collections.Generic;
using log4net;

using QCV.Base;
using QCV.Base.Extensions;

namespace Example.Basic {

  [Addin]
  public class Logging : IFilter, IFilterListProvider {
    /// <summary>
    /// Logger for logging purposes.
    /// <summary/>
    private static readonly ILog _logger = LogManager.GetLogger(typeof(Logging));

    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        this
      };
    }

    public void Execute(Dictionary<string, object> bundle) {
      _logger.Info("Hello World!");

      bundle.GetRuntime().RequestStop();
    }

  }
}
```

The logging output is printed to the Console Tab in <tt>qcv.exe</tt>. What is being printed to the console is currently configured globally in the <tt>qcv.log4net</tt> file that resides in the applications binary folder.


# Using Addins #

QCV is based around a simple plugin framework. Plugins are called addins in QCV. When creating filter lists, you may not have a direct assembly reference to all filters you want to initialize. In this case you can use the <tt>QCV.Base.AddinHost</tt> to find and create instance of filters by name.

```
// qcv.exe -s using_addins.cs Example.Basic.UsingAddins --run

using System;
using System.Collections.Generic;
using log4net;

using QCV.Base;
using QCV.Base.Extensions;

namespace Example.Basic {

  /// <summary>
  /// An dummy filter
  /// </summary>
  /// <remarks>Make sure to flag your addins with the 
  /// <see cref="QCV.Base.Addins.AddinAttribute"/> and provide a 
  /// public class modifier.</remarks>
  [Addin]
  public class AddinFilter : IFilter {
    public void Execute(Dictionary<string, object> bundle) {
      Console.WriteLine("Hello World!");
    }
  }
  
  [Addin]
  public class UsingAddins : IFilterListProvider {
    
    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        // Create an instance of QCV.Toolbox.Camera
        host.CreateInstance<IFilter>("QCV.Toolbox.Camera", new object[] {0,320,200,"camera"} ),
        // Create an instance of "Example.Basic.AddinFilter"
        host.CreateInstance<IFilter>("Example.Basic.AddinFilter")
      };
    }
  }
}
```

By default <tt>qcv.exe</tt> scans the working directory for assemblies containing addins and all loaded assemblies in the current application domain.

| **Tip** If your target assemblies reside in directories not scanned by <tt>qcv.exe</tt>  you can pass those directories as command line arguments to <tt>qcv.exe</tt> via the <tt>-a</tt> parameter. |
|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|