using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Web;

namespace WindowsPrinterAPI
{
    public class Methods
    {
		public static string[] GetInstalledPrinters()
		{
			PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;
			string[] array = new string[printers.Count];

			for (int i = 0; i < printers.Count; i++)
				array[i] = printers[i];

			return array;
		}

		public static string GetDefaultPrinter()
		{
			return new PrinterSettings().PrinterName;
		}

		public static PrintSystemJobInfo[] GetPrintQueue(string printer)
		{
			if (!new PrinterSettings { PrinterName = printer }.IsValid)
				printer = new PrinterSettings().PrinterName;

			PrintQueue queue;

			if (printer.Substring(0, 2) == @"\\")
			{
				int index = printer.IndexOf('\\', 2);

				if (index > 0)
					queue = new PrintServer(printer.Substring(0, index)).GetPrintQueue(printer.Substring(index + 1));
			}

			queue = new PrintServer().GetPrintQueue(printer);

			return queue.GetPrintJobInfoCollection().ToArray();
		}

		public static PrintQueue GetPrinterStatus(string printer)
		{
			if (!new PrinterSettings { PrinterName = printer }.IsValid)
				printer = new PrinterSettings().PrinterName;

			PrintQueue queue;

			if (printer.Substring(0, 2) == @"\\")
			{
				int index = printer.IndexOf('\\', 2);

				if (index > 0)
					queue = new PrintServer(printer.Substring(0, index)).GetPrintQueue(printer.Substring(index + 1));
			}

			queue = new PrintServer().GetPrintQueue(printer);

			return queue;
		}

		public static bool Print(string printer, PrintSettings settings, string filename)
		{
			PrinterSettings printerSettings = new PrinterSettings { PrinterName = printer };

			if (!printerSettings.IsValid)
				printerSettings = new PrinterSettings();

			printerSettings.Copies = settings.copies;
			printerSettings.Collate = settings.collate;

			Duplex duplex = Duplex.Default;
			string settingsDuplex = settings.duplex.ToLower().Trim();
			
			foreach (int i in Enum.GetValues(typeof(Duplex)))
			{
				Duplex duplex0 = (Duplex)i;

				if (settingsDuplex == duplex0.ToString().ToLower())
				{
					duplex = duplex0;
					break;
				}
			}

			if (settings.fromPage > 0)
				printerSettings.FromPage = settings.fromPage;

			if (settings.toPage > 0)
				printerSettings.ToPage = settings.toPage;

			printerSettings.Duplex = duplex;

			PageSettings pageSettings = new PageSettings(printerSettings)
			{
				Margins = new Margins(0, 0, 0, 0),
				Color = settings.color,
				Landscape = settings.landscape
			};

			string paperSize = settings.paperSize.ToLower();

			switch (paperSize)
			{
				case "custom":
					int num = paperSize.IndexOf('.');
					int num2 = paperSize.IndexOf('x');
					int width = int.Parse(paperSize.Substring(num + 1, num2 - num - 1));
					int height = int.Parse(paperSize.Substring(num2 + 1));
					pageSettings.PaperSize = new PaperSize("Custom", width, height);
					break;

				default:
					foreach (PaperSize v in printerSettings.PaperSizes)
						if (v.PaperName.ToLower() == paperSize)
						{
							pageSettings.PaperSize = v;
							break;
						}

					break;
			}

			string mimeMapping = MimeMapping.GetMimeMapping(filename);
			string[] source = new string[]
			{
				".c++",
				".cc",
				".com",
				".conf",
				".hh",
				".java",
				".log"
			};

			return mimeMapping.Contains("text/") && PrintText(filename, settings, printerSettings, pageSettings);
		}

