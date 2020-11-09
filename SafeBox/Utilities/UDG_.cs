using System;
using NLECloudSDK;

namespace Utilities
{
    /// <summary>
    /// 用户定义的全局变量（Udg：User define global）
    /// </summary>
    public static class UDG_
    {
        private const String API_HOST = nameof(API_HOST); // 云平台API域名
        public const String NLE_API = nameof(NLE_API); // 云平台API接口
        public const String ACCESS_TOKEN = nameof(ACCESS_TOKEN); // 访问令牌

        public const int DEVICE_ID = 145500; // 设备ID
        public const string APITAG_DEFENSE = "defense";  // 执行器：控制安防
        public const string APITAG_CONTROL = "ctrl"; // 执行器：控制门锁
        public const string APITAG_ALARM = "alarm"; // 传感器：报警信号
        public const string APITAG_DEFENSE_STATUS = "DefenseState"; // 传感器：安防状态

        public const int ALARM_STATUS_CLOSE = 0; // 报警信号：保险柜已上锁
        public const int ALARM_STATUS_OPEN = 1; // 报警信号：保险柜已打开
        public const int ALARM_STATUS_ALARM = 2; // 报警信号：保险柜异常
        public const int DEFENSE_STATUS_OFF = 0; // 安防状态：关
        public const int DEFENSE_STATUS_ON = 1; // 安防状态：开

        public static void Initialize()
        {
            Store.Set(API_HOST, ApplicationSettings.Get(CFG_.API_HOST));
            Store.Set(NLE_API, new NLECloudAPI(Store.Get<String>(API_HOST)));
            Store.Set<object>(ACCESS_TOKEN, null);
        }
    }
}