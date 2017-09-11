using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Server.Services.Stats {
	class StatTable {
		public List<List<string>> Rows = new List<List<string>>();

		public void AddNewRow(params string[] cells) {
			Rows.Add(new List<string>(cells));
		}

		public void AddToRow(params string[] cells) {
			Rows.Last().AddRange(cells);
		}

		string GetAt(List<string> row, int colIndex) {
			return colIndex < row.Count ? row[colIndex] : string.Empty;
		}

		int GetMaxWidth(int colIndex) {
			return Rows.Max(row => GetAt(row, colIndex).Length);
		}

		public void Append(StringBuilder sb) {
			for ( var rowIndex = 0; rowIndex < Rows.Count; rowIndex++ ) {
				sb.Append(" ");
				for ( var colIndex = 0; colIndex < Rows[0].Count; colIndex++ ) {
					FormatField(sb, GetAt(Rows[rowIndex], colIndex), GetMaxWidth(colIndex));
				}
				sb.Append('\n');
			}
		}

		void FormatField(StringBuilder sb, string value, int width) {
			var addingCount = width - value.Length;
			while ( addingCount > 0 ) {
				sb.Append(' ');
				addingCount--;
			}
			sb.Append(value);
			sb.Append(" | ");
		}
	}
}
