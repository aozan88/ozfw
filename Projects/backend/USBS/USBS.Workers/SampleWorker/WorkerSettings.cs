using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBS.Workers.SampleWorker
{
    public class WorkerSettings
    {
        // Worker'ın çalıştırma aralığı (saniye cinsinden)
        public int IntervalInSeconds { get; set; } = 10;

        // Worker başlatılmadan önce bekleyecek süre (örneğin: 5 saniye)
        public int InitialDelayInSeconds { get; set; } = 5;

        // İşlem bir hata alırsa kaç kere tekrar denenecek?
        public int RetryCount { get; set; } = 3;

        // İşlem başarısız olursa her tekrar denemede bekleme süresi (ms cinsinden)
        public int RetryDelayMilliseconds { get; set; } = 2000;

        // Maksimum çalışma süresi (saniye cinsinden), süre aşımına uğrarsa iptal edilir
        public int MaxExecutionTimeInSeconds { get; set; } = 60;

        // Aynı anda kaç tane Worker işlemi çalışabilir?
        public int MaxParallelJobs { get; set; } = 1;

        // İşlem kuyruğu destekleniyor mu?
        public bool UseQueue { get; set; } = false;

        // Detaylı logları aktif et
        public bool EnableDetailedLogging { get; set; } = false;

        // Çalışma ortamı (Production, Development, Test)
        public string Environment { get; set; } = "Development";
    }
}
