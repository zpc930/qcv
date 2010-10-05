﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using NDesk.Options;
using System.CodeDom.Compiler;
using log4net.Config;
using log4net;

using QCV.Extensions;

namespace QCV {
  public partial class Main : Form {

    private static readonly ILog _logger = LogManager.GetLogger(typeof(Main));
    private HookableTextWriter _console_hook = new HookableTextWriter();
    private Base.InstantCompiler _ic = null;
    private Base.Addins.AddinHost _ah = null;
    private Dictionary<string, object> _env = null;
    private Base.FilterList _fl = null;
    private Base.Runtime _runtime = null;
    private CommandLine.CLIArgs _args = null;
    private bool _appexit_requested = false;

    public Main() {
      InitializeComponent();

      // Redirect console output
      _console_hook.StringAppendedEvent += new HookableTextWriter.StringAppendedEventHandler(ConsoleStringAppendedEvent);
      Console.SetOut(_console_hook);

      _query_ctrl.OnQueryBeginEvent += new QueryControl.OnQueryBeginEventHandler(OnQueryBeginEvent);
      _query_ctrl.OnQueryEndEvent += new QueryControl.OnQueryEndEventHandler(OnQueryEndEvent);

      // Configure logging
      XmlConfigurator.Configure(new System.IO.FileInfo("QCV.log4net"));

      // Parse command line
      CommandLine cl = new CommandLine();
      _args = cl.Args;

      // Compile all scripts
      _ic = new QCV.Base.InstantCompiler(
        _args.script_paths,
        _args.references.Union(new string[] { 
            "mscorlib.dll", "System.dll", "System.Drawing.dll", "System.Xml.dll",
            "QCV.Base.dll", "QCV.Toolbox.dll", "Emgu.CV.dll", "Emgu.Util.dll"}).Distinct(),
        _args.enable_debugger
      );
      _ic.BuildSucceededEvent += new QCV.Base.InstantCompiler.BuildEventHandler(BuildSucceededEvent);

      _ah = new QCV.Base.Addins.AddinHost();
      _ah.DiscoverInDomain();
      _ah.DiscoverInDirectory(Environment.CurrentDirectory);

      _env = new Dictionary<string, object>() {
        {"interactor", this}
      };

      _runtime = new QCV.Base.Runtime();
      _runtime.RuntimeStartingEvent += new EventHandler(RuntimeStartingEvent);
      _runtime.RuntimeStoppedEvent += new EventHandler(RuntimeStoppedEvent);

      _ic.Compile();

      if (_args.immediate_execute) {
        _runtime.Run(_fl, _env, 0);
      }
    }

    void OnQueryEndEvent(object sender, bool results) {
      _tp_query.Text = _tp_query.Text.TrimEnd(new char[] { '*' });
    }

    void OnQueryBeginEvent(object sender, string text, object query) {
      _tp_query.Text += "*";
    }

    void ConsoleStringAppendedEvent(object sender, string text) {
      _rtb_console.InvokeIfRequired(() => {
        _rtb_console.SelectionColor = ColorFromText(text);
        _rtb_console.AppendText(text);
        _rtb_console.ScrollToCaret();
      });
    }

    private Color ColorFromText(string text) {
      if (text.StartsWith("INFO")) {
        return Color.DarkGreen;
      } else if (text.StartsWith("ERROR")) {
        return Color.DarkRed;
      } else if (text.StartsWith("WARN")) {
        return Color.DarkOrange;
      } else {
        return Color.Black;
      }
    }

    void RuntimeStartingEvent(object sender, EventArgs e) {
      _btn_run.InvokeIfRequired(() => {
        _btn_run.Text = "Stop";
        _btn_run.BackColor = Color.LightGreen;
      });
    }

    void RuntimeStoppedEvent(object sender, EventArgs e) {
      _btn_run.InvokeIfRequired(() => {
        if (_appexit_requested || _args.auto_shutdown) {
          this.Close();
        } else {
          _btn_run.Text = "Run";
          _btn_run.BackColor = Control.DefaultBackColor;
        }
      });
    }

    void BuildSucceededEvent(object sender, QCV.Base.Compiler compiler) {
      bool running = _runtime.Running;
      if (running) {
        _query_ctrl.Cancel();
        _runtime.Stop(true);
      }

      QCV.Base.Addins.AddinHost tmp = new QCV.Base.Addins.AddinHost();
      tmp.DiscoverInAssembly(compiler.CompiledAssemblies);
      _ah.MergeByFullName(tmp);

      if (_fl == null) {
        // First run
        _fl = new QCV.Base.FilterList();
        IEnumerable<Base.Addins.AddinInfo> providers = _ah.FindAddins(
            typeof(Base.IFilterListProvider),
            (ai) => (ai.DefaultConstructible && _args.filterlist_providers.Contains(ai.FullName)));

        foreach (Base.Addins.AddinInfo ai in providers) {
          Base.IFilterListProvider p = _ah.CreateInstance(ai) as Base.IFilterListProvider;
          _fl.AddRange(p.CreateFilterList(_ah));
        }

        _logger.Info(String.Format("Created {0} filters.", _fl.Count));
      } else {
        // Subsequent runs
        QCV.Base.Reconfiguration r = new QCV.Base.Reconfiguration();
        QCV.Base.FilterList fl_new;
        r.Update(_fl, _ah, out fl_new);
        r.CopyPropertyValues(_fl, fl_new);
        _fl = fl_new;
      }

      _filter_properties.Filters = _fl;

      if (running) {
        _runtime.Run(_fl, _env, 0);
      }
    }

    private Base.FilterList LoadAndCombineFilterLists(List<string> list) {
      Base.FilterList fl = new QCV.Base.FilterList();
      foreach (string path in list) {
        fl.AddRange(Base.FilterList.Load(path));
      }
      return fl;
    }

    void AnyFormClosing(object sender, FormClosingEventArgs e) {
      if (e.CloseReason != CloseReason.FormOwnerClosing) {
        e.Cancel = true;
        (sender as Form).Hide();
      }
    }

    private void _btn_play_Click(object sender, EventArgs e) {
      if (_runtime.Running) {
        _query_ctrl.Cancel();
        _runtime.Stop(false);
      } else {
        _runtime.Run(_fl, _env, 0);
      }
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e) {
      _appexit_requested = true;
      e.Cancel = Shutdown();
    }

    private bool Shutdown() {
      _query_ctrl.Cancel();
      _runtime.Stop(false);
      return _runtime.Running;
    }

    private void _mnu_help_arguments_Click(object sender, EventArgs e) {
      CommandLine cl = new CommandLine();
      MessageBox.Show(cl.GetHelp(), "qcv.exe", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void _mnu_save_filter_list_Click(object sender, EventArgs e) {
      if (this.saveFileDialog1.ShowDialog() == DialogResult.OK) {
        Base.FilterList.Save(this.saveFileDialog1.FileName, _fl);
      }
    }
  }
}
