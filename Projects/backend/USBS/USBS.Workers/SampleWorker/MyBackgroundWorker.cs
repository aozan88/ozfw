using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using USBS.Workers.SampleWorker;

public class MyBackgroundWorker : BackgroundService
{
    private readonly ILogger<MyBackgroundWorker> _logger;
    private readonly WorkerSettings _settings;

    public MyBackgroundWorker(ILogger<MyBackgroundWorker> logger, IOptions<WorkerSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker başlatıldı.");

        // İlk gecikme (opsiyonel)
        await Task.Delay(TimeSpan.FromSeconds(_settings.InitialDelayInSeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Worker çalışıyor: {Time}", DateTimeOffset.Now);

                // İşlem süresi sınırlandırılmış
                using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.MaxExecutionTimeInSeconds)))
                {
                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutToken.Token);

                    await ProcessWorkAsync(linkedToken.Token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker'da hata oluştu.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalInSeconds), stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker durduruluyor.");
        return base.StopAsync(cancellationToken);
    }

    private async Task ProcessWorkAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("İşlem başlatıldı...");

        try
        {
            // Burada Worker'ın yapacağı işlemi tanımlayabilirsin
            await Task.Delay(2000, cancellationToken); // Örnek: 2 saniye bekle
            _logger.LogInformation("İşlem başarıyla tamamlandı.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("İşlem iptal edildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlem sırasında hata oluştu.");
        }
    }
}