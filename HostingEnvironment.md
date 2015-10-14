QCV provides a graphical user interface application for hosting and running filters. This application is started by
```
qcv.exe
```

The following image annotates parts of <tt>qcv.exe</tt>.

![http://qcv.googlecode.com/svn/trunk/etc/doc/qcv-callouts.png](http://qcv.googlecode.com/svn/trunk/etc/doc/qcv-callouts.png)

<tt>qcv.exe</tt> provides
  * creation <tt>QCV.Base.FilterList</tt> from <tt>QCV.Base.IFilterListProvider</tt>,
  * control mechanisms for processing filter lists (Start/Stop),
  * viewing and modifying properties of <tt>QCV.Base.IFilter</tt> instances,
  * triggering of filter event notifications,
  * saving and loading the state of filter lists, and
  * on-the-fly compilation of filters in source form.

<tt>qcv.exe</tt> supports the following command line parameters.

```
Usage: qcv.exe [OPTIONS] IFilterListProvider [,IFilterListProvider]*
Options:
  -a, --addin-path=PATH      load addins from assembly located at PATH
  -r, --reference=ASSEMBLY   provide ASSEMBLY as reference for compilation
  -l, --load=PATH            load persisted FilterList from PATH
  -s, --source=PATH          compile PATH and load as possible addin
      --run                  immediately start executing
      --shutdown             automatically exit application when runtime stops
      --debug                enable attaching a debugger
      --fps=VALUE            target FPS to achieve
      --nofps                disable FPS control
  -h, --help                 show this help
```