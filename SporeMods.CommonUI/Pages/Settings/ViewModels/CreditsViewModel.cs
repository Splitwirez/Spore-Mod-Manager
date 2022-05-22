using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SporeMods.Core;
using SporeMods.CommonUI;
using System.Reflection;
using System.IO;

namespace SporeMods.ViewModels
{
	public class CreditsViewModel : NotifyPropertyChangedBase
	{
		ObservableCollection<CreditsItem> _credits = new ObservableCollection<CreditsItem>()
		{
			new CreditsItem("Credits broke", "Oh no", @"https://yeah.rip/")
		};

		public ObservableCollection<CreditsItem> Credits
		{
			get => _credits;
			private set
            {
				_credits = value;
				NotifyPropertyChanged();
            }
		}

		public CreditsViewModel()
			: base()
		{
			var lines = GetResLines();
			Credits = ParseCredits(lines);
		}

		const string CREDITS_NAME_START = "### ";
		const string CREDITS_NAME_HL_START = "[";
		const string CREDITS_NAME_HL_SEP = "](";
		const string CREDITS_NAME_HL_END = ")";
		const string CREDITS_CM_START = "<!--";
		const string CREDITS_CM_END = "-->";
		private ObservableCollection<CreditsItem> ParseCredits(List<string> lines)
		{
			lines.Add(string.Empty);

			ObservableCollection<CreditsItem> credits = new ObservableCollection<CreditsItem>();

			bool isInComment = false;
			bool isInEntry = false;

			string entryName = null;
			string entryDesc = string.Empty;
			string entryLink = null;
			
			for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
			{
				string line = lines[lineIndex].Trim();
				if (!isInComment)
				{
					if (line.Contains(CREDITS_CM_START))
					{
						if (line.StartsWith(CREDITS_CM_START) && line.EndsWith(CREDITS_CM_END))
							continue;
						else
							isInComment = true;

						if (line.StartsWith(CREDITS_CM_START))
							continue;
						else
							line = line.Substring(0, line.IndexOf(CREDITS_CM_START));
					}

				}
				else
				{
					if (line.Contains(CREDITS_CM_END))
					{
						isInComment = false;

						if (line.EndsWith(CREDITS_CM_END))
							continue;
						else
							line = line.Substring(line.IndexOf(CREDITS_CM_END) + CREDITS_CM_END.Length);
					}
				}

				line = lines[lineIndex].Trim();

				if (line.IsNullOrEmptyOrWhiteSpace())
				{
					if (isInEntry && (!isInComment))
					{
						credits.Add(new CreditsItem(entryName, entryDesc.TrimEnd('\n'), entryLink));

						entryName = null;
						entryDesc = string.Empty;
						entryLink = null;

						isInEntry = false;
					}
					continue;
				}

				if (isInComment)
					continue;

				if ((!isInEntry) && line.StartsWith(CREDITS_NAME_START))
				{
					isInEntry = true;

					line = line.Substring(CREDITS_NAME_START.Length);
					if (line.StartsWith(CREDITS_NAME_HL_START) && line.Contains(CREDITS_NAME_HL_SEP) && line.EndsWith(CREDITS_NAME_HL_END))
					{
						line = line.Substring(CREDITS_NAME_HL_START.Length);
						int separatorIndex = line.IndexOf(CREDITS_NAME_HL_SEP);
						entryName = line.Substring(0, separatorIndex);
						entryLink = line.Substring(separatorIndex + CREDITS_NAME_HL_SEP.Length, line.Length - (entryName.Length + CREDITS_NAME_HL_SEP.Length + CREDITS_NAME_HL_END.Length));
					}
					else
						entryName = line;
				}
				else if (isInEntry)
				{
					entryDesc += $"{line}\n";
				}
			}
			return credits;
		}

		private List<string> GetResLines()
		{
			List<string> lines = new List<string>();

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SporeMods.CommonUI.CREDITS.md"))
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					lines.Add(line);
				}
			}

			return lines;
		}


		public FuncCommand<string> OpenUrlCommand
			= new FuncCommand<string>(url => WineHelper.OpenUrl(url));
	}
}
