using System;
using System.Text;
using System.IO;

public class EbayDraftCsvGenerator
{
    private StringBuilder csv;

    // Initialize CSV with headers and info lines
    public void Initialize()
    {
        csv = new StringBuilder();

        // Add info headers
        csv.AppendLine("#INFO,Version=0.0.2,Template= eBay-draft-listings-template_US,,,,,,,,");
        csv.AppendLine("#INFO Action and Category ID are required fields. 1) Set Action to Draft 2) Please find the category ID for your listings here: https://pages.ebay.com/sellerinformation/news/categorychanges.html,,,,,,,,,,");
        csv.AppendLine("\"#INFO After you've successfully uploaded your draft from the Seller Hub Reports tab, complete your drafts to active listings here: https://www.ebay.com/sh/lst/drafts\",,,,,,,,,,");
        csv.AppendLine("#INFO,,,,,,,,,,");

        // Column headers
        csv.AppendLine("Action(SiteID=US|Country=US|Currency=USD|Version=1193|CC=UTF-8),Custom label (SKU),Category ID,Title,UPC,Price,Quantity,Item photo URL,Condition ID,Description,Format,Shipping service 1 option,Shipping service 1 cost");
    }

    // Add a single draft listing
    public void AddDraft(
        string action,
        string sku,
        string categoryId,
        string title,
        string upc,
        string price,
        string quantity,
        string imageUrl,
        string conditionId,
        string description,
        string format,
        string shippingOption,
        string shippingCost
    )
    {
        if (csv == null)
        {
            throw new InvalidOperationException("CSV not initialized. Call Initialize() first.");
        }

        csv.AppendLine($"{action},{sku},{categoryId},{title},{upc},{price},{quantity},{imageUrl},{conditionId},\"{description}\",{format},{shippingOption},{shippingCost}");
    }

    // Save CSV to file
    public void Save(string outputFile)
    {
        if (csv == null)
        {
            throw new InvalidOperationException("CSV not initialized. Call Initialize() first.");
        }

        File.WriteAllText(outputFile, csv.ToString(), Encoding.UTF8);
        Console.WriteLine($"✅ Draft CSV created: {outputFile}");
    }
}
