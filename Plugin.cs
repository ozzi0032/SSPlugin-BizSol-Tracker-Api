using SmartStore.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BizSol.Tracker.Api
{
    public class Plugin : BasePlugin
    {
        public static string SystemName => "BizSol.Tracker.Api";
        public override void Install()
        {
            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}