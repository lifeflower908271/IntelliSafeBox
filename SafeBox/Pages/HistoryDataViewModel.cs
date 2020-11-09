using System;
using System.Windows;
using Stylet;
using StyletIoC;
using NLECloudSDK;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.Charts;

using System.Windows.Controls;
using Utilities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes;
using System.Windows.Media.Animation;
using NLECloudSDK.Model;
using System.Linq;
using System.Collections.ObjectModel;

namespace SafeBox.Pages
{
    public class HistoryDataViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;

        private NLECloudAPI nle_api; // api接口
        private String m_token; // 接口访问令牌
        private object _mutex = new object(); // 互斥锁的同步块索引对象
        private Thread threadWork;
        private bool isRunning = false;


        #region 数据源
        /// <summary>
        /// 容器的传感数据集合
        /// </summary>
        public ObservableCollection<SensorDataPointDTO> SensorDatas { get; set; } = null;
        #endregion

        public HistoryDataViewModel(IWindowManager windowManager, IContainer container)
        {
            _windowManager = windowManager;
            _container = container;

            nle_api = Store.Get<NLECloudAPI>(UDG_.NLE_API);
            m_token = Store.Get<String>(UDG_.ACCESS_TOKEN);
        }

        protected override void OnViewLoaded()
        {
            InitThread();
        
        }



        /// <summary>
        /// 更新状态
        /// </summary>
        private void Run()
        {
            while (isRunning)
            {
                Thread.Sleep(1000);

                lock (_mutex)
                {

                    var paras = new SensorDataFuzzyQryPagingParas()
                    {
                        DeviceID = UDG_.DEVICE_ID,
                        ApiTags = UDG_.APITAG_ALARM,
                        Method = 3,
                        TimeAgo = 7,
                        Sort = "DESC",
                        PageSize = 1000,
                        PageIndex = 1
                    };

                    var resp = nle_api.GetSensorDatas(paras, m_token);
                    if (resp.IsSuccess())
                    {
                        var rstObj = resp.ResultObj;
                        var list = rstObj?.DataPoints?.ToList()[0].PointDTO;
                        SensorDatas = new ObservableCollection<SensorDataPointDTO>(list);
                    }

                }
            }
        }


        private void InitThread()
        {
            if (this.threadWork == null)
            {
                this.isRunning = true;
                this.threadWork = new Thread(new ThreadStart(this.Run));
                this.threadWork.IsBackground = true;
                this.threadWork.Start();
            }
        }

        protected override void OnClose()
        {
            isRunning = false;
            threadWork.Abort();
            base.OnClose();
        }
    }
}
