using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace KFA.ItemCodes.Classes
{
	static internal class PlayWrightAutomator
	{
		public static IPage? page { get; private set; }
		//Playwright? playwright = null;
		public record DynamicsItem(string ItemCode, string ItemName, string CostCentreCode, decimal CostPrice, decimal SellingPrice);

		public static async Task Generate(DynamicsItem item, string username, string password)
		{
			using var playwright = await Playwright.CreateAsync();
			// string url = "https://mis.kenyafarmersassociation.co.ke/BC130/";
			await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
			{
				Headless = false
			});

			var context = await browser.NewContextAsync(new()
			{
				HttpCredentials = new HttpCredentials
				{
					Username = username,
					Password = password
				},
				ViewportSize = null
			});
			//context.SetDefaultTimeout(400000000);

			page = await context.NewPageAsync();
			page.Close += (xx, yy) => page = null;

			await page.GotoAsync("https://mis.kenyafarmersassociation.co.ke/BC130/");

			await page.GetByRole(AriaRole.Button, new() { Name = "" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Type to start search:").FillAsync("items");

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByText("Items", new() { Exact = true }).First.ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Menuitem, new() { Name = "New" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("No., (Blank)", new() { Exact = true }).FillAsync("090678");

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Description, (Blank)", new() { Exact = true }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Description, (Blank)", new() { Exact = true }).FillAsync("TUFUGE LAYERS MASH 20KG");

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("[id=\"\\35 15DD\"]").GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, BAGS").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("[id=\"\\35 15CC\"]").GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("[id=\"\\35 15CC\"]").GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, 01").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("[id=\"\\35 15CC\"]").GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, 09").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Button, new() { Name = "Costs & Posting, This group contains one or more mandatory fields that are not filled in." }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Costing Method, FIFO").SelectOptionAsync(new[] { "3" });

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("div").Filter(new() { HasTextRegex = new Regex("^Gen\\. Prod\\. Posting GroupThe value for this field is required\\.$") }).GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, INVENTORY").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("div").Filter(new() { HasTextRegex = new Regex("^VAT Prod\\. Posting GroupThe value for this field is required\\.$") }).GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, EXEMPT").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("div").Filter(new() { HasTextRegex = new Regex("^Inventory Posting GroupThe value for this field is required\\.$") }).GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("div").Filter(new() { HasTextRegex = new Regex("^Inventory Posting GroupThe value for this field is required\\.$") }).GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, INVENTORY").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Costs & Posting, Show more").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").Locator("div").Filter(new() { HasTextRegex = new Regex("^VAT Prod\\. Posting Group$") }).GetByTitle("Look up value").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, VAT-0").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Menuitem, new() { Name = "Process" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Menuitem, new() { Name = "Process" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Menuitem, new() { Name = "Process" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Menuitem, new() { Name = "Cost Price Matrix" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Row, new() { Name = " Item No., 090678 Item Name, TUFUGE LAYERS MASH 20KG Store No., (Blank) Cost, 0.00 Price, 0.00" }).GetByLabel("Store No., (Blank)").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Gridcell, new() { Name = " Store No., (Blank)" }).GetByLabel("Store No., (Blank)").FillAsync("mer");

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Code, 5400").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Cost, 0.00", new() { Exact = true }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByLabel("Cost, 0.00", new() { Exact = true }).FillAsync("970");

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Gridcell, new() { Name = "Price, (Blank)" }).ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Row, new() { Name = "Item No., 090678  Item Name, TUFUGE LAYERS MASH 20KG Store No., 5400 Cost, 970.00 Price, 0.00" }).GetByLabel("Price, 0.00").ClickAsync();

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Row, new() { Name = "Item No., 090678  Item Name, TUFUGE LAYERS MASH 20KG Store No., 5400 Cost, 970.00 Price, 0.00" }).GetByLabel("Price, 0.00").FillAsync("1200");

			await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Button, new() { Name = "Back" }).ClickAsync();


			//await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Button, new() { Name = "Back" }).ClickAsync();

			//await page.FrameLocator("iframe[title=\"Main Content\"]").GetByRole(AriaRole.Button, new() { Name = "" }).ClickAsync();

		}

		private static void Context_Close(object? sender, IBrowserContext e)
		{
			throw new NotImplementedException();
		}
	}
}
