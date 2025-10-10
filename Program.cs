using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

class Program
{
    static void Main()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Records");

        string outputHtml = Path.Combine(Directory.GetCurrentDirectory(), "catalog.html");
        string outputCss = Path.Combine(Directory.GetCurrentDirectory(), "style.css");
        string outputJs = Path.Combine(Directory.GetCurrentDirectory(), "script.js");

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"❌ Error: Folder '{folderPath}' not found.");
            return;
        }

        string[] extensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var imageFiles = Directory
            .GetFiles(folderPath)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
            .OrderBy(f => f)
            .ToList();

        if (imageFiles.Count == 0)
        {
            Console.WriteLine("⚠️ No image files found in 'Records' folder.");
            return;
        }

        Console.WriteLine("🛠 Optimizing images...\n");
        foreach (var file in imageFiles)
        {
            try { OptimizeImage(file, 1600); }
            catch (Exception ex) { Console.WriteLine($"⚠️ Skipped {Path.GetFileName(file)}: {ex.Message}"); }
        }
        Console.WriteLine("\n✅ Image optimization complete.\n");

        // --- CSS ---
        string cssContent = @"
body { font-family: Arial, sans-serif; background: url('images/background.png') no-repeat center center fixed; background-size: cover; color: #eee; text-align: center; }
.gallery-popup { display:flex; flex-direction:column; gap:20px; max-height:70vh; overflow-y:auto; padding:20px; }
.card { display:flex; flex-direction:row; align-items:center; background:#222; border-radius:12px; padding:10px; box-shadow:0 0 10px rgba(0,0,0,0.5); transition: transform 0.2s, box-shadow 0.2s, opacity 0.3s; }
.card img { width:120px; height:auto; border-radius:8px; margin-right:15px; }
.card-info { display:flex; flex-direction:column; text-align:left; }
.card-info h3, .card-info p { margin:4px 0; }
.card.highlight { box-shadow:0 0 15px 4px #0ff; transform:scale(1.02); opacity:1; }
.card.not-highlight { opacity:0.5; }
input[type='text'] { padding:10px; width:60%; max-width:400px; border-radius:6px; border:none; font-size:1rem; margin-bottom:15px; }";
        File.WriteAllText(outputCss, cssContent, Encoding.UTF8);

        // --- JS ---
        string jsContent = @"
const searchBox = document.getElementById('searchBox');
const gallery = document.getElementById('gallery');
searchBox.addEventListener('input', () => {
    const term = searchBox.value.toLowerCase();
    const cards = Array.from(gallery.children);
    if (!term) {
        cards.forEach(card => { card.classList.add('highlight'); card.classList.remove('not-highlight'); });
    } else {
        const matching = [], nonMatching = [];
        cards.forEach(card => {
            const info = card.getAttribute('data-info');
            if (info.includes(term)) { card.classList.add('highlight'); card.classList.remove('not-highlight'); matching.push(card); }
            else { card.classList.remove('highlight'); card.classList.add('not-highlight'); nonMatching.push(card); }
        });
        gallery.innerHTML = '';
        matching.forEach(c => gallery.appendChild(c));
        nonMatching.forEach(c => gallery.appendChild(c));
    }
});";
        File.WriteAllText(outputJs, jsContent, Encoding.UTF8);

        // --- Build HTML ---
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='en'>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='UTF-8'>");
        html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("<title>Record Gallery</title>");
        html.AppendLine("<link rel='stylesheet' href='style.css'>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<header style='text-align:left; padding:10px;'>");
        html.AppendLine("<a href='index.html'><img src='images/pigstyle_text.png' alt='PigStyle' style='height:60px; vertical-align:middle;'></a>");
        html.AppendLine("</header>");
        html.AppendLine("<h1>🎵 Record Gallery</h1>");
        html.AppendLine("<div class='search-container'>");
        html.AppendLine("<input type='text' id='searchBox' placeholder='Search artist or record...'>");
        html.AppendLine("</div>");
        html.AppendLine("<div class='gallery-popup' id='gallery'>");

        foreach (var file in imageFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string artist = "Unknown Artist";
            string title = fileName;
            string price = "$19.99";

            if (fileName.Contains(" - "))
            {
                var parts = fileName.Split(new[] { " - " }, 2, StringSplitOptions.None);
                artist = parts[0];
                title = parts[1];
            }

            string imgSrc = $"https://raw.githubusercontent.com/ArjanShaw/PigStyleRecords/main/Records/{Uri.EscapeDataString(Path.GetFileName(file))}";

            html.AppendLine($"<div class='card highlight' data-info='{EscapeHtml((artist + " " + title).ToLower())}'>");
            html.AppendLine($"  <img src='{imgSrc}' alt='{EscapeHtml(title)}'>");
            html.AppendLine($"  <div class='card-info'>");
            html.AppendLine($"    <h3>{EscapeHtml(title)}</h3>");
            html.AppendLine($"    <p>{EscapeHtml(artist)}</p>");
            html.AppendLine($"    <p class='price'>{price}</p>");
            html.AppendLine($"  </div>");
            html.AppendLine("</div>");
        }

        html.AppendLine("</div>");
        html.AppendLine("<script src='script.js'></script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        File.WriteAllText(outputHtml, html.ToString(), Encoding.UTF8);

        EbayDraftCsvGenerator ebayDraftCsvGenerator = new EbayDraftCsvGenerator();

        ebayDraftCsvGenerator.Initialize();

        foreach (var file in imageFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string artist = "Unknown Artist";
            string title = fileName;

            if (fileName.Contains(" - "))
            {
                var parts = fileName.Split(new[] { " - " }, 2, StringSplitOptions.None);
                artist = parts[0];
                title = parts[1];
            }


            ebayDraftCsvGenerator.AddDraft(
                 action: "Draft",
                 sku: "SF-FLYING-NUN",
                 categoryId: "176985",
                 title: "Sally Fields - The Flying Nun",
                 upc: "",
                 price: "99",
                 quantity: "1",
                 imageUrl: "https://raw.githubusercontent.com/ArjanShaw/PigStyleRecords/refs/heads/main/Records/Null%20Division%20-%20Parallel%20Universe.png",
                 conditionId: "NEW",
                 description: "<p><CENTER><H4>This is a draft listing for Sally Fields - The Flying Nun</H4></CENTER><P>No actual merchandise yet. Do not bid.</P>",
                 format: "FixedPrice",
                 shippingOption: "USPSMedia",
                 shippingCost: "5.72"
             );


        }
        ebayDraftCsvGenerator.Save("draft_listing.csv");

 
        Console.WriteLine($"✅ Gallery created: {outputHtml}");
        Console.WriteLine($"✅ Stylesheet created: {outputCss}");
        Console.WriteLine($"✅ Script created: {outputJs}");
    }

    static void OptimizeImage(string filePath, int maxDimension)
    {
        using (var image = Image.Load(filePath))
        {
            int width = image.Width;
            int height = image.Height;

            if (width <= maxDimension && height <= maxDimension) return;

            float scale = (float)maxDimension / Math.Max(width, height);
            int newWidth = (int)(width * scale);
            int newHeight = (int)(height * scale);

            image.Mutate(x => x.Resize(newWidth, newHeight));
            var encoder = new JpegEncoder { Quality = 90 };
            image.Save(filePath, encoder);
        }
    }

    static string EscapeHtml(string text)
    {
        return System.Net.WebUtility.HtmlEncode(text);
    }

    static string EscapeCsv(string text)
    {
        if (text.Contains(",") || text.Contains("\""))
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        return text;
    }
}
