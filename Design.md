Simply speaking, QCV processes a list of objects in a loop until a stopping criterium is met.

These objects are termed filters and have to implement the <tt>QCV.Base.IFilter</tt> interface. The chronological order of filters is determined by their absolute position in an instance of <tt>QCV.Base.FilterList</tt>. The filter list represents a sequential 'list' ordering of the filters to be executed. Classes that support creating of filter lists have to implement <tt>QCV.Base.IFilterListProvider</tt>.

Such list of filters can then be passed to a <tt>QCV.Base.Runtime</tt> to process the filters asynchronously. The runtime invokes each filter and passes a so called bundle, an open dictionary of objects, containing parameters for the filter.

The filter can read from and write to this bundle to communicate with other filters in the current filter list. To interact with the user, an instance of <tt>QCV.Base.IDataInteractor</tt> is contained in the filters bundle.

The data interactor allows the filter to
  * expose events,
  * show values and images to the user,
  * query values from the user.

The runtime stops processing of the filter list if one of the following conditions are met
  * a filter requests the stop,
  * a user requests stops the runtime, or
  * an exception occurred while processing one of the filters.

QCV makes heavy use of plugins, termed addins in QCV. Any class carrying the <tt>QCV.Base.Addins.AddinAttribute</tt> is considered an addin. QCV can detects plugins in loaded assemblies, assemblies stored at disk, and assemblies generated on-the-fly by compiling code.

Addins are collected in instances of <tt>QCV.Base.Addins.AddinHost</tt>. The host supports addin queries and is responsible for creating addins. All filters should be addins, so that they can be created by filter list providers implemented as addin.