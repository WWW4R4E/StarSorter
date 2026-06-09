using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace StarSorter.ViewModels
{
	public partial class ExploreViewModel : ObservableObject
	{

		public ExploreViewModel()
		{
			GenerateTestData();
		}

		public void GenerateTestData()
		{
		}

		public void LoadExploreChannels()
		{
			GenerateTestData();
		}
	}
}