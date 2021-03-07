using System.ServiceProcess;

namespace RuneService
{
    public partial class RuneService : ServiceBase
    {
        SWProxy proxy;

        public RuneService()
        {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            proxy = new SWProxy();
            proxy.Start();
        }

        protected override void OnStop()
        {
            proxy.Stop();
        }
    }
}
