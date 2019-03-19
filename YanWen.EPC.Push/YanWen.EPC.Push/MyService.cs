using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ComputerCenterScheduling;
using log4net;
using YanWen.EPC.Push.PushJobs;

namespace YanWen.EPC.Push
{
    public class MyService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MyService));
        private readonly ConcurrentDictionary<string, IJobRunner> _runners;

        public MyService()
        {
            _runners = new ConcurrentDictionary<string, IJobRunner>();

            //TODO: 根据配置文件加载
            _runners.TryAdd("1", new JobRunner(new EpcPushJob()));
            _runners.TryAdd("2", new JobRunner(new EpcCombinePushJob()));
            _runners.TryAdd("3", new JobRunner(new EpcMqPushJob()));
            _runners.TryAdd("4", new JobRunner(new DingDingRemindJob()));
        }

        static CancellationTokenSource cts = new CancellationTokenSource();

        public void Start()
        {
            Logger.Debug($"RunnerCount={_runners.Count}");
            foreach (var runner in _runners)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            runner.Value.RunAsync(null, runner.Key, "0").Wait();
                        }
                        catch (AggregateException ex)
                        {
                            Logger.Error(ex.Flatten());
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.StackTrace);
                        }
                    }
                });
            }

        }

        public void Stop()
        {
            Logger.Debug(@"接口推送暂停...");
            cts.Cancel();
        }
    }
}
