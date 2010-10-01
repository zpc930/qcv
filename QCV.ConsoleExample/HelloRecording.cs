﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using QCV.Base.Extensions;
using Emgu.CV;
using Emgu.CV.Structure;

namespace QCV.ConsoleExample {

  [Base.Addins.Addin]
  public class HelloRecording : IExample {

    public void Run(string[] args) {
      QCV.Toolbox.Sources.Camera c = new QCV.Toolbox.Sources.Camera();
      c.Name = "camera";
      c.DeviceIndex = 0;
      c.FrameWidth = 640;
      c.FrameHeight = 480;

      QCV.Toolbox.ShowImage si = new QCV.Toolbox.ShowImage();
      si.BagName = "camera";

      QCV.Toolbox.RecordVideo rv = new QCV.Toolbox.RecordVideo();
      rv.VideoPath = "myvideo.avi";
      rv.BagName = "camera";
      rv.FPS = 30;
      rv.FrameWidth = 160;
      rv.FrameHeight = 100;

      QCV.Base.FilterList f = new QCV.Base.FilterList();
      f.Add(c);
      f.Add(si);
      f.Add(rv);
        
      QCV.Base.Runtime runtime = new QCV.Base.Runtime();
      runtime.FPS = 30.0;
      runtime.Run(f, new QCV.Base.ConsoleInteraction(), 3);
    }
  }
}