		private static bool PrintText(string filename,
									  PrintSettings settings,
									  PrinterSettings printerSettings,
									  PageSettings pageSettings)
		{
			Font pagerFont = new Font("Consolas", 8f);
			Font bodyFont = new Font("Consolas", 11f);
			Margins margins = new Margins(
				Math.Max(0, settings.margin_left),
				Math.Max(0, settings.margin_right),
				Math.Max(0, settings.margin_top),
				Math.Max(0, settings.margin_bottom)
			);
			bool landscape = pageSettings.Landscape;
			bool color = pageSettings.Color;
			int pages = 0;
			string header = "";
			string footer = "";
			string body = "";
			string[] lines = File.ReadAllLines(filename);

			foreach (string line in lines)
				if (line.StartsWith("^"))
					header += line.Substring(1) + "\n";
				else if (line.StartsWith("$"))
					footer += "\n" + line.Substring(1);
				else
					body += "\n" + line;

			if (body.Length > 0)
				body = body.Substring(1);

			try
			{
				using (PrintDocument printDocument = new PrintDocument
				{
					PrinterSettings = printerSettings,
					DefaultPageSettings = pageSettings
				})
				{
					string bodyCurrent = body;
					int page = 0;

					printDocument.PrintPage += (object sender, PrintPageEventArgs e) =>
					{
						Rectangle marginBounds = new Rectangle(
							margins.Left,
							margins.Top,
							e.MarginBounds.Width - margins.Left - margins.Right,
							e.MarginBounds.Height - margins.Top - margins.Bottom
						);


						// Page

						{
							string content = $"Page {page} of {pages}";
							SizeF bounds = e.Graphics.MeasureString(
								content,
								pagerFont,
								e.MarginBounds.Size,
								StringFormat.GenericTypographic,
								out int chars,
								out _
							);

							e.Graphics.DrawString(
								content,
								pagerFont,
								Brushes.Black,
								new Rectangle(
									e.MarginBounds.Width - (int)bounds.Width - settings.pager_margin_offset,
									12,
									e.MarginBounds.Width,
									e.MarginBounds.Height
								),
								StringFormat.GenericTypographic
							);
						}


						// Content

						{
							int x = marginBounds.X;
							int y = marginBounds.Y;
							string content = header + bodyCurrent;

							SizeF bounds = e.Graphics.MeasureString(
								content + footer,
								bodyFont,
								marginBounds.Size,
								StringFormat.GenericTypographic,
								out int chars,
								out _
							);

							if (settings.margin_auto_x)
								x = (int)Math.Floor((marginBounds.Width - bounds.Width) / 2f) + settings.margin_left;

							if (settings.margin_auto_y)
								y = (int)Math.Floor((marginBounds.Height - bounds.Height) / 2f) + settings.margin_top;

							if (chars - content.Length >= footer.Length)
								content += footer;
							else
								chars -= footer.Length;

							e.Graphics.DrawString(
								content.Substring(0, chars),
								bodyFont,
								Brushes.Black,
								new Rectangle(
									x,
									y,
									marginBounds.Width,
									marginBounds.Height
								),
								StringFormat.GenericTypographic
							);

							if (chars >= content.Length)
								bodyCurrent = "";
							else
								bodyCurrent = content.Substring(chars);
						}

						e.HasMorePages = bodyCurrent.Length > 0;
						page++;
					};

					printDocument.QueryPageSettings += (object sender, QueryPageSettingsEventArgs e) =>
					{
						e.PageSettings.Landscape = landscape;
						e.PageSettings.Color = color;
					};


					// Start

					pages = CountPages(printDocument);
					page = 1;
					bodyCurrent = body;

					printDocument.Print();

					return true;
				}
			}
			catch (Exception _)
			{
				return false;
			}
		}

		private static int CountPages(PrintDocument doc)
		{
			PrintController controller = doc.PrintController;
			int count = 0;

			void counter(object sender, PrintPageEventArgs e) => count++;

			doc.PrintController = new PreviewPrintController();
			doc.PrintPage += counter;

			doc.Print();

			doc.PrintController = controller;
			doc.PrintPage -= counter;

			return count;
		}
	}
}
