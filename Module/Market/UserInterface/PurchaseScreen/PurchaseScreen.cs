using Godot;
using System;

public partial class PurchaseScreen : CanvasLayer
{
	private VBoxContainer stockInfoContainer;
	private PackedScene stockInfoWidget;
	private stock_info activeStockInfo;
	private Stock currentStock;
	private float currentBuyAmt;
	[Export]
	public GameManager gm;

	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		if (Visible) {
			foreach (stock_info si in stockInfoContainer.GetChildren()){
				si.updateStockPriceLabel();
			}
		}
		
	}

	public void loadStocks(Stock[] stocks)
	{

		stockInfoContainer = GetNode<VBoxContainer>("HBoxContainer/ScrollContainer/StockInfoContainer");

		foreach (Stock s in stocks)
		{
			stockInfoWidget = GD.Load<PackedScene>("res://Module/Market/UserInterface/InfoScreen/stock_info.tscn");
			stock_info si = (stock_info)stockInfoWidget.Instantiate();
			si.Stock = s;
			stockInfoContainer.AddChild(si);
			si.activateScreen += stockSelected;
		}
	}

	public void stockSelected(stock_info stockInfo, Stock stock)
	{

		activeStockInfo = stockInfo;

		foreach (stock_info si in stockInfoContainer.GetChildren())
		{
			if (si != stockInfo)
			{
				si.Active = false;
			}
		}

		currentBuyAmt = 0;

		currentStock = activeStockInfo.stock;
		updatePurchaseScreen();
	}

	public void updatePurchaseScreen() {

		if (currentStock != null) {
			GetNode<Label>("HBoxContainer/PurchaseContainer/ColorRect/GridContainer/PurchaseLabel").Text = currentStock.companyName;
			GetNode<Label>("HBoxContainer/PurchaseContainer/ColorRect/GridContainer/CurrentShares Label").Text = "Shares Held: " + currentStock.sharesHeld.ToString("0.00");
		} else {
			GetNode<Label>("HBoxContainer/PurchaseContainer/ColorRect/GridContainer/PurchaseLabel").Text = "N/A";
			GetNode<Label>("HBoxContainer/PurchaseContainer/ColorRect/GridContainer/CurrentShares Label").Text = "";
		}

		GetNode<Label>("HBoxContainer/PurchaseContainer/ColorRect/GridContainer/ColorRect/BuyAmtLabel").Text = currentBuyAmt.ToString("0.00");

	}

	public void playSFX() {
		AudioStreamPlayer sp = GetNode<AudioStreamPlayer>("UISfx");

		sp.PitchScale = (float)GD.RandRange(0.6, 1.4);

		sp.Play();
	}

	private float getCurrentJump() {
		float res = (gm.money / (float)currentStock.stockPrice) / 10f;
		return res;
	}

	public void buyAmtUp() {
		playSFX();
		currentBuyAmt += getCurrentJump();
		clampBuyAmount();
		updatePurchaseScreen();
	}

	public void buyAmtDown() {
		playSFX();
		currentBuyAmt -= getCurrentJump();
		clampBuyAmount();
		updatePurchaseScreen();
	}

	public void clampBuyAmount() {
		if (currentStock != null) {
			if (currentBuyAmt * (float)currentStock.stockPrice >= gm.money) {
				currentBuyAmt = gm.money / (float)currentStock.stockPrice - 0.001f;
			}
			if (currentBuyAmt < 0 ) {
				currentBuyAmt = 0;
			}
		} else {
			currentBuyAmt = 0;
		}
	}

	public void OnPurchasePressed() {

		playSFX();

		clampBuyAmount();
		if (gm.money >= currentBuyAmt * currentStock.stockPrice) {
			gm.GetNode<StockManager>("Stock Manager").purchaseStock(currentStock, currentBuyAmt);
			gm.money -= currentBuyAmt * (float)currentStock.stockPrice;
			currentBuyAmt = 0;
			updatePurchaseScreen();
		} else {
			
		}

		
	}
}
