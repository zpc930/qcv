// qcv.exe -s event_notifications.cs Tutorial.EventNotifications --run

using System;
using System.Collections.Generic;
using System.Diagnostics;

using QCV.Base;
using QCV.Base.Addins;
using QCV.Base.Extensions;

namespace Tutorial {

  [Addin]
  public class EventNotifications : IFilter, IFilterListProvider {
    
    public FilterList CreateFilterList(AddinHost host) {
      return new FilterList() {
        this
      };
    }
    
    public void OnPrintGreeting(Dictionary<string, object> bundle) {
      Console.WriteLine("Hello EventNotifications!");
    }
    
    public void Execute(Dictionary<string, object> bundle) {
      IDataInteractor idi = bundle.GetInteractor();
      // Process all pending events, supplying them with the current bundle information
      idi.ExecutePendingEvents(this, bundle);
    }
    
  }
}