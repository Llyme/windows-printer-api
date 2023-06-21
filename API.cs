using System.Threading.Tasks;

namespace WindowsPrinterAPI
{
	public class API
	{
		public async Task<object> GetInstalledPrinters(object input)
		{
			return Methods.GetInstalledPrinters();
		}

		public async Task<object> GetDefaultPrinter(object input)
		{
			return Methods.GetDefaultPrinter();
		}

		public async Task<object> GetPrintQueue(object input)
		{
			return Methods.GetPrintQueue((string)input);
		}

		public async Task<object> GetPrinterStatus(object input)
		{
			return Methods.GetPrinterStatus((string)input);
		}

		public async Task<object> Print(dynamic input)
		{
			return Methods.Print(
				(string)input.printer,
				new PrintSettings
				{
					copies = (short)input.copies,
					collate = (bool)input.collate,
					duplex = (string)input.duplex,
					fromPage = (int)input.fromPage,
					toPage = (int)input.toPage,
					color = (bool)input.color,
					landscape = (bool)input.landscape,
					paperSize = (string)input.paperSize,
					margin_left = (int)input.margin_left,
					margin_right = (int)input.margin_right,
					margin_top = (int)input.margin_top,
					margin_bottom = (int)input.margin_bottom,
					margin_auto_x = (bool)input.margin_auto_x,
					margin_auto_y = (bool)input.margin_auto_y,
					pager_margin_offset = (int)input.pager_margin_offset
				},
				(string)input.filepath
			);
		}
	}
}
