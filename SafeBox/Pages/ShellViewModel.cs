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

namespace SafeBox.Pages
{
    public class ShellViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;
        public ShellView TransView => (ShellView)View; // 与当前vm关联的视图

        private NLECloudAPI nle_api; // api接口
        private String m_token; // 接口访问令牌
        private object _mutex = new object(); // 互斥锁的同步块索引对象
        private Thread threadWork;

        private bool isRunning = false;


        #region 数据源
        public Boolean? IsDefense { get; set; }
        public Boolean? IsBoxOpen { get; set; }
        #endregion

        public ShellViewModel(IWindowManager windowManager, IContainer container)
        {
            _windowManager = windowManager;
            _container = container;

            nle_api = Store.Get<NLECloudAPI>(UDG_.NLE_API);
        }

        protected override void OnViewLoaded()
        {
            // 登录排队机系统
            this.View.Visibility = Visibility.Hidden;
            var vmLogin = _container.Get<LoginViewModel>();
            var rst = _windowManager.ShowDialog(vmLogin);
            if (rst == false)
            {
                RequestClose();
                return;
            }
            this.View.Visibility = Visibility.Visible;

            m_token = Store.Get<String>(UDG_.ACCESS_TOKEN);

            InitThread();
        
        }

        /// <summary>
        /// 控制安防
        /// </summary>
        public async void CmdDenfense()
        {
            int val; if ((Boolean?)IsDefense ?? false) val = 1; else val = 0;
            var isSuccess = await Task<bool>.Factory.StartNew(() =>
            {
                lock (_mutex)
                {
                    var resp = nle_api.Cmds(UDG_.DEVICE_ID, UDG_.APITAG_DEFENSE, val, m_token);
                    return resp.IsSuccess();
                }
            });
        }

        /// <summary>
        /// 控制门锁
        /// </summary>
        public async void CmdControl()
        {
            if ((Boolean?)IsBoxOpen ?? false)
            {
                var isSuccess = await Task<bool>.Factory.StartNew(() =>
                {
                    lock (_mutex)
                    {
                        var resp = nle_api.Cmds(UDG_.DEVICE_ID, UDG_.APITAG_CONTROL, 1, m_token);
                        return resp.IsSuccess();
                    }
                });

            }
            else
            {
                IsBoxOpen = true;
                _windowManager.ShowMessageBox("当前只支持远程开启保险箱，不能操作关闭", "提醒");
            }
        }


        /// <summary>
        /// 批量查询数据
        /// </summary>
        public void CmdQuery()
        {
            _windowManager.ShowWindow(_container.Get<HistoryDataViewModel>());
        }


        /// <summary>
        /// 更新状态
        /// </summary>
        private void Run()
        {
            while (isRunning)
            {
                Thread.Sleep(1000);

                #region 直接获取单个传感信息，反应很慢
                // 获取安防状态，更新视图
                //{
                //    var resp = nle_api.GetSensorInfo(UDG_.DEVICE_ID, UDG_.APITAG_DEFENSE_STATUS, m_token);
                //    if (resp.IsSuccess())
                //    {
                //        var rstObj = resp.ResultObj;
                //        var val = int.Parse(rstObj.Value.ToString());
                //        switch (val)
                //        {
                //            case UDG_.DEFENSE_STATUS_OFF:
                //                IsDefense = false;
                //                break;
                //            case UDG_.DEFENSE_STATUS_ON:
                //                IsDefense = true;
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}


                // 获取报警信号，更新视图
                //{
                //    var resp = nle_api.GetSensorInfo(UDG_.DEVICE_ID, UDG_.APITAG_ALARM, m_token);
                //    if (resp.IsSuccess())
                //    {
                //        var rstObj = resp.ResultObj;
                //        var val = int.Parse(rstObj.Value.ToString());
                //        switch (val)
                //        {
                //            case UDG_.ALARM_STATUS_OPEN:
                //                IsBoxOpen = true;
                //                break;
                //            case UDG_.ALARM_STATUS_CLOSE:
                //                IsBoxOpen = false;
                //                break;
                //            case UDG_.ALARM_STATUS_ALARM:
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}
                #endregion

                #region 通过获取历史数据来得到第一个数据，反应快
                lock (_mutex)
                {
                    // 获取报警信号，更新视图
                    {
                        var paras = new SensorDataFuzzyQryPagingParas()
                        {
                            DeviceID = UDG_.DEVICE_ID,
                            ApiTags = UDG_.APITAG_ALARM,
                            Method = 1,
                            TimeAgo = 1,
                            Sort = "DESC",
                            PageSize = 1,
                            PageIndex = 1
                        };

                        var resp = nle_api.GetSensorDatas(paras, m_token);
                        if (resp.IsSuccess())
                        {
                            var rstObj = resp.ResultObj;
                            int val;
                            var rst = int.TryParse(rstObj?.DataPoints?.ToArray()[0]?.PointDTO?.ToArray()[0].Value.ToString(), out val);
                            if (rst == true)
                            {
                                switch (val)
                                {
                                    case UDG_.ALARM_STATUS_OPEN:
                                        IsBoxOpen = true;
                                        break;
                                    case UDG_.ALARM_STATUS_CLOSE:
                                        IsBoxOpen = false;
                                        break;
                                    case UDG_.ALARM_STATUS_ALARM:
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    // 获取安防状态，更新视图
                    {
                        var paras = new SensorDataFuzzyQryPagingParas()
                        {
                            DeviceID = UDG_.DEVICE_ID,
                            ApiTags = UDG_.APITAG_DEFENSE_STATUS,
                            Method = 1,
                            TimeAgo = 1,
                            Sort = "DESC",
                            PageSize = 1,
                            PageIndex = 1
                        };

                        var resp = nle_api.GetSensorDatas(paras, m_token);
                        if (resp.IsSuccess())
                        {
                            var rstObj = resp.ResultObj;
                            int val;
                            var rst = int.TryParse(rstObj?.DataPoints?.ToArray()[0]?.PointDTO?.ToArray()[0].Value.ToString(), out val);
                            if (rst == true)
                            {
                                switch (val)
                                {
                                    case UDG_.DEFENSE_STATUS_OFF:
                                        IsDefense = false;
                                        break;
                                    case UDG_.DEFENSE_STATUS_ON:
                                        IsDefense = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }


                    }
                }
                #endregion
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
