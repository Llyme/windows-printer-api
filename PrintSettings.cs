namespace WindowsPrinterAPI
{
	public struct PrintSettings
	{
		// Number of copies per page.
		public short copies;
		public bool collate;
		public string duplex;
		public int fromPage;
		public int toPage;
		public bool color;
		public bool landscape;
		public string paperSize;
		public int margin_top;
		public int margin_left;
		public int margin_bottom;
		public int margin_right;
		// If 'true', centers the contents of the page along the x-axis.
		// 'margin_left' will still be used, while 'margin-right' will be ignored.
		public bool margin_auto_x;
		// If 'true', centers the contents of the page along the y-axis.
		// 'margin_top' will still be used, while 'margin-bottom' will be ignored.
		public bool margin_auto_y;
		public int pager_margin_offset;
	}
}
