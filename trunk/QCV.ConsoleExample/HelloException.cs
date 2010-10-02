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
  public class HelloException : IExample {

    public void Run(string[] args) {

      QCV.Base.FilterList f = new QCV.Base.FilterList();
      f.Add(
        new QCV.Base.AnonymousFilter(
          (b, ev) => {
            throw new Exception("ups!");
          })
      );

      QCV.Base.Runtime runtime = new QCV.Base.Runtime(
        new QCV.Base.ConsoleInteraction()
      );
      runtime.Run(f, 10);

      Console.WriteLine(runtime.Error.Message);
      Console.WriteLine("Check the log files.");
    }
  }
}
