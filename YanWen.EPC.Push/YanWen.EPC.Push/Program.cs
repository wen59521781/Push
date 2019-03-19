using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace YanWen.EPC.Push
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<MyService>(s =>
                {
                    s.ConstructUsing(name => new MyService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();
                x.EnablePauseAndContinue();
                x.SetDescription("EPC推送");//描述
                x.SetDisplayName("YanWen.EPC.Push");//显示名称
                x.SetServiceName("YanWen.EPC.Push");//服务名称
            });
        }
    }
}
