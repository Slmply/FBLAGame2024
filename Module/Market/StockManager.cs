using Godot;
using Godot.Collections;
using System;
using System.Collections;

public partial class StockManager : Node
{
	[Export]
	public Stock[] stocks;
	[Export]
	public Event[] eventList;
	[Export]
	public float minEventWait;
	[Export]
	public float maxEventWait;

	private float nextEventTime;
	private float i = 0;
	[Export]
	public StocksInfoScreen sis = null;
	[Export]
	public PurchaseScreen pss = null;

	public override void _Ready()
	{
		// sis = GetNode<StocksInfoScreen>("StocksInfoScreen");
		initializeStocks();
	}

	public void initializeStocks()
	{
		Random random = new Random();
		nextEventTime = ((float)random.NextDouble() * maxEventWait) + minEventWait;
		GD.Print("Next Event at: " + nextEventTime);
		for (int k = 0; k < stocks.Length; k++)
		{
			stocks[k].init();
		}
		sis.loadStocks(stocks);
		pss.loadStocks(stocks);
	}

	public void purchaseStock(Stock s, float amount)
	{
		s.sharesHeld += amount;
	}

	public void updateStocks(float time)
	{

		for (int k = 0; k < stocks.Length; k++)
		{
			stocks[k].update(time);
		}

		if (time >= nextEventTime)
		{
			eventSelection(time);
		}
	}

	public float getTotalStockInventoryValue() {
		float res = 0;

		foreach (Stock s in stocks) {
			res += (float) s.stockPrice * s.sharesHeld;
		}


		return res;
	}

	public void eventSelection(float time)
	{

		Random random = new Random();

		Stock randomStock = randomWeighted();
		Event randomEvent = eventList[random.Next(0, eventList.Length)];

		randomStock.beginEvent(randomEvent, time);

		nextEventTime = (float)random.NextDouble() * maxEventWait + minEventWait + time + randomEvent.eventDuration;
		GD.Print("Next Event at: " + nextEventTime);
	}

	public Stock randomWeighted()
	{

		float total = 0;

		foreach (Stock s in stocks)
		{
			total += (float)s.volatility;
		}

		Random rand = new Random();
		float random = (float)rand.NextDouble() * total;

		float cursor = 0;
		foreach (Stock s in stocks)
		{
			cursor += (float)s.volatility;
			if (cursor >= random)
			{
				return s;
			}
		}
		return null;
	}

}

