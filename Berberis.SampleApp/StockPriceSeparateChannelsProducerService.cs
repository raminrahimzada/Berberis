﻿using Berberis.Messaging;

namespace Berberis.SampleApp;

public sealed class StockPriceSeparateChannelsProducerService : BackgroundService
{
    private readonly ICrossBar _xBar;

    private readonly string[] _symbols;
    private readonly int _minTickInterval;
    private readonly int _maxTickInterval;

    public StockPriceSeparateChannelsProducerService(ICrossBar xBar)
    {
        _xBar = xBar;

        _symbols = new[]
                    {
                        "mks", "gs", "ms", "msft",
                        "bt", "amd", "intc", "aapl",
                        "goog", "nvax", "smt", "iag", "nwc",
                        "emg", "nvda", "tsla", "amzn",
                        "se", "shop", "u", "pypl", "zi", "zs"
                    };

        _minTickInterval = 10;
        _maxTickInterval = 100;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);

        const string destination = "stock.prices";

        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            var index = random.Next(0, _symbols.Length);
            var price = new StockPrice(_symbols[index], random.NextDouble());

            await _xBar.Publish($"{destination}.{price.Symbol}", price, 0, key: price.Symbol, store: true, from: nameof(StockPriceSeparateChannelsProducerService));

            await Task.Delay(random.Next(_minTickInterval, _maxTickInterval), stoppingToken);
        }
    }
}